using System;
using System.Collections.Generic;
using System.IO;
using AzurePipelineRunner.BuildDefinitions.Steps;
using AzurePipelineRunner.Helpers;
using Newtonsoft.Json;

namespace AzurePipelineRunner.Tasks
{
    public class Task : BaseTask, ITaskStep
    {
        public Task(ITaskStep step) : base(step)
        {
            Inputs = step.Inputs;
            TaskType = step.TaskType;

            if (string.IsNullOrEmpty(DisplayName))
                DisplayName = TaskType;
        }

        public Dictionary<string, object> Inputs { get; set; }

        public string TaskType { get; set; }

        public override void Run()
        {
            var task = TaskType.Replace("@", "V");

            string content = GetTaskScript(task);

            var inputs = new Dictionary<string, object>();

            foreach (var input in Inputs)
                inputs.Add($"input.{input.Key}", input.Value);

            if (!inputs.ContainsKey("input.workingDirectory"))
                inputs.Add("input.workingDirectory", Path.Combine(Environment.CurrentDirectory, "..\\..\\..\\"));

            var taskVariables = new Dictionary<string, object> {
                { "task.agent.tempDirectory", "D:\\temp" }
            };

            var variables = new Dictionary<string, object>();

            foreach (var item in inputs)
                variables.Add(item.Key, item.Value);

            foreach (var item in taskVariables)
                variables.Add(item.Key, item.Value);

            PowerShellInvoker.RunPowerShellScript(
                content,
                $@"D:\Repositories\azure-pipelines-tasks-master\Tasks\{task}",
                new List<string> { @"D:\Repositories\AzurePipelineRunner\AzurePipelineRunner\Helpers\powershell-common.ps1" },
                variables);
        }

        private string GetTaskScript(string task)
        {
            var taskInfoAsJson = File.ReadAllText($@"D:\Repositories\azure-pipelines-tasks-master\Tasks\{task}\task.json");
            var taskInfo = JsonConvert.DeserializeObject<TaskInfo>(taskInfoAsJson);

            var targetScript = taskInfo?.execution?.PowerShell3?.target;

            if (string.IsNullOrEmpty(targetScript))
                throw new NotSupportedException($"The Task '{TaskType}' doesn't have PowerShell3 support.");

            var content = File.ReadAllText($@"D:\Repositories\azure-pipelines-tasks-master\Tasks\{task}\{targetScript}");
            return content;
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
    }

    internal class TaskInfo
    {
        public Execution execution { get; set; }
    }
}
