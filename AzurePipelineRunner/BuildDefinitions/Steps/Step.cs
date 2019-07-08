using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace AzurePipelineRunner.BuildDefinitions.Steps
{
    public class Step : IShortcutCommandLineScriptStep, IShortcutPowershellStep, ITaskStep
    {
        public string Script { get; set; }

        public string Powershell { get; set; }

        [YamlMember(Alias = "task")]
        public string TaskType { get; set; }

        public string WorkingDirectory { get; set; }

        public bool FailOnStderr { get; set; } = false;

        public string DisplayName { get; set; }

        public string Name { get; set; }

        public string Condition { get; set; }

        public bool ContinueOnError { get; set; } = false;

        public bool Enabled { get; set; } = true;

        public int TimeoutInMinutes { get; set; }

        public Dictionary<string, string> Env { get; set; }

        public bool IgnoreLASTEXITCODE { get; set; } = false;

        public Dictionary<string, object> Inputs { get; set; }

        public ErrorActionPreference ErrorActionPreference { get; set; }
    }
}
