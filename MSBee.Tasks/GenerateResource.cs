namespace MSBee.Tasks;

public class GenerateResource : ToolTask {
    [Required]
    public ITaskItem[]? Sources { get; set; }

    [Output]
    public ITaskItem[]? OutputResources { get; set; }

    protected override string ToolName => "Resgen.exe";

    public override bool Execute() {
        Log.LogWarning("Resource generation is not supported.");

        return true;
    }

    protected override string GenerateFullPathToTool() => ToolName;
}
