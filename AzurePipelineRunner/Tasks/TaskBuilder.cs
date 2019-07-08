using AzurePipelineRunner.BuildDefinitions.Steps;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace AzurePipelineRunner.Tasks
{
    public class TaskBuilder : ITaskBuilder
    {
        public virtual IEnumerable<Task> Build(
            IList<Step> steps,
            Dictionary<string, string> variables,
            IConfiguration configuration)
        {
            // TODO At the moment of programming the path is set to the path basded on VS debug mode path.
            // This will be changed to config and by default work when not running in debug mode in VS.
            var workingDir = Path.Combine(Environment.CurrentDirectory, "..\\..\\..\\");
            var sourceFolder = Path.Combine(Environment.CurrentDirectory, "..\\..\\..\\..\\");
            var artifactStagingDir = Path.Combine(Environment.CurrentDirectory, "..\\..\\..\\..\\artifact");

            // TODO Make sure all default Agent parameters is added if not specified
            // Make sure it will be easy to change the value of those properties by some 
            // kind of config

            if (!variables.ContainsKey("build.artifactStagingDirectory"))
                variables.Add("build.artifactStagingDirectory", artifactStagingDir);

            foreach (var step in steps)
            {
                step.UpdatePropertiesWithVariableValues(variables);

                Task task;

                if (!string.IsNullOrEmpty(step.Script))
                    task = CreateCommandLineTask(step, configuration);
                else if (!string.IsNullOrEmpty(step.Powershell))
                    task = CreatePowerShell3Task(step, configuration);
                else if (!string.IsNullOrEmpty(step.TaskType))
                    task = new Task(step, configuration);
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

        private Task CreateCommandLineTask(Step step, IConfiguration configuration)
        {
            step.TaskType = "CmdLine@2";

            var task = new Task(step, configuration);

            task.Inputs.Add("script", step.Script);
            task.Inputs.Add("failOnStderr", step.FailOnStderr);

            return task;
        }

        private Task CreatePowerShell3Task(Step step, IConfiguration configuration)
        {
            step.TaskType = "PowerShell@2";

            var task = new Task(step, configuration);

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
