using AzurePipelineRunner.BuildDefinitions.Steps;
using System.Collections.Generic;

namespace AzurePipelineRunner.BuildDefinitions
{
    public class Build
    {
        public Dictionary<string, object> Variables { get; set; }

        public List<Step> Steps { get; set; }
    }
}