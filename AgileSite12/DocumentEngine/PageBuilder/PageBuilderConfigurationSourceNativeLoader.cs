using System;

using CMS;
using CMS.Core;
using CMS.DocumentEngine.PageBuilder.Internal;

[assembly: RegisterImplementation(typeof(IPageBuilderConfigurationSourceLoader), typeof(PageBuilderConfigurationSourceNativeLoader), Priority = RegistrationPriority.Fallback)]

namespace CMS.DocumentEngine.PageBuilder.Internal
{
    /// <summary>
    /// Loads Page builder configuration from page data.
    /// </summary>
    internal sealed class PageBuilderConfigurationSourceNativeLoader : IPageBuilderConfigurationSourceLoader
    {
        /// <summary>
        /// Name of the field column in documents' table storing the Page builder widgets configuration.
        /// </summary>
        internal const string WIDGETS_CONFIGURATION_FIELD_NAME = "DocumentPageBuilderWidgets";


        /// <summary>
        /// Name of the field column in documents' table storing the Page builder template configuration.
        /// </summary>
        internal const string TEMPLATE_CONFIGURATION_FIELD_NAME = "DocumentPageTemplateConfiguration";


        /// <summary>
        /// Loads Page builder configuration for a page.
        /// </summary>
        /// <param name="page">Page from which the configuration will be loaded.</param>
        /// <returns>Returns the configuration source.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        public PageBuilderConfigurationSource Load(TreeNode page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            return new PageBuilderConfigurationSource
            {
                WidgetsConfiguration = page.GetStringValue(WIDGETS_CONFIGURATION_FIELD_NAME, null),
                PageTemplateConfiguration = page.GetStringValue(TEMPLATE_CONFIGURATION_FIELD_NAME, null)
            };
        }
    }
}