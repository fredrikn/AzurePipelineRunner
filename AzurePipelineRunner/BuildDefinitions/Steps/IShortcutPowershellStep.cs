namespace AzurePipelineRunner.BuildDefinitions.Steps
{
    public interface IShortcutPowershellStep : IStep
    {
        string Powershell { get; set; }

        bool IgnoreLASTEXITCODE { get; set; }

        ErrorActionPreference ErrorActionPreference { get; set; }
    }
}
