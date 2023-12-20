using System;
using System.Linq;

using CMS;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.DocumentEngine.PageBuilder.Internal;
using CMS.OnlineMarketing.Web.UI.Internal;

[assembly: RegisterImplementation(typeof(IPageBuilderConfigurationSourceLoader), typeof(PageBuilderConfigurationSourceABTestVariantLoader), Priority = RegistrationPriority.SystemDefault)]

namespace CMS.OnlineMarketing.Web.UI.Internal
{
    /// <summary>
    /// Loads Page builder configuration from page data.
    /// </summary>
    internal class PageBuilderConfigurationSourceABTestVariantLoader : IPageBuilderConfigurationSourceLoader
    {
        private readonly IPageBuilderConfigurationSourceLoader pageBuilderConfigurationLoader;
        private readonly IABTestManager abTestManager;


        /// <summary>
        /// Initializes a new instance of the <see cref="PageBuilderConfigurationSourceABTestVariantLoader"/> class with given <paramref name="pageBuilderConfigurationLoader"/> and <paramref name="abTestManager"/>.
        /// </summary>
        public PageBuilderConfigurationSourceABTestVariantLoader(IPageBuilderConfigurationSourceLoader pageBuilderConfigurationLoader, IABTestManager abTestManager)
        {
            this.pageBuilderConfigurationLoader = pageBuilderConfigurationLoader ?? throw new ArgumentNullException(nameof(pageBuilderConfigurationLoader));
            this.abTestManager = abTestManager ?? throw new ArgumentNullException(nameof(abTestManager));
        }


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

            if (!ABTestInfoProvider.ABTestingEnabled(page.NodeSiteName))
            {
                return LoadByDefault(page);
            }

            var variants = abTestManager.GetVariants(page);
            var variantIdentifier = ABTestUIVariantHelper.GetPersistentVariantIdentifier(page.DocumentID);
            var variant = variants.FirstOrDefault(v => v.Guid == variantIdentifier);
            if (variant?.IsOriginal ?? true)
            {
                return LoadByDefault(page);
            }

            return new PageBuilderConfigurationSource
            {
                WidgetsConfiguration = variant.PageBuilderWidgets,
                PageTemplateConfiguration = variant.PageTemplate
            };
        }


        private PageBuilderConfigurationSource LoadByDefault(TreeNode page)
        {
            return pageBuilderConfigurationLoader.Load(page);
        }
    }
}