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
                // TODO At the moment of programming the path is set to the path basded on VS debug mode path.
                // This will be changed to config and by default work when not running in debug mode in VS.
                var workingDir = Path.Combine(Environment.CurrentDirectory, "..\\..\\..\\");
                var sourceFolder = Path.Combine(Environment.CurrentDirectory, "..\\..\\..\\..\\");
                var artifactStagingDir = Path.Combine(Environment.CurrentDirectory, "..\\..\\..\\..\\artifact");

                // TODO Make sure all default Agent parameters is added if not specified
                // Make sure it will be easy to change the value of those properties by some 
                // kind of config

                if (!Variables.ContainsKey("build.artifactStagingDirectory"))
                    Variables.Add("build.artifactStagingDirectory", artifactStagingDir);

                foreach (var step in AllSteps)
                {
                    step.UpdatePropertiesWithVariableValues(Variables);

                    Task task;

                    if (!string.IsNullOrEmpty(step.Script))
                        task = CreateCommandLineTask(step);
                    else if (!string.IsNullOrEmpty(step.Powershell))
                        task = CreatePowerShell3Task(step);
                    else if (!string.IsNullOrEmpty(step.TaskType))
                        task = new Task(step);
                    else
                        throw new NotSupportedException();

                    if (!task.Inputs.ContainsKey("workingDirectory"))
                        task.Inputs.Add("workingDirectory", step.WorkingDirectory == null ? workingDir : step.WorkingDirectory);

                    if (task.Inputs.ContainsKey("solution"))
                    {
                        if (!Path.IsPathRooted((string)task.Inputs["solution"]))
                        {
                            task.Inputs["solution"] = Path.Combine(sourceFolder, task.Inputs["solution"].ToString());
                        }
                    }

                    yield return task;
                }
            }
        }

        private static Task CreateCommandLineTask(Step step)
        {
            step.TaskType = "CmdLine@2";

            var task = new Task(step);

            task.Inputs.Add("script", step.Script);
            task.Inputs.Add("failOnStderr", step.FailOnStderr);

            return task;
        }

        private static Task CreatePowerShell3Task(Step step)
        {
            step.TaskType = "PowerShell@2";

            var task = new Task(step);

            task.Inputs.Add("script", step.Powershell);
            task.Inputs.Add("targetType", "INLINE");
            task.Inputs.Add("pwsh", false);
            task.Inputs.Add("errorActionPreference", step.ErrorActionPreference);
            task.Inputs.Add("ignoreLASTEXITCODE", step.IgnoreLASTEXITCODE);
            task.Inputs.Add("failOnStderr", step.FailOnStderr);

            return task;
        }
    }
}
