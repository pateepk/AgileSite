using System.Reflection;

namespace CMS.Tests
{
    /// <summary>
    /// Reads content of an embedded resource which name is provided in the way of relative path within executing assembly (project).
    /// </summary>
    /// <remarks>
    /// Class is meant to be injected into test class and for single class, it should be instantiate only once (as the embedded resources do not change during runtime).
    /// </remarks>
    public class EmbeddedResourceReader<TAnySourceAssemblyType> : EmbeddedResourceReader
        where TAnySourceAssemblyType : class
    {
        /// <summary>
        /// Create new instance of the embedded resource reader for given assembly.
        /// </summary>
        public EmbeddedResourceReader() : base(Assembly.GetAssembly(typeof (TAnySourceAssemblyType)))
        {
        }
    }
}
