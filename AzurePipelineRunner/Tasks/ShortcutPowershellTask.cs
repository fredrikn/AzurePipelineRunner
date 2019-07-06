using AzurePipelineRunner.BuildDefinitions.Steps;
using System;

namespace AzurePipelineRunner.Tasks
{
    public class ShortcutPowershellTask : BaseTask, IShortcutPowershellStep
    {
        public ShortcutPowershellTask(IShortcutPowershellStep scriptProperties) : base(scriptProperties)
        {
            Powershell = scriptProperties.Powershell;
            IgnoreLASTEXITCODE = scriptProperties.IgnoreLASTEXITCODE;
            ErrorActionPreference = ErrorActionPreference;
        }

        public string Powershell { get; set; }

        public bool IgnoreLASTEXITCODE { get; set; }

        public ErrorActionPreference ErrorActionPreference { get; set; }

        public override void Run()
        {
            var powershellTask = new PowershellTask();
            powershellTask.Run(Powershell);
        }
    }
}
