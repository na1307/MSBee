namespace MSBee.Tasks;

public sealed class RewriteReferencePaths : Task {
#if FX1_0
    private const string FXVersion = "v1.0.3705";
#elif FX1_1
    private const string FXVersion = "v1.1.4322";
#endif

    [Required]
    [Output]
    public string[] References { get; set; } = [];

    private static string? Framework32 {
        get {
            if (field is not null) {
                return field;
            }

            const string regPath = @"SOFTWARE\Microsoft\.NETFramework";
            const string regPathWow64 = @"SOFTWARE\WOW6432Node\Microsoft\.NETFramework";
            var dotNetFramework = Registry.LocalMachine.OpenSubKey(regPathWow64);
            dotNetFramework ??= Registry.LocalMachine.OpenSubKey(regPath);

            using (dotNetFramework) {
                if (dotNetFramework?.GetValue("InstallRoot") is string installRoot) {
                    field = installRoot.TrimEnd('\\');
                }
            }

            return field;
        }
    }

    public override bool Execute() {
        List<string> refs = [];

        foreach (var reference in References) {
            var rewrited = reference.Replace($"{Framework32}64", Framework32, StringComparison.OrdinalIgnoreCase)
                .Replace($"{Framework32}\\v2.0.50727", $"{Framework32}\\{FXVersion}", StringComparison.OrdinalIgnoreCase);

            refs.Add(rewrited);
            Log.LogMessage("Rewrited from \"{0}\" to \"{1}\"", reference, rewrited);
        }

        References = refs.ToArray();

        return true;
    }
}
