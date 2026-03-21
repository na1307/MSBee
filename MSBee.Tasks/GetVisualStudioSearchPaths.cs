using System.Security;

namespace MSBee.Tasks;

public sealed class GetVisualStudioSearchPaths() : GetLocalMachineRegistryValues(VisualStudioSearchPathKey) {
#if FX1_0
    public const string VisualStudioSearchPathKey = @"Software\Microsoft\VisualStudio\7.0\AssemblyFolders";
#elif FX1_1
    public const string VisualStudioSearchPathKey = @"Software\Microsoft\VisualStudio\7.1\AssemblyFolders";
#endif

    public override bool Execute() {
        if (base.Execute()) {
            try {
                if (RootKey is not null) {
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
