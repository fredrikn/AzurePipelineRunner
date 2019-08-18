using AzurePipelineRunner.Configuration;
using AzurePipelineRunner.Helpers;
using AzurePipelineRunner.Tasks;
using System;
using System.IO;

namespace AzurePipelineRunner.TaskExecutioners
{
    public class ProcessExecutioner
    {
        private readonly IAppConfiguration _configuration;

        public ProcessExecutioner(IAppConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Execute(TaskStep task)
        {
            if (!task.Inputs.KeyExists("filename"))
                throw new ArgumentException($"You need to specify the input 'filename' in the build definition file for task '{task.TaskType}'.");

            var fileName = task.Inputs.GetValueAsString("filename");
            string workingFolder = GetWorkingFolder(task, fileName);

            bool failOnStandardError = task.Inputs.GetValueAsBool("failOnStandardError", false);

            var timeoutInMinutes = task.TimeoutInMinutes * 60 * 1000;

            try
            {
                ProcessRunner.RunProcess(fileName, null, workingFolder, timeoutInMinutes == 0 ? -1 : timeoutInMinutes);
            }
            catch (Exception e)
            {
                throw new ApplicationException($"Error while trying to run batch script '{fileName}' in the folder '{workingFolder}'", e);
            }


     
            //    "name": "modifyEnvironment",
            //    "type": "boolean",
            //    "label": "Modify Environment",
            //    "defaultValue": "False",
            //    "required": false,
            //    "helpMarkDown": "Determines whether environment variable modifications will affect subsequent tasks."
            //},
            //{
            //    "name": "failOnStandardError",

        }

        /// <summary>
        /// Gets working directory for the script. If not specified the Defaults will be the folder where the script is located."
        /// </summary>
        private static string GetWorkingFolder(TaskStep task, string fileName)
        {
            if (task.Inputs.KeyExists("workingFolder"))
                return task.Inputs.GetValueAsString("workingFolder");
            else
            {
                if (Path.IsPathRooted(fileName))
                    return Path.GetDirectoryName(fileName);
                else
                    return Path.GetDirectoryName(Path.Combine(Environment.CurrentDirectory, fileName));
            }
        }
    }
}
