using System.Linq;

using Kentico.Content.Web.Mvc;

namespace Kentico.Components.Web.Mvc.Internal
{
    /// <summary>
    /// Encapsulates selectors' configuration data sent to client and used by Page builder's script.
    /// </summary>
    public sealed class SelectorsScriptConfiguration
    {
        private readonly string applicationPath;


        /// <summary>
        /// Creates an instance of <see cref="SelectorsScriptConfiguration"/> class.
        /// </summary>
        /// <param name="applicationPath">Application path for endpoints resolution.</param>
        public SelectorsScriptConfiguration(string applicationPath)
        {
            this.applicationPath = applicationPath;
        }


        /// <summary>
        /// Gets initialization object for Page builder configuration.
        /// </summary>
        /// <param name="decorator">Decorates the paths which need preview context to be initialized.</param>
        internal dynamic GetBuilderInitializationParameter(IPathDecorator decorator)
        {
            return new
            {
                dialogEndpoints = new
                {
                    mediaFilesSelector = TryDecoratePath($"{applicationPath?.TrimEnd('/')}/{SelectorRoutes.MEDIA_FILES_SELECTOR_ROUTE}", decorator),
                    pageSelector = TryDecoratePath($"{applicationPath?.TrimEnd('/')}/{SelectorRoutes.PAGE_SELECTOR_ROUTE}", decorator),
                }
            };
        }


        private string TryDecoratePath(string path, IPathDecorator pathDecorator)
        {
            var decoratedPath = path.Replace("'", "\\'");

            if (pathDecorator != null)
            {
                decoratedPath = pathDecorator.Decorate(decoratedPath);
            }

            return decoratedPath;
        }
    }
}