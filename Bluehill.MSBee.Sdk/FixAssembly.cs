using dnlib.DotNet;
using Microsoft.Build.Framework;
using Task = Microsoft.Build.Utilities.Task;

namespace MSBee.AssemblyFixer;

public sealed class FixAssembly : Task {
    [Required]
    public required string TargetPath { get; init; }

    public override bool Execute() {
        var ad = AssemblyDef.Load(TargetPath);
        var module = ad.ManifestModule;
        module.Cor20HeaderRuntimeVersion = 0x20000;
        module.TablesHeaderVersion = 0x100;

        var originalPath = Path.Combine(Path.GetDirectoryName(TargetPath)!,
            Path.GetFileNameWithoutExtension(TargetPath) + "-original" + Path.GetExtension(TargetPath));

        File.Delete(originalPath);
        File.Move(TargetPath, originalPath);
        ad.Write(TargetPath);

        return true;
    }
}
