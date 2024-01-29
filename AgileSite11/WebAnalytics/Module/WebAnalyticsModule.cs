using System.Data;

using CMS;
using CMS.Activities;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.MacroEngine;
using CMS.WebAnalytics;
using CMS.WebAnalytics.Internal;

[assembly: RegisterModule(typeof(WebAnalyticsModule))]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Represents the Web Analytics module.
    /// </summary>
    public class WebAnalyticsModule : Module
    {
        /// <summary>
        /// Online marketing group constant
        /// </summary>
        public const string ONLINEMARKETING = "##ONLINEMARKETING##";

        /// <summary>
        /// Web analytics group constant
        /// </summary>
        public const string WEBANALYTICS = "##WEBANALYTICS##";


        /// <summary>
        /// Default constructor
        /// </summary>
        public WebAnalyticsModule()
            : base(new WebAnalyticsModuleMetadata())
        {
        }


        /// <summary>
        /// Pre-initializes the module.
        /// </summary>
        protected override void OnPreInit()
        {
            base.OnPreInit();

            DebugHelper.RegisterDebug(AnalyticsDebug.Settings);
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            RegisterContext<AnalyticsContext>("AnalyticsContext");

            // Init events handlers
            WebAnalyticsHandlers.Init();

            InitMacros();

            RegisterActivityModifiers();

            RegisterAssetModelStrategies();

            RegisterDataTypes();
        }        


        /// <summary>
        /// Registers custom data types.
        /// </summary>
        private void RegisterDataTypes()
        {
            DataTypeManager.RegisterDataTypes(
                new DataType<DataTable>("Type_Analytics_CampaignConversionHitsTable", "", DataTypeManager.PLAIN, (value, defaultValue, culture) => value as DataTable ?? defaultValue)
                {
                    DbType = SqlDbType.Structured,
                    Hidden = true
                });
        }


        /// <summary>
        /// Initializes the web analytics macros
        /// </summary>
        private static void InitMacros()
        {
            ExtendList<MacroResolverStorage, MacroResolver>.With("AnalyticsResolver").WithLazyInitialization(() => WebAnalyticsResolvers.AnalyticsResolver);

            var r = MacroContext.GlobalResolver;

            r.SetNamedSourceData(new MacroField("Visitor", () => MacroNamespace<VisitorNamespace>.Instance), false);
            r.AddAnonymousSourceData(MacroNamespace<VisitorNamespace>.Instance);
            r.SetNamedSourceData(new MacroField("Campaign", () => Service.Resolve<ICampaignService>().CampaignCode), false);
        }


        /// <summary>
        /// Registers the object type of this module
        /// </summary>
        protected override void RegisterCommands()
        {
            base.RegisterCommands();

            RegisterCommand("LogOnSiteKeyWords", LogOnSiteKeyWords);
        }


        /// <summary>
        /// Log on-site key words
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object LogOnSiteKeyWords(object[] parameters)
        {
            AnalyticsHelper.LogOnSiteSearchKeywords((string)parameters[0], null, (string)parameters[1], (string)parameters[2], 0, 1);
            return null;
        }


        private static void RegisterActivityModifiers()
        {
            var activityLogService = Service.Resolve<IActivityLogService>();

            RegisterCampaignActivityModifier(activityLogService);
            RegisterUrlHashActivityModifier(activityLogService);
        }


        private static void RegisterCampaignActivityModifier(IActivityLogService activityLogService)
        {
            var campaignService = Service.Resolve<ICampaignService>();
            var modifier = new CampaignActivityModifier(campaignService);
            activityLogService.RegisterModifier(modifier);
        }


        private static void RegisterUrlHashActivityModifier(IActivityLogService activityLogService)
        {
            var hashService = Service.Resolve<IActivityUrlHashService>();
            var modifier = new UrlHashActivityModifier(hashService);
            activityLogService.RegisterModifier(modifier);
        }


        private static void RegisterAssetModelStrategies()
        {
            var service = Service.Resolve<ICampaignAssetModelService>();

            service.RegisterAssetModelStrategy(TreeNode.OBJECT_TYPE, new PageAssetModelStrategy());
            service.RegisterAssetModelStrategy(CampaignAssetUrlInfo.OBJECT_TYPE, new CampaignAssetUrlModelStrategy());
        }
    }
}