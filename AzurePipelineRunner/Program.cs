﻿using AzurePipelineRunner.BuildDefinitions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AzurePipelineRunner
{
    class Program
    {
        private readonly IConfiguration _configuration;

        public Program() {}

        public Program(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        static void Main(string[] args)
        {
            var services = ConfigureServices();

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetService<Program>().Run();
        }

        private void Run()
        {
            var build = GetBuild(File.ReadAllText("BuildTest.yaml"));

            var outputStepReport = RunBuild(build);

            var buildReporter = new BuildReporter();
            buildReporter.ReportBuildResults(outputStepReport);

            Console.WriteLine("Done!");
        }

        private List<StepReport> RunBuild(Build build)
        {
            var tasks = new TaskBuilder(build.Steps, build.Variables, _configuration).Build();

            var stepInvoker = new StepInvoker();

            var outputStepReport = new List<StepReport>();

            foreach (var step in tasks)
            {
                if (!step.Enabled)
                    continue;

                Console.Write(Environment.NewLine);
                Console.WriteLine($"========================== BEGIN STEP '{step.DisplayName}' ==========================");
                Console.Write(Environment.NewLine);

                var stepReport = stepInvoker.RunStep(step);
                outputStepReport.Add(stepReport);

                Console.Write(Environment.NewLine);
                Console.WriteLine($"========================== END STEP '{step.DisplayName}' =============================");
                Console.Write(Environment.NewLine);

                if (!step.ContinueOnError && !stepReport.Succeed)
                    break;
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

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            var config = LoadConfiguration();
            services.AddSingleton(config);

            services.AddTransient<Program>();
            return services;
        }

        public static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true,
                             reloadOnChange: true);

            return builder.Build();
        }
    }
}