using AzurePipelineRunner.Tasks;
using System;
using System.Diagnostics;

namespace AzurePipelineRunner
{
    public class StepInvoker
    {
        public StepReport RunStep(BaseTask step)
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
            }
            finally
            {
                stopWatch.Stop();
                stepReport.Time = stopWatch.Elapsed;
            }

            return stepReport;
        }
    }
}
