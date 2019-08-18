using AzurePipelineRunner.Configuration;
using AzurePipelineRunner.Helpers;
using AzurePipelineRunner.Tasks;
using AzurePipelineRunner.Tasks.Definition;
using System.Collections.Generic;
using System.IO;

namespace AzurePipelineRunner.TaskExecutioners
{
    public class PowerShellExecutioner
    {
        private readonly IAppConfiguration _configuration;

        public PowerShellExecutioner(IAppConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Execute(TaskStep task)
        {
            var inputs = new Dictionary<string, object>();

            foreach (var input in task.Inputs)
                inputs.Add($"{input.Key}", input.Value);

            var taskVariables = CreateTaskVariables();

            var variables = new Dictionary<string, object>();

            foreach (var item in inputs)
                variables.Add("INPUT_" + item.Key.Replace(' ', '_').ToUpperInvariant(), item.Value);

            foreach (var item in taskVariables)
                variables.Add(item.Key.Replace(".", "_"), item.Value);

            var scriptName = task.TaskDefinition.Execution["PowerShell3"]["target"].ToString();

            string scriptToRun = Path.Combine(task.TaskTargetFolder, scriptName);

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
                    { "agent.tempDirectory", _configuration.TempDir },
                    { "System.Debug", _configuration.SystemDebug}
                };
        }
    }
}
