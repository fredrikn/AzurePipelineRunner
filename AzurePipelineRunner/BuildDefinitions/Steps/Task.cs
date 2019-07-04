namespace AzurePipelineRunner.BuildDefinitions.Steps
{
    public class Task : ITask, IScriptTaskProperties, IPowershellTaskProperties
    {
        public string Script { get; set; }

        public string Powershell { get; set; }

        /// <summary>
        /// Identifier for this step (A-Z, a-z, 0-9, and underscore)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Friendly name displayed in the UI
        /// </summary>
        public string DisplayName { get; set; }
    }
}
