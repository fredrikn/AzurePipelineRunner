using AzurePipelineRunner.BuildDefinitions;
using AzurePipelineRunner.Report;
using AzurePipelineRunner.Tasks;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace AzurePipelineRunner
{
    class MainProgram
    {
        private readonly IConfiguration _configuration;
        private readonly IBuildReporter _buildReporter;
        private readonly IBuildDefinitionReader _buildDefinitionReader;
        private readonly ITaskBuilder _taskBuilder;

        public MainProgram(
            IConfiguration configuration,
            ITaskBuilder taskBuilder,
            IBuildDefinitionReader buildDefinitionReader,
            IBuildReporter buildReporter)
        {
            _configuration = configuration;
            _buildReporter = buildReporter;
            _taskBuilder = taskBuilder;
            _buildDefinitionReader = buildDefinitionReader;
        }

        public void Run(string buildYamlPath)
        {
            var build = _buildDefinitionReader.GetBuild(buildYamlPath);

            var outputStepReport = RunBuild(build);

            _buildReporter.ReportBuildResults(outputStepReport);

            Console.WriteLine("Done!");
        }

        private List<StepReport> RunBuild(Build build)
        {
            var tasks = _taskBuilder.Build(build.Steps, build.Variables, _configuration);

            var stepInvoker = new StepInvoker();

            var outputStepReport = new List<StepReport>();

            foreach (var step in tasks)
            {
                if (!step.Enabled)
                    continue;

                RenderBeginStepText(step);

                var stepReport = stepInvoker.RunStep(step);
                outputStepReport.Add(stepReport);

                RenderEndStepText(step);

                if (!step.ContinueOnError && !stepReport.Succeed)
                    break;
            }

            return outputStepReport;
        }

        private static void RenderEndStepText(BaseTask step)
        {
            Console.Write(Environment.NewLine);
            Console.WriteLine($"========================== END STEP '{step.DisplayName}' =============================");
            Console.Write(Environment.NewLine);
        }

        private static void RenderBeginStepText(BaseTask step)
        {
            Console.Write(Environment.NewLine);
            Console.WriteLine($"========================== BEGIN STEP '{step.DisplayName}' ==========================");
            Console.Write(Environment.NewLine);
        }
    }
}
