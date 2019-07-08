using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AzurePipelineRunner.BuildDefinitions
{
    public class BuildDefinitionReader : IBuildDefinitionReader
    {
        public Build GetBuild(string buildYamlPath)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .IgnoreUnmatchedProperties()
                .Build();

            var build = File.ReadAllText(buildYamlPath);
            var input = new StringReader(build);

            return deserializer.Deserialize<Build>(input);
        }
    }
}
