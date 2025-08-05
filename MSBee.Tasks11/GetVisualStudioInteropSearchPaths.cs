namespace MSBee.Tasks11;

public sealed class GetVisualStudioInteropSearchPaths() : GetLocalMachineRegistryValues(VisualStudioInteropSearchPathKey) {
    public const string VisualStudioInteropSearchPathKey
        = @"Software\Microsoft\.NetFramework\v2.0.50727\AssemblyFoldersEx\Primary Interop Assemblies";
}
