using System.Reflection;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Holds information about <see cref="EmbeddedView"/>s in assembly.
    /// </summary>
    internal class EmbeddedViewAssembly
    {
        public Assembly Assembly { get; set; }


        public EmbeddedViewAssemblyAttribute Attribute { get; set; }
    }
}
