using System;
using System.Collections.Generic;
using System.Linq;

namespace AzurePipelineRunner.Report
{
    public class BuildReporter : IBuildReporter
    {
        public void ReportBuildResults(List<StepReport> outputStepReport)
        {
            Console.Write(Environment.NewLine);
            Console.WriteLine("============================= Build Restult ============================= ");
            Console.Write(Environment.NewLine);

            foreach (var report in outputStepReport)
            {
                var name = report.Name?.Substring(0, Math.Min(report.Name.Length, 40));

                if (name.Length > 37)
                    name += "...";

                name = name.PadRight(40, ' ');

                var succeed = report.Succeed ? "Succeed" : "Failed";
                Console.ForegroundColor = report.Succeed ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine($" {name}  [{report.Time.ToString("c")}]  {succeed}");
                Console.ResetColor();
            }

            var totalTime = new TimeSpan(outputStepReport.Sum(r => r.Time.Ticks));

            var total = "Total time:".PadRight(40, ' ');

            Console.Write(Environment.NewLine);
            Console.WriteLine($" {total}  [{totalTime.ToString("c")}]");

            if (outputStepReport.Any( r => r.Succeed == false ))
            {
                Console.Write(Environment.NewLine);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("============================= BUILD FAIL ===============================");
                Console.ResetColor();
                Console.Write(Environment.NewLine);
            }
            else
            {
                Console.Write(Environment.NewLine);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("=========================== Build Succeeded ============================");
                Console.ResetColor();
                Console.Write(Environment.NewLine);
            }
        }
    }
}
