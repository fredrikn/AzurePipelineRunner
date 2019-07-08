using System.Collections.Generic;

namespace AzurePipelineRunner.Report
{
    public interface IBuildReporter
    {
        /// <summary>
        /// Report build results.
        /// </summary>
        /// <param name="outputStepReport"></param>
        void ReportBuildResults(List<StepReport> outputStepReport);
    }
}
