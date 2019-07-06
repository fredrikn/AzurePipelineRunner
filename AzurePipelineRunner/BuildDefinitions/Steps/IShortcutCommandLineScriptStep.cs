namespace AzurePipelineRunner.BuildDefinitions.Steps
{
    public interface IShortcutCommandLineScriptStep : IStep
    {
        /// <summary>
        /// Contents of the script you want to run
        /// </summary>
        string Script { get; set; }

        /// <summary>
        /// Initial working directory for the step
        /// </summary>
        string WorkingDirectory { get; set; }

        /// <summary>
        /// If the script writes to stderr, should that be treated as the step failing?
        /// </summary>
        bool FailOnStderr { get; set; }
    }
}
