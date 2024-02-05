namespace CMS.MacroEngine
{
    /// <summary>
    /// Class containing settings for macro expression resolving process.
    /// </summary>
    public class ResolveExpressionSettings
    {
        #region "Variables"

        private string mType = "%";

        #endregion


        #region "Properties"

        /// <summary>
        /// Macro expression without {% %} brackets.
       /// </summary>
        public string Expression
        {
            get;
            set;
        }


        /// <summary>
        /// If true, when the result is InfoObject it is the result, if false, object is resolved as its displayname (for backward compatibility).
        /// </summary>
        public bool KeepObjectsAsResult
        {
            get;
            set;
        }


        /// <summary>
        /// If true, security check is not performed
        /// </summary>
        public bool SkipSecurityCheck
        {
            get;
            set;
        }


        /// <summary>
        /// If true no exceptions during parsing are thrown.
        /// </summary>
        public bool SupressParsingError
        {
            get;
            set;
        }


        /// <summary>
        /// Type of the expression (? or $ or %).
        /// </summary>
        public string Type
        {
            get
            {
                return mType;
            }
            set
            {
                mType = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance of resolve process settings for given expression.
        /// </summary>
        /// <param name="expression">Expression to be resolved</param>
        public ResolveExpressionSettings(string expression)
        {
            Expression = expression;
        }

        #endregion
    }
}