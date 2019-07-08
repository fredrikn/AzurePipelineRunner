using AzurePipelineRunner.BuildDefinitions.Steps;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace AzurePipelineRunner.Tasks
{
    public interface ITaskBuilder
    {
        IEnumerable<BaseTask> Build(
            IList<Step> steps,
            Dictionary<string, string> variables,
            IConfiguration configuration);
    }
}
