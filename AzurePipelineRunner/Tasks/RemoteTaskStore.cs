using AzurePipelineRunner.Configuration;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace AzurePipelineRunner.Tasks
{
    public class RemoteTaskStore : ITaskStore
    {
        private IAppConfiguration _configuration;
        private TaskAgentHttpClient _httpClient;
        private List<TaskDefinition> _taskDefenitions;

        public RemoteTaskStore(IAppConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<TaskDefinition> GetTaskDefinition(string taskName, int version)
        {
            if (_httpClient == null)
                _httpClient = ConnectToServer(_configuration);

            if (_taskDefenitions == null)
            {
                _taskDefenitions = await _httpClient.GetTaskDefinitionsAsync();
                _taskDefenitions = FilterWithinMajorVersion(_taskDefenitions);
            }

            TaskDefinition taskDefinition;

            //If taskVersion is -1, we always get the latest version.
            if (version == -1)
            {
                var maxVersion = _taskDefenitions.Where( t => t.Name == taskName).Max(t => t.Version);
                taskDefinition = _taskDefenitions.FirstOrDefault( t => t.Name == taskName && t.Version == maxVersion);
            }
            else
            {
                taskDefinition = _taskDefenitions.FirstOrDefault(t => t.Version.Major == version && t.Name == taskName);
            }

            if (taskDefinition == null)
                throw new ArgumentException($"Can't find definition for task '{taskName}' and version '{version}'");

            return taskDefinition;
        }

        public async Task<string> DownloadTask(TaskDefinition taskDefinition)
        {
            var taskDestFolder = TaskStoreHelper.GetTaskLocation(taskDefinition.Name, taskDefinition.Version.Major);

            if (TaskStoreHelper.IsTaskAlreadyDownloaded(taskDestFolder))
                return taskDestFolder;

            var zipFile = Path.Combine(_configuration.TempDir, string.Format("{0}.zip", Guid.NewGuid()));

            await DownloadTask(taskDefinition, zipFile);

            TaskStoreHelper.ClearOrCreateTaskLocationFolder(taskDestFolder);
            ZipFile.ExtractToDirectory(zipFile, taskDestFolder);

            File.Delete(zipFile);

            return taskDestFolder;
        }

        private async Task DownloadTask(TaskDefinition taskDefinition, string zipFile)
        {
            using (var fs = new FileStream(zipFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                using (var result = await _httpClient.GetTaskContentZipAsync(taskDefinition.Id, taskDefinition.Version))
                {
                    //81920 is the default used by System.IO.Stream.CopyTo and is under the large object heap threshold (85k). 
                    await result.CopyToAsync(fs, 81920);
                    await fs.FlushAsync();
                }
            }
        }

        private List<TaskDefinition> FilterWithinMajorVersion(List<TaskDefinition> tasks)
        {
            return tasks
                .GroupBy(x => new { Id = x.Id, MajorVersion = x.Version }) // Group by ID and major-version
                .Select(x => x.OrderByDescending(y => y.Version).First()) // Select the max version
                .ToList();
        }

        private static TaskAgentHttpClient ConnectToServer(IAppConfiguration configuration)
        {
            var remoteUrl = configuration.RemoteUrl;

            if (!string.IsNullOrWhiteSpace(remoteUrl))
            {
                if (configuration.RemoteLoginType.ToUpper() == "PAT")
                {
                    var cred = new VssBasicCredential("anything", configuration.RemoteToken);
                    return new TaskAgentHttpClient(new Uri(remoteUrl), cred);
                }
            }

            return null;
        }
    }
}
