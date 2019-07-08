using System;
using System.Collections.Generic;
using System.IO;
using AzurePipelineRunner.BuildDefinitions.Steps;
using AzurePipelineRunner.TaskExecutioners;
using AzurePipelineRunner.Tasks.Definition;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AzurePipelineRunner.Tasks
{
    public class Task
    {
        private Dictionary<string, object> _inputs;

        private IConfiguration _configuration;

        public Task(ITaskStep step, IConfiguration configuration)
        {
            _configuration = configuration;

            TaskType = step.TaskType;
            Name = step.Name;
            DisplayName = step.DisplayName;
            Condition = step.Condition;
            ContinueOnError = step.ContinueOnError;
            Enabled = step.Enabled;
            TimeoutInMinutes = step.TimeoutInMinutes;
            Env = step.Env;
            Inputs = step.Inputs;

            if (string.IsNullOrEmpty(DisplayName))
                DisplayName = TaskType;

            TaskTargetFolder = GetTaskMainFolder(TaskType.Replace("@", "V"));
        }

        public string TaskType { get; set; }

        internal string TaskTargetFolder { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Condition { get; set; }

        public bool ContinueOnError { get; set; }

        public bool Enabled { get; set; }

        public int TimeoutInMinutes { get; set; }

        public Dictionary<string, string> Env { get; set; }

        public Dictionary<string, object> Inputs
        {
            get
            {
                if (_inputs == null)
                    _inputs = new Dictionary<string, object>();

                return _inputs;
            }
            set => _inputs = value;
        }

        public virtual void Run()
        {
            var taskExectionInfo = GetTaskExecutionInfo();

            if(taskExectionInfo.IsBatchCommand())
            {
                var invoker = new ProcessExecutioner(_configuration);
                invoker.Execute(this, taskExectionInfo);
            }
            else if (taskExectionInfo.IsPowerShell3Supported())
            {
                var invoker = new PowerShellExecutioner(_configuration);
                invoker.Execute(this, taskExectionInfo);
            }
            else if (taskExectionInfo.IsNodeSupported())
            {
                Console.WriteLine($"The task '{TaskType}' only has Node script, that is currently not supported.");
            }
        }

        private Execution GetTaskExecutionInfo()
        {
            var taskInfoAsJson = File.ReadAllText(Path.Combine(TaskTargetFolder, "task.json"));
            var taskInfo = JsonConvert.DeserializeObject<TaskInfo>(taskInfoAsJson);
            return taskInfo.execution;
        }

        private string GetTaskMainFolder(string task)
        {
            return Path.Combine(_configuration.GetValue<string>("taskFolder"), task);
        }
    }
}
