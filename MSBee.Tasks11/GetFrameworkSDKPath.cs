using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Win32;

namespace MSBee.Tasks11;

public sealed class GetFrameworkSDKPath : Task {
    public GetFrameworkSDKPath() {
        const string regPath = @"SOFTWARE\Microsoft\.NETFramework";
        const string regPathWow64 = @"SOFTWARE\WOW6432Node\Microsoft\.NETFramework";
        var dotNetFramework = Registry.LocalMachine.OpenSubKey(regPathWow64);
        dotNetFramework ??= Registry.LocalMachine.OpenSubKey(regPath);

        if (dotNetFramework is null) {
            throw new NotSupportedException();
        }

        using (dotNetFramework) {
            if (dotNetFramework.GetValue("sdkInstallRootv1.1") is string sdkInstallRootv11) {
                Path = sdkInstallRootv11;
            }
        }
    }

    [Output]
    public string? Path { get; }

    public override bool Execute() {
        if (string.IsNullOrEmpty(Path)) {
            Log.LogError(".NET Framework 1.1 SDK not found.");
        }

        return !Log.HasLoggedErrors;
    }
}
