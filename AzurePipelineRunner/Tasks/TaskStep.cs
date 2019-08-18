using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzurePipelineRunner.Configuration;
using AzurePipelineRunner.TaskExecutioners;
using Microsoft.TeamFoundation.DistributedTask.WebApi;

namespace AzurePipelineRunner.Tasks
{
    public class TaskStep
    {
        private Dictionary<string, object> _inputs;

        private IAppConfiguration _configuration;

        public TaskStep(IAppConfiguration configuration)
        {
            _configuration = configuration;
        }

        internal string TaskTargetFolder { get; set; }

        public string TaskType { get; set; }

        public string Name { get; set; }

        public int Version { get; set; }

        public string DisplayName { get; set; }

        public TaskDefinition TaskDefinition {get; set; }

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
            if (IsBatchCommand())
            {
                var invoker = new ProcessExecutioner(_configuration);
                invoker.Execute(this);
            }
            else if (IsPowerShell3Supported())
            {
                var invoker = new PowerShellExecutioner(_configuration);
                invoker.Execute(this);
            }
            else if (IsNodeSupported())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"The task '{TaskType}' only has Node script, that is currently not supported.");
                Console.ResetColor();
            }

            return;
        }

        private bool IsBatchCommand()
        {
            if (!TaskDefinition.Execution.ContainsKey("Process"))
                return false;

            return true; //!string.IsNullOrWhiteSpace(TaskDefinition.Execution["Process"]);
        }

        private bool IsNodeSupported()
        {
            if (!TaskDefinition.Execution.ContainsKey("Node"))
                return false;

            return true; //!string.IsNullOrWhiteSpace(TaskDefinition.Execution["Process"]);
        }

        private bool IsPowerShell3Supported()
        {
            if (!TaskDefinition.Execution.ContainsKey("PowerShell3"))
                return false;

            return true; //!string.IsNullOrWhiteSpace(TaskDefinition.Execution["Process"]);
        }
    }
}
