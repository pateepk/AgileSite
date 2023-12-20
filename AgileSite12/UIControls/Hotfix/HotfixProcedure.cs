using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.Modules;
using CMS.UIControls.Internal;
using CMS.WorkflowEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Applies additional hotfix changes which cannot be applied by the Hotfix utility.
    /// </summary>
    internal static class HotfixProcedure
    {
        private const int SERVICE_PACK_HOTFIX_VERSION = 29;
        private static HotfixHelper mHotfixHelper;


        private static HotfixHelper HotfixHelper
        {
            get
            {
                return mHotfixHelper ?? (mHotfixHelper = new HotfixHelper());
            }
        }


        /// <summary>
        /// Applies additional hotfix changes, if the instance has been hotfixed by the utility.
        /// </summary>
        /// <remarks>
        /// Only the main application (indicated by <see cref="SystemContext.IsCMSRunningAsMainApplication"/>) performs hotfixing.
        /// </remarks>
        public static void Hotfix()
        {
            if (DatabaseHelper.IsDatabaseAvailable && (SystemContext.IsCMSRunningAsMainApplication || HotfixTestHelper.IsRunningInTestMode))
            {
                try
                {
                    if (HotfixHelper.SkipHotfixing(out int hotfixVersion, out int hotfixDataVersion))
                    {
                        return;
                    }

                    var server = HttpContext.Current?.Server;
                    if (server != null)
                    {
                        server.ScriptTimeout = 14400;
                    }

                    using (var transaction = new CMSTransactionScope())
                    {
                        if (!HotfixHelper.MarkHotfixProcedureStart())
                        {
                            // The mark operation is blocking, should always succeed
                            return;
                        }

                        if (HotfixHelper.RevalidateSkipHotfixing(out hotfixVersion, out hotfixDataVersion))
                        {
                            return;
                        }

                        using (var context = new CMSActionContext())
                        {
                            context.LogLicenseWarnings = false;
                            context.CreateVersion = false;
                            context.LogIntegration = false;
                            context.LogSynchronization = false;

                            // Update class (alt)form definitions
                            UpdateClassFormDefinitions();

                            ApplyHotfix(hotfixDataVersion);
                        }

                        SettingsKeyInfoProvider.SetGlobalValue("CMSHotfixDataVersion", hotfixVersion);

                        HotfixHelper.MarkHotfixProcedureEnd();
                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    Service.Resolve<IEventLogService>().LogException(HotfixHelper.EVENT_SOURCE, HotfixHelper.EVENT_CODE, ex);
                }
            }
        }


        /// <summary>
        /// Applies hotfix to bring the database data from <paramref name="fromHotfixVersion"/> to current version.
        /// </summary>
        /// <param name="fromHotfixVersion">Hotfix version as applied by the last hotfix procedure run.</param>
        private static void ApplyHotfix(int fromHotfixVersion)
        {
            ResignMacrosForServicePack(fromHotfixVersion);
        }


        /// <summary>
        /// Signs all macros (even already signed) of objects that has been modified for service pack with <see cref="UserInfoProvider.AdministratorUser"/>.
        /// </summary>
        /// <param name="fromHotfixVersion">Hotfix version as applied by the last hotfix procedure run.</param>
        private static void ResignMacrosForServicePack(int fromHotfixVersion)
        {
            if (fromHotfixVersion < SERVICE_PACK_HOTFIX_VERSION)
            {
                var baseInfoResigningList = new List<BaseInfo>
                {
                    DataClassInfoProvider.GetDataClassInfo("om.abtest"),
                    DataClassInfoProvider.GetDataClassInfo("cms.macrorule"),
                    DataClassInfoProvider.GetDataClassInfo("Ecommerce.GiftCard"),
                    DataClassInfoProvider.GetDataClassInfo("Ecommerce.SKU"),
                    UIElementInfoProvider.GetUIElementInfo(new Guid("281a6791-6179-4a92-a86d-2fd410a19b45")),
                    UIElementInfoProvider.GetUIElementInfo(new Guid("60d1cc5f-1232-4012-a2b6-46160c630ac8")),
                    UIElementInfoProvider.GetUIElementInfo(new Guid("7dc5113e-991a-447c-8783-3d7e1472ebac")),
                    UIElementInfoProvider.GetUIElementInfo(new Guid("79deea0a-98a5-4df6-86b1-d5b7a859fc43")),
                    AlternativeFormInfoProvider.GetAlternativeForms().WhereEquals("FormGUID", new Guid("4afa3a64-f525-4e2f-b7e7-784f969d333c")).TopN(1).FirstOrDefault(),
                    WorkflowActionInfoProvider.GetWorkflowActionInfo(new Guid("2d3729a5-f9ea-4552-9ff4-0c6ed5215ea1"))
                };

                AddNewReportsToResigningList(baseInfoResigningList);

                var identityOption = MacroIdentityOption.FromUserInfo(UserInfoProvider.AdministratorUser);

                baseInfoResigningList.ForEach(baseInfo => MacroSecurityProcessor.RefreshSecurityParameters(baseInfo, identityOption, true));
            }
        }


        private static void AddNewReportsToResigningList(List<BaseInfo> baseInfoResigningList)
        {
            var newReports = new ObjectQuery(PredefinedObjectType.REPORT).WhereStartsWith("ReportName", "mvcabtest").ToList();

            var newReportGraphs = new ObjectQuery("Reporting.ReportGraph").WhereIn("GraphReportID", newReports.Select(x => x.Generalized.ObjectID).ToList()).ToList();

            baseInfoResigningList.AddRange(newReports);
            baseInfoResigningList.AddRange(newReportGraphs);
        }


        /// <summary>
        /// Updates class form definitions and alternative form definitions based on records from "Temp_FormDefinition" table
        /// which may or may not be created by hotfix SQL script.
        /// </summary>
        private static void UpdateClassFormDefinitions()
        {
            var classFormDefinitionUpdateHelper = new ClassFormDefinitionUpdateHelper(HotfixHelper.EVENT_SOURCE);
            classFormDefinitionUpdateHelper.UpdateClassFormDefinitions();
        }
    }
}
