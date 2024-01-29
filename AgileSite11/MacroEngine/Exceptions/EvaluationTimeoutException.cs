using System;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Exception used when evaluation of the expression took to long (longer than the specified timeout threshold).
    /// </summary>
    public class EvaluationTimeoutException : MacroException
    {
        private readonly TimeSpan mOverTime;

        #region "Properties"

        /// <summary>
        /// Returns evaluation timeout error message.
        /// </summary>
        public override string Message
        {
            get
            {
                return String.Format(GetAPIString("macros.evaluationaborted", "Evaluation timeout in the expression '{0}' with overtime '{1}'."), OriginalExpression, mOverTime);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="originalExpression">The whole expression which was being processed when this error occurred.</param>
        /// <param name="overTimeSpan">Indicates how much timeout exceeds</param>
        public EvaluationTimeoutException(string originalExpression, TimeSpan overTimeSpan) :
            base(originalExpression)
        {
            mOverTime = overTimeSpan;
        }

        #endregion
    }
}
