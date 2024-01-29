using System;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Context for macro processing (context for parsing the text for macro expressions).
    /// </summary>
    public class MacroProcessingContext
    {
        #region "Properties"

        /// <summary>
        /// Custom object parameter passed to the processing handler.
        /// </summary>
        internal IMacroProcessingParameters Parameters
        {
            get;
            set;
        }


        /// <summary>
        /// Replacement string passed to the macro handler.
        /// </summary>
        public string Replacement
        {
            get;
            set;
        }


        /// <summary>
        /// Identified macro expression including all the parameters.
        /// </summary>
        public string Expression
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the expression is transformed open expression (condition, loop).
        /// </summary>
        public bool IsOpenExpression
        {
            get;
            set;
        }


        /// <summary>
        /// Text in which the macro was found.
        /// </summary>
        public string SourceText
        {
            get;
            set;
        }


        /// <summary>
        /// Start position of the macro (position of starting bracket).
        /// </summary>
        public int MacroStart
        {
            get;
            set;
        }


        /// <summary>
        /// End of the macro (position after end bracket).
        /// </summary>
        public int MacroEnd
        {
            get;
            set;
        }


        /// <summary>
        /// Type of the macro ($, %, #, etc.).
        /// </summary>
        public string MacroType
        {
            get;
            set;
        }


        /// <summary>
        /// Type of the baracket - used for backward compatibility when macros could be nested '{(0)% %(0)}'
        /// </summary>
        public string BracketType
        {
            get;
            set;
        }

        #endregion
        
        #region "Methods"

        /// <summary>
        /// Creates a new MacroProcessingContext instance
        /// </summary>
        public MacroProcessingContext()
        {
            Parameters = new MacroProcessingParameters();
        }


        /// <summary>
        /// Returns the whole version of the expression created by concatenation of the parameters. The result is for example this: {(0)%test%(0)}.
        /// </summary>
        /// <returns></returns>
        public string GetWholeMacroExpression()
        {
            return "{" + BracketType + MacroType + Expression + MacroType + BracketType + "}";
        }


        /// <summary>
        /// Returns macro expression demarcated by macro start and macro end in entire source text
        /// </summary>
        public string GetOriginalExpression()
        {
            if (SourceText == null)
            {
                throw new InvalidOperationException("SourceText has to be set to get expression");
            }

            return SourceText.Substring(MacroStart, MacroEnd - MacroStart);
        }

        #endregion
    }
}