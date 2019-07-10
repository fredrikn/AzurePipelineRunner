using AzurePipelineRunner.BuildDefinitions;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzurePipelineRunner.Tasks
{
    public interface ITaskBuilder
    {
        Task<IEnumerable<TaskStep>> Build(
            Build build,
            IConfiguration configuration);
    }
}
