using AzurePipelineRunner.BuildDefinitions;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace AzurePipelineRunner.Tasks
{
    public interface ITaskBuilder
    {
        IEnumerable<Task> Build(
            Build build,
            IConfiguration configuration);
    }
}
