using AzurePipelineRunner.BuildDefinitions.Steps;
using AzurePipelineRunner.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
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
                    {
                        step.TaskType = "CmdLine@2";
                        var task = new Task(step);

                        task.Inputs.Add("script", step.Script);
                        task.Inputs.Add("failOnStderr", step.FailOnStderr);
                        task.Inputs.Add("workingDirectory", step.WorkingDirectory == null ? Path.Combine(Environment.CurrentDirectory, "..\\..\\..\\") : step.WorkingDirectory);

                        yield return task;
                    }
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
