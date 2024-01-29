using System;

using CMS;
using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.Base;
using CMS.Membership;
using CMS.Reporting;

[assembly: RegisterModule(typeof(ReportingModule))]

namespace CMS.Reporting
{
    /// <summary>
    /// Represents the Reporting module.
    /// </summary>
    public class ReportingModule : Module
    {
        #region "Constants"

        internal const string REPORTING = "##REPORTING##";


        /// <summary>
        /// Name of email template type for reporting.
        /// </summary>
        public const string REPORTING_EMAIL_TEMPLATE_TYPE_NAME = "reporting";

        #endregion


        /// <summary>
        /// Default constructor
        /// </summary>
        public ReportingModule()
            : base(new ReportingModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            // Import export handlers
            ReportExport.Init();
            ReportImport.Init();

            ExtendList<MacroResolverStorage, MacroResolver>.With("ReportingResolver").WithLazyInitialization(() => ReportingResolvers.ReportingResolver);

            // Register custom event handlers
            MacroRuleInfo.TYPEINFO.Events.CheckPermissions.Before += CheckPermissions_Before;
        }


        /// <summary>
        /// Registers the object type of this module
        /// </summary>
        protected override void RegisterCommands()
        {
            base.RegisterCommands();

            RegisterCommand("GetDefaultReportConnectionString", GetDefaultReportConnectionString);
            RegisterCommand("RefreshCategoryDataCount", RefreshCategoryDataCount);
        }


        /// <summary>
        /// Item's default connection string 
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static String GetDefaultReportConnectionString(object[] parameters)
        {
            return ReportHelper.GetDefaultReportConnectionString();
        }


        /// <summary>
        /// Refresh category child count
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object RefreshCategoryDataCount(object[] parameters)
        {
            ReportCategoryInfo category = parameters[0] as ReportCategoryInfo;
            if (category != null)
            {
                ReportCategoryInfoProvider.UpdateReportCategoryChildCount(0, category.CategoryID);
                ReportCategoryInfoProvider.UpdateCategoryChildCount(0, category.CategoryID);
            }

            return null;
        }


        /// <summary>
        /// Checks permissions for reporting macro rules.
        /// </summary>
        private void CheckPermissions_Before(object sender, ObjectSecurityEventArgs e)
        {
            var rule = e.Object as MacroRuleInfo;
            var permission = e.Permission;

            if (rule != null)
            {
                // Special permission check for reporting macro rules and global macro rules
                switch (rule.MacroRuleResourceName.ToLowerCSafe())
                {
                    case "cms.reporting":
                    case "":
                        {
                            switch (permission)
                            {
                                case PermissionsEnum.Read:
                                    e.Result = UserSecurityHelper.IsAuthorizedPerResource("cms.reporting", "Read", e.SiteName, (UserInfo)e.User).ToAuthorizationResultEnum();
                                    break;

                                case PermissionsEnum.Create:
                                case PermissionsEnum.Delete:
                                case PermissionsEnum.Modify:
                                case PermissionsEnum.Destroy:
                                    e.Result = UserSecurityHelper.IsAuthorizedPerResource("cms.reporting", "Modify", e.SiteName, (UserInfo)e.User).ToAuthorizationResultEnum();
                                    break;
                            }

                            // Skip default check
                            e.Cancel();
                        }
                        break;
                }
            }
        }
    }
}