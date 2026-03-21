namespace MSBee.Tasks;

public sealed class GetFrameworkSDKPath : Task {
#if FX1_0
    private const string SdkInstallRoot = "sdkInstallRoot";
    private const string FXVersionName = "1.0";
#elif FX1_1
    private const string SdkInstallRoot = "sdkInstallRootv1.1";
    private const string FXVersionName = "1.1";
#endif

    public GetFrameworkSDKPath() {
        const string regPath = @"SOFTWARE\Microsoft\.NETFramework";
        const string regPathWow64 = @"SOFTWARE\WOW6432Node\Microsoft\.NETFramework";
        var dotNetFramework = Registry.LocalMachine.OpenSubKey(regPathWow64);
        dotNetFramework ??= Registry.LocalMachine.OpenSubKey(regPath);

        if (dotNetFramework is null) {
            throw new NotSupportedException();
        }

        using (dotNetFramework) {
            if (dotNetFramework.GetValue(SdkInstallRoot) is string sdkInstallRoot) {
                Path = sdkInstallRoot;
            }
        }
    }

    [Output]
    public string? Path { get; }

    public override bool Execute() {
        if (string.IsNullOrEmpty(Path)) {
            Log.LogError(".NET Framework {0} SDK not found.", FXVersionName);
        }

        return !Log.HasLoggedErrors;
    }
}
