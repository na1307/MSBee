using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Win32;

namespace MSBee.Tasks11;

public sealed class GetFrameworkPath : Task {
    public GetFrameworkPath() {
        const string regPath = @"SOFTWARE\Microsoft\.NETFramework";
        const string regPathWow64 = @"SOFTWARE\WOW6432Node\Microsoft\.NETFramework";
        var dotNetFramework = Registry.LocalMachine.OpenSubKey(regPathWow64);
        dotNetFramework ??= Registry.LocalMachine.OpenSubKey(regPath);

        if (dotNetFramework is null) {
            throw new NotSupportedException();
        }

        using (dotNetFramework) {
            if (dotNetFramework.GetValue("InstallRoot") is string installRoot
                && File.Exists(System.IO.Path.Combine(System.IO.Path.Combine(installRoot, "v1.1.4322"), "csc.exe"))) {
                Path = System.IO.Path.Combine(installRoot, "v1.1.4322");
            }
        }
    }

    [Output]
    public string? Path { get; }

    [Output]
    public string? FrameworkVersion35Path => Path;

    [Output]
    public string? FrameworkVersion40Path => Path;

    public override bool Execute() {
        if (string.IsNullOrEmpty(Path)) {
            Log.LogError(".NET Framework 1.1 not found.");
        }

        return !Log.HasLoggedErrors;
    }
}
