namespace AzurePipelineRunner.BuildDefinitions.Steps
{
    public interface IShortcutPowershellStep : IStep
    {
        string Powershell { get; set; }
    }
}
