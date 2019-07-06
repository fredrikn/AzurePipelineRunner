using System;
using System.Collections.Generic;

namespace AzurePipelineRunner
{
    public class BuildReporter : IBuildReporter
    {
        public void ReportBuildResults(List<StepReport> outputStepReport)
        {
            Console.Write(Environment.NewLine);
            Console.Write(Environment.NewLine);
            Console.WriteLine("----------------------Start of Build Restult-----------------------");
            Console.Write(Environment.NewLine);

            foreach (var report in outputStepReport)
            {
                var name = report.Name?.Substring(0, Math.Min(report.Name.Length, 36));

                if (name.Length > 33)
                    name += "...";

                name = name.PadRight(36, ' ');

                var succeed = report.Succeed ? "Succeed" : "Failed";
                Console.WriteLine($" {name}  [{report.Time.ToString("c")}]  {succeed}");
            }

            Console.Write(Environment.NewLine);
            Console.WriteLine("-----------------------End of Build Restult------------------------");
        }
    }
}
