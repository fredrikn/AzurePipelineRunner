using AzurePipelineRunner.BuildDefinitions;
using AzurePipelineRunner.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzurePipelineRunner.Tasks
{
    public interface ITaskBuilder
    {
        Task<IEnumerable<TaskStep>> Build(
            Build build,
            IAppConfiguration configuration);
    }
}
