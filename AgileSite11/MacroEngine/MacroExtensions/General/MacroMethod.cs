using System;
using System.Collections.Generic;

using CMS.Core;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Macro method object.
    /// </summary>
    public class MacroMethod : MacroExtension, IMacroMethod
    {
        #region "Variables"

        private Type mType = typeof(object);
        private string mComment = "";
        private string[] mSpecialParameters = null;
        private List<IMacroMethodParam> mParameters = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets reference to a method.
        /// </summary>
        public Func<object[], object> Method
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets reference to a method.
        /// </summary>
        public Func<MacroResolver, object[], object> MethodResolver
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets reference to a method.
        /// </summary>
        public Func<EvaluationContext, object[], object> MethodContext
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a return type of the method.
        /// </summary>
        public Type Type
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


        /// <summary>
        /// Gets or sets a list of types for which the method is applicable (set to null for all types to be allowed).
        /// </summary>
        public List<Type> AllowedTypes
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a comment for the method.
        /// </summary>
        public string Comment
        {
            get
            {
                return mComment;
            }
            set
            {
                mComment = value;
            }
        }


        /// <summary>
        /// Gets or sets a code snippet which is used in AutoCompletion when TAB is pressed (for determining the cursor position use pipe).
        /// </summary>
        public string Snippet
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the list of special parameters needed to be supplied by resolver.
        /// </summary>
        public string[] SpecialParameters
        {
            get
            {
                if (mSpecialParameters == null)
                {
                    mSpecialParameters = new string[] { };
                }
                return mSpecialParameters;
            }
            set
            {
                mSpecialParameters = value;
            }
        }


        /// <summary>
        /// Gets or sets the parameters for the method.
        /// </summary>
        public List<IMacroMethodParam> Parameters
        {
            get
            {
                if (mParameters == null)
                {
                    mParameters = new List<IMacroMethodParam>();
                }
                return mParameters;
            }
            set
            {
                mParameters = value;
            }
        }


        /// <summary>
        /// Gets or sets the minimal number of parameters needed by the method.
        /// </summary>
        public int MinimumParameters
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the method won't be visible in IntelliSense (but will be normally executed when called).
        /// </summary>
        public bool IsHidden
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new MacroMethod object.
        /// </summary>
        public MacroMethod()
        {
        }


        /// <summary>
        /// Creates new instance of MacroMethod object.
        /// </summary>
        /// <param name="name">Method name</param>
        /// <param name="method">Method delegate</param>
        public MacroMethod(string name, Func<object[], object> method)
        {
            MethodResolver = null;
            MethodContext = null;
            AllowedTypes = null;
            Snippet = null;
            MinimumParameters = 0;
            Name = name;
            Method = method;
        }


        /// <summary>
        /// Creates new instance of MacroMethod object.
        /// </summary>
        /// <param name="name">Method name</param>
        /// <param name="method">Method delegate</param>
        public MacroMethod(string name, Func<MacroResolver, object[], object> method)
        {
            Method = null;
            MethodContext = null;
            AllowedTypes = null;
            Snippet = null;
            MinimumParameters = 0;
            Name = name;
            MethodResolver = method;
        }


        /// <summary>
        /// Creates new instance of MacroMethod object.
        /// </summary>
        /// <param name="name">Method name</param>
        /// <param name="method">Method delegate</param>
        public MacroMethod(string name, Func<EvaluationContext, object[], object> method)
        {
            Method = null;
            MethodResolver = null;
            AllowedTypes = null;
            Snippet = null;
            MinimumParameters = 0;
            Name = name;
            MethodContext = method;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Adds parameter to the method definition.
        /// </summary>
        /// <param name="parameter">Parameter</param>
        public void AddParameter(IMacroMethodParam parameter)
        {
            Parameters.Add(parameter);
        }


        /// <summary>
        /// Adds parameter to the method definition.
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="type">Type of the parameter</param>
        /// <param name="comment">Comment of the parameter</param>
        /// <param name="isParams">If true, parameter is declared with params keyword</param>
        /// <param name="asExpression">If true, the parameter is passed to the method as expression (MacroExpression object), not evaluated</param>
        public void AddParameter(string name, Type type, string comment, bool isParams = false, bool asExpression = false)
        {
            MacroMethodParam parameter = new MacroMethodParam(name, type, comment, asExpression) {IsParams = isParams};
            AddParameter(parameter);
        }


        /// <summary>
        /// Returns a name of the index-th parameter.
        /// </summary>
        /// <param name="index">Index of the parameter</param>
        public string GetParameterName(int index)
        {
            if (Parameters.Count > index)
            {
                return Parameters[index].Name;
            }

            return "";
        }


        /// <summary>
        /// Returns a type of the index-th parameter.
        /// </summary>
        /// <param name="index">Index of the parameter</param>
        public Type GetParameterType(int index)
        {
            if (Parameters.Count > index)
            {
                return Parameters[index].Type;
            }

            return typeof(object);
        }


        /// <summary>
        /// Returns a comment of the index-th parameter.
        /// </summary>
        /// <param name="index">Index of the parameter</param>
        public string GetParameterComment(int index)
        {
            if (Parameters.Count > index)
            {
                return Parameters[index].Comment;
            }

            return "";
        }


        /// <summary>
        /// Executes given method with parameters.
        /// </summary>
        /// <param name="parameters">Method parameters</param>
        public object ExecuteMethod(params object[] parameters)
        {
            return ExecuteMethod(null, parameters);
        }


        /// <summary>
        /// Executes given method with parameters.
        /// </summary>
        /// <param name="context">Resolver object</param>
        /// <param name="parameters">Method parameters</param>
        public object ExecuteMethod(EvaluationContext context, params object[] parameters)
        {
            if (MethodContext != null)
            {
                // Call with context object
                return MethodContext(context, parameters);
            }
            else if (MethodResolver != null)
            {
                // Call with resolver object
                return MethodResolver((context == null ? null : context.Resolver), parameters);
            }
            else if (Method != null)
            {
                // Call without resolver object
                return Method(parameters);
            }

            return null;
        }

        #endregion
    }
}