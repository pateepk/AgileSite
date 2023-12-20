using CMS;
using CMS.Activities;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.WebAnalytics;
using CMS.WebAnalytics.Internal;

using Kentico.Content.Web.Mvc.Routing;
using Kentico.OnlineMarketing.Web.Mvc;

[assembly: RegisterModule(typeof(OnlineMarketingWebMvcModule))]

namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Represents the Kentico.OnlineMarketing.Web.Mvc module.
    /// </summary>
    internal class OnlineMarketingWebMvcModule : Module
    {
        /// <summary>
        /// Identifier of <see cref="OnlineMarketingWebMvcModule"/>.
        /// </summary>
        public const string MODULE_NAME = "Kentico.OnlineMarketing.Web.Mvc";


        /// <summary>
        /// Initializes a new instance of the <see cref="OnlineMarketingWebMvcModule"/> class.
        /// </summary>
        public OnlineMarketingWebMvcModule()
            : base(MODULE_NAME)
        {
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            ABTestingHandlers.Init();

            RegisterActivityModifiers();

            HitLogProvider.LogDirectly = true;
        }


        /// <summary>
        /// Register classes for modifying tracked activities.
        /// </summary>
        private static void RegisterActivityModifiers()
        {
            var activityLogService = Service.Resolve<IActivityLogService>();

            activityLogService.RegisterModifier(new UrlActivityModifier(Service.Resolve<ISiteService>(), Service.Resolve<IAlternativeUrlsService>(), Service.Resolve<IActivityUrlHashService>()));
        }
    }
}
