using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;
using CMS.Newsletters;

using CMS.Core;
using CMS.Newsletters.Issues.Widgets;
using CMS.WebAnalytics;

[assembly: RegisterModule(typeof(NewsletterModule))]

namespace CMS.Newsletters
{
    /// <summary>
    /// Represents the Newsletter module.
    /// </summary>
    public class NewsletterModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public NewsletterModule()
            : base(new NewsletterModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            // Import export handlers
            NewsletterExport.Init();
            ImportSpecialActions.Init();

            NewsletterHandlers.Init();

            NewsletterResolvers.Register();
            WidgetResolvers.Register();

            // Set event logging of issues based on web.config key
            IssueInfo.TYPEINFO.LogEvents = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSLogNewsletterIssueEvents"], true);

            // Register new macros to CM
            MacroRuleMetadataContainer.RegisterNewsletterMetadata();

            var service = Service.Resolve<ICampaignAssetModelService>();
            service.RegisterAssetModelStrategy(PredefinedObjectType.NEWSLETTERISSUE, new NewsletterAssetModelStrategy());

            RegisterDataTypes();
        }


        /// <summary>
        /// Registers custom data types.
        /// </summary>
        private void RegisterDataTypes()
        {
            DataTypeManager.RegisterDataTypes(
                new DataType<IEnumerable<ContactEmailRecord>>("Type_OM_ContactEmailTable", "", DataTypeManager.PLAIN, (value, defaultValue, culture) =>
                {
                    throw new NotSupportedException("This data type is used for internal purposes only.");
                })
                {
                    DbType = SqlDbType.Structured,
                    Hidden = true,
                    TypeName = "Type_OM_ContactEmailTable"
                });
        }
    }


    /// <summary>
    /// Data structure representing contact email record.
    /// </summary>
    internal struct ContactEmailRecord
    {
        /// <summary>
        /// Contact email.
        /// </summary>
        public string Email { get; set; }
    }
}