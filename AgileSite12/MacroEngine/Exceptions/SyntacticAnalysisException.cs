using System;

using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Class for the exception during the syntactic analysis of a K# macro expression.
    /// </summary>
    public class SyntacticAnalysisException : ParsingException
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
        /// Gets or sets the prefix of the analysis type to generate correct message.
        /// </summary>
        protected override string AnalysisType
        {
            get
            {
                return "syntacticanalysis";
            }
        }


        /// <summary>
        /// Returns evaluation error message.
        /// </summary>
        public override string Message
        {
            get
            {
                if (!string.IsNullOrEmpty(ExactMessage))
                {
                    return String.Format(GetString("macros.syntacticanalysiserror"), OriginalExpression, ExactMessage);
                }
                else
                {
                    return base.Message;
                }
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="originalExpression">The whole expression which was being parsed when this error occurred.</param>
        /// <param name="errorIndex">Index within OriginalExpression where the parsing error occurred</param>
        public SyntacticAnalysisException(string originalExpression, int errorIndex) :
            base(originalExpression, errorIndex)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="originalExpression">The whole expression which was being parsed when this error occurred.</param>
        /// <param name="exactMessage">Message describing where the evaluation threw the exception</param>
        public SyntacticAnalysisException(string originalExpression, string exactMessage) :
            base(originalExpression, -1)
        {
            ExactMessage = exactMessage;
        }

        #endregion
    }
}
