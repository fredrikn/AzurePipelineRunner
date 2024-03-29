﻿using AzurePipelineRunner.Report;
using AzurePipelineRunner.Tasks;
using System;
using System.Diagnostics;

namespace AzurePipelineRunner
{
    public class StepInvoker
    {
        public StepReport RunStep(TaskStep step)
        {
            var stepReport = new StepReport() { Name = step.DisplayName };

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                step.Run();

                stepReport.Succeed = true;
            }
            catch (Exception e)
            {
                stepReport.Error = e;
                stepReport.Succeed = false;

                Console.WriteLine(e);
            }
            finally
            {
                stepReport.Time = stopWatch.Elapsed;
                stopWatch.Stop();
            }

            return stepReport;
        }
    }
}
