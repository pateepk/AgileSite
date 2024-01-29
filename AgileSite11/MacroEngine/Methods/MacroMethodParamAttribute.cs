using System;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Adds action to the page.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class MacroMethodParamAttribute : Attribute
    {
        #region "Properties"

        /// <summary>
        /// Index of the breadcrumb.
        /// </summary>
        public int Index
        {
            get;
            set;
        }


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
        /// <param name="index">Index of the parameter</param>
        /// <param name="name">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <param name="comment">Parameter comment</param>
        public MacroMethodParamAttribute(int index, string name, Type type, string comment) :
            this(index, name, type, comment, false)
        {
        }


        /// <summary>
        /// Creates new instance of MacroMethodParam object.
        /// </summary>
        /// <param name="index">Index of the parameter</param>
        /// <param name="name">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <param name="comment">Parameter comment</param>
        /// <param name="asExpression">If true, the parameter is passed to the method as expression (MacroExpression object), not evaluated</param>
        public MacroMethodParamAttribute(int index, string name, Type type, string comment, bool asExpression)
        {
            Index = index;
            Name = name;
            Type = type;
            Comment = comment;
            AsExpression = asExpression;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates new MacroMethodParam object from data of this attribute.
        /// </summary>
        public MacroMethodParam GetMacroParam()
        {
            return new MacroMethodParam(Name, Type, Comment, AsExpression)
            {
                IsParams = IsParams
            };
        }

        #endregion
    }
}