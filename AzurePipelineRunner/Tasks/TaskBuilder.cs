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
                UpdatePropertiesWithVariableValues(step, variables);
                UpdateInputsWithVariableValue(step, variables);

                Task task;

                if (!string.IsNullOrEmpty(step.Script))
                    task = CreateCommandLineTask(step, configuration);
                else if (!string.IsNullOrEmpty(step.Powershell))
                    task = CreatePowerShell3Task(step, configuration);
                else if (!string.IsNullOrEmpty(step.TaskType))
                    task = new Task(step, configuration);
                else
                    throw new NotSupportedException();

                // Makes sure all Inputs has the correct Key format for the Tasks
                task.Inputs = GetFormatedVariables(task.Inputs);

                var workingDirectoryKey = "workingDirectory";

                if (!task.Inputs.KeyExists(workingDirectoryKey))
                    task.Inputs.AddKey(workingDirectoryKey, step.WorkingDirectory == null ? workingDir : step.WorkingDirectory);

                var soloutionKey = "solution";

                if (task.Inputs.KeyExists(soloutionKey))
                {
                    if (!Path.IsPathRooted(task.Inputs.GetValueAsString(soloutionKey)))
                    {
                        task.Inputs.SetKey(soloutionKey, Path.Combine(sourceFolder, task.Inputs[soloutionKey].ToString()));
                    }
                }

                yield return task;
            }
        }


        internal void UpdatePropertiesWithVariableValues(Step step, Dictionary<string, object> variables)
        {
            foreach (var property in step.GetType().GetProperties())
            {
                if (property.CanWrite && property.CanRead && property.PropertyType == typeof(string))
                    property.SetValue(step, UpdateValueWithVariables(property.GetValue(step) as string, variables));
            }
        }

        internal void UpdateInputsWithVariableValue(Step step, Dictionary<string, object> variables)
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

                    newInputs.Add(input.Key, newValue);
                }

                step.Inputs = newInputs;
            }
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

            throw new System.ArgumentException($"The variable '{variableName}' can't be found");
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

        private Task CreateCommandLineTask(Step step, IConfiguration configuration)
        {
            step.TaskType = "CmdLine@2";

            var task = new Task(step, configuration);

            task.Inputs.AddKey("script", step.Script);
            task.Inputs.AddKey("failOnStderr", step.FailOnStderr);

            return task;
        }

        private Task CreatePowerShell3Task(Step step, IConfiguration configuration)
        {
            step.TaskType = "PowerShell@2";

            var task = new Task(step, configuration);

            task.Inputs.AddKey("script", step.Powershell);
            task.Inputs.AddKey("targetType", "INLINE");
            task.Inputs.AddKey("pwsh", false);
            task.Inputs.AddKey("errorActionPreference", step.ErrorActionPreference);
            task.Inputs.AddKey("ignoreLASTEXITCODE", step.IgnoreLASTEXITCODE);
            task.Inputs.AddKey("failOnStderr", step.FailOnStderr);

            return task;
        }
    }
}
