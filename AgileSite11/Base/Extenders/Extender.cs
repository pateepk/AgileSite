using System;

namespace CMS.Base
{
    /// <summary>
    /// Represents a behavior that can be attached to objects of specific type.
    /// </summary>
    /// <typeparam name="T">The type of objects that this behavior can be attached to.</typeparam>
    /// <remarks>
    /// The derived classes must implement a parameterless constructor.
    /// The extender is a singleton, the same instance is used to attach behavior to multiple ojects.
    /// The process of attaching a behavior specifies a scope and excludes extenders with a scope that does not match.
    /// Two scopes are considered equal when the string values are equal. The comparison is case-insensitive.
    /// </remarks>
    public abstract class Extender<T> where T : class
    {
        #region "Variables"

        /// <summary>
        /// The extender scope.
        /// </summary>
        private readonly string mScope;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the extender scope.
        /// </summary>
        internal string Scope
        {
            get
            {
                return mScope;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the Extender class with the specified scope.
        /// </summary>
        /// <param name="scope">The extender scope.</param>
        /// <remarks>
        /// Two scopes are considered equal when the string values are equal. The comparison is case-insensitive.
        /// </remarks>
        protected Extender(string scope)
        {
            if (String.IsNullOrEmpty(scope))
            {
                throw new ArgumentException("The scope must be specified.", "scope");
            }
            mScope = scope;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Allows the extender to attach a behavior to the specified instance. This method is for internal use only.
        /// </summary>
        /// <param name="instance">The instance to attach a behavior to.</param>
        internal void InitializeInternal(T instance)
        {
            Initialize(instance);
        }

        /// <summary>
        /// Allows the extender to attach a behavior to the specified instance.
        /// </summary>
        /// <param name="instance">The instance to attach a behavior to.</param>
        protected abstract void Initialize(T instance);

        #endregion
    }

}