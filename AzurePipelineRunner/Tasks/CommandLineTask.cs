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
            var content = File.ReadAllText(@"D:\Repositories\azure-pipelines-tasks-master\Tasks\CmdLineV2\cmdline.ps1");

            var inputs = new Dictionary<string, object> {
                { "input.script", scriptToRun },
                { "input.failOnStderr", "$TRUE" },
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
                Environment.CurrentDirectory,
                new List<string> { @"D:\Repositories\AzurePipelineRunner\AzurePipelineRunner\Helpers\powershell-common.ps1" },
                variables);
        }
    }
}
