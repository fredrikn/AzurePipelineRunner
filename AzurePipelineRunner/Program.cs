using AzurePipelineRunner.BuildDefinitions;
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
            var build = GetBuild(File.ReadAllText("BuildTest.yaml"));

            var outputStepReport = RunBuild(build);

            var buildReporter = new BuildReporter();
            buildReporter.ReportBuildResults(outputStepReport);

            Console.WriteLine("Done!");
        }

        private static List<StepReport> RunBuild(Build build)
        {
            var stepInvoker = new StepInvoker();

            var outputStepReport = new List<StepReport>();

            foreach (var step in build.Tasks)
            {
                Console.Write(Environment.NewLine);
                Console.WriteLine($"========================== BEGIN STEP '{step.DisplayName}' ==========================");
                Console.Write(Environment.NewLine);

                var stepReport = stepInvoker.RunStep(step);
                outputStepReport.Add(stepReport);

                Console.Write(Environment.NewLine);
                Console.WriteLine($"========================== END STEP '{step.DisplayName}' =============================");
                Console.Write(Environment.NewLine);
            }

            return outputStepReport;
        }

        private static Build GetBuild(string build)
        {
            var input = new StringReader(build);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .IgnoreUnmatchedProperties()
                .Build();

            return deserializer.Deserialize<Build>(input);
        }
    }
}