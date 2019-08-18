using AzurePipelineRunner.Configuration;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzurePipelineRunner.Tasks
{
    public class LocalTaskStore : ITaskStore
    {
        private IAppConfiguration _configuration;

        public LocalTaskStore(IAppConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> DownloadTask(TaskDefinition taskDefinition)
        {
            var taskDestFolder = TaskStoreHelper.GetTaskLocation(taskDefinition.Name, taskDefinition.Version.Major);

            if (TaskStoreHelper.IsTaskAlreadyDownloaded(taskDestFolder))
                return taskDestFolder;

            await Task.Run( () => 
                DirectoryCopy(GetLocalTaskFolder(taskDefinition.Name, taskDefinition.Version.Major), taskDestFolder));

            return taskDestFolder;
        }

        public async Task<TaskDefinition> GetTaskDefinition(string task, int version)
        {
            if (version <= 0)
                throw new ArgumentException($"The version '{version}' can't be zero or lesser.");

            var taskInfoAsJson = await File.ReadAllTextAsync(Path.Combine(GetLocalTaskFolder(task, version), "task.json"));
            return JsonConvert.DeserializeObject<TaskDefinition>(taskInfoAsJson);
        }

        private string GetLocalTaskFolder(string task, int version)
        {
            return Path.Combine(_configuration.TaskLocalPath, $"{task}V{version}");
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName)
        {
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: '{sourceDirName}'");

            var dirs = dir.GetDirectories();

            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            foreach (var file in dir.GetFiles())
                file.CopyTo(Path.Combine(destDirName, file.Name), false);

            foreach (DirectoryInfo subdir in dirs)
                DirectoryCopy(subdir.FullName, Path.Combine(destDirName, subdir.Name));
        }
    }
}
