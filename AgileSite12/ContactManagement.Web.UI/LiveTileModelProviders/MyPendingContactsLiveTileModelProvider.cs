using System;

using CMS.Activities;
using CMS.ApplicationDashboard.Web.UI;
using CMS.Automation;
using CMS.Base;
using CMS.ContactManagement.Web.UI;
using CMS.Core;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.WorkflowEngine;

[assembly: RegisterLiveTileModelProvider(ModuleName.ONLINEMARKETING, "MyContacts", typeof(MyPendingContactsLiveTileModelProvider))]

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Provides live tile model for the My pending contacts dashboard tile.
    /// </summary>
    internal class MyPendingContactsLiveTileModelProvider : ILiveTileModelProvider
    {
        /// <summary>
        /// Loads total number of pending contacts for the given user.
        /// </summary>
        /// <param name="liveTileContext">Context of the live tile. Contains information about the user and the site the model is requested for</param>
        /// <exception cref="ArgumentNullException"><paramref name="liveTileContext"/> is null</exception>
        /// <returns>Live tile model</returns>
        public LiveTileModel GetModel(LiveTileContext liveTileContext)
        {
            if (liveTileContext == null)
            {
                throw new ArgumentNullException("liveTileContext");
            }

            return CacheHelper.Cache(() =>
            {
                if (!ActivitySettingsHelper.OnlineMarketingEnabled(liveTileContext.SiteInfo.SiteName))
                {
                    return null;
                }

                var pendingContactsCount = GetPendingContactsCount(liveTileContext.SiteInfo, liveTileContext.UserInfo);

                return new LiveTileModel
                {
                    Value = pendingContactsCount,
                    Description = ResHelper.GetString("ma.pendingcontacts.livetiledescription")
                };
            }, new CacheSettings(2, "MyPendingContactsLiveTileModelProvider", liveTileContext.SiteInfo.SiteID, liveTileContext.UserInfo.UserID));
        }


        /// <summary>
        /// Gets total number of pending contacts for given user.
        /// </summary>
        /// <param name="site">Contacts' site</param>
        /// <param name="user">User the contacts are assigned to</param>
        /// <returns>Number of pending contacts</returns>
        private static int GetPendingContactsCount(SiteInfo site, IUserInfo user)
        {
            // Get complete where condition for pending steps
            var condition = WorkflowStepInfoProvider.GetAutomationPendingStepsWhereCondition(user as UserInfo, site.SiteID);

            // Get site condition
            condition.WhereEquals("StateSiteID", site.SiteID);

            // Get automation steps specified by condition with permission control
            var automationWorkflowSteps = WorkflowStepInfoProvider.GetWorkflowSteps()
                                                                  .Where(condition)
                                                                  .Column("StepID")
                                                                  .WhereEquals("StepWorkflowType", (int)WorkflowTypeEnum.Automation);

            var contactIDs = ContactInfoProvider.GetContacts()
                                                .Column("ContactID")
                                                .WhereEquals("ContactOwnerUserID", user.UserID)
                                                .AsMaterializedList("ContactID");

            // Get all pending contacts from automation state where status is Pending and current user is the owner
            var pendingContactsCount = AutomationStateInfoProvider.GetAutomationStates()
                                                                  .WhereIn("StateObjectID", contactIDs)
                                                                  .WhereIn("StateStepID", automationWorkflowSteps)
                                                                  .WhereEquals("StateStatus", (int)ProcessStatusEnum.Pending)
                                                                  .WhereEquals("StateObjectType", ContactInfo.OBJECT_TYPE)
                                                                  .Count;
            return pendingContactsCount;
        }
    }
}