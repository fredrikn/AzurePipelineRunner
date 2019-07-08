using AzurePipelineRunner.BuildDefinitions;
using AzurePipelineRunner.BuildDefinitions.Steps;
using AzurePipelineRunner.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AzurePipelineRunner.Tasks
{
    public class TaskBuilder : ITaskBuilder
    {
        public virtual IEnumerable<Task> Build(
            Build build,
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

            var artifactStagingDirectoryKey = "build.artifactStagingDirectory";

            if (!build.Variables.KeyExists(artifactStagingDirectoryKey))
                build.Variables.AddKey(artifactStagingDirectoryKey, artifactStagingDir);

            // Makes sure all Variables has the correct Key format for the Tasks
            var variables = GetFormatedVariables(build.Variables);

            foreach (var step in build.Steps)
            {
                Task task;

                if (!string.IsNullOrEmpty(step.Script))
                    task = CreateCommandLineTask(step, variables, configuration);
                else if (!string.IsNullOrEmpty(step.Powershell))
                    task = CreatePowerShell3Task(step, variables, configuration);
                else if (!string.IsNullOrEmpty(step.TaskType))
                    task = CreateTask(step, variables, configuration);
                else
                    throw new NotSupportedException();

                var workingDirectoryKey = "workingDirectory";

                if (!task.Inputs.KeyExists(workingDirectoryKey))
                    task.Inputs.AddKey(workingDirectoryKey, step.WorkingDirectory == null ? workingDir : step.WorkingDirectory);

                var soloutionKey = "solution";

                if (task.Inputs.KeyExists(soloutionKey))
                {
                    if (!Path.IsPathRooted(task.Inputs.GetValueAsString(soloutionKey)))
                    {
                        task.Inputs.SetKey(soloutionKey, Path.Combine(sourceFolder, task.Inputs.GetValueAsString(soloutionKey)));
                    }
                }

                yield return task;
            }
        }

        private Task CreateTask(
            Step step,
            Dictionary<string, object> variables,
            IConfiguration configuration)
        {
            return CreateTask(step.TaskType, step, variables, configuration);
        }

        private Task CreateTask(
           string taskType,
           Step step,
           Dictionary<string, object> variables,
           IConfiguration configuration)
        {
            var task = new Task(configuration);
            task.TaskType = taskType.Replace("@", "V");
            task.Name = step.Name;
            task.DisplayName = GetValueWithVariableValues(step.DisplayName, variables); ;
            task.Condition = step.Condition;
            task.ContinueOnError = step.ContinueOnError;
            task.Enabled = step.Enabled;
            task.TimeoutInMinutes = step.TimeoutInMinutes;
            task.Env = step.Env;
            task.Inputs = GetInputsWithVariableValue(step, variables);

            if (string.IsNullOrEmpty(task.DisplayName))
                task.DisplayName = task.TaskType;

            task.TaskTargetFolder = Path.Combine(configuration.GetValue<string>("taskFolder"), task.TaskType);

            return task;
        }

        internal string GetValueWithVariableValues(string value, Dictionary<string, object> variables)
        {
            return UpdateValueWithVariables(value, variables);
        }

        internal Dictionary<string, object> GetInputsWithVariableValue(Step step, Dictionary<string, object> variables)
        {
            if (step.Inputs != null && step.Inputs.Count > 0)
            {
                var newInputs = new Dictionary<string, object>();

                foreach (var input in step.Inputs)
                {
                    object newValue = null;

                    if (input.Value is string)
                        newValue = UpdateValueWithVariables((string)input.Value, variables);
                    else
                        newValue = input.Value;

                    newInputs.AddKey(input.Key, newValue);
                }

                return newInputs;
            }

            return new Dictionary<string, object>();
        }

        private string UpdateValueWithVariables(string value, Dictionary<string, object> variables)
        {
            if (value == null)
                return null;

            if (value.Length == 0)
                return value;

            return Regex.Replace(value, @"\$\(([a-zA-Z0-9- _:.]+)\)", new MatchEvaluator(m => ReplaceVariable(m, variables)));
        }

        private string ReplaceVariable(Match m, Dictionary<string, object> variables)
        {
            var variableName = m.Groups[1].Value;

            if (variables.KeyExists(variableName))
                return variables.GetValueAsString(variableName);

            throw new ArgumentException($"The variable '{variableName}' can't be found");
        }


        private static Dictionary<string, object> GetFormatedVariables(Dictionary<string, object> dictionary)
        {
            var newDictionary = new Dictionary<string, object>();

            foreach (var item in dictionary)
            {
                newDictionary.AddKey(item.Key, item.Value);
            }

            return newDictionary;
        }

        private Task CreateCommandLineTask(Step step, Dictionary<string, object> variables, IConfiguration configuration)
        {
            var task = CreateTask("CmdLine@2", step, variables, configuration);

            task.Inputs.AddKey("script", GetValueWithVariableValues(step.Script, variables));
            task.Inputs.AddKey("failOnStderr", step.FailOnStderr);

            return task;
        }

        private Task CreatePowerShell3Task(Step step, Dictionary<string, object> variables, IConfiguration configuration)
        {
            var task = CreateTask("PowerShell@2", step, variables, configuration);

            task.Inputs.AddKey("script", GetValueWithVariableValues(step.Powershell, variables));
            task.Inputs.AddKey("targetType", "INLINE");
            task.Inputs.AddKey("pwsh", false);
            task.Inputs.AddKey("errorActionPreference", step.ErrorActionPreference);
            task.Inputs.AddKey("ignoreLASTEXITCODE", step.IgnoreLASTEXITCODE);
            task.Inputs.AddKey("failOnStderr", step.FailOnStderr);

            return task;
        }
    }
}
