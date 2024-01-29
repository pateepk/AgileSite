namespace CMS.MacroEngine
{
    /// <summary>
    /// Class for the exception during the lexical analysis of a K# macro expression.
    /// </summary>
    public class LexicalAnalysisException : ParsingException
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets the prefix of the analysis type to generate correct message.
        /// </summary>
        protected override string AnalysisType
        {
            get
            {
                return "lexicalanalysis";
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="originalExpression">The whole expression which was being parsed when this error occurred.</param>
        /// <param name="errorIndex">Index within OriginalExpression where the parsing error occurred</param>
        public LexicalAnalysisException(string originalExpression, int errorIndex) :
            base(originalExpression, errorIndex)
        {
        }

        #endregion
    }
}
