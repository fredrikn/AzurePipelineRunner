namespace AzurePipelineRunner.Tasks
{
    public abstract class BaseTask
    {
        public abstract void Run();

        public string Name { get; set; }

        public string DisplayName { get; set; }
    }
}
