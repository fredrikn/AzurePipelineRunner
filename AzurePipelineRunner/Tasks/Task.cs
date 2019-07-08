using System;
using System.Collections.Generic;
using System.IO;
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

        public Task(IConfiguration configuration)
        {
            _configuration = configuration;
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
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"The task '{TaskType}' only has Node script, that is currently not supported.");
                Console.ResetColor();
            }
        }

        private Execution GetTaskExecutionInfo()
        {
            var taskInfoAsJson = File.ReadAllText(Path.Combine(TaskTargetFolder, "task.json"));
            var taskInfo = JsonConvert.DeserializeObject<TaskInfo>(taskInfoAsJson);
            return taskInfo.execution;
        }
    }
}
