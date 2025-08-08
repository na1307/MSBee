namespace MSBee.Tasks11;

public sealed class RewriteReferencePaths : Task {
    private static string? framework32;

    [Required]
    [Output]
    public string[] References { get; set; } = [];

    private static string? Framework32 {
        get {
            if (framework32 is not null) {
                return framework32;
            }

            const string regPath = @"SOFTWARE\Microsoft\.NETFramework";
            const string regPathWow64 = @"SOFTWARE\WOW6432Node\Microsoft\.NETFramework";
            var dotNetFramework = Registry.LocalMachine.OpenSubKey(regPathWow64);
            dotNetFramework ??= Registry.LocalMachine.OpenSubKey(regPath);

            using (dotNetFramework) {
                if (dotNetFramework?.GetValue("InstallRoot") is string installRoot) {
                    framework32 = installRoot.TrimEnd('\\');
                }
            }

            return framework32;
        }
    }

    public override bool Execute() {
        List<string> refs = [];

        foreach (var reference in References) {
            var rewrited = reference.Replace($"{Framework32}64", Framework32, StringComparison.OrdinalIgnoreCase)
                .Replace($"{Framework32}\\v2.0.50727", $"{Framework32}\\v1.1.4322", StringComparison.OrdinalIgnoreCase);

            refs.Add(rewrited);
            Log.LogMessage("Rewrited from \"{0}\" to \"{1}\"", reference, rewrited);
        }

        References = refs.ToArray();

        return true;
    }
}
