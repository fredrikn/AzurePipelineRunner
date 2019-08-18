using AzurePipelineRunner.BuildDefinitions;
using AzurePipelineRunner.BuildDefinitions.Steps;
using AzurePipelineRunner.Configuration;
using AzurePipelineRunner.Helpers;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AzurePipelineRunner.Tasks
{
    public class TaskBuilder : ITaskBuilder
    {
        private ITaskStore _taskLocator;

        public virtual async Task<IEnumerable<TaskStep>> Build(
            Build build,
            IAppConfiguration configuration)
        {
            _taskLocator = CreateTaskLocator(configuration);

            // TODO At the moment of programming the path is set to the path basded on VS debug mode path.
            // This will be changed to config and by default work when not running in debug mode in VS.
            var workingDir = Path.Combine(System.Environment.CurrentDirectory, "..\\..\\..\\");
            var sourceFolder = Path.Combine(System.Environment.CurrentDirectory, "..\\..\\..\\..\\");
            var artifactStagingDir = Path.Combine(System.Environment.CurrentDirectory, "..\\..\\..\\..\\artifact");

            // TODO Make sure all default Agent parameters is added if not specified
            // Make sure it will be easy to change the value of those properties by some 
            // kind of config

            var artifactStagingDirectoryKey = "build.artifactStagingDirectory";

            if (!build.Variables.KeyExists(artifactStagingDirectoryKey))
                build.Variables.AddKey(artifactStagingDirectoryKey, artifactStagingDir);

            // Makes sure all Variables has the correct Key format for the Tasks
            var variables = GetFormatedVariables(build.Variables);

            var steps = new List<TaskStep>();

            foreach (var step in build.Steps)
            {
                TaskStep task;

                if (!string.IsNullOrEmpty(step.Script))
                    task = await CreateCommandLineTask(step, variables, configuration);
                else if (!string.IsNullOrEmpty(step.Powershell))
                    task = await CreatePowerShell3Task(step, variables, configuration);
                else if (!string.IsNullOrEmpty(step.TaskType))
                    task = await CreateTask(step, variables, configuration);
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

                steps.Add(task);
            }

            return steps;
        }

        private ITaskStore CreateTaskLocator(IAppConfiguration configuration)
        {
            if (configuration.IsRemoteConfigured)
            {
                return new RemoteTaskStore(configuration);
            }
            else
            {
                return new LocalTaskStore(configuration);
            }
        }

        private async Task<TaskStep> CreateTask(
            Step step,
            Dictionary<string, object> variables,
            IAppConfiguration configuration)
        {
            return await CreateTask(step.TaskType, step, variables, configuration);
        }

        private async Task<TaskStep> CreateTask(
           string taskType,
           Step step,
           Dictionary<string, object> variables,
           IAppConfiguration configuration)
        {
            var task = new TaskStep(configuration);
            task.TaskType = taskType;
            task.Name = GetTaskName(taskType);
            task.Version = GetTaskVersion(taskType);
            task.DisplayName = GetValueWithVariableValues(step.DisplayName, variables);
            task.Condition = step.Condition;
            task.ContinueOnError = step.ContinueOnError;
            task.Enabled = step.Enabled;
            task.TimeoutInMinutes = step.TimeoutInMinutes;
            task.Env = step.Env;
            task.Inputs = GetInputsWithVariableValue(step, variables);

            if (string.IsNullOrEmpty(task.DisplayName))
                task.DisplayName = task.Name;

            task.TaskDefinition = await _taskLocator.GetTaskDefinition(task.Name, task.Version);
            task.TaskTargetFolder = await _taskLocator.DownloadTask(task.TaskDefinition);

            return task;
        }

        private static TaskDefinition GetTaskDefinition(IAppConfiguration configuration, TaskStep task)
        {
            if (configuration.IsRemoteConfigured)
            {
                return new RemoteTaskStore(configuration).GetTaskDefinition(task.Name, task.Version).Result;
            }
            else
            {
                return new LocalTaskStore(configuration).GetTaskDefinition(task.Name, task.Version).Result;
            }
        }

        private static string GetTaskName(string name)
        {
            if (name.Contains('@'))
            {
                return name.Substring(0, name.IndexOf('@'));
            }

            return name;
        }

        private static int GetTaskVersion(string name)
        {
            if (name.Contains('@'))
            {
                int version;

                if (int.TryParse(name.Substring(name.IndexOf('@')+1, 1), out version))
                    return version;
            }

            return -1;
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

        private async Task<TaskStep> CreateCommandLineTask(Step step, Dictionary<string, object> variables, IAppConfiguration configuration)
        {
            var task = await CreateTask("CmdLine@2", step, variables, configuration);

            task.Inputs.AddKey("script", GetValueWithVariableValues(step.Script, variables));
            task.Inputs.AddKey("failOnStderr", step.FailOnStderr);

            return task;
        }

        private async Task<TaskStep> CreatePowerShell3Task(Step step, Dictionary<string, object> variables, IAppConfiguration configuration)
        {
            var task = await CreateTask("PowerShell@2", step, variables, configuration);

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
