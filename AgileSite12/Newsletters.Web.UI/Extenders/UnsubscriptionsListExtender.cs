﻿using System;
using System.Data;

using CMS;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.Helpers;
using CMS.Membership;
using CMS.Newsletters.Web.UI;
using CMS.SiteProvider;
using CMS.UIControls;

[assembly: RegisterCustomClass("UnsubscriptionsListExtender", typeof(UnsubscriptionsListExtender))]

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Extends Unsubscription listing unigrid.
    /// </summary>
    public class UnsubscriptionsListExtender : ControlExtender<UniGrid>
    {
        /// <summary>
        /// Initializes extender.
        /// </summary>
        public override void OnInit()
        {
            // Object type must be reset to be able to use custom DataSource
            Control.ObjectType = null;

            Control.OnDataReload += Control_OnDataReload;
            Control.OnAction += Control_ActionDelete;

            if (!RequestHelper.IsPostBack())
            {
                ShowCreateConfirmation();
            }
        }


        /// <summary>
        /// Handles control action buttons.
        /// </summary>
        private void Control_ActionDelete(string actionName, object actionArgument)
        {
            if (!UserInfoProvider.IsAuthorizedPerResource(ModuleName.NEWSLETTER, "ManageSubscribers", SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser, false))
            {
                CMSPage.RedirectToAccessDenied("cms.newsletter", "managesubscribers");
            }

            string email = ValidationHelper.GetString(actionArgument, string.Empty);
            if (actionName == "remove" && !string.IsNullOrEmpty(email))
            {
                Service.Resolve<IUnsubscriptionProvider>().RemoveUnsubscriptionsFromAllNewsletters(email);
            }
        }


        /// <summary>
        /// Returns unsubscriptions that should be visible in the UI.
        /// </summary>
        private DataSet Control_OnDataReload(string completeWhere, string currentOrder, int currentTopN, string columns, int currentOffset, int currentPageSize, ref int totalRecords)
        {
            var query = Service.Resolve<IUnsubscriptionProvider>().GetUnsubscriptionsFromAllNewsletters();

            // Columns are being enforced to group everything by email - for one email address, there can be multiple rows 
            // in the database (which can happen by switching the global/site unsubscription list setting)
            query.Columns("UnsubscriptionEmail", "MIN(UnsubscriptionCreated) as UnsubscriptionCreated")
                 .GroupBy("UnsubscriptionEmail");

            // Append all the parameters sent from unigrid control
            query.Where(completeWhere)
                 .OrderBy(currentOrder)
                 .TopN(currentTopN);

            query.Offset = currentOffset;
            query.MaxRecords = currentPageSize;

            totalRecords = query.TotalRecords;

            return query;
        }


        /// <summary>
        /// Displays message about successful unsubscription, if unsubscription query string is set.
        /// Content of displayed message depends on whether the email address was already unsubscribed or not.
        /// </summary>
        private void ShowCreateConfirmation()
        {
            string unsubscribedEmail = QueryHelper.GetString("unsubscriptionEmail", null);

            if (String.IsNullOrEmpty(unsubscribedEmail))
            {
                return;
            }

            bool alreadyUnsubscribed = QueryHelper.GetBoolean("alreadyUnsubscribed", false);

            string resourceKey = alreadyUnsubscribed ?
                "emailmarketing.ui.unsubscriptions.new.createconfirmation.alreadyunsubscribed" :
                "emailmarketing.ui.unsubscriptions.new.createconfirmation";

            Control.ShowConfirmation(String.Format(ResHelper.GetString(resourceKey), unsubscribedEmail));
        }
    }
}