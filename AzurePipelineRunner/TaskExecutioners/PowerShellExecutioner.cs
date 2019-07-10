using AzurePipelineRunner.Helpers;
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

        public void Execute(Task task, Execution taskExectionInfo)
        {
            // TODO Make the possibility to download task from own Azure DevOps account.

           // var cred = new VssBasicCredential("test", "PAT");
           // var client = new TaskAgentHttpClient(new Uri("URL"), cred);

           // System.Threading.Tasks.Task.Run(async () =>
           //{

           //    var tasks = await client.GetTaskDefinitionsAsync();

           //    var zipFile = Path.Combine("d:\\temp", string.Format("{0}.zip", Guid.NewGuid()));

           //    using (FileStream fs = new FileStream(zipFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
           //    {
           //        using (Stream result = await client.GetTaskContentZipAsync(Guid.Parse("D9BAFED4-0B18-4F58-968D-86655B4D2CE9"), new TaskVersion("2.151.1")))
           //        {
           //             //81920 is the default used by System.IO.Stream.CopyTo and is under the large object heap threshold (85k). 
           //             await result.CopyToAsync(fs, 81920);
           //            await fs.FlushAsync();
           //        }
           //    }
           //}).Wait();

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
