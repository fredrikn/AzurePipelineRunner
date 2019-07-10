using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AzurePipelineRunner.TaskExecutioners;
using AzurePipelineRunner.Tasks.Definition;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AzurePipelineRunner.Tasks
{
    public class TaskStep
    {
        private Dictionary<string, object> _inputs;

        private IConfiguration _configuration;

        public TaskStep(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string TaskType { get; set; }

        internal string TaskTargetFolder { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Condition { get; set; }

        public bool ContinueOnError { get; set; }

        public bool Enabled { get; set; }

        public int TimeoutInMinutes { get; set; }

        public Dictionary<string, string> Env { get; set; }

        public Dictionary<string, object> Inputs
        {
            get
            {
                if (_inputs == null)
                    _inputs = new Dictionary<string, object>();

                return _inputs;
            }
            set => _inputs = value;
        }

        public virtual async Task Run()
        {
            var taskExectionInfo = GetTaskExecutionInfo();

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

            if (taskExectionInfo.IsBatchCommand())
            {
                var invoker = new ProcessExecutioner(_configuration);
                invoker.Execute(this, taskExectionInfo);
            }
            else if (taskExectionInfo.IsPowerShell3Supported())
            {
                var invoker = new PowerShellExecutioner(_configuration);
                invoker.Execute(this, taskExectionInfo);
            }
            else if (taskExectionInfo.IsNodeSupported())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"The task '{TaskType}' only has Node script, that is currently not supported.");
                Console.ResetColor();
            }

            return;
        }

        private Execution GetTaskExecutionInfo()
        {
            var taskInfoAsJson = File.ReadAllText(Path.Combine(TaskTargetFolder, "task.json"));
            var taskInfo = JsonConvert.DeserializeObject<TaskInfo>(taskInfoAsJson);
            return taskInfo.execution;
        }
    }
}
