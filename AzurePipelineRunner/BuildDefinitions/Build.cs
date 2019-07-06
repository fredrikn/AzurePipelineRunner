using AzurePipelineRunner.BuildDefinitions.Steps;
using AzurePipelineRunner.Tasks;
using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace AzurePipelineRunner.BuildDefinitions
{
    public class Build
    {
        public Dictionary<string, string> Variables { get; set; }

        [YamlMember(Alias = "steps")]
        public List<Step> AllSteps { get; set; }

        public IEnumerable<BaseTask> Tasks
        {
            get
            {
                foreach (var step in AllSteps)
                {
                    step.UpdatePropertiesWithVariableValues(Variables);

                    if (!string.IsNullOrEmpty(step.Script))
                        yield return new ShortcutCommandLineTask(step);
                    else if (!string.IsNullOrEmpty(step.Powershell))
                        yield return new ShortcutPowershellTask(step);
                    else if (!string.IsNullOrEmpty(step.TaskType))
                        yield return new Task(step);
                    else
                        throw new NotSupportedException();
                }
            }
        }
    }
}
