using System.Collections.Generic;

namespace AzurePipelineRunner.Tasks.Definition
{
    public class PowerShell3
    {
        public string target { get; set; }
        public List<string> platforms { get; set; }
    }

    public class Node
    {
        public string target { get; set; }
        public string argumentFormat { get; set; }
    }

    public class Execution
    {
        public PowerShell3 PowerShell3 { get; set; }

        public Node Node { get; set; }

        public bool IsNodeSupported()
        {
            return !string.IsNullOrEmpty(Node?.target);
        }

        public bool IsPowerShell3Supported()
        {
            // TODO: Check for platform support
            return !string.IsNullOrEmpty(PowerShell3?.target);
        }

    }

    public class TaskInfo
    {
        public Execution execution { get; set; }
    }
}
