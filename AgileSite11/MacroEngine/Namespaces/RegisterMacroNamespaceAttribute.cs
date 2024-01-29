using System;

using CMS.Core;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Registers a macro namespace within the macro engine
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterMacroNamespaceAttribute : Attribute, IPreInitAttribute
    {
        #region "Properties"

        /// <summary>
        /// Namespace type
        /// </summary>
        public Type MarkedType
        {
            get;
            set;
        }


        /// <summary>
        /// If true, namespace members are allowed to be used also as anonymous
        /// </summary>
        public bool AllowAnonymous
        {
            get;
            set;
        }


        /// <summary>
        /// Namespace name
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the namespace is hidden and doesn't show up in the Intellisense
        /// </summary>
        public bool Hidden
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Namespace class type</param>
        public RegisterMacroNamespaceAttribute(Type type)
        {
            MarkedType = type;
        }


        /// <summary>
        /// Initializes the attribute
        /// </summary>
        public void PreInit()
        {
            var r = MacroContext.GlobalResolver;
            var ns = (IMacroNamespace)ObjectFactory.GetFactory(MarkedType).Singleton;

            r.RegisterNamespace(ns, Name, AllowAnonymous, Hidden);
        }

        #endregion
    }
}