using System.Collections.Generic;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace AzurePipelineRunner.BuildDefinitions.Steps
{
    public class Step : IShortcutCommandLineScriptStep, IShortcutPowershellStep, ITaskStep
    {
        public string Script { get; set; }

        public string Powershell { get; set; }

        [YamlMember(Alias = "task")]
        public string TaskType { get; set; }

        public string WorkingDirectory { get; set; }

        public bool FailOnStderr { get; set; } = false;

        public string DisplayName { get; set; }

        public string Name { get; set; }

        public string Condition { get; set; }

        public bool ContinueOnError { get; set; } = false;

        public bool Enabled { get; set; } = true;

        public int TimeoutInMinutes { get; set; }

        public Dictionary<string, string> Env { get; set; }

        public bool IgnoreLASTEXITCODE { get; set; } = false;

        public Dictionary<string, object> Inputs { get; set; }

        public ErrorActionPreference ErrorActionPreference { get; set; }

        internal void UpdatePropertiesWithVariableValues(Dictionary<string, string> variables)
        {
            foreach (var property in GetType().GetProperties())
            {
                if (property.CanWrite && property.CanRead && property.PropertyType == typeof(string))
                    property.SetValue(this, UpdateValueWithVariables(property.GetValue(this) as string, variables));
            }

            if(Inputs != null && Inputs.Count > 0)
            {
                var newInputs = new Dictionary<string, object>();

                foreach (var input in Inputs)
                {
                    object newValue = null;

                    if (input.Value is string)
                        newValue = UpdateValueWithVariables((string)input.Value, variables);
                    else
                        newValue = input.Value;

                    newInputs.Add(input.Key, newValue);
                }

                Inputs = newInputs;
            }
        }

        private string UpdateValueWithVariables(string value, Dictionary<string, string> variables)
        {
            if (value == null)
                return null;

            if (value.Length == 0)
                return value;

            return Regex.Replace(value, @"\$\((\w+)\)", new MatchEvaluator(m => ReplaceVariable(m, variables)));
        }

        public string ReplaceVariable(Match m, Dictionary<string, string> variables)
        {
            var variableName = m.Groups[1].Value;

            if (variables.ContainsKey(variableName))
                return variables[m.Groups[1].Value] == null ? string.Empty : variables[m.Groups[1].Value];

            throw new System.ArgumentException($"The variable '{variableName}' can't be found");
        }
    }
}
