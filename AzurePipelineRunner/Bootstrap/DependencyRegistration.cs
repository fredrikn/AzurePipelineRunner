using AzurePipelineRunner.BuildDefinitions;
using AzurePipelineRunner.Configuration;
using AzurePipelineRunner.Report;
using AzurePipelineRunner.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace AzurePipelineRunner.Bootstrap
{
    public static class DependencyRegistration
    {
        public static IServiceCollection RegisterServices()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<ITaskBuilder>(new TaskBuilder());

            services.AddSingleton<IBuildReporter>(new BuildReporter());

            services.AddSingleton<IBuildDefinitionReader>(new BuildDefinitionReader());

            services.AddSingleton<IAppConfiguration>(new AppConfiguration(LoadConfiguration()));

            services.AddTransient<MainProgram>();

            return services;
        }

        public static IServiceProvider BuildServiceProvider()
        {
            var services = RegisterServices();
            return services.BuildServiceProvider();
        }

        internal static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true,
                             reloadOnChange: true);

            return builder.Build();
        }
    }
}
