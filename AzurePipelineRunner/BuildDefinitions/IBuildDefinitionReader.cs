namespace AzurePipelineRunner.BuildDefinitions
{
    public interface IBuildDefinitionReader
    {
        Build GetBuild(string buildYamlPath);
    }
}
