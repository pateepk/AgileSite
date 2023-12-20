using CMS.Helpers;

using Kentico.Content.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Decorates path with virtual context prefix to include Form builder context.
    /// </summary>
    internal class FormBuilderPathDecorator : IPathDecorator
    {
        /// <summary>
        /// Decorates given path with virtual context prefix to include Form builder context.
        /// </summary>
        /// <param name="path">Path to decorate.</param>
        public string Decorate(string path)
        {
            return VirtualContext.GetFormBuilderPath(path);
        }
    }
}
