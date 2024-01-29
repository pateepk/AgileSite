using System;
using System.Collections.Generic;

namespace CMS.Core
{
    /// <summary>
    /// Represents method executable in MacroEngine.
    /// </summary>
    public interface IMacroMethod
    {
        #region "Variables"

        /// <summary>
        /// Gets or sets reference to a method.
        /// </summary>
        Func<object[], object> Method
        {
            get;
            set;
        }


        /// <summary>
        /// Returns name of the method.
        /// </summary>
        string Name
        {
            get;
            set;
        }


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
        /// Gets or sets a code snippet which is used in AutoCompletion when TAB is pressed (for determining the cursor position use pipe).
        /// </summary>
        string Snippet
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the minimal number of parameters needed by the method.
        /// </summary>
        int MinimumParameters
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the method won't be visible in IntelliSense (but will be normally executed when called).
        /// </summary>
        bool IsHidden
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the parameters for the method.
        /// </summary>
        List<IMacroMethodParam> Parameters
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the list of special parameters needed to be supplied by resolver.
        /// </summary>
        string[] SpecialParameters
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Executes given method with parameters.
        /// </summary>
        /// <param name="parameters">Method parameters</param>
        object ExecuteMethod(params object[] parameters);

        /// <summary>
        /// Adds parameter to the method definition.
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="type">Type of the parameter</param>
        /// <param name="comment">Comment of the parameter</param>
        /// <param name="isParams">If true, parameter is declared with params keyword</param>
        /// <param name="asExpression">If true, the parameter is passed to the method as expression (MacroExpression object), not evaluated</param>
        void AddParameter(string name, Type type, string comment, bool isParams = false, bool asExpression = false);

        #endregion
    }
}
