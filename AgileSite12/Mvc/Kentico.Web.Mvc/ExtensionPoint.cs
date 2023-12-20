using System;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Represents a point for extension methods.
    /// </summary>
    /// <typeparam name="T">The type of the class that is a target of extension methods.</typeparam>
    public sealed class ExtensionPoint<T> where T : class
    {
        /// <summary>
        /// Instance of class that is a target of extension methods.
        /// </summary>
        public T Target { get; }


        internal ExtensionPoint(T target)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }
    }
}
