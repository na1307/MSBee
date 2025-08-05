using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Win32;

namespace MSBee.Tasks10;

public class GenerateResource : ToolTask {
    // Constants used when deleting the temp subdirectory for resgen.
    // Maximum number of delete attempts.
    protected const int MaxDeleteAttempts = 10;

    // Time between delete attempts in milliseconds.
    protected const int DeleteDelay = 100;

    // FxCop wants me to create a property and keep the field private. However, any property I make is accessible
    // by MSBuild via the task and I want this field hidden so I'm choosing to keep it protected.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    // Path to temp subdirectory where resgen.exe and any references will be copied to.
    protected string? tempResGenPath;

    [Required]
    public ITaskItem[]? Sources { get; set; }

    [Output]
    public ITaskItem[]? OutputResources { get; set; }

    public ITaskItem[]? References { get; set; }

    protected override string ToolName => "Resgen.exe";

    protected override string GenerateFullPathToTool() => Path.Combine(Path.GetFullPath(tempResGenPath), ToolName);

    private string? OriginalPathToTool() {
        string? resgenPath = null;
        const string regPath = @"SOFTWARE\Microsoft\.NETFramework";
        const string regPathWow64 = @"SOFTWARE\WOW6432Node\Microsoft\.NETFramework";
        var dotNetFramework = Registry.LocalMachine.OpenSubKey(regPathWow64);
        dotNetFramework ??= Registry.LocalMachine.OpenSubKey(regPath);

        if (dotNetFramework is null) {
            throw new NotSupportedException();
        }

        using (dotNetFramework) {
            if (dotNetFramework.GetValue("sdkInstallRoot") is string sdkInstallRoot) {
                resgenPath = Path.Combine(Path.Combine(sdkInstallRoot, "Bin"), ToolName);
            }
        }

        if (string.IsNullOrEmpty(resgenPath)) {
            Log.LogError(
                """Task failed because "{0}" was not found, or the .NET Framework SDK {1} is not installed.  The task is looking for "{0}" in the "bin" subdirectory beneath the location specified in the {2} value of the registry key {3}.  You may be able to solve the problem by doing one of the following:  1.) Install the .NET Framework SDK {1}.  2.) Manually set the above registry key to the correct location.  3.) Pass the correct location into the "ToolPath" parameter of the task.""",
                ToolName, "v1.0", "SDKInstallRoot", "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\.NETFramework");
        }

        return resgenPath;
    }

    protected bool CreateOutputResourcesNames() {
        // If the user already provided names for the resources files, use them.
        if (OutputResources != null) {
            if (OutputResources.Length == Sources!.Length) {
                return true;
            } else {
                Log.LogMessage(MessageImportance.High, "OutputResources does not have one file name for each name in sources. Creating default file names for OutputResources.");
            }
        }

        // If the user didn't provide any names, create a new outputResources array and use the defaults.
        OutputResources = new ITaskItem[Sources!.Length];

        var i = 0;
        try {
            for (i = 0; i < Sources.Length; ++i) {
                OutputResources[i] = new TaskItem(Path.ChangeExtension(Sources[i].ItemSpec, ".resources"));
            }
        } catch (ArgumentException e) {
            Log.LogError("""Resource file "{0}" has an invalid name. {1}""", Sources[i].ItemSpec, e.Message);
            return false;
        }

        return true;
    }

    protected string? CreateUniqueTempDirectory() {
        string tempFile;
        string tempDir;

        try {
            // Create a uniquely named zero-byte file
            tempFile = Path.GetTempFileName();

            // Build a directory path using the uniquely named file.
            tempDir = Path.Combine(Path.GetDirectoryName(tempFile), Path.GetFileNameWithoutExtension(tempFile));
            tempDir += Path.DirectorySeparatorChar;

            // Create a directory under temp using the directory path.
            Directory.CreateDirectory(tempDir);

            // Delete the temp file.
            File.Delete(tempFile);
        } catch (IOException ex) {
            Log.LogError("""Task failed while creating a directory due to "{0}"{1}""", ex.Message, ex.StackTrace);
            return null;
        }

        // Return the name of the unique directory.
        return tempDir;
    }

    protected bool CopyReferences() {
        string referenceShortName;
        string destFileName;

        // If there are no references, then there's nothing to copy so just return true.
        if (References == null || References.Length == 0) {
            return true;
        }

        foreach (var reference in References) {
            referenceShortName = Path.GetFileName(reference.ItemSpec);
            destFileName = Path.Combine(tempResGenPath, referenceShortName);

            try {
                Log.LogMessage(MessageImportance.Normal, """Copying file from "{0}" to "{1}".""", reference, destFileName);
                File.Copy(reference.ItemSpec, destFileName, true);

                // Set the copied file's attributes to Normal so the file is deleteable.
                File.SetAttributes(destFileName, FileAttributes.Normal);
            } catch (IOException ex) {
                Log.LogError("""Task failed to copy "{0}" to "{1}" due to "{2}"{3}""", reference, destFileName, ex.Message, ex.StackTrace);
                return false;
            }
        }

        return true;
    }

    protected bool CopyResGen() {
        var srcFileName = OriginalPathToTool();
        var destFileName = GenerateFullPathToTool();

        try {
            Log.LogMessage(MessageImportance.Normal, """Copying file from "{0}" to "{1}".""", srcFileName, destFileName);
            File.Copy(srcFileName, destFileName);
        } catch (IOException ex) {
            Log.LogError("""Task failed to copy "{0}" to "{1}" due to "{2}"{3}""", srcFileName, destFileName, ex.Message, ex.StackTrace);
            return false;
        }

        return true;
    }

    protected bool DeleteTempResGenPath() {
        foreach (var tempFile in Directory.GetFiles(tempResGenPath)) {
            try {
                Log.LogMessage(MessageImportance.Normal, """Deleting file "{0}".""", tempFile);
                File.Delete(tempFile);
            } catch (UnauthorizedAccessException) {
                Log.LogMessage(MessageImportance.Normal, """Access was denied to "{0}"; will re-attempt deletion.""", tempFile);
            }
        }

        try {
            Log.LogMessage(MessageImportance.Normal, """Deleting directory "{0}".""", tempResGenPath);
            Directory.Delete(tempResGenPath, true);
        } catch (UnauthorizedAccessException uaex) {
            Log.LogError("""Task failed to delete "{0}" due to "{1}"{2}""", tempResGenPath, uaex.Message, uaex.StackTrace);
            return false;
        }

        return true;
    }

    private bool ExecuteResgen() {
        // Build the command line
        var retVal = -1;
        CommandLineBuilder commandLineBuilder = new();

        // Append the compile switch
        commandLineBuilder.AppendSwitch("/compile");

        // Append the resources to compile
        for (var i = 0; i < Sources!.Length; i++) {
            commandLineBuilder.AppendFileNamesIfNotNull([Sources[i].ItemSpec, OutputResources![i].ItemSpec], ",");
        }

        tempResGenPath = CreateUniqueTempDirectory();

        // Create the temp directory and copy references and resgen.exe into it.
        if (string.IsNullOrEmpty(tempResGenPath) || !Directory.Exists(tempResGenPath)) {
            return false;
        }

        if (!CopyReferences() || !CopyResGen()) {
            // Clean up if either copy method fails and return false.
            DeleteTempResGenPath();
            return false;
        }

        // Log the full command line and execute resgen.exe in the temp directory.
        Log.LogCommandLine(MessageImportance.High, $"{GenerateFullPathToTool()} {commandLineBuilder}");
        retVal = base.ExecuteTool(GenerateFullPathToTool(), null, commandLineBuilder.ToString());

        // Delete the temporary directory and its contents.
        DeleteTempResGenPath();

        // If Resgen.exe was successful, return true. Otherwise, return false.
        if (retVal == 0) {
            // Returns a failure if an error was logged after resgen.exe executed.
            return !Log.HasLoggedErrors;
        }

        Log.LogError($"Resgen.exe was not successful: {retVal}");

        return false;
    }

    public override bool Execute() {
        // If there are no sources to process, just return (with success) and report the condition.
        if (Sources == null || Sources.Length == 0) {
            // Indicate we generated nothing
            Log.LogMessage(MessageImportance.Low, """No resources specified in "Sources". Skipping resource generation.""");
            OutputResources = null;
            return true;
        }

        // If Resgen.exe isn't present under SDK\bin, return false.
        if (!File.Exists(OriginalPathToTool())) {
            OutputResources = null;
            return false;
        }

        // If creating the resource names failed, return false.
        if (!CreateOutputResourcesNames()) {
            OutputResources = null;
            return false;
        }

        for (var i = 0; i < Sources.Length; i++) {
            // Attributes from input items should be forwarded to output items.
            Sources[i].CopyMetadataTo(OutputResources![i]);
        }

        // If there are resources out of date, call ExecuteResgen. If ExecuteResgen fails, return false;
        if (!ExecuteResgen()) {
            OutputResources = null;
            return false;
        }

        // Return !Log.HasLoggedErrors so if errors are logged in a method but false isn't returned,
        // we'll still return false here.
        return !Log.HasLoggedErrors;
    }
}
