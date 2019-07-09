using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace AzurePipelineRunner.Helpers
{
    public static class ProcessRunner
    {
        public static string RunProcess(
            string fileName,
            string commandArguments,
            string workerDirectory,
            int timeout)
        {
            var processInfo = SetupProcessStartInfo(commandArguments, fileName, workerDirectory);

            var process = new Process();
            process.StartInfo = processInfo;

            var output = new StringBuilder();
            var error = new StringBuilder();

            using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
            using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                        outputWaitHandle.Set();
                    else
                    {
                        output.AppendLine(e.Data);
                        Console.WriteLine(e.Data);
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                        errorWaitHandle.Set();
                    else
                        error.AppendLine(e.Data);
                };

                //// Copy the environment variables.
                //if (environmentVariables != null && environmentVariables.Count > 0)
                //{
                //    foreach (var variable in environmentVariables)
                //    {
                //        process.StartInfo.Environment[variable.Key] = variable.Value;
                //    }
                //}

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Close the input stream to prevent commands from blocking the build waiting for user input.
                if (process.StartInfo.RedirectStandardInput)
                    process.StandardInput.Close();

                if (process.WaitForExit(timeout) &&
                    outputWaitHandle.WaitOne(timeout) &&
                    errorWaitHandle.WaitOne(timeout))
                {
                    if (process.ExitCode != 0 || !string.IsNullOrEmpty(error.ToString()))
                        throw new ApplicationException(string.Format("Error while executing batch script '{0}' - {1}", fileName, error));
                }
                else
                {
                    if (process != null && !process.HasExited)
                        process.Kill();

                    throw new ApplicationException($"The barch script '{fileName}' timed out");
                }
            }

            Console.WriteLine(error);

            return output.ToString();
        }


        private static ProcessStartInfo SetupProcessStartInfo(string commandArguments, string fileToRun, string workerDirectory)
        {
            return new ProcessStartInfo(fileToRun, commandArguments)
            {
                WorkingDirectory = workerDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true
            };
        }
    }
}
