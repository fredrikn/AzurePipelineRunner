using System.IO;

namespace AzurePipelineRunner.Tasks
{
    public static class TaskStoreHelper
    {
        public static bool IsTaskAlreadyDownloaded(string taskDestFolder)
        {
            return Directory.Exists(taskDestFolder) && !IsTaskDirectoryEmpty(taskDestFolder);
        }

        public static bool IsTaskDirectoryEmpty(string taskDestFolder)
        {
            if (!Directory.Exists(taskDestFolder))
                return true;

            return Directory.GetFiles(taskDestFolder, "*", SearchOption.TopDirectoryOnly).Length == 0;
        }

        public static string GetTaskLocation(string taskName, int taskMajorVersion)
        {
            return Path.Combine(System.Environment.CurrentDirectory, "_tasks", $"{taskName}V{taskMajorVersion}");
        }

        public static void ClearOrCreateTaskLocationFolder(string taskDestFolder)
        {
            if (!IsTaskDirectoryEmpty(taskDestFolder))
                Directory.Delete(taskDestFolder, true);

            if (!Directory.Exists(taskDestFolder))
                Directory.CreateDirectory(taskDestFolder);
        }
    }
}
