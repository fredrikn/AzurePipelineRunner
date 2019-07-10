﻿using AzurePipelineRunner.Helpers;
using AzurePipelineRunner.Tasks;
using AzurePipelineRunner.Tasks.Definition;
using Microsoft.Extensions.Configuration;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace AzurePipelineRunner.TaskExecutioners
{
    public class PowerShellExecutioner
    {
        private readonly IConfiguration _configuration;

        public PowerShellExecutioner(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Execute(TaskStep task, Execution taskExectionInfo)
        {
            var inputs = new Dictionary<string, object>();

            foreach (var input in task.Inputs)
                inputs.Add($"{input.Key}", input.Value);

            var taskVariables = CreateTaskVariables();

            var variables = new Dictionary<string, object>();

            foreach (var item in inputs)
                variables.Add("INPUT_" + item.Key.Replace(' ','_').ToUpperInvariant(), item.Value);

            foreach (var item in taskVariables)
                variables.Add(item.Key.Replace(".", "_"), item.Value);

            string scriptToRun = Path.Combine(task.TaskTargetFolder, taskExectionInfo.PowerShell3.target);

            var timeout = task.TimeoutInMinutes * 60 * 1000;

            PowerShellInvoker.RunPowerShellScript(
                scriptToRun,
                task.TaskTargetFolder,
                null,
                variables,
                timeout: timeout == 0 ? -1 : timeout);
        }

        private Dictionary<string, object> CreateTaskVariables()
        {
            return new Dictionary<string, object> {
                    { "agent.tempDirectory", _configuration.GetValue<string>("agentTmpDir") },
                    { "System.Debug", _configuration.GetValue<bool>("systemDebug")}
                };
        }
    }
}
