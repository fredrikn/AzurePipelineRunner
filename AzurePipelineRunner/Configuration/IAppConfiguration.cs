namespace AzurePipelineRunner.Configuration
{
    public interface IAppConfiguration
    {
        string TaskLocalPath { get; }

        string RemoteUrl { get; }

        string RemoteLoginType { get; }

        string RemoteToken { get; }

        string TempDir { get; }

        bool SystemDebug { get; }

        bool IsRemoteConfigured { get; }
    }
}
