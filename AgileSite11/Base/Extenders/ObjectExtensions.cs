using System;
using System.Linq;

namespace CMS.Base
{
    /// <summary>
    /// Provides methods to initialize extenders.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Attaches new behavior using extenders with the matching scope.
        /// </summary>
        /// <typeparam name="T">The type of instance to attach new behavior to.</typeparam>
        /// <param name="instance">The instance to attach new behavior to.</param>
        /// <param name="scope">The scope to limit a selection of extenders.</param>
        public static void InitializeExtenders<T>(this T instance, string scope) where T : class // where T : IExtensible or IExtensionPoint
        {
            if (String.IsNullOrEmpty(scope))
            {
                throw new ArgumentException("The scope must be specified.", "scope");
            }

            if (instance == null)
            {
                return;
            }

            foreach (var extender in Extension<Extender<T>>.GetExtensions().Select(x => x.Value).Where(x => StringComparer.InvariantCultureIgnoreCase.Equals(x.Scope, scope)))
            {
                extender.InitializeInternal(instance);
            }
        }
    }
}