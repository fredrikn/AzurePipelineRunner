using AzurePipelineRunner.BuildDefinitions.Steps;
using System;

namespace AzurePipelineRunner.Tasks
{
    public class PowershellTask : BaseTask, IPowershellTaskProperties
    {
        public PowershellTask(IPowershellTaskProperties scriptProperties)
        {
            Powershell = scriptProperties.Powershell;
            DisplayName = scriptProperties.DisplayName;
        }

        public string Powershell { get; set; }

        public override void Run()
        {
            Console.WriteLine(DisplayName);
        }
    }
}
