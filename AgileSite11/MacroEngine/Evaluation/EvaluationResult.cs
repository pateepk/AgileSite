using System.Diagnostics;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Context for macro expression evaluation.
    /// </summary>
    [DebuggerDisplay("Match: {Match}; Result: {Result}; SecurityPassed: {SecurityPassed};")]
    public class EvaluationResult
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets the parent context (null if the context was created by constructor).
        /// </summary>
        public bool Match
        {
            get;
            set;
        }


        /// <summary>
        /// This will be true if the evaluation process was skiped because the resolver used had different name than required resolver.
        /// </summary>
        public bool Skipped
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether the security check passed (if the user and hash parameters correspond to the given expression and if the user has sufficient permission).
        /// </summary>
        public bool SecurityPassed
        {
            get;
            set;
        }


        /// <summary>
        /// Result of the expression evaluation
        /// </summary>
        public object Result
        {
            get;
            set;
        }


        /// <summary>
        /// Evaluation context used to get this result.
        /// </summary>
        public EvaluationContext ContextUsed
        {
            get;
            set;
        }

        #endregion


        /// <summary>
        /// Returns result to string (returns "null" if result is null).
        /// </summary>
        public override string ToString()
        {
            return Result != null ? Result.ToString() : "null";
        }
    }
}