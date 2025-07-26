// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
// TYPELIBATTR clashes with the one in InteropServices.
using TYPELIBATTR = System.Runtime.InteropServices.ComTypes.TYPELIBATTR;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Build.Shared;

namespace Microsoft.Build.Extras.FX1_1
{
    /// <summary>
    /// Implements the "ResolveComReference" task, which invokes RCRFX1_1.exe
    /// to resolve COM references and generate any necessary COM interop.
    /// </summary>
    public class ResolveComReference : ToolTask
    {
        // The delimeter used to seperate metadata in TaskItem strings.
        private const string metadataDelimiter = "|~!&";

        #region Constructors

        /// <summary>
        /// public constructor
        /// </summary>
        public ResolveComReference() 
        {
            // Create a resource manager.
            TaskResources = new ResourceManager("Microsoft.Build.Extras.FX1_1.Strings", Assembly.GetExecutingAssembly());
        }

        #endregion

        #region Properties

        /// <summary>
        /// COM references specified by guid/version/lcid.
        /// </summary>
        public ITaskItem[] TypeLibNames
        {
            get
            {
                return typeLibNames;
            }
            set
            {
                typeLibNames = value;
            }
        }

        private ITaskItem[] typeLibNames;

        /// <summary>
        /// COM references specified by type library file path.
        /// </summary>
        public ITaskItem[] TypeLibFiles
        {
            get
            {
                return typeLibFiles;
            }
            set
            {
                typeLibFiles = value;
            }
        }

        private ITaskItem[] typeLibFiles;

        /// <summary>
        /// Where the directory wrapper files get generated.
        /// </summary>
        public string WrapperOutputDirectory
        {
            get
            {
                return wrapperOutputDirectory;
            }
            set
            {
                wrapperOutputDirectory = value;
            }
        }

        private string wrapperOutputDirectory;

        /// <summary>
        /// Source of resolved .NET assemblies; we need this for ActiveX wrappers since we can't resolve .NET assembly
        /// references ourselves.
        /// </summary>
        public ITaskItem[] ResolvedAssemblyReferences
        {
            get
            {
                return resolvedAssemblyReferences;
            }
            set
            {
                resolvedAssemblyReferences = value;
            }
        }

        private ITaskItem[] resolvedAssemblyReferences;

        /// <summary>
        /// Container name for public/private keys.
        /// </summary>
        public string KeyContainer
        {
            get
            {
                return keyContainer;
            }
            set
            {
                keyContainer = value;
            }
        }

        private string keyContainer;

        /// <summary>
        /// File containing public/private keys.
        /// </summary>
        public string KeyFile
        {
            get
            {
                return keyFile;
            }
            set
            {
                keyFile = value;
            }
        }

        private string keyFile;

        /// <summary>
        /// Causes the task to delay sign wrappers.
        /// </summary>
        public bool DelaySign
        {
            get
            {
                return delaySign;
            }
            set
            {
                delaySign = value;
            }
        }

        private bool delaySign;

        /// <summary>
        /// Passes the TypeLibImporterFlags.PreventClassMembers flag to tlb wrapper generation.
        /// </summary>
        public bool NoClassMembers
        {
            get
            {
                return noClassMembers;
            }
            set
            {
                noClassMembers = value;
            }
        }

        private bool noClassMembers;

        /// <summary>
        /// Paths to found/generated reference wrappers.
        /// </summary>
        [Output]
        public ITaskItem[] ResolvedFiles
        {
            get
            {
                return resolvedFiles;
            }
            set
            {
                resolvedFiles = value;
            }
        }

        private ITaskItem[] resolvedFiles;

        /// <summary>
        /// Paths to found modules (needed for isolation).
        /// </summary>
        [Output]
        public ITaskItem[] ResolvedModules
        {
            get
            {
                return resolvedModules;
            }
            set
            {
                resolvedModules = value;
            }
        }

        private ITaskItem[] resolvedModules;

        /// <summary>
        /// Cache file for COM component timestamps. If not present, every run will regenerate all the wrappers.
        /// </summary>
        public string StateFile
        {
            get { return stateFile; }
            set { stateFile = value; }
        }

        private string stateFile;

        #endregion

        protected override string ToolName
        {
            get
            {
                return "RCRFX1_1.exe";
            }
        }

        /// <summary>
        /// GenerateFullPathToTool() returns a path to RCRFX1_1.exe, based on the location of MSBee.dll.
        /// </summary>
        /// <remarks>
        /// This method finds MSBee.DLL and uses its location to build a path to RCRFX1_1.exe.
        /// </remarks>
        /// <returns>String containing a path to RCRFX1_1.exe.</returns>
        protected override string GenerateFullPathToTool()
        {
            // Path to the directory that contains the executing MSBee DLL.
            string pathToMSBee = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString();
            string pathToTool = "";

            try
            {
                pathToTool = Path.Combine(pathToMSBee, ToolName);
            }
            catch (ArgumentException aex)
            {
                Log.LogErrorFromResources("ToolPathGenerationFailed", aex.Message, aex.StackTrace);
            }

            return pathToTool;
        }

        /// <summary>
        /// Task entry point.
        /// </summary>
        /// <returns>True if task succeeds, which includes RCRFX1_1.exe succeeding. Otherwise, returns false.</returns>
        public override bool Execute()
        {
            // Add COM reference and assembly reference metadata to the response file and build the RCRFX1_1.exe command line.
            StringBuilder responseFileCommands = ObtainReferencesMetadata();
            CommandLineBuilder commandLineBuilder = BuildCommandLine();

            // Create the temp file that RCRFX1_1.exe will write its output to.
            string exeOutputFile = CreateTempOutputFile();

            if (String.IsNullOrEmpty(exeOutputFile))
            {
                return false;
            }
            else
            {
                // Append the path to the command line.
                commandLineBuilder.AppendSwitchIfNotNull("@", exeOutputFile);
            }

            // Log the full command line and execute RCRFX1_1.exe in the temp directory. 
            Log.LogCommandLine(MessageImportance.High, GenerateFullPathToTool() + " " + commandLineBuilder.ToString());

            // Execute the ResolveComReference executable.
            int retVal = base.ExecuteTool(GenerateFullPathToTool(), responseFileCommands.ToString(), commandLineBuilder.ToString());

            // If the executable returns success, try parsing its output file.
            if (retVal == 0)
            {
                if (File.Exists(exeOutputFile))
                {
                    // We're not returning true or false here. Instead, if an error is logged, it's accounted for
                    // in the final !Log.HasLoggedErrors return.
                    try
                    {
                        // Read the output file and then delete it.
                        ReadOutputFile(exeOutputFile);
                    }
                    catch (IOException ioe)
                    {
                        Log.LogErrorFromResources("OutputFileFailedProcessing", exeOutputFile, ioe.Message, ioe.StackTrace);
                    }
                    finally
                    {
                        // Always delete the outputFile.
                        File.Delete(exeOutputFile);
                    }
                }
                else
                {
                    Log.LogErrorFromResources("OutputFileDoesNotExist", ToolName, exeOutputFile);
                    return false;
                }
            }
            else
            {
                Log.LogErrorFromResources("ToolFailed", GenerateFullPathToTool(), retVal);
                return false;
            }

            // Return !Log.HasLoggedErrors so if errors are logged in a method but false isn't returned,
            // we'll still return false here.
            return !Log.HasLoggedErrors;
        }

        /// <summary>
        /// Converts all provided COM references and assembly reference metadata to strings 
        /// that are appended together.
        /// </summary>
        /// <returns>A StringBuilder object containing the COM and assembly references metadata.</returns>
        private StringBuilder ObtainReferencesMetadata()
        {
            StringBuilder arguments = new StringBuilder();

            // All TaskItems contain metadata; include it with the switch.
            if (typeLibNames != null && typeLibNames.Length > 0)
            {
                for (int t = 0; t < typeLibNames.Length; t++)
                {
                    arguments.Append("/typeLibName" + metadataDelimiter + ConvertItemMetadataToString(typeLibNames[t]));
                }
            }

            if (typeLibFiles != null && typeLibFiles.Length > 0)
            {
                for (int t = 0; t < typeLibFiles.Length; t++)
                {
                    arguments.Append("/typeLibFile" + metadataDelimiter + ConvertItemMetadataToString(typeLibFiles[t]));
                }
            }

            if (resolvedAssemblyReferences != null && resolvedAssemblyReferences.Length > 0)
            {
                for (int t = 0; t < resolvedAssemblyReferences.Length; t++)
                {
                    arguments.Append("/assemRef" + metadataDelimiter + ConvertItemMetadataToString(resolvedAssemblyReferences[t]));
                }
            }

            return arguments;
        }

        /// <summary>
        /// Adds switches to the RCRFX1_1.exe command line based on the task's property values.
        /// </summary>
        /// <returns>A CommandLineBuilder object representing the RCRFX1_1.exe command line.</returns>
        private CommandLineBuilder BuildCommandLine()
        {
            CommandLineBuilder clb = new CommandLineBuilder();

            // Append additional switches if they're set.
            clb.AppendSwitchIfNotNull("wrapperDir:", wrapperOutputDirectory);
            clb.AppendSwitchIfNotNull("keyContainer:", keyContainer);
            clb.AppendSwitchIfNotNull("keyFile:", keyFile);
            clb.AppendSwitchIfNotNull("stateFile:", stateFile);

            if (delaySign)
            {
                clb.AppendSwitch("delaySign");
            }

            if (noClassMembers)
            {
                clb.AppendSwitch("noClassMembers");
            }

            return clb;
        }

        /// <summary>
        /// Generate a unique temp file where RCRFX1_1.exe will store its output data.
        /// </summary>
        /// <returns>A path to the temp file if successful, null otherwise.</returns>
        private string CreateTempOutputFile()
        {
            try
            {
                return Path.GetTempFileName();
            }
            catch (IOException ioex)
            {
                Log.LogErrorFromResources("CreatingTempFileFailed", ioex.Message, ioex.StackTrace);
            }
            return null;
        }

        /// <summary>
        /// Reads the executable's output file and converts its data into TaskItems that are
        /// added to the task's arrays.
        /// </summary>
        /// <param name="filePath">The path to an existing output file.</param>
        protected void ReadOutputFile(string filePath)
        {
            ArrayList files = new ArrayList();
            ArrayList modules = new ArrayList();

            StreamReader sr;
            string fileData;

            // Try reading the response file.
            try
            {
                sr = new StreamReader(filePath);
                fileData = sr.ReadToEnd();
            }
            catch (IOException ioex)
            {
                Log.LogErrorFromResources("FileReadFailed", filePath, ioex.Message, ioex.StackTrace);
                return;
            }

            // Prepare the RegEx.
            // This string represents the "|" character.
            string bar = Regex.Escape(metadataDelimiter);

            // This string is a class for all character except "/" and "|", which are our delimiters.
            string noDelimWordClass = "([^" + bar + "/]+)";

            Regex fileParser = new Regex(@"/" + noDelimWordClass + "(?:" + bar + noDelimWordClass + ")+");
            Match m = fileParser.Match(fileData);

            // Traverses the matches in order. Each match represents a TaskItem returned from the RCR executable.
            while (m.Success)
            {
                if (m.Groups == null || m.Groups.Count < 2)
                {
                    Log.LogErrorFromResources("RegExFailedToMatch", filePath);
                    sr.Close();
                    return;
                }

                // Get the collection name, which is always in group 1.
                string collection = m.Groups[1].Value;

                // Get the TaskItem's metadata, which are always in group 2.
                string[] metadata = new string[m.Groups[2].Captures.Count];

                // A CaptureCollection maintains the order of the captures.
                CaptureCollection cc = m.Groups[2].Captures;
                for (int j = 0; j < cc.Count; j++)
                {
                    metadata[j] = cc[j].Value;
                }

                // Convert the Metadata to a TaskItem that is added to the proper ArrayList.
                if (collection.Equals("resolvedFile"))
                {
                    files.Add(ConvertStringsToItemMetadata(metadata));
                }
                else if (collection.Equals("resolvedModule"))
                {
                    modules.Add(ConvertStringsToItemMetadata(metadata));
                }
                else
                {
                    Log.LogErrorFromResources("OutputFileHasUnexpectedSwitch", collection);
                    sr.Close();
                    return;
                }

                // Returns the next match, where the next match starts at the end of the previous match.
                m = m.NextMatch();
            }

            resolvedFiles = (ITaskItem[])files.ToArray(typeof(ITaskItem));
            resolvedModules = (ITaskItem[])modules.ToArray(typeof(ITaskItem));

            sr.Close();
        }

        /// <summary>
        /// Converts a TaskItem's metadata into a string.
        /// </summary>
        /// <param name="ti">The TaskItem to be converted.</param>
        /// <returns>A string representing the TaskItem's metadata.</returns>
        /// <remarks>
        /// If you're wondering why I don't serialize the TaskItems, 
        /// it's because they're not Serializable.
        /// </remarks>
        protected static string ConvertItemMetadataToString(ITaskItem ti)
        {
            // If the TaskItem is somehow null, return "".
            if (ti == null)
            {
                return String.Empty;
            }

            // Always supply the ItemSpec value first.
            StringBuilder pairs = new StringBuilder(ti.ItemSpec + metadataDelimiter);

            // We need at least one delimiter to seperate each value accompanying the switch.
            // In the case, I've arbitrarily chosen the delimiter to be a '|'. Since we'll be 
            // writing these arguments to a temporary response file, we don't need to worry
            // about the pipe's behavior. A pipe is great since it's a character that can't
            // appear in a file or directory name.

            foreach (string metadataName in ti.MetadataNames)
            {
                string value = ti.GetMetadata(metadataName);
                if (!String.IsNullOrEmpty(value))
                {
                    pairs.Append(metadataName + metadataDelimiter);
                    pairs.Append(value + metadataDelimiter);
                }
            }

            // Remove the final delimiter since it's unnecessary.
            pairs.Remove(pairs.Length - 1, 1);

            return pairs.ToString();
        }

        /// <summary>
        /// Convert a string array of metadata into a TaskItem
        /// </summary>
        /// <param name="values">A string array of metadata.</param>
        /// <returns>A TaskItem containing the metadata.</returns>
        protected TaskItem ConvertStringsToItemMetadata(string[] values)
        {
            if (values == null)
            {
                Log.LogErrorFromResources("NullParameter", "values");
                return null;
            }

            // The values array should always have at least one value
            // and should always contain an odd number of elements.
            if (values.Length < 1 || values.Length % 2 == 0)
            {
                Log.LogErrorFromResources("IncorrectNumberOfMetadata", values.Length);
                return null;
            }

            // Build an IDictionary of metadata for the TaskItem.
            Hashtable pairs = new Hashtable(values.Length);

            for (int v = 1; v < values.Length; v += 2)
            {
                pairs.Add(values[v], values[v + 1]);
            }

            // The first item in the values array is always the "ItemSpec" value.
            return new TaskItem(values[0], pairs);
        }
    }
}