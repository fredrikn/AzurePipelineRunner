using Microsoft.Extensions.Configuration;
using System.IO;

namespace AzurePipelineRunner.Configuration
{
    public class AppConfiguration : IAppConfiguration
    {
        IConfiguration _configuration;

        public AppConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string TaskLocalPath => _configuration.GetSection("taskLocation:localPath").Value;

        public string RemoteUrl => _configuration.GetSection("taskLocation:remote:url").Value;

        public string RemoteLoginType => _configuration.GetSection("taskLocation:remote:login:type").Value;

        public string RemoteToken => _configuration.GetSection("taskLocation:remote:login:token").Value;

        public string TempDir
        {
            get
            {
                var tmpDir = _configuration.GetSection("tmpDir").Value;

                if (string.IsNullOrWhiteSpace(tmpDir))
                    return Path.Combine(System.Environment.CurrentDirectory, "_temp");

                return tmpDir;
            }
        }

        public bool SystemDebug => bool.Parse(_configuration.GetSection("systemDebug").Value);

        public bool IsRemoteConfigured => _configuration.GetSection("taskLocation:remote") != null && !string.IsNullOrWhiteSpace(RemoteUrl) && !string.IsNullOrWhiteSpace(RemoteLoginType);
    }
}
