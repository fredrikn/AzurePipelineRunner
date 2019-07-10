using AzurePipelineRunner.Bootstrap;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzurePipelineRunner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = DependencyRegistration.BuildServiceProvider();

            string buildYamlPath;

            if (args != null && args.Length > 0)
            {
                buildYamlPath = args[0];
                if (!File.Exists(buildYamlPath))
                    throw new FileNotFoundException($"Can't find the specified '{buildYamlPath}' Build YAML file.");

                await serviceProvider.GetService<MainProgram>().Run(buildYamlPath);
            }
            else
            {
                Console.WriteLine("Usage: AzurePipelineRunner <your buildfile>.yaml");
            }
        }
    }
}