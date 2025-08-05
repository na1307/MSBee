using System.Security;

namespace MSBee.Tasks11;

public sealed class GetVisualStudioSearchPaths() : GetLocalMachineRegistryValues(VisualStudioSearchPathKey) {
    public const string VisualStudioSearchPathKey = @"Software\Microsoft\VisualStudio\7.1\AssemblyFolders";

    public override bool Execute() {
        if (base.Execute()) {
            try {
                if (RootKey != null) {
                    // Now, process the next level of subkeys; we already know that SubKeyCount is greater than 0.
                    foreach (var subkey in RootKey.GetSubKeyNames()) {
                        AddValuesToRegistryValuesList(RootKey.OpenSubKey(subkey));
                    }
                }

                return !Log.HasLoggedErrors;
            }
            // If an exception was thrown, log the exception and return failure.
            catch (SecurityException ex) {
                Log.LogErrorFromException(ex, true);

                return false;
            }
        }

        return false;
    }
}
