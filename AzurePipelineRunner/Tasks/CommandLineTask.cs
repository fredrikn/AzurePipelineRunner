using AzurePipelineRunner.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace AzurePipelineRunner.Tasks
{
    //https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/utility/command-line?view=azure-devops&tabs=yaml

    //https://github.com/microsoft/azure-pipelines-tasks/tree/master/Tasks/CmdLineV2

    public class CommandLineTask
    {
        public void Run(string scriptToRun)
        {
            var content = File.ReadAllText(@"D:\Repositories\AzurePipelineRunner\azure-pipelines-tasks-master\Tasks\CmdLineV2\cmdline.ps1");

            PowerShellInvoker.RunPowerShellScript(
                content,
                Environment.CurrentDirectory,
                new List<string> { @"D:\Repositories\AzurePipelineRunner\AzurePipelineRunner\Helpers\powershell-common.ps1" },
                new Dictionary<string, object> { { "script", scriptToRun } } );
        }
    }
}
