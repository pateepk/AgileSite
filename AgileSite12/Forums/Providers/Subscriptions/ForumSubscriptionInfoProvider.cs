using System;
using System.Collections.Generic;
using System.Data;
using System.Web;

using CMS.Activities;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.SiteProvider;
using CMS.WebAnalytics;

namespace CMS.Forums
{
    /// <summary>
    /// Class providing ForumSubscriptionInfo management.
    /// </summary>
    public class ForumSubscriptionInfoProvider : AbstractInfoProvider<ForumSubscriptionInfo, ForumSubscriptionInfoProvider>
    {
        /// <summary>
        /// Returns the ForumSubscriptionInfo structure for the specified forumSubscription.
        /// </summary>
        /// <param name="forumSubscriptionId">ForumSubscription id</param>
        public static ForumSubscriptionInfo GetForumSubscriptionInfo(int forumSubscriptionId)
        {
            ForumSubscriptionInfo forumSubscriptionObj = null;

            if (forumSubscriptionId > 0)
            {
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@Id", forumSubscriptionId);

                DataSet ds = ConnectionHelper.ExecuteQuery("Forums.ForumSubscription.select", parameters);

                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    forumSubscriptionObj = new ForumSubscriptionInfo(ds.Tables[0].Rows[0]);
                }
            }

            return forumSubscriptionObj;
        }


        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static ForumSubscriptionInfo GetForumSubscriptionInfoByGUID(Guid guid)
        {
            string where = "SubscriptionGUID = '" + guid.ToString() + "'";

            DataSet ds = ConnectionHelper.ExecuteQuery("Forums.ForumSubscription.selectall", null, where);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new ForumSubscriptionInfo(ds.Tables[0].Rows[0]);
            }
            return null;
        }


        /// <summary>
        /// Returns the ForumSubscriptionInfo structure for the specified forumSubscritpionGuid.
        /// </summary>
        /// <param name="forumSubscritpionGuid">ForumSubscription GUID</param>
        public static ForumSubscriptionInfo GetForumSubscriptionInfo(Guid forumSubscritpionGuid)
        {
            DataSet ds = ConnectionHelper.ExecuteQuery("Forums.ForumSubscription.selectall", null, "SubscriptionGUID = '" + forumSubscritpionGuid.ToString() + "'");

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new ForumSubscriptionInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Returns the ForumSubscriptionInfo structure for the specified forum subscription hash code.
        /// </summary>
        /// <param name="subscriptionHash">Subscription hash.</param>
        public static ForumSubscriptionInfo GetForumSubscriptionInfo(string subscriptionHash)
        {
            DataSet ds = GetSubscriptions("SubscriptionApprovalHash = '" + SqlHelper.GetSafeQueryString(subscriptionHash) + "'", null, 1, null);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new ForumSubscriptionInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Returns dataset with subscription with dependence on input conditions.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by condition</param>
        /// <param name="topN">TOP N</param>
        /// <param name="columns">Columns</param>
        public static DataSet GetSubscriptions(string where, string orderBy, int topN, string columns)
        {
            return ConnectionHelper.ExecuteQuery("Forums.ForumSubscription.selectall", null, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Sets (updates or inserts) specified forumSubscription.
        /// </summary>
        /// <param name="forumSubscription">ForumSubscription to set</param>
        public static void SetForumSubscriptionInfo(ForumSubscriptionInfo forumSubscription)
        {
            SetForumSubscriptionInfo(forumSubscription, true, null, null);
        }


        /// <summary>
        /// Sets (updates or inserts) specified forumSubscription.
        /// </summary>
        /// <param name="forumSubscription">ForumSubscription to set</param>
        /// <param name="sendConfirmationEmail">If confirmation email should be sent or not</param>
        /// <param name="baseUrl">Forum base URL</param>
        /// <param name="unsubscriptionUrl">Forum un-subscription URL</param>
        public static void SetForumSubscriptionInfo(ForumSubscriptionInfo forumSubscription, bool sendConfirmationEmail, string baseUrl, string unsubscriptionUrl)
        {
            if (forumSubscription != null)
            {
                if (forumSubscription.SubscriptionID > 0)
                {
                    forumSubscription.Generalized.UpdateData();
                }
                else
                {
                    forumSubscription.Generalized.InsertData();

                    // Send confirmation email
                    if (sendConfirmationEmail)
                    {
                        SendConfirmationEmail(forumSubscription, true, baseUrl, unsubscriptionUrl);
                    }


                }
            }
            else
            {
                throw new Exception("[ForumSubscriptionInfoProvider.SetForumSubscriptionInfo]: No ForumSubscriptionInfo object set.");
            }
        }


        /// <summary>
        /// Returns true if current email is already subscribed to the selected forum, post or subpost.
        /// </summary>
        /// <param name="email">Email</param>
        /// <param name="forumId">Forum ID</param>
        /// <param name="postId">Post ID</param>
        public static bool IsSubscribed(string email, int forumId, int postId)
        {
            string where = GetSubscriptionWhereCondition(email, 0, forumId, postId, true);
            where = SqlHelper.AddWhereCondition(where, "(ISNULL(SubscriptionApproved, 1) = 1)");
            return !DataHelper.DataSourceIsEmpty(GetSubscriptions(where, null, 1, "SubscriptionID"));
        }


        /// <summary>
        /// Returns true if current user is already subscribed to the selected forum, post or subpost.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="forumId">Forum ID</param>
        /// <param name="postId">Post ID</param>
        public static bool IsSubscribed(int userId, int forumId, int postId)
        {
            string where = GetSubscriptionWhereCondition(null, userId, forumId, postId, true);
            where = SqlHelper.AddWhereCondition(where, "(ISNULL(SubscriptionApproved, 1) = 1)");
            return !DataHelper.DataSourceIsEmpty(GetSubscriptions(where, null, 1, "SubscriptionID"));
        }


        /// <summary>
        /// Deletes specified forumSubscription.
        /// </summary>
        /// <param name="forumSubscriptionObj">ForumSubscription object</param>
        public static void DeleteForumSubscriptionInfo(ForumSubscriptionInfo forumSubscriptionObj)
        {
            DeleteForumSubscriptionInfo(forumSubscriptionObj, true);
        }


        /// <summary>
        /// Deletes specified forumSubscription.
        /// </summary>
        /// <param name="forumSubscriptionObj">ForumSubscription object</param>
        /// <param name="sendConfirmationEmail">If confirmation email should be sent or not</param>
        public static void DeleteForumSubscriptionInfo(ForumSubscriptionInfo forumSubscriptionObj, bool sendConfirmationEmail)
        {
            if (forumSubscriptionObj != null)
            {
                // Send confirmation email
                if (sendConfirmationEmail && forumSubscriptionObj.SubscriptionApproved)
                {
                    SendConfirmationEmail(forumSubscriptionObj, false, null, null);
                }

                forumSubscriptionObj.Generalized.DeleteData();
            }
        }


        /// <summary>
        /// Deletes specified forumSubscription.
        /// </summary>
        /// <param name="forumSubscriptionId">ForumSubscription id</param>
        public static void DeleteForumSubscriptionInfo(int forumSubscriptionId)
        {
            ForumSubscriptionInfo forumSubscriptionObj = GetForumSubscriptionInfo(forumSubscriptionId);
            DeleteForumSubscriptionInfo(forumSubscriptionObj);
        }


        /// <summary>
        /// Deletes specified forumSubscription.
        /// </summary>
        /// <param name="forumSubscriptionId">ForumSubscription id</param>
        /// <param name="sendConfirmationEmail">If confirmation email should be sent or not</param>
        public static void DeleteForumSubscriptionInfo(int forumSubscriptionId, bool sendConfirmationEmail)
        {
            ForumSubscriptionInfo forumSubscriptionObj = GetForumSubscriptionInfo(forumSubscriptionId);
            DeleteForumSubscriptionInfo(forumSubscriptionObj, sendConfirmationEmail);
        }


        /// <summary>
        /// Sends subscription or unsubscription confirmation mail.
        /// </summary>
        /// <param name="forumSubscription">Forum subscription info</param>
        /// <param name="isSubscription">If true subscription email is sent else unsubscription email is sent</param>
        /// <param name="baseUrl">Forum base URL</param>
        /// <param name="unsubscriptionUrl">Unsubscription URL</param>
        public static void SendConfirmationEmail(ForumSubscriptionInfo forumSubscription, bool isSubscription, string baseUrl, string unsubscriptionUrl)
        {
            SendConfirmationEmail(forumSubscription, isSubscription, false, baseUrl, unsubscriptionUrl);
        }


        /// <summary>
        /// Sends double opt-in mail.
        /// </summary>
        /// <param name="forumSubscription">Forum subscription info</param>
        public static void SendDoubleOptInEmail(ForumSubscriptionInfo forumSubscription)
        {
            SendConfirmationEmail(forumSubscription, false, true, null, null);
        }

        /// <summary>
        /// Sends subscription or unsubscription confirmation mail.
        /// </summary>
        /// <param name="forumSubscription">Forum subscription info</param>
        /// <param name="isSubscription">If true subscription email is sent else unsubscription email is sent</param>
        /// <param name="isOptIn">Indicates if opt-in email should be sent</param>
        /// <param name="baseUrl">Forum base URL</param>
        /// <param name="unsubscriptionUrl">Unsubscription URL</param>
        private static void SendConfirmationEmail(ForumSubscriptionInfo forumSubscription, bool isSubscription, bool isOptIn, string baseUrl, string unsubscriptionUrl)
        {
            // Check whether context exists
            if ((CMSHttpContext.Current == null) || (forumSubscription == null))
            {
                return;
            }

            // Determine if its post or forum subscription 
            bool isPost = (forumSubscription.SubscriptionPostID != 0);

            // Get forum 
            ForumInfo fi = ForumInfoProvider.GetForumInfo(forumSubscription.SubscriptionForumID);
            if (fi == null)
            {
                return;
            }

            // Get forum post
            ForumPostInfo fpi = null;
            if (isPost)
            {
                fpi = ForumPostInfoProvider.GetForumPostInfo(forumSubscription.SubscriptionPostID);
                if (fpi == null)
                {
                    return;
                }
            }

            // Get site
            ForumGroupInfo fgi = ForumGroupInfoProvider.GetForumGroupInfo(fi.ForumGroupID);
            if (fgi == null)
            {
                return;
            }

            SiteInfo si = SiteInfoProvider.GetSiteInfo(fgi.GroupSiteID);
            if (si == null)
            {
                return;
            }

            string emailLink;

            if (!String.IsNullOrEmpty(baseUrl))
            {
                emailLink = URLHelper.ResolveUrl(baseUrl);
            }
            else
            {
                emailLink = URLHelper.ResolveUrl(DataHelper.GetNotEmpty(fi.ForumBaseUrl, RequestContext.CurrentURL));
            }

            // Remove query string if is present
            if (!String.IsNullOrEmpty(emailLink))
            {
                emailLink = URLHelper.RemoveQuery(emailLink);

                // Make forum link absolute with domain
                emailLink = URLHelper.GetAbsoluteUrl(emailLink, RequestContext.FullDomain, URLHelper.GetFullApplicationUrl(), URLHelper.GetAbsoluteUrl(RequestContext.CurrentURL));
            }

            // For Ad-Hoc forum set base URL by document URL
            if (fi.ForumDocumentID > 0)
            {
                TreeProvider tp = new TreeProvider();
                TreeNode tn = tp.SelectSingleDocument(fi.ForumDocumentID);
                if (tn != null)
                {
                    string url = DocumentURLProvider.GetUrl(tn);
                    if (!String.IsNullOrEmpty(url))
                    {
                        emailLink = URLHelper.GetAbsoluteUrl(url, RequestContext.FullDomain, URLHelper.GetFullApplicationUrl(), URLHelper.GetAbsoluteUrl(RequestContext.CurrentURL));
                    }
                }
            }

            int thrId = 0;
            if (isPost)
            {
                thrId = ForumPostInfoProvider.GetPostRootFromIDPath(fpi.PostIDPath);
            }

            // Sets full url with query string for post and unsubscription
            emailLink = MacroResolver.Resolve(emailLink + "?forumid=" + fi.ForumID);

            if (isPost)
            {
                emailLink += "&threadid=" + thrId;
            }

            // Prepare the macro resolver and enable HTML encoding
            MacroResolver resolver = MacroContext.CurrentResolver;
            resolver.Settings.EncodeResolvedValues = true;
            resolver.SetNamedSourceData(new Dictionary<string, object>
            {
                { "forumdisplayname", fi.ForumDisplayName },
                { "subject", isPost ? fpi.PostSubject : "" },
                { "link", emailLink },
                { "unsubscribelink", isSubscription ? GetUnsubscriptionUrl(forumSubscription, fi, fpi, unsubscriptionUrl) : "" },
                { "subscribelink", isOptIn ? GetActivationUrl(forumSubscription, fi, fpi) : "" },
                { "separator", isPost ? ResHelper.GetAPIString("forums.confirmationtemplateseparator", " - ") : "" },
                { "optininterval", isOptIn ? ForumGroupInfoProvider.DoubleOptInInterval(si.SiteName).ToString() : "" }
            }, isPrioritized: false);

            resolver.SetNamedSourceData("Forum", fi);
            if (fpi != null)
            {
                resolver.SetNamedSourceData("ForumPost", fpi);
            }
            resolver.SetNamedSourceData("ForumGroup", fgi);

            // Get email template
            EmailTemplateInfo template;
            if (isOptIn)
            {
                template = EmailTemplateProvider.GetEmailTemplate("Forums.SubscriptionRequest", si.SiteName);
            }
            else if (isSubscription)
            {
                template = EmailTemplateProvider.GetEmailTemplate("Forums.SubscribeConfirmation", si.SiteName);
            }
            else
            {
                template = EmailTemplateProvider.GetEmailTemplate("Forums.UnsubscribeConfirmation", si.SiteName);
            }

            if (template == null)
            {
                return;
            }

            string from = EmailHelper.GetSender(template, DataHelper.GetNotEmpty(SettingsKeyInfoProvider.GetValue(si.SiteName + ".CMSSendForumEmailsFrom"), String.Empty));

            if (!String.IsNullOrEmpty(from))
            {
                // Send message
                EmailMessage message = new EmailMessage();
                message.EmailFormat = EmailFormatEnum.Default;
                message.From = from;
                message.Recipients = forumSubscription.SubscriptionEmail;

                if (message.Recipients != null && message.Recipients.Trim() != "")
                {
                    EmailSender.SendEmailWithTemplateText(si.SiteName, message, template, resolver, false);
                }
            }
            else
            {
                EventLogProvider.LogEvent(EventType.ERROR, "Forums", "EmailSenderNotSpecified");
            }
        }


        /// <summary>
        /// Subscribe method intend to subscribe user to forum/forum post and send confirmation/opt-in email 
        /// </summary>
        /// <param name="fsi">Subscription to be saved</param>
        /// <param name="when">Time of subscription</param>
        /// <param name="sendConfirmationEmail">Indicates if confirmation email should be send</param>
        /// <param name="sendOptInEmail">Indicates if opt-in email should be send</param>
        public static void Subscribe(ForumSubscriptionInfo fsi, DateTime when, bool sendConfirmationEmail, bool sendOptInEmail)
        {
            ForumInfo fi = ForumInfoProvider.GetForumInfo(fsi.SubscriptionForumID);
            if (fi != null)
            {
                // Remove old not approved subscriptions
                string where = GetSubscriptionWhereCondition(fsi.SubscriptionEmail, fsi.SubscriptionUserID, fsi.SubscriptionForumID, fsi.SubscriptionPostID, !(fsi.SubscriptionPostID > 0));
                where = SqlHelper.AddWhereCondition(where, "(SubscriptionApproved = 0)");
                DataSet dsSubs = GetSubscriptions(where, null, -1, null);
                if (!DataHelper.DataSourceIsEmpty(dsSubs))
                {
                    foreach (DataRow dr in dsSubs.Tables[0].Rows)
                    {
                        ForumSubscriptionInfo fsub = new ForumSubscriptionInfo(dr);
                        DeleteForumSubscriptionInfo(fsub, false);
                    }
                }

                // Check if sending double opt-in e-mail is depending on forum settings or sending double opt-in is required
                bool approved = !(sendOptInEmail && fi.ForumEnableOptIn);

                fsi.SubscriptionApproved = approved;
                SetForumSubscriptionInfo(fsi, false, null, null);

                // Send double opt-in if not approved
                if (!approved)
                {
                    // Send activation link
                    SendDoubleOptInEmail(fsi);
                }
                // Send forum confirmation e-mail
                else if (sendConfirmationEmail)
                {
                    SendConfirmationEmail(fsi, true, null, null);
                }
            }
        }


        /// <summary>
        /// Validates request hash and checks if request was approved in needed interval.  
        /// </summary>
        /// <param name="fsi">Forum subscription info.</param>
        /// <param name="requestHash">Hash parameter representing specific subscription</param>
        /// <param name="datetime">Date and time of request.</param>
        /// <param name="siteName">Site name.</param>
        public static OptInApprovalResultEnum ValidateHash(ForumSubscriptionInfo fsi, string requestHash, string siteName, DateTime datetime)
        {
            if (fsi == null)
            {
                return OptInApprovalResultEnum.NotFound;
            }

            DateTime now = DateTime.Now;
            TimeSpan span = now.Subtract(datetime);

            // Get interval from settings
            double interval = ForumGroupInfoProvider.DoubleOptInInterval(siteName);

            // Check interval
            if ((interval > 0) && (span.TotalHours > interval))
            {
                return OptInApprovalResultEnum.TimeExceeded;
            }

            // Get forum and forum post
            ForumInfo fi = ForumInfoProvider.GetForumInfo(fsi.SubscriptionForumID);
            ForumPostInfo fpi = ForumPostInfoProvider.GetForumPostInfo(fsi.SubscriptionPostID);

            if (fi != null)
            {
                // Validate hash
                if (!SecurityHelper.ValidateConfirmationEmailHash(requestHash, fi.ForumGUID + "|" + (fpi?.PostGUID.ToString() ?? Guid.Empty.ToString()) + "|" + fsi.SubscriptionEmail, datetime))
                {
                    return OptInApprovalResultEnum.Failed;
                }
            }
            else
            {
                return OptInApprovalResultEnum.Failed;
            }

            return OptInApprovalResultEnum.Success;
        }



        /// <summary>
        /// Returns unsubscription URL which is used to cancel existing subscription
        /// </summary>
        /// <param name="subscription">Forum subscription object</param>
        /// <param name="forum">Forum object</param>
        /// <param name="forumPost">Forum post to which user was subscribed</param>
        /// <param name="unsubscriptionUrl">Custom unsubscription page URL</param>
        public static string GetUnsubscriptionUrl(ForumSubscriptionInfo subscription, ForumInfo forum, ForumPostInfo forumPost, string unsubscriptionUrl)
        {
            string unsubscribeLink;
            if (!String.IsNullOrEmpty(unsubscriptionUrl))
            {
                unsubscribeLink = unsubscriptionUrl;
            }
            else
            {
                unsubscribeLink = "~/CMSModules/Forums/CMSPages/unsubscribe.aspx";

                // Checks whether forum unsubscription URL is defined
                if (!String.IsNullOrEmpty(forum.ForumUnsubscriptionUrl))
                {
                    unsubscribeLink = DataHelper.GetNotEmpty(forum.ForumUnsubscriptionUrl, RequestContext.CurrentURL);
                }
            }

            // Sets absolute unsubscribe link
            unsubscribeLink = URLHelper.ResolveUrl(unsubscribeLink);
            unsubscribeLink = URLHelper.GetAbsoluteUrl(unsubscribeLink, RequestContext.FullDomain, URLHelper.GetFullApplicationUrl(), URLHelper.GetAbsoluteUrl(RequestContext.CurrentURL));

            // Add parameters to unsubscribe URL
            if ((subscription != null) && (forum != null))
            {
                if (!forum.ForumEnableOptIn)
                {
                    unsubscribeLink = string.Format("{0}?forumsubguid={1}&forumid={2}",
                                      unsubscribeLink,
                                      subscription.SubscriptionGUID,
                                      subscription.SubscriptionForumID);
                }
                else
                {
                    // Generate subscription hash and save it to the database
                    DateTime time = DateTime.Now;

                    string hash = SecurityHelper.GenerateConfirmationEmailHash(forum.ForumGUID.ToString() + "|" + ((forumPost != null) ? forumPost.PostGUID.ToString() : Guid.Empty.ToString()) + "|" + subscription.SubscriptionEmail, time);
                    subscription.SubscriptionApprovalHash = hash;
                    SetForumSubscriptionInfo(subscription, false, null, null);
                    unsubscribeLink = string.Format("{0}?forumsubscriptionhash={1}&datetime={2}",
                                      unsubscribeLink,
                                      subscription.SubscriptionApprovalHash,
                                      DateTimeUrlFormatter.Format(time));
                }
            }

            return MacroResolver.Resolve(unsubscribeLink);
        }


        /// <summary>
        /// Returns approval page URL to confirm forum subscription.
        /// </summary>
        /// <param name="subscription">Forum subscription object</param>
        /// <param name="forum">Forum object</param>
        /// <param name="forumPost">Forum post to which user was subscribed</param>
        private static string GetActivationUrl(ForumSubscriptionInfo subscription, ForumInfo forum, ForumPostInfo forumPost)
        {
            // Get activation page from newsletter or fall back to page defined in settings
            string activationPage;
            if (!string.IsNullOrEmpty(forum.ForumOptInApprovalURL))
            {
                activationPage = forum.ForumOptInApprovalURL;
            }
            else
            {
                activationPage = "~/CMSModules/Forums/CMSPages/SubscriptionApproval.aspx";
            }

            // Process link from path selector
            if (activationPage.StartsWith("/", StringComparison.Ordinal))
            {
                activationPage = DocumentURLProvider.GetUrl(activationPage);
            }

            // Sets absolute unsubscribe link
            activationPage = URLHelper.ResolveUrl(activationPage);
            activationPage = URLHelper.GetAbsoluteUrl(activationPage, RequestContext.FullDomain, URLHelper.GetFullApplicationUrl(), URLHelper.GetAbsoluteUrl(RequestContext.CurrentURL));

            // Generate subscription hash and save it to the database
            DateTime time = DateTime.Now;
            string hash = SecurityHelper.GenerateConfirmationEmailHash(forum.ForumGUID + "|" + (forumPost?.PostGUID.ToString() ?? Guid.Empty.ToString()) + "|" + subscription.SubscriptionEmail, time);
            subscription.SubscriptionApprovalHash = hash;
            SetForumSubscriptionInfo(subscription, false, null, null);

            string url = string.Format("{0}?forumsubscriptionhash={1}&datetime={2}&cid={3}&siteid={4}&url={5}&docid={6}&camp={7}",
                                        activationPage,
                                        subscription.SubscriptionApprovalHash,
                                        DateTimeUrlFormatter.Format(time),
                                        ModuleCommands.OnlineMarketingGetCurrentContactID(),
                                        SiteContext.CurrentSiteID,
                                        HttpUtility.UrlEncode(RequestContext.CurrentRelativePath),
                                        (DocumentContext.CurrentDocument != null) ? DocumentContext.CurrentDocument.DocumentID : 0,
                                        Service.Resolve<ICampaignService>().CampaignCode);

            return URLHelper.AddParameterToUrl(url, "hash", QueryHelper.GetHash(url, false));
        }


        /// <summary>
        /// Approves existing subscription - sets SubscriptionApproved to true and SubscriptionApprovedWhen to current time. 
        /// Checks if subscription wasn't already approved. Confirmation e-mail may be sent optionally.
        /// </summary>
        /// <param name="fsi">Forum subscription object</param>
        /// <param name="subscriptionHash">Hash parameter representing specific subscription</param>
        /// <param name="sendConfirmationEmail">Indicates if confirmation e-mail should be sent. Confirmation e-mail may also be sent if newsletter settings requires so</param>
        /// <param name="datetime">Date and time of request.</param>
        /// <param name="siteName">Site name.</param>
        /// <returns>Returns TRUE if subscription found and not already approved. Returns FALSE if subscription not found or already approved.</returns>
        public static OptInApprovalResultEnum ApproveSubscription(ForumSubscriptionInfo fsi, string subscriptionHash, bool sendConfirmationEmail, string siteName, DateTime datetime)
        {
            // Get subscription info
            if (fsi == null)
            {
                fsi = GetForumSubscriptionInfo(subscriptionHash);
            }
            // Validate hash 
            OptInApprovalResultEnum result = ValidateHash(fsi, subscriptionHash, siteName, datetime);

            // If hash validation succeeded then approve subscription
            if (result == OptInApprovalResultEnum.Success)
            {
                // Check if subscription is not already approved
                if (fsi.SubscriptionApproved)
                {
                    return OptInApprovalResultEnum.NotFound;
                }

                // Set subscription to be approved
                fsi.SubscriptionApproved = true;
                SetForumSubscriptionInfo(fsi, false, null, null);

                // Get forum
                ForumInfo forum = ForumInfoProvider.GetForumInfo(fsi.SubscriptionForumID);

                // Check if forum requires sending confirmation message
                if (sendConfirmationEmail || ((forum != null) && (forum.ForumSendOptInConfirmation)))
                {
                    SendConfirmationEmail(fsi, true, null, null);
                }
            }
            else if (result == OptInApprovalResultEnum.TimeExceeded)
            {
                DeleteForumSubscriptionInfo(fsi);
            }

            return result;
        }


        /// <summary>
        /// Unsubscribes user from forum or forum post.
        /// </summary>
        /// <param name="subscriptionHash">Subscription hash.</param>
        /// <param name="sendConfirmationEmail">Indicates whether send confirmation e-mail</param>
        /// <param name="datetime">Date and time of request</param>
        /// <param name="siteName">Site name.</param>
        public static OptInApprovalResultEnum Unsubscribe(string subscriptionHash, bool sendConfirmationEmail, string siteName, DateTime datetime)
        {
            OptInApprovalResultEnum result = OptInApprovalResultEnum.NotFound;

            // Get subscription info
            ForumSubscriptionInfo fsi = GetForumSubscriptionInfo(subscriptionHash);

            if (fsi != null)
            {
                // Validate hash 
                result = ValidateHash(fsi, subscriptionHash, siteName, datetime);

                // Unsubscribe if hash validation was successful
                if (result == OptInApprovalResultEnum.Success)
                {
                    DeleteForumSubscriptionInfo(fsi, sendConfirmationEmail);
                }
            }

            return result;
        }


        /// <summary>
        /// Logs activity using context values.
        /// </summary>
        /// <param name="fsi">Forum subscription</param>
        /// <param name="fi">Forum object</param>
        /// <param name="activityType">Activity type to log</param>
        /// <param name="checkViewMode">Indicates if view mode should be checked</param>
        public static void LogSubscriptionActivity(ForumSubscriptionInfo fsi, ForumInfo fi, string activityType, bool checkViewMode)
        {
            LogSubscriptionActivity(fsi, fi, ModuleCommands.OnlineMarketingGetCurrentContactID(), SiteContext.CurrentSiteID, RequestContext.CurrentRelativePath, (DocumentContext.CurrentDocument != null) ? DocumentContext.CurrentDocument.DocumentID : 0, Service.Resolve<ICampaignService>().CampaignCode, checkViewMode);
        }


        /// <summary>
        /// Logs activity.
        /// </summary>
        /// <param name="fsi">Forum subscription</param>
        /// <param name="fi">Forum object</param>
        /// <param name="contactId">Contact ID</param>
        /// <param name="campaign">Campaign</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="documentId">Document ID</param>
        /// <param name="url">URL</param>
        /// <param name="checkViewMode">Indicates if view mode should be checked</param>
        public static void LogSubscriptionActivity(ForumSubscriptionInfo fsi, ForumInfo fi, int contactId, int siteId, string url, int documentId, string campaign, bool checkViewMode)
        {
            if ((fsi != null) && (fi == null))
            {
                fi = ForumInfoProvider.GetForumInfo(fsi.SubscriptionForumID);
            }

            if ((fi == null) || (fsi == null) || !fi.ForumLogActivity)
            {
                return;
            }

            TreeNode currentDocument = DocumentHelper.GetDocument(documentId, null);

            var forumSubscriptionInitializer = new ForumSubscriptionActivityInitializer(fi, fsi, currentDocument)
                .WithContactId(contactId)
                .WithSiteId(siteId)
                .WithUrl(url)
                .WithCampaign(campaign);

            Service.Resolve<IActivityLogService>().Log(forumSubscriptionInitializer, CMSHttpContext.Current.Request, checkViewMode);
        }


        /// <summary>
        /// Returns where condition to get the selected forum, post or subpost.
        /// </summary>
        /// <param name="email">Email</param>
        /// <param name="userId">User ID</param>
        /// <param name="forumId">Forum ID</param>
        /// <param name="postId">Post ID</param>
        /// <param name="forceForumCondition">Indicates if also forum condition should be generated</param>
        private static string GetSubscriptionWhereCondition(string email, int userId, int forumId, int postId, bool forceForumCondition)
        {
            if ((!String.IsNullOrEmpty(email) || (userId > 0)) && (forumId > 0))
            {
                string where;

                // Get user condition
                if (userId > 0)
                {
                    where = "SubscriptionUserID = " + userId;
                }
                else
                {
                    where = "SubscriptionEmail = N'" + SqlHelper.GetSafeQueryString(email, false) + "'";
                }

                string forumWhere = null;

                if (forceForumCondition)
                {
                    forumWhere = "SubscriptionForumID = " + forumId + " AND SubscriptionPostID IS NULL";
                }

                // Add forum post condition if subscription is related to post
                if (postId > 0)
                {
                    ForumPostInfo fpi = ForumPostInfoProvider.GetForumPostInfo(postId);
                    if (fpi != null)
                    {
                        forumWhere = SqlHelper.AddWhereCondition(forumWhere, "SubscriptionForumID = " + forumId + " AND EXISTS((SELECT SubscriptionPostID FROM Forums_ForumSubscription WHERE SubscriptionForumID = " + fpi.PostForumID + " AND SubscriptionPostID IN ( SELECT PostID FROM [Forums_ForumPost] WHERE PostForumID = " + fpi.PostForumID + " AND '" + fpi.PostIDPath + "' LIKE PostIDPath + '%') AND " + where + "))", "OR");
                    }
                }
                return SqlHelper.AddWhereCondition(forumWhere, where);
            }
            return "1=0";
        }
    }
}