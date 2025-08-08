using System.Security;

namespace MSBee.Tasks10;

public abstract class GetLocalMachineRegistryValues : Task {
    // The registry key/path.
    private readonly string registryKey;

    // The base registry key; the default is HKEY_LOCAL_MACHINE.
    private readonly RegistryKey baseRegistryKey = Registry.LocalMachine;

    // List of values found underneath the provided registry key.
    private readonly List<string> registryValues;

    protected GetLocalMachineRegistryValues(string registryKey) {
        this.registryKey = registryKey;
        registryValues = [];

        try {
            RootKey = baseRegistryKey.OpenSubKey(registryKey);
        } catch (SecurityException ex) {
            Log.LogErrorFromException(ex, true);
        }
    }

    [Output]
    public string[] Values => registryValues.ToArray();

    protected RegistryKey? RootKey { get; }

    public override bool Execute() {
        // If an exception was raised in constructor, return immediately.
        if (Log.HasLoggedErrors) {
            return true;
        }

        try {
            if (RootKey != null) {
                AddValuesToRegistryValuesList(RootKey);
            } else {
                var registryKeyPath = baseRegistryKey.ToString() + Path.DirectorySeparatorChar + registryKey;

                Log.LogMessage(MessageImportance.Normal, "\"{0}\" does not exist in the local machine's registry.", registryKeyPath);
            }

            // Return !Log.HasLoggedErrors so if errors are logged in a method but false isn't returned,
            // we'll still return false here.
            return !Log.HasLoggedErrors;
        }
        // If an exception was thrown, log the exception and return failure.
        catch (SecurityException ex) {
            Log.LogErrorFromException(ex, true);

            return false;
        }
    }

    protected void AddValuesToRegistryValuesList(RegistryKey baseKey) {
        foreach (var value in baseKey.GetValueNames()) {
            registryValues.Add(baseKey.GetValue(value).ToString());
        }
    }
}
