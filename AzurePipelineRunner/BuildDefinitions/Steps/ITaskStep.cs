using System.Collections.Generic;

namespace AzurePipelineRunner.BuildDefinitions.Steps
{
    public interface ITaskStep : IStep
    {
        string TaskType { get; set; }

        /// <summary>
        /// Tasks inputs
        /// </summary>
        Dictionary<string, object> Inputs { get; set; }
    }
}
