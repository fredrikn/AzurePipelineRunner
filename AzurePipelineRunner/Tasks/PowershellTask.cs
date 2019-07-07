using AzurePipelineRunner.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace AzurePipelineRunner.Tasks
{
    //https://docs.microsoft.com/en-us/azure/devops/pipelines/yaml-schema?view=azure-devops&tabs=example#powershell

    //https://github.com/microsoft/azure-pipelines-tasks/blob/master/Tasks/PowerShellV2/powershell.ps1

    //https://github.com/microsoft/azure-pipelines-task-lib/blob/master/powershell/VstsTaskSdk/LegacyFindFunctions.ps1

    public class PowershellTask
    {
        public void Run(string scriptToRun)
        {
            var content = File.ReadAllText(@"D:\Repositories\azure-pipelines-tasks-master\Tasks\PowerShellV2/powershell.ps1");

            var inputs = new Dictionary<string, object> {
                { "input.script", scriptToRun },
                { "input.arguments", "" },
                { "input.pwsh", false },
                { "input.failOnStderr", true },
                { "input.ignoreLASTEXITCODE", false },
                { "input.targetType", "INLINE" },
                { "input.errorActionPreference", "Stop" },
                { "input.workingDirectory", Path.Combine(Environment.CurrentDirectory, "..\\..\\..\\") }
            };

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
                "D:\\temp",
                new List<string> { @"D:\Repositories\AzurePipelineRunner\AzurePipelineRunner\Helpers\powershell-common.ps1" },
                variables);
        }
    }
}
