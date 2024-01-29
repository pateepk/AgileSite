using System;

using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Base for the exceptions thrown during the macro resolving process.
    /// </summary>
    public class ParsingException : MacroException
    {
        /// <summary>
        /// Gets or sets the prefix of the analysis type to generate correct message.
        /// </summary>
        protected virtual string AnalysisType
        {
            get
            {
                return "generalanalysis";
            }
        }


        /// <summary>
        /// Gets or sets the index within OriginalExpression where the parsing error occurred.
        /// </summary>
        public int ErrorIndex
        {
            get;
            set;
        }


        /// <summary>
        /// Returns the formatted message of the parsing error.
        /// </summary>
        public override string Message
        {
            get
            {
                if (string.IsNullOrEmpty(OriginalExpression))
                {
                    // No expression supplied, provide just basic fail message
                    return GetString("macros." + AnalysisType + ".error.nodetail");
                }

                // Get 25 characters prior the error occurrence and 25 characters after the occurrence
                int startIndex = Math.Max(0, ErrorIndex - 25);
                int endIndex = Math.Min(ErrorIndex + 25, OriginalExpression.Length);
                int errIndex = Math.Min(ErrorIndex, OriginalExpression.Length);
                string before = OriginalExpression.Substring(startIndex, errIndex - startIndex);
                string after = OriginalExpression.Substring(errIndex, endIndex - errIndex);

                // Return the error in the following format
                // lexem lexem ->>wronglexem
                return String.Format(GetString("macros." + AnalysisType + ".error"), before + "->>" + after);
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="originalExpression">The whole expression which was being parsed when this error occurred.</param>
        /// <param name="errorIndex">Index within OriginalExpression where the parsing error occurred</param>
        public ParsingException(string originalExpression, int errorIndex) :
            base(originalExpression)
        {
            ErrorIndex = errorIndex;
        }
    }
}
