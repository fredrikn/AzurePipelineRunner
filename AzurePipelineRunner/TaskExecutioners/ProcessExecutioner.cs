using AzurePipelineRunner.Helpers;
using AzurePipelineRunner.Tasks;
using AzurePipelineRunner.Tasks.Definition;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;

namespace AzurePipelineRunner.TaskExecutioners
{
    public class ProcessExecutioner
    {
        private readonly IConfiguration _configuration;

        public ProcessExecutioner(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Execute(Task task, Execution taskExectionInfo)
        {
            if (!task.Inputs.KeyExists("filename"))
                throw new ArgumentException($"You need to specify the input 'filename' in the build definition file for task '{task.TaskType}'.");

            var fileName = task.Inputs.GetValueAsString("filename");
            string workingFolder = GetWorkingFolder(task, fileName);

            bool failOnStandardError = task.Inputs.GetValueAsBool("failOnStandardError", false);

            try
            {
                RunProcess(fileName, null, workingFolder);
            }
            catch (Exception e)
            {
                throw new ApplicationException($"Error while trying to run batch script '{fileName}'", e);
            }


            //RunProcess()

            //      -task: BatchScript@1
            //displayName: 'Run script build_in_container.bat'
            //inputs:
            //  filename: 'build_in_container.bat'
            //  arguments: 'Default %BUILD_DEFINITIONVERSION% %BUILD_SOURCESDIRECTORY%\ %BUILD_ARTIFACTSTAGINGDIRECTORY% sentbuildp02.lindex.local:9000'

            //    "name": "modifyEnvironment",
            //    "type": "boolean",
            //    "label": "Modify Environment",
            //    "defaultValue": "False",
            //    "required": false,
            //    "helpMarkDown": "Determines whether environment variable modifications will affect subsequent tasks."
            //},
            //{
            //    "name": "workingFolder",
            //    "type": "filePath",
            //    "label": "Working folder",
            //    "defaultValue": "",
            //    "required": false,
            //    "helpMarkDown": "Current working directory when script is run.  Defaults to the folder where the script is located.",
            //    "groupName": "advanced"
            //},
            //{
            //    "name": "failOnStandardError",

        }

        /// <summary>
        /// Gets working directory for the script. If not specified the Defaults will be the folder where the script is located."
        /// </summary>
        private static string GetWorkingFolder(Task task, string fileName)
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

        private static string RunProcess(string fileName, string commandArguments, string workerDirectory)
        {
            var processInfo = SetupProcessStartInfo(commandArguments, fileName, workerDirectory);

            var process = System.Diagnostics.Process.Start(processInfo);

            var error = process.StandardError.ReadToEnd();
            var output = process.StandardOutput.ReadToEnd();

            Console.WriteLine(error);
            Console.WriteLine(output);

            process.WaitForExit();

            if (process.ExitCode != 0 || !string.IsNullOrEmpty(error))
                throw new ApplicationException(string.Format("Error while executing batch script '{0}' - {1}", fileName, error));

            return output;
        }


        private static ProcessStartInfo SetupProcessStartInfo(string commandArguments, string fileToRun, string workerDirectory)
        {
            return new ProcessStartInfo(fileToRun, commandArguments)
            {
                WorkingDirectory = workerDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
        }
    }
}
