using AzurePipelineRunner.BuildDefinitions.Steps;

namespace AzurePipelineRunner.Tasks
{
    public class ShortcutCommandLineTask : BaseTask, IShortcutCommandLineScriptStep
    {
        public ShortcutCommandLineTask(IShortcutCommandLineScriptStep scriptProperties) : base(scriptProperties)
        {
            Script = scriptProperties.Script;
            WorkingDirectory = scriptProperties.WorkingDirectory;
            FailOnStderr = scriptProperties.FailOnStderr;
        }

        public string Script { get; set; }

        public string WorkingDirectory { get; set; }

        public bool FailOnStderr { get; set; }

        public override void Run()
        {
            var commandLineTask = new CommandLineTask();
            commandLineTask.Run(Script);
        }
    }

}
