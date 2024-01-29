using System.Collections.Generic;

namespace CIConsistencyChecker
{
    /// <summary>
    /// Represents output logger.
    /// </summary>
    internal interface IOutput
    {
        /// <summary>
        /// Logs issues as errors.
        /// </summary>
        void LogErrors(IEnumerable<Issue> issues);


        /// <summary>
        /// Logs info message.
        /// </summary>
        void LogInfo(string message);
    }
}
