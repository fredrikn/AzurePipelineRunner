using System;
using System.Collections.Generic;
using System.IO;
using AzurePipelineRunner.BuildDefinitions.Steps;
using AzurePipelineRunner.Helpers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AzurePipelineRunner.Tasks
{
    public class Task : BaseTask, ITaskStep
    {
        private string _taskBasePath;

        private Dictionary<string, object> _inputs;

        private IConfiguration _configuration;

        public Task(ITaskStep step, IConfiguration configuration) : base(step)
        {
            _configuration = configuration;

            _taskBasePath = _configuration.GetValue<string>("taskFolder");

            Inputs = step.Inputs;
            TaskType = step.TaskType;

            if (string.IsNullOrEmpty(DisplayName))
                DisplayName = TaskType;

            var task = TaskType.Replace("@", "V");
            TaskTargetFolder = GetTaskMainFolder(task);
        }

        public Dictionary<string, object> Inputs
        {
            get
            {
                if (_inputs == null)
                {
                    _inputs = new Dictionary<string, object>();
                }

                return _inputs;
            }
            set => _inputs = value;
        }

        public string TaskType { get; set; }

        private string TaskTargetFolder { get; set; }

        public override void Run()
        {
            var taskExectionInfo = GetTaskExecutionInfo();

            if (taskExectionInfo.IsPowerShell3Supported())
            {
                InvokePowershellTask(taskExectionInfo);
            }
            else if (taskExectionInfo.IsNodeSupported())
            {
                Console.WriteLine($"The task '{TaskType}' only has Node script, that is currently not supported.");
            }
        }

        private void InvokePowershellTask(Execution taskExectionInfo)
        {
            var inputs = new Dictionary<string, object>();

            foreach (var input in Inputs)
                inputs.Add($"{input.Key}", input.Value);

            var taskVariables = new Dictionary<string, object> {
                    { "agent.tempDirectory", _configuration.GetValue<string>("agentTmpDir") },
                    { "System.Debug", _configuration.GetValue<bool>("systemDebug")}
                };

            var variables = new Dictionary<string, object>();

            foreach (var item in inputs)
                variables.Add("INPUT_" + item.Key.ToUpperInvariant(), item.Value);

            foreach (var item in taskVariables)
                variables.Add(item.Key.Replace(".", "_"), item.Value);

            string scriptToRun = GetTargetScriptPath(taskExectionInfo.PowerShell3.target);

            PowerShellInvoker.RunPowerShellScript(
                scriptToRun,
                TaskTargetFolder,
                null,
                variables);
        }

        private string GetTargetScriptPath(string targetFile)
        {
            return Path.Combine(TaskTargetFolder, targetFile);
        }

        private Execution GetTaskExecutionInfo()
        {
            var taskInfoAsJson = File.ReadAllText(Path.Combine(TaskTargetFolder, "task.json"));
            var taskInfo = JsonConvert.DeserializeObject<TaskInfo>(taskInfoAsJson);
            return taskInfo?.execution;
        }

        private string GetTaskMainFolder(string task)
        {
            return Path.Combine(_taskBasePath, task);
        }
    }

    internal class PowerShell3
    {
        public string target { get; set; }
        public List<string> platforms { get; set; }
    }

    internal class Node
    {
        public string target { get; set; }
        public string argumentFormat { get; set; }
    }

    internal class Execution
    {
        public PowerShell3 PowerShell3 { get; set; }

        public Node Node { get; set; }

        public bool IsNodeSupported()
        {
            return !string.IsNullOrEmpty(Node?.target);
        }

        public bool IsPowerShell3Supported()
        {
            // TODO: Check for platform support
            return !string.IsNullOrEmpty(PowerShell3?.target);
        }

    }

    internal class TaskInfo
    {
        public Execution execution { get; set; }
    }
}
