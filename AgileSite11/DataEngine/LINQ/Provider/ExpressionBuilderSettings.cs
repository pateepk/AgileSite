using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Object to encapsulate the expression builder settings
    /// </summary>
    public class ExpressionBuilderSettings
    {
        #region "Properties"

        /// <summary>
        /// String builder where to collect results
        /// </summary>
        public StringBuilder Result
        {
            get;
            set;
        }

        /// <summary>
        /// Separator for member access
        /// </summary>
        public string MemberSeparator
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the built expression is a where condition
        /// </summary>
        public bool IsWhereCondition
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the built expression is a column list
        /// </summary>
        public bool IsColumnList
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentSettings">Parent settings</param>
        public ExpressionBuilderSettings(ExpressionBuilderSettings parentSettings)
        {
            Result = new StringBuilder();

            if (parentSettings != null)
            {
                IsWhereCondition = parentSettings.IsWhereCondition;
                IsColumnList = parentSettings.IsColumnList;
            }
        }

        #endregion
    }
}