using AzurePipelineRunner.BuildDefinitions.Steps;
using System;

namespace AzurePipelineRunner.Tasks
{
    public class ShortcutCommandLineTask : BaseTask, IScriptTaskProperties
    {
        public ShortcutCommandLineTask(IScriptTaskProperties scriptProperties)
        {
            Script = scriptProperties.Script;
            DisplayName = scriptProperties.DisplayName;
        }

        public string Script { get; set; }

        public override void Run()
        {
            var commandLineTask = new CommandLineTask();

            commandLineTask.Run(Script);

            Console.WriteLine(DisplayName);
        }
    }

}
