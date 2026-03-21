namespace MSBee.Tasks;

public sealed class GetFrameworkPath : Task {
#if FX1_0
    private const string FXVersion = "v1.0.3705";
    private const string FXVersionName = "1.0";
#elif FX1_1
    private const string FXVersion = "v1.1.4322";
    private const string FXVersionName = "1.1";
#endif

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
                && File.Exists(System.IO.Path.Combine(System.IO.Path.Combine(installRoot, FXVersion), "csc.exe"))) {
                Path = System.IO.Path.Combine(installRoot, FXVersion);
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
            Log.LogError(".NET Framework {0} not found.", FXVersionName);
        }

        return !Log.HasLoggedErrors;
    }
}
