using System;
using System.Linq;

using CMS;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.DocumentEngine.PageBuilder;
using CMS.OnlineMarketing.Internal;
using CMS.OnlineMarketing.Web.UI;

[assembly: RegisterImplementation(typeof(ITempPageBuilderWidgetsPropagator), typeof(ABTestTempPageBuilderWidgetsPropagator), Priority = RegistrationPriority.SystemDefault)]

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Propagates widgets and template configuration from temporary data to page data or A/B test variant data.
    /// </summary>
    internal class ABTestTempPageBuilderWidgetsPropagator : TempPageBuilderWidgetsPropagator
    {
        private readonly ITempPageBuilderWidgetsPropagator tempPageBuilderWidgetsPropagator;
        private readonly IABTestManager abTestManager;
        private readonly IABTestVariantIdentifierProvider abTestVariantIdentifierProvider;


        /// <summary>
        /// Initializes a new instance of the <see cref="ABTestTempPageBuilderWidgetsPropagator"/> class.
        /// </summary>
        /// <param name="tempPageBuilderWidgetsPropagator">Previously registered <see cref="ITempPageBuilderWidgetsPropagator"/> implementation instance.</param>
        /// <param name="abTestManager">AB test manager service.</param>
        /// <param name="abTestVariantIdentifierProvider">Provides A/B test variant identifier.</param>
        public ABTestTempPageBuilderWidgetsPropagator(ITempPageBuilderWidgetsPropagator tempPageBuilderWidgetsPropagator, IABTestManager abTestManager, IABTestVariantIdentifierProvider abTestVariantIdentifierProvider)
        {
            this.tempPageBuilderWidgetsPropagator = tempPageBuilderWidgetsPropagator ?? throw new ArgumentNullException(nameof(tempPageBuilderWidgetsPropagator));
            this.abTestManager = abTestManager ?? throw new ArgumentNullException(nameof(abTestManager));
            this.abTestVariantIdentifierProvider = abTestVariantIdentifierProvider ?? throw new ArgumentNullException(nameof(abTestVariantIdentifierProvider));
        }


        /// <summary>
        /// Propagates widgets and template configuration from temporary data to a page data or A/B test variant data
        /// based on the variant GUID obtained from a persistent store.
        /// </summary>
        /// <param name="instanceGuid">Instance GUID of editing.</param>
        /// <param name="page">Page to propagate data into.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        /// <seealso cref="ABTestVariantIdentifierProvider"/>
        public override void Propagate(TreeNode page, Guid instanceGuid)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }
            
            var tempData = GetTempDataQuery(instanceGuid).TopN(1).FirstOrDefault();
            if (tempData == null)
            {
                return;
            }

            var variantGuid = abTestVariantIdentifierProvider.GetVariantIdentifier();
            if (variantGuid == null)
            {
                tempPageBuilderWidgetsPropagator.Propagate(page, instanceGuid);

                return;
            }

            var source = new VariantConfigurationSource { Widgets = tempData.PageBuilderWidgetsConfiguration, PageTemplate = tempData.PageBuilderTemplateConfiguration };
            abTestManager.UpdateVariant(page, variantGuid.Value, source);
        }
    }
}
