using Microsoft.TeamFoundation.DistributedTask.WebApi;
using System.Threading.Tasks;

namespace AzurePipelineRunner.Tasks
{
    /// <summary>
    /// Used to locate the path to the task script to be executed
    /// </summary>
    public interface ITaskStore
    {
        /// <summary>
        /// Gets the Defintion of the Task.
        /// </summary>
        /// <param name="task">The task to get, e.g CmdLine</param>
        /// <param name="version">The version to get, if version is set to -1, the latest will be used if any.</param>
        /// <returns></returns>
        Task<TaskDefinition> GetTaskDefinition(string task, int version);

        /// <summary>
        /// Locally download task to a temp folder to be used during the build
        /// </summary>
        /// <param name="taskDefinition">The task to be downloaded locally</param>
        /// <returns>Returns the path where the task is downloaded</returns>
        Task<string> DownloadTask(TaskDefinition taskDefinition);
    }
}
