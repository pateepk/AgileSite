using System;

using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Exception used when evaluation could not proceed (wrong types, etc.).
    /// </summary>
    public class EvaluationException : MacroException
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets the message describing where the evaluation threw the exception.
        /// </summary>
        public string ExactMessage
        {
            get;
            set;
        }


        /// <summary>
        /// Returns evaluation error message.
        /// </summary>
        public override string Message
        {
            get
            {
                return String.Format(GetString("macros.evaluationerror"), OriginalExpression, ExactMessage);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="originalExpression">The whole expression which was being processed when this error occurred.</param>
        /// <param name="exactMessage">Message describing where the evaluation threw the exception</param>
        /// <param name="innerException">Reference to the inner exception that is the cause of this exception.</param>
        public EvaluationException(string originalExpression, string exactMessage = null, Exception innerException = null) :
            base(originalExpression, innerException)
        {
            ExactMessage = exactMessage;
        }

        #endregion
    }
}
