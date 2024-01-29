using System;
using System.Linq;

using CMS.Base;
using CMS.ContactManagement;
using CMS.ContactManagement.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;
using CMS.WebAnalytics;

namespace CMS.Newsletters
{
    /// <summary>
    /// Newsletter events handlers.
    /// </summary>
    internal class NewsletterHandlers
    {
        /// <summary>
        /// Query parameter name for the campaign tracking.
        /// </summary>
        private const string CAMPAIGN_URL_TRACKING_PARAMETER = "utm_campaign";


        /// <summary>
        /// Query parameter name for the source tracking.
        /// </summary>
        private const string SOURCE_URL_TRACKING_PARAMETER = "utm_source";


        /// <summary>
        /// Query parameter name for the content tracking.
        /// </summary>
        private const string CONTENT_URL_TRACKING_PARAMETER = "utm_content";


        /// <summary>
        /// Initializes the events handlers.
        /// </summary>
        public static void Init()
        {
            SettingsKeyInfoProvider.OnSettingsKeyChanged += CancelEmailSending;

            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                ApplicationEvents.PostStart.Execute += ClearEmailSendingStatus;
            }

            ContactGroupInfo.TYPEINFO.Events.Update.Before += RenameContactGroupSubscriber;
            ContactInfo.TYPEINFO.Events.Update.Before += SynchronizeNameAndEmailOfContactSubscriber;
            ContactInfo.TYPEINFO.Events.Insert.After += ResetEmailBounces;
            ContactInfo.TYPEINFO.Events.Update.After += ResetEmailBounces;

            WebAnalyticsEvents.CampaignLaunched.Execute += SendCampaignEmailAssets;
            WebAnalyticsEvents.CampaignUTMChanged.Execute += ChangeUTMInCampaignEmail;

            NewsletterEvents.SubscriberOpensEmail.Execute += SubscriberOpensEmail;
            NewsletterEvents.SubscriberClicksTrackedLink.Execute += SubscriberClicksTrackedLink;

            NewsletterEvents.SubscriberUnsubscribes.Execute += FindUnsubscribedContactAndMergeWithCurrent;
            NewsletterEvents.SubscriberUnsubscribes.Execute += LogNewsletterUnsubscribingActivity;
            NewsletterEvents.SubscriberUnsubscribes.Execute += LogNewsletterUnsubscribingFromAllActivity;

            ContactManagementEvents.ContactMerge.Execute += MoveContactSubscribers;
            ContactManagementEvents.ContactMerge.Execute += MoveEmailQueueItems;
            ContactManagementEvents.ContactInfosDeleted.Execute += DeleteContactSubscribers;
        }


        private static void DeleteContactSubscribers(object sender, ContactInfosDeletedHandlerEventArgs e)
        {
            using (var cs = new CMSConnectionScope())
            {
                cs.CommandTimeout = ConnectionHelper.LongRunningCommandTimeout;

                SubscriberInfoProvider.ProviderObject.BulkDelete(
                    new WhereCondition()
                        .WhereIn("SubscriberRelatedID", e.DeletedContactsIds.ToList()),
                    new BulkDeleteSettings
                    {
                        RemoveDependencies = true,
                        ObjectType = SubscriberInfo.OBJECT_TYPE_CONTACT
                    });
            }
        }


        private static void MoveEmailQueueItems(object sender, CMSEventArgs<ContactMergeModel> e)
        {
            var contactMergeModel = e.Parameter;
            EmailQueueItemInfoProvider.BulkMoveEmailsInQueueToAnotherContact(contactMergeModel.SourceContact, contactMergeModel.TargetContact);
        }


        private static void MoveContactSubscribers(object sender, CMSEventArgs<ContactMergeModel> e)
        {
            var contactMergeModel = e.Parameter;
            new ContactMergeSubscriberUpdater(contactMergeModel.SourceContact, contactMergeModel.TargetContact).Update();
        }
        
        
        /// <summary>
        /// Finds contact by email address. This contact represents visitor that is being unsubscribed from email marketing communication.
        /// Merge him with current contact if current contact is anonymous.
        /// </summary>
        internal static void FindUnsubscribedContactAndMergeWithCurrent(object sender, UnsubscriptionEventArgs e)
        {
            if (!Service.Resolve<ISiteService>().IsLiveSite)
            {
                return;
            }

            // Do not merge with current contact so that the visitor won't get a contact cookie
            // This is due to security issue in version 8.2 and below
            if (NewsletterContext.UnsubscriptionLinksBackwardCompatibilityMode)
            {
                return;
            }

            if (!Service.Resolve<IContactProcessingChecker>().CanProcessContactInCurrentContext())
            {
                return;
            }
            
            var contact = ContactInfoProvider.GetContactInfo(e.Email);
            if (contact != null)
            {
                Service.Resolve<ICurrentContactMergeService>().MergeCurrentContactWithContact(contact);
            }
        }


        /// <summary>
        /// Clears e-mail sending status of all newsletters
        /// </summary>
        private static void ClearEmailSendingStatus(object sender, EventArgs eventArgs)
        {
            EmailQueueManager.ClearEmailsSendingStatus();
        }


        /// <summary>
        /// Cancels sending of emails if related settings key changed
        /// </summary>
        private static void CancelEmailSending(object sender, SettingsKeyChangedEventArgs e)
        {
            if (string.Equals(e.KeyName, "CMSEmailsEnabled", StringComparison.OrdinalIgnoreCase) && (e.SiteID <= 0) && !e.KeyValue.ToBoolean(false))
            {
                // Stop current sending of e-mails and newsletters if e-mails are disabled in global settings
                ThreadEmailSender.CancelSending();
            }
        }


        /// <summary>
        /// Updates UTM in email (and all its variants if A/B).
        /// </summary>
        private static void ChangeUTMInCampaignEmail(object sender, CMSEventArgs<CampaignUTMChangedData> e)
        {
            var parameters = e.Parameter;
            var campaign = e.Parameter.Campaign;

            var masterIssue = IssueInfoProvider.GetIssueInfo(parameters.OriginalEmailID);

            if (masterIssue == null)
            {
                return;
            }

            var oldUTMSource = string.IsNullOrEmpty(masterIssue.IssueUTMSource) ? null : masterIssue.IssueUTMSource;
            var normalizedUTMSource = ConvertToUtmNamingConvention(parameters.NewUTMSource ?? oldUTMSource ?? campaign.CampaignDisplayName);
            var useUTM = !string.IsNullOrEmpty(normalizedUTMSource);

            masterIssue.IssueUTMSource = normalizedUTMSource;
            masterIssue.IssueUTMCampaign = campaign.CampaignUTMCode;
            masterIssue.IssueUseUTM = useUTM;

            IssueInfoProvider.SetIssueInfo(masterIssue);

            // Set A/B variants if any
            if (masterIssue.IssueIsABTest)
            {
                var variants = IssueInfoProvider.GetIssues()
                                                .OnSite(campaign.CampaignSiteID)
                                                .WhereEquals("IssueVariantOfIssueID", masterIssue.IssueID)
                                                .ToList();

                foreach (var variant in variants)
                {
                    variant.IssueUTMSource = normalizedUTMSource;
                    variant.IssueUTMCampaign = campaign.CampaignUTMCode;
                    variant.IssueUseUTM = useUTM;

                    IssueInfoProvider.SetIssueInfo(variant);
                }
            }
        }


        /// <summary>
        /// Sends campaign email assets.
        /// </summary>
        private static void SendCampaignEmailAssets(object sender, CMSEventArgs<CampaignInfo> e)
        {
            var campaign = e.Parameter;

            var issueGuids = CampaignAssetInfoProvider.GetCampaignAssets()
                                                      .WhereEquals("CampaignAssetCampaignID", campaign.CampaignID)
                                                      .WhereEquals("CampaignAssetType", IssueInfo.OBJECT_TYPE)
                                                      .Column("CampaignAssetAssetGuid")
                                                      .Distinct();

            var issues = IssueInfoProvider.GetIssues()
                                          .WhereEmpty("IssueMailoutTime")
                                          .OnSite(campaign.CampaignSiteID)
                                          .WhereEqualsOrNull("IssueStatus", (int)IssueStatusEnum.Idle)
                                          .WhereIn("IssueGUID", issueGuids)
                                          .ToList();

            issues.ForEach(issue =>
            {
                UpdateIssueUTMCampaignCode(issue, e.Parameter.CampaignUTMCode);

                Service.Resolve<IIssueScheduler>().ScheduleIssue(issue, DateTime.Now);
            });
        }


        /// <summary>
        /// Updates <see cref="IssueInfo.IssueUTMCampaign"/> field of given <paramref name="issue"/> with <paramref name="campaignUTMCode"/>. 
        /// If <paramref name="issue"/> is AB test parent, sets the <paramref name="campaignUTMCode"/> to its variants as well.
        /// </summary>
        /// <remarks>
        /// Besides <see cref="IssueInfo.IssueUTMCampaign"/> sets <see cref="IssueInfo.IssueUseUTM"/> to <c>true</c> as well, 
        /// since this field should be always set to <c>true</c> once the <see cref="IssueInfo.IssueUTMCampaign"/> is filled.
        /// </remarks>
        /// <param name="issue"><see cref="IssueInfo"/> to be updated</param>
        /// <param name="campaignUTMCode">Campaign code to be set to <paramref name="issue"/></param>
        private static void UpdateIssueUTMCampaignCode(IssueInfo issue, string campaignUTMCode)
        {
            // Set CampaignUTMCode to the current campaign to cover the case when email has been assigned to other campaigns as well
            issue.IssueUTMCampaign = campaignUTMCode;
            issue.IssueUseUTM = true;

            IssueInfoProvider.SetIssueInfo(issue);

            if (issue.IssueIsABTest)
            {
                var variants = IssueInfoProvider.GetIssues()
                                 .WhereEquals("IssueVariantOfIssueID", issue.IssueID);

                foreach (var variant in variants)
                {
                    variant.IssueUTMCampaign = campaignUTMCode;
                    variant.IssueUseUTM = true;

                    IssueInfoProvider.SetIssueInfo(variant);
                }
            }
        }


        /// <summary>
        /// Returns string that can be later used in URL and it is safe for the analytics.
        /// </summary>
        /// <remarks>Converts spaces into under underscores.</remarks>
        private static string ConvertToUtmNamingConvention(string input)
        {
            if (input == null)
            {
                return null;
            }

            input = input.Replace(' ', '_');
            return ValidationHelper.GetCodeName(input, "", 200, useUnicode: false).ToLowerInvariant();
        }
        
        
        /// <summary>
        /// Logs NewsletterClickThrough activity for contact specified in <paramref name="e"/>.
        /// </summary>
        private static void SubscriberClicksTrackedLink(object sender, LinksEventArgs e)
        {
            if (!LicenseKeyInfoProvider.IsFeatureAvailable(RequestContext.CurrentDomain, FeatureEnum.FullContactManagement))
            {
                return;
            }

            var issue = e.IssueInfo;
            var newsletter = e.NewsletterInfo;

            if (issue == null)
            {
                throw new ArgumentException("Property IssueInfo is null", nameof(e));
            }

            if (newsletter == null)
            {
                throw new ArgumentException("Property NewsletterInfo is null", nameof(e));
            }

            var email = e.AdditionalParameters.Get("email");
            var contactID = GetContactIDFromEmail(email);

            if (contactID == 0)
            {
                return;
            }

            var originalURL = e.AdditionalParameters.Get("originalURL");

            SetUtmParametersForTrackedLink(newsletter.NewsletterSiteID, originalURL);

            new NewslettersActivityLogger().LogNewsletterClickThroughActivity(originalURL, issue, newsletter, contactID);
        }


        private static void SetUtmParametersForTrackedLink(int siteId, string orginalUrl)
        {
            var urlCampaign = URLHelper.GetQueryValue(orginalUrl, CAMPAIGN_URL_TRACKING_PARAMETER);
            if (string.IsNullOrEmpty(urlCampaign))
            {
                return;
            }

            var urlSource = URLHelper.GetQueryValue(orginalUrl, SOURCE_URL_TRACKING_PARAMETER);
            var urlContent = URLHelper.GetQueryValue(orginalUrl, CONTENT_URL_TRACKING_PARAMETER);

            var siteName = SiteInfoProvider.GetSiteName(siteId);

            var campaignService = Service.Resolve<ICampaignService>();
            campaignService.SetCampaign(urlCampaign, siteName, urlSource, urlContent);
        }


        /// <summary>
        /// Logs NewsletterOpen activity for contact specified in <paramref name="e"/>.
        /// </summary>
        private static void SubscriberOpensEmail(object sender, LinksEventArgs e)
        {
            if (!LicenseKeyInfoProvider.IsFeatureAvailable(RequestContext.CurrentDomain, FeatureEnum.FullContactManagement))
            {
                return;
            }

            var issue = e.IssueInfo;
            var newsletter = e.NewsletterInfo;

            if (issue == null)
            {
                throw new ArgumentException("Property IssueInfo is null", nameof(e));
            }

            if (newsletter == null)
            {
                throw new ArgumentException("Property NewsletterInfo is null", nameof(e));
            }

            var email = e.AdditionalParameters.Get("email");
            var contactID = GetContactIDFromEmail(email);

            if (contactID == 0)
            {
                return;
            }

            SetUtmParametersFromIssue(issue);
            
            new NewslettersActivityLogger().LogNewsletterOpenedEmailActivity(issue, newsletter, contactID);
        }


        private static void SetUtmParametersFromIssue(IssueInfo issue)
        {
            if (string.IsNullOrEmpty(issue.IssueUTMCampaign))
            {
                return;
            }

            var siteName = SiteInfoProvider.GetSiteName(issue.IssueSiteID);

            var campaignService = Service.Resolve<ICampaignService>();
            campaignService.SetCampaign(issue.IssueUTMCampaign, siteName, issue.IssueUTMSource);
        }


        /// <summary>
        /// Logs newsletter unsubscribing from all activity when unsubscription happened on live site.
        /// </summary>
        private static void LogNewsletterUnsubscribingFromAllActivity(object sender, UnsubscriptionEventArgs e)
        {
            var licenseService = ObjectFactory<ILicenseService>.StaticSingleton();
            if (!licenseService.IsFeatureAvailable(FeatureEnum.FullContactManagement))
            {
                return;
            }

            // Log only when unsubscribing from all happened (newsletter is not specified)
            // Also don't log when unsubscription didn't happen on live site
            if (e.Newsletter != null || !Service.Resolve<ISiteService>().IsLiveSite)
            {
                return;
            }

            var contactID = GetContactIDFromEmail(e.Email);
            if (contactID > 0)
            {
                IssueInfo issue = null;
                if (e.IssueID.HasValue)
                {
                    issue = IssueInfoProvider.GetIssueInfo(e.IssueID.Value);
                }

                new NewslettersActivityLogger().LogUnsubscribeFromAllNewslettersActivity(issue, contactID);
            }
        }


        /// <summary>
        /// Logs newsletter unsubscribing activity when unsubscription happened on live site.
        /// </summary>
        private static void LogNewsletterUnsubscribingActivity(object sender, UnsubscriptionEventArgs e)
        {
            var licenseService = ObjectFactory<ILicenseService>.StaticSingleton();
            if (!licenseService.IsFeatureAvailable(FeatureEnum.FullContactManagement))
            {
                return;
            }

            // Don't log when newsletter is null (this means that unsubscribing was done for all newsletters)
            // Also don't log when unsubscription didn't happen on live site
            if ((e.Newsletter == null) || !Service.Resolve<ISiteService>().IsLiveSite)
            {
                return;
            }

            var contactID = GetContactIDFromEmail(e.Email);
            if (contactID > 0)
            {
                var activityLogger = new NewslettersActivityLogger();
                activityLogger.LogUnsubscribeFromSingleNewsletterActivity(e.Newsletter, e.IssueID, contactID);
            }
        }


        /// <summary>
        /// Updates matching information in contact subscriber when e-mail address, firstName, middleName or lastName have changed.
        /// </summary>
        private static void SynchronizeNameAndEmailOfContactSubscriber(object sender, ObjectEventArgs e)
        {
            var changedColumns = e.Object.ChangedColumns();
            if (!changedColumns.Intersect(new[] { "ContactFirstName", "ContactLastName", "ContactMiddleName", "ContactEmail" }).Any())
            {
                return;
            }

            var contact = (ContactInfo)e.Object;

            var newName = new SubscriberFullNameFormater().GetContactSubscriberName(contact.ContactFirstName, contact.ContactMiddleName, contact.ContactLastName);

            SubscriberInfoProvider.SynchronizeSubscriberInfomation(ContactInfo.OBJECT_TYPE, contact.ContactID, newName, contact.ContactFirstName, contact.ContactLastName, contact.ContactEmail);
        }


        /// <summary>
        /// Renames contact group subscriber when name of contact group changes.
        /// </summary>
        private static void RenameContactGroupSubscriber(object sender, ObjectEventArgs e)
        {
            var changedColumns = e.Object.ChangedColumns();
            if (!changedColumns.Contains("ContactGroupDisplayName"))
            {
                return;
            }

            var contactGroup = (ContactGroupInfo)e.Object;

            var newName = new SubscriberFullNameFormater().GetContactGroupSubscriberName(contactGroup.ContactGroupDisplayName);

            SubscriberInfoProvider.SynchronizeSubscriberInfomation(ContactGroupInfo.OBJECT_TYPE, contactGroup.ContactGroupID, newName);
        }


        /// <summary>
        /// Reset email bounces count if e-mail address changed so that contact can receive newsletters even if he had invalid e-mail address before
        /// </summary>
        private static void ResetEmailBounces(object sender, ObjectEventArgs e)
        {
            var changedColumns = e.Object.ChangedColumns();

            if (changedColumns.Contains("ContactEmail"))
            {
                var contact = (ContactInfo)e.Object;
                var contactSubscriber = SubscriberInfoProvider.GetSubscriberInfo("om.contact", contact.ContactID, SiteContext.CurrentSiteID);
                if (contactSubscriber != null)
                {
                    contactSubscriber.SubscriberBounces = 0;
                    SubscriberInfoProvider.SetSubscriberInfo(contactSubscriber);
                }
            }
        }


        /// <summary>
        /// Gets Contact ID from given <paramref name="email"/>.
        /// </summary>
        /// <param name="email">Email of the contact</param>
        /// <returns>Contact ID of found <see cref="ContactInfo"/></returns>
        private static int GetContactIDFromEmail(string email)
        {
            var contactID = ContactInfoProvider.GetContacts()
                                               .WithEmail(email)
                                               .Column("ContactID")
                                               .TopN(1)
                                               .GetScalarResult(0);
            return contactID;
        }
    }
}