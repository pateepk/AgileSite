using System;

using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Object encapsulating custom macro resolving as ISimpleDataContainer.
    /// </summary>
    public class CustomMacroContainer : ISimpleDataContainer
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets resolver used to resolve Custom macros.
        /// </summary>
        public MacroResolver Resolver
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance of CustomMacroObject.
        /// </summary>
        /// <param name="resolver">ContextResolver object to use to resolve macros</param>
        public CustomMacroContainer(MacroResolver resolver)
        {
            Resolver = resolver;
        }

        #endregion


        #region ISimpleDataContainer Members

        /// <summary>
        /// Gets or sets the value of the column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object this[string columnName]
        {
            get
            {
                return GetValue(columnName);
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Resolves the custom macro.
        /// </summary>
        /// <param name="customMacro">Custom macro to resolve</param>
        public object GetValue(string customMacro)
        {
            if (Resolver != null)
            {
                EvaluationResult result = Resolver.ResolveCustomMacro(customMacro, customMacro);
                if (result != null)
                {
                    return result.Result;
                }
            }
            return null;
        }


        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="customMacro">Custom macro</param>
        /// <param name="value">New value</param>
        public bool SetValue(string customMacro, object value)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}