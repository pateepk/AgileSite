using System;

using CMS.Core;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Macro method parameter object.
    /// </summary>
    public class MacroMethodParam : IMacroMethodParam
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets a return type of the method.
        /// </summary>
        public Type Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a comment for the method.
        /// </summary>
        public string Comment
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the name of the method.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the parameter is declared with params keyword.
        /// </summary>
        public bool IsParams
        {
            get;
            set;
        }

        /// <summary>
        /// If true, the parameter is passed to the method as expression (MacroExpression object), not evaluated.
        /// </summary>
        public bool AsExpression
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance of MacroMethodParam object.
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <param name="comment">Parameter comment</param>
        public MacroMethodParam(string name, Type type, string comment):
            this(name, type, comment, false)
        {
        }


        /// <summary>
        /// Creates new instance of MacroMethodParam object.
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <param name="comment">Parameter comment</param>
        /// <param name="asExpression">If true, the parameter is passed to the method as expression (MacroExpression object), not evaluated</param>
        public MacroMethodParam(string name, Type type, string comment, bool asExpression)
        {
            Name = name;
            Type = type;
            Comment = comment;
            AsExpression = asExpression;
        }

        #endregion
    }
}