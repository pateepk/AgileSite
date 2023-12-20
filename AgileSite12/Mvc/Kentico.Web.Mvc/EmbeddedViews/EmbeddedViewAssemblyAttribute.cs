using System;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Holds information about embedded views in assembly.
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class EmbeddedViewAssemblyAttribute : Attribute
    {
        /// <summary>
        /// List of view paths
        /// </summary>
        public string[] ViewPaths { get; }


        /// <summary>
        /// Creates new instance of <see cref="EmbeddedViewAssemblyAttribute"/>.
        /// </summary>
        /// <param name="viewPaths">View paths</param>
        public EmbeddedViewAssemblyAttribute(params string[] viewPaths)
        {
            ViewPaths = viewPaths;
        }
    }
}
