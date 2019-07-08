using System;
using System.Collections.Generic;

namespace AzurePipelineRunner.Report
{
    public class BuildReporter : IBuildReporter
    {
        public void ReportBuildResults(List<StepReport> outputStepReport)
        {
            Console.Write(Environment.NewLine);
            Console.WriteLine("========================== Start of Build Restult ========================== ");
            Console.Write(Environment.NewLine);

            foreach (var report in outputStepReport)
            {
                var name = report.Name?.Substring(0, Math.Min(report.Name.Length, 40));

                if (name.Length > 37)
                    name += "...";

                name = name.PadRight(40, ' ');

                var succeed = report.Succeed ? "Succeed" : "Failed";
                Console.WriteLine($" {name}  [{report.Time.ToString("c")}]  {succeed}");
            }

            Console.Write(Environment.NewLine);
            Console.WriteLine("========================== End of Build Restult ========================== ");
            Console.Write(Environment.NewLine);
        }
    }
}
