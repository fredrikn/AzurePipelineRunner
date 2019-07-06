using System;

namespace AzurePipelineRunner
{
    /// <summary>
    /// Information about the execution of a step
    /// </summary>
    public class StepReport
    {
        /// <summary>
        /// The name of the step that was executed
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// If the executed step succeed or fail
        /// </summary>
        public bool Succeed { get; set; }

        /// <summary>
        /// Gets the <see cref="Exception"/> of a failed build step.
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// The time it took for the step to be executed
        /// </summary>
        public TimeSpan Time { get; set; }
    }
}
