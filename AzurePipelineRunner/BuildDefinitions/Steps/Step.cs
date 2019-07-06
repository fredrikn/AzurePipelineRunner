using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AzurePipelineRunner.BuildDefinitions.Steps
{
    public class Step : IShortcutCommandLineScriptStep, IShortcutPowershellStep
    {
        public string Script { get; set; }

        public string Powershell { get; set; }

        public string WorkingDirectory { get; set; }

        public bool FailOnStderr { get; set; }

        public string DisplayName { get; set; }

        public string Name { get; set; }

        public string Condition { get; set; }

        public bool ContinueOnError { get; set; }

        public bool Enabled { get; set; }

        public int TimeoutInMinutes { get; set; }

        public Dictionary<string, string> Env { get; set; }

        internal void UpdatePropertiesWithVariableValues(Dictionary<string, string> variables)
        {
            Script = Regex.Replace(Script, @"\$\((\w+)\)", new MatchEvaluator( m => ReplaceCC(m,variables)));
        }

        public string ReplaceCC(Match m, Dictionary<string, string> variables)
        {
            var variableName = m.Groups[1].Value;

            if (variables.ContainsKey(variableName))
                return variables[m.Groups[1].Value];

            throw new System.ArgumentException($"The variable '{variableName}' can't be found");
        }
    }
}
