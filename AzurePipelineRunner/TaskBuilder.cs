using AzurePipelineRunner.BuildDefinitions.Steps;
using AzurePipelineRunner.Tasks;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace AzurePipelineRunner
{
    public class TaskBuilder
    {
        private readonly IConfiguration _configuration;

        public Dictionary<string, string> _variables { get; set; }

        public IList<Step> _steps { get; set; }

        public TaskBuilder(
            IList<Step> steps,
            Dictionary<string, string> variables,
            IConfiguration configuration)
        {
            _variables = variables;
            _steps = steps;
            _configuration = configuration;
        }

        public IEnumerable<BaseTask> Build()
        {
            // TODO At the moment of programming the path is set to the path basded on VS debug mode path.
            // This will be changed to config and by default work when not running in debug mode in VS.
            var workingDir = Path.Combine(Environment.CurrentDirectory, "..\\..\\..\\");
            var sourceFolder = Path.Combine(Environment.CurrentDirectory, "..\\..\\..\\..\\");
            var artifactStagingDir = Path.Combine(Environment.CurrentDirectory, "..\\..\\..\\..\\artifact");

            // TODO Make sure all default Agent parameters is added if not specified
            // Make sure it will be easy to change the value of those properties by some 
            // kind of config

            if (!_variables.ContainsKey("build.artifactStagingDirectory"))
                _variables.Add("build.artifactStagingDirectory", artifactStagingDir);

            foreach (var step in _steps)
            {
                step.UpdatePropertiesWithVariableValues(_variables);

                Task task;

                if (!string.IsNullOrEmpty(step.Script))
                    task = CreateCommandLineTask(step);
                else if (!string.IsNullOrEmpty(step.Powershell))
                    task = CreatePowerShell3Task(step);
                else if (!string.IsNullOrEmpty(step.TaskType))
                    task = new Task(step, _configuration);
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

        private Task CreateCommandLineTask(Step step)
        {
            step.TaskType = "CmdLine@2";

            var task = new Task(step, _configuration);

            task.Inputs.Add("script", step.Script);
            task.Inputs.Add("failOnStderr", step.FailOnStderr);

            return task;
        }

        private Task CreatePowerShell3Task(Step step)
        {
            step.TaskType = "PowerShell@2";

            var task = new Task(step, _configuration);

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
