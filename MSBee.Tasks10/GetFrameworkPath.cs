using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Win32;

namespace MSBee.Tasks10;

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
                && File.Exists(System.IO.Path.Combine(System.IO.Path.Combine(installRoot, "v1.0.3705"), "csc.exe"))) {
                Path = System.IO.Path.Combine(installRoot, "v1.0.3705");
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
            Log.LogError(".NET Framework 1.0 not found.");
        }

        return !Log.HasLoggedErrors;
    }
}
