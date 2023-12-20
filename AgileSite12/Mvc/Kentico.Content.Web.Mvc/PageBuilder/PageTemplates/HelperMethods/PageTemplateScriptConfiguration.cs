using System;
using System.Linq;

using CMS.DocumentEngine;
using CMS.Core;
using CMS.Base;

using Kentico.Content.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates.Internal
{
    /// <summary>
    /// Encapsulates page template data sent to client and used by Page builder's script.
    /// </summary>
    public sealed class PageTemplateScriptConfiguration
    {
        private readonly PageTemplateDefinitionProvider defaultTemplatesProvider;
        private readonly CustomPageTemplateProvider customTemplatesProvider;


        /// <summary>
        /// Endpoint for page template selector dialog markup retrieval.
        /// </summary>
        public string SelectorDialogEndpoint { get; internal set; }


        /// <summary>
        /// Endpoint for changing page template of a page.
        /// </summary>
        public string ChangeTemplateEndpoint { get; internal set; }


        /// <summary>
        /// Indicates if there is more than one page template to be selected for a page.
        /// </summary>
        public bool IsSelectable { get; internal set; }


        /// <summary>
        /// Creates an instance of <see cref="PageTemplateScriptConfiguration"/> class.
        /// </summary>
        /// <param name="page">Page where the Page builder feature is used.</param>
        /// <param name="applicationPath">Application path for endpoints resolution.</param>
        public PageTemplateScriptConfiguration(TreeNode page, string applicationPath) : 
            this(page, applicationPath, new PageTemplateDefinitionProvider(), new CustomPageTemplateProvider(Service.Resolve<ISiteService>()))
        {
        }


        internal PageTemplateScriptConfiguration()
        {

        }


        internal PageTemplateScriptConfiguration(TreeNode page, string applicationPath, PageTemplateDefinitionProvider defaultTemplatesProvider, CustomPageTemplateProvider customTemplatesProvider)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            this.defaultTemplatesProvider = defaultTemplatesProvider;
            this.customTemplatesProvider = customTemplatesProvider;

            ChangeTemplateEndpoint = $"{applicationPath?.TrimEnd('/')}/{PageBuilderRoutes.CONFIGURATION_CHANGE_TEMPLATE_ROUTE}";
            SelectorDialogEndpoint = $"{applicationPath?.TrimEnd('/')}/{PageTemplateRoutes.TEMPLATE_SELECTOR_DIALOG_ROUTE}";
            IsSelectable = IsTemplateSelectable(page);
        }


        private bool IsTemplateSelectable(TreeNode page)
        {
            var filterContext = new PageTemplateFilterContext(page.Parent, page.ClassName, page.DocumentCulture);
            var defaultTemplates = defaultTemplatesProvider.GetFilteredTemplates(filterContext);
            var customTemplates = customTemplatesProvider.GetTemplates(defaultTemplates);

            return defaultTemplates.Count() + customTemplates.Count() > 1;
        }


        /// <summary>
        /// Gets initialization object for Page builder configuration.
        /// </summary>
        /// <param name="decorator">Decorates the paths which need preview context to be initialized.</param>
        internal dynamic GetBuilderInitializationParameter(IPathDecorator decorator)
        {
            return new
            {
                selectorDialogEndpoint = TryDecoratePath(SelectorDialogEndpoint, decorator),
                changeTemplateEndpoint = TryDecoratePath(ChangeTemplateEndpoint, decorator),
                isSelectable = IsSelectable
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