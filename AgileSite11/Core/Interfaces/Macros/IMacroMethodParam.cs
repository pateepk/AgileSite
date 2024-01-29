using System;

namespace CMS.Core
{
    /// <summary>
    /// Represents method executable in MacroEngine.
    /// </summary>
    public interface IMacroMethodParam
    {
        /// <summary>
        /// Gets or sets a return type of the method.
        /// </summary>
        Type Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a comment for the method.
        /// </summary>
        string Comment
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the name of the method.
        /// </summary>
        string Name
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the parameter is declared with params keyword.
        /// </summary>
        bool IsParams
        {
            get;
            set;
        }

        /// <summary>
        /// If true, the parameter is passed to the method as expression (MacroExpression object), not evaluated.
        /// </summary>
        bool AsExpression
        {
            get;
            set;
        }
    }
}
