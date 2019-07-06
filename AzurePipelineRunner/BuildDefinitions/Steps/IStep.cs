using System.Collections.Generic;

namespace AzurePipelineRunner.BuildDefinitions.Steps
{
    public interface IStep
    {
        /// <summary>
        /// Friendly name displayed in the UI
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// Identifier for this step (A-Z, a-z, 0-9, and underscore)
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// https://docs.microsoft.com/en-us/azure/devops/pipelines/process/conditions?tabs=yaml&view=azure-devops
        /// </summary>
        string Condition { get; set; }

        /// <summary>
        /// 'true' if future steps should run even if this step fails; defaults to 'false'
        /// </summary>
        bool ContinueOnError { get; set; }

        /// <summary>
        /// Whether or not to run this step; defaults to 'true'
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// https://docs.microsoft.com/en-us/azure/devops/pipelines/process/phases?tabs=yaml&view=azure-devops#timeouts
        /// </summary>
        int TimeoutInMinutes { get; set; }

        /// <summary>
        /// List of environment variables to add
        /// </summary>
        Dictionary<string,string> Env { get; set; }
    }
}