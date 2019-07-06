using System.Collections.Generic;

namespace AzurePipelineRunner
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
