namespace AzurePipelineRunner.BuildDefinitions.Steps
{
    public interface ITask
    {
        /// <summary>
        /// Friendly name displayed in the UI
        /// </summary>
        string DisplayName { get; set; }
    }
}