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
            var input = new StringReader(Document);

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

        static string Document = @"
# ASP.NET Core
# Build and test ASP.NET Core projects targeting .NET Core.
# Add steps that run tests, create a NuGet package, deploy, and more:
# https://docs.microsoft.com/azure/devops/pipelines/languages/dotnet-core

trigger:
- master

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'

steps:
- script: dotnet build --configuration $(buildConfiguration)
  displayName: 'dotnet build $(buildConfiguration)'
- powershell: Write-Host 'Hello World!'
  displayName: 'Write-Host Hello World!'
";
    }

    public class Build
    {
        public Dictionary<string, string> Variables { get; set; }

        [YamlMember(Alias = "steps")]
        public List<Task> AllSteps { get; set; }

        public IEnumerable<BaseTask> Tasks
        {
            get
            {
                foreach (var step in AllSteps)
                {
                    if (!string.IsNullOrEmpty(step.Script))
                        yield return new ShortcutCommandLineTask(step);
                    else if (!string.IsNullOrEmpty(step.Powershell))
                        yield return new PowershellTask(step);
                    else
                       throw new NotSupportedException();
                }
            }
        }
    }
}