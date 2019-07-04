namespace AzurePipelineRunner.BuildDefinitions.Steps
{
    public interface IScriptTaskProperties : ITask
    {
        string Script { get; set; }

        string Name { get; set; }

        /*
        workingDirectory: string  # initial working directory for the step
          failOnStderr: boolean
        #if the script writes to stderr, should that be treated as the step failing?
          condition: string
          continueOnError: boolean  # 'true' if future steps should run even if this step fails; defaults to 'false'
          enabled: boolean  # whether or not to run this step; defaults to 'true'
          timeoutInMinutes: number
          env: { string: string }  # list of environment variables to add
          */
    }
}
