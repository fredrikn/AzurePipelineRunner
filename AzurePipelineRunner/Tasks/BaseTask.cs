using System.Collections.Generic;
using AzurePipelineRunner.BuildDefinitions.Steps;

namespace AzurePipelineRunner.Tasks
{
    public abstract class BaseTask : IStep
    {
        protected BaseTask(IStep step)
        {
            Name = step.Name;
            DisplayName = step.DisplayName;
            Condition = step.Condition;
            ContinueOnError = step.ContinueOnError;
            Enabled = step.Enabled;
            TimeoutInMinutes = step.TimeoutInMinutes;
            Env = step.Env;
        }

        public abstract void Run();

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Condition { get; set; }

        public bool ContinueOnError { get; set; }

        public bool Enabled { get; set; }

        public int TimeoutInMinutes { get; set; }

        public Dictionary<string, string> Env { get; set; }
    }
}
