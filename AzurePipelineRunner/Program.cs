using AzurePipelineRunner.BuildDefinitions.Steps;
using AzurePipelineRunner.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AzurePipelineRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = new StringReader(File.ReadAllText("BuildTest.yaml"));

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .IgnoreUnmatchedProperties()
                .Build();

            var build = deserializer.Deserialize<Build>(input);

            foreach (var step in build.Tasks)
            {
                step.Run();
            }

            Console.WriteLine("Done!");
        }
    }

    public class Build
    {
        public Dictionary<string, string> Variables { get; set; }

        [YamlMember(Alias = "steps")]
        public List<Step> AllSteps { get; set; }

        public IEnumerable<BaseTask> Tasks
        {
            get
            {
                foreach (var step in AllSteps)
                {
                    step.UpdatePropertiesWithVariableValues(Variables);

                    if (!string.IsNullOrEmpty(step.Script))
                        yield return new ShortcutCommandLineTask(step);
                    else if (!string.IsNullOrEmpty(step.Powershell))
                        yield return new ShortcutPowershellTask(step);
                    else
                       throw new NotSupportedException();
                }
            }
        }
    }
}