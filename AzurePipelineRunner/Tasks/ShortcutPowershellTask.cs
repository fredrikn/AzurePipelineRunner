using AzurePipelineRunner.BuildDefinitions.Steps;
using System;

namespace AzurePipelineRunner.Tasks
{
    public class ShortcutPowershellTask : BaseTask, IShortcutPowershellStep
    {
        public ShortcutPowershellTask(IShortcutPowershellStep scriptProperties) : base(scriptProperties)
        {
            Powershell = scriptProperties.Powershell;
        }

        public string Powershell { get; set; }

        public override void Run()
        {
            Console.WriteLine(DisplayName);
        }
    }
}
