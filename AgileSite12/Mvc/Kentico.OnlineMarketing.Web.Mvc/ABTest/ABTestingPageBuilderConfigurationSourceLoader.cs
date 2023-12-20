using System;
using System.Linq;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.DocumentEngine.PageBuilder.Internal;
using CMS.OnlineMarketing;
using CMS.OnlineMarketing.Internal;
using CMS.SiteProvider;

using Kentico.Content.Web.Mvc;
using Kentico.OnlineMarketing.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;

[assembly: RegisterImplementation(typeof(IPageBuilderConfigurationSourceLoader), typeof(ABTestingPageBuilderConfigurationSourceLoader), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Loads Page builder configuration. If A/B testing is enabled for a page being loaded a proper test variant is chosen.
    /// </summary>
    internal class ABTestingPageBuilderConfigurationSourceLoader : IPageBuilderConfigurationSourceLoader
    {
        private readonly IPageBuilderConfigurationSourceLoader pageBuilderConfigurationLoader;
        private readonly IABTestVariantSelectionArbiter abTestVariantSelector;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IABTestManager abTestManager;
        private readonly IABTestVisitLogger aBTestVisitLogger;
        private readonly IOutputCacheUrlToPageMapper urlPageOutputCacheMapper;


        /// <summary>
        /// Initializes a new instance of the <see cref="ABTestingPageBuilderConfigurationSourceLoader"/> class with given <paramref name="pageBuilderConfigurationLoader"/>, <paramref name="httpContextAccessor"/>,
        /// <paramref name="abTestManager"/>, <paramref name="abTestVariantSelector"/> and <paramref name="aBTestVisitLogger"/> and <paramref name="urlPageOutputCacheMapper"/>.
        /// </summary>
        public ABTestingPageBuilderConfigurationSourceLoader(IPageBuilderConfigurationSourceLoader pageBuilderConfigurationLoader, IHttpContextAccessor httpContextAccessor,
            IABTestManager abTestManager, IABTestVariantSelectionArbiter abTestVariantSelector, IABTestVisitLogger aBTestVisitLogger, IOutputCacheUrlToPageMapper urlPageOutputCacheMapper)
        {
            this.pageBuilderConfigurationLoader = pageBuilderConfigurationLoader ?? throw new ArgumentNullException(nameof(pageBuilderConfigurationLoader));
            this.abTestVariantSelector = abTestVariantSelector ?? throw new ArgumentNullException(nameof(abTestVariantSelector));
            this.abTestManager = abTestManager ?? throw new ArgumentNullException(nameof(abTestManager));
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.aBTestVisitLogger = aBTestVisitLogger ?? throw new ArgumentNullException(nameof(aBTestVisitLogger));
            this.urlPageOutputCacheMapper = urlPageOutputCacheMapper ?? throw new ArgumentNullException(nameof(urlPageOutputCacheMapper));
        }


        /// <summary>
        /// Loads Page builder configuration for a page. Loads an A/B variant specific configuration if page is being A/B tested.
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

            if (!ABTestInfoProvider.ABTestingEnabled(SiteContext.CurrentSiteName))
            {
                return LoadByDefault(page);
            }

            if (IsInPagesApplication())
            {
                return LoadForPagesApplication(page);
            }

            bool isFirstVisit;
            ABTestVariant selectedVariant = abTestVariantSelector.SelectVariant(page, out isFirstVisit);

            urlPageOutputCacheMapper.Add(httpContextAccessor.HttpContext.Request.Url, page);

            aBTestVisitLogger.LogVisit(page, isFirstVisit);

            if (selectedVariant?.IsOriginal ?? true)
            {
                return LoadByDefault(page);
            }

            return new PageBuilderConfigurationSource
            {
                WidgetsConfiguration = selectedVariant.PageBuilderWidgets,
                PageTemplateConfiguration = selectedVariant.PageTemplate
            };
        }


        /// <summary>
        /// Loads Page builder configuration for a page.
        /// </summary>
        /// <returns>Returns the configuration source.</returns>
        private PageBuilderConfigurationSource LoadByDefault(TreeNode page)
        {
            return pageBuilderConfigurationLoader.Load(page);
        }


        /// <summary>
        /// Returns <c>true</c> whether the page will be displayed in the pages application.
        /// </summary>
        private bool IsInPagesApplication()
        {
            var context = httpContextAccessor.HttpContext.Kentico();

            return context.PageBuilder().EditMode || context.Preview().Enabled;
        }


        private PageBuilderConfigurationSource LoadForPagesApplication(TreeNode page)
        {
            if (!TryGetABTestVariantFromQueryString(ABTestConstants.AB_TEST_VARIANT_QUERY_STRING_PARAMETER_NAME, out var selectedVariantGuid))
            {
                return LoadByDefault(page);
            }

            var originalSource = LoadByDefault(page);

            var variants = abTestManager.GetVariants(page);
            var selectedVariant = variants.FirstOrDefault(v => v.Guid == selectedVariantGuid);

            if (selectedVariant == null || selectedVariant.IsOriginal)
            {
                return originalSource;
            }

            return new PageBuilderConfigurationSource
            {
                WidgetsConfiguration = selectedVariant.PageBuilderWidgets,
                PageTemplateConfiguration = selectedVariant.PageTemplate
            };
        }


        /// <summary>
        /// Gets the A/B test variant GUID from the query string, if the variant GUID parameter is present.
        /// </summary>
        private bool TryGetABTestVariantFromQueryString(string parameterName, out Guid variantGuid)
        {
            var variantGuidString = httpContextAccessor.HttpContext.Request.QueryString[parameterName];

            return Guid.TryParse(variantGuidString, out variantGuid);
        }
    }
}
