using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AzurePipelineRunner.Helpers
{
    public static class PowerShellInvoker
    {
        public static string RunPowerShellScript(
            string script, 
            string workerDirectory, 
            List<string> includeScripts = null,
            Dictionary<string, object> variables = null,
            bool verbose = false,
            int timeout = -1)
        {
            var bootstrapFile = CreatePowerShellBootstrapFile(script, workerDirectory, includeScripts, variables, verbose);

            try
            {
                var commandArguments = CreateProcessCommandArguments(bootstrapFile);
                return RunPowershellProcess(script, commandArguments, workerDirectory, timeout);
            }
            finally
            {
                File.Delete(bootstrapFile);
            }
        }
    
        private static StringBuilder CreateProcessCommandArguments(string bootstrapFile)
        {
            var commandArguments = new StringBuilder();

            commandArguments.Append("-NonInteractive ");
            commandArguments.Append("-NoLogo ");
            commandArguments.Append("-ExecutionPolicy ByPass ");
            commandArguments.AppendFormat("-File \"{0}\"", bootstrapFile);
    
            return commandArguments;
        }
    
        private static string CreatePowerShellBootstrapFile(
            string script,
            string workerPath,
            List<string> includeScrips = null,
            Dictionary<string, object> variables = null,
            bool verbose = false)
        {
            var bootstrapFile = Path.Combine(workerPath, "TempDeploy." + Guid.NewGuid() + ".ps1");
    
            using (var writer = new StreamWriter(bootstrapFile))
            {
                var vstsTaskSdkPath = Path.Combine(workerPath, @"ps_modules\VstsTaskSdk");

                writer.WriteLine(@"Import-Module -Name " + vstsTaskSdkPath + " -ArgumentList @{ NonInteractive = $true; }");

                AddVariablesToScript(variables, writer);

                // Fake an agent version
                writer.WriteLine("$env:AGENT_VERSION = '2.115.0'");

                writer.WriteLine("## Invoke Script:");
                writer.WriteLine($"Invoke-VstsTaskScript -ScriptBlock ([scriptblock]::Create('{script}'))");

                //TODO make sure verbose is something that is passed from the main argument instead as an optional settings

                if (verbose)
                    writer.Write(" -Verbose");

                writer.Flush();
            }
    
            return bootstrapFile;
        }
    
    
        private static void AddVariablesToScript(Dictionary<string, object> variables, StreamWriter writer)
        {
            foreach (var variable in variables)
            {
                string value;

                if (variable.Value is bool)
                    value = (bool)variable.Value ? "$true" : "$false";
                else
                    value = CreatePowerShellEncondingText(variable.Value);

                WriteVariableLine(TransformToValidPowerShellVariableName(variable.Key), value, writer);
            }
        }
    
        private static string CreatePowerShellEncondingText(object value)
        {
            return string.Format(
                "[System.Text.Encoding]::Unicode.GetString( [Convert]::FromBase64String( \"{0}\" ) )",
                Convert.ToBase64String(Encoding.Unicode.GetBytes(value == null ? string.Empty : value.ToString())));
        }
    
    
        private static string TransformToValidPowerShellVariableName(string variableKey)
        {
            if (string.IsNullOrWhiteSpace(variableKey))
                throw new ArgumentNullException("variableKey");

            return variableKey.Replace(".", string.Empty)
                              .Replace("-", string.Empty);
        }
    
        private static void WriteVariableLine(string variableName, string value, StreamWriter writer)
        {
            writer.WriteLine("$Env:{0} = {1}", variableName, value);
        }
    
    
        private static string RunPowershellProcess(string originalScript, StringBuilder commandArguments, string workerDirectory, int timeout)
        {
            var powerShellExe = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";

            return ProcessRunner.RunProcess(powerShellExe, commandArguments.ToString(), workerDirectory, timeout);
        }
    }
}
