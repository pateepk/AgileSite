using System;
using System.Collections.Generic;
using System.Data;
using System.Web;

using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;
using CMS.DocumentEngine;
using CMS.MacroEngine;
using CMS.DataEngine;

namespace CMS.Forums
{
    /// <summary>
    /// Sends forum e-mail messages in a separate thread.
    /// </summary>
    public class ThreadEmailSender
    {
        #region "Variables"

        private readonly DataSet mRecipients;

        private readonly SiteInfo mSiteInfo;

        private readonly string mSubject = string.Empty;

        private readonly bool mModeratorEmail;

        private readonly ForumPostInfo mForumPostInfo;

        private readonly ForumInfo mForumInfo;

        private readonly ForumGroupInfo mForumGroupInfo;

        private readonly string mUnsubscriptionUrl;

        private readonly string mBaseUrl;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadEmailSender"/> class.
        /// </summary>
        /// <param name="postId">The post ID</param>
        /// <param name="moderatorEmail">If set to <c>true</c>, email is sent to moderator</param>
        public ThreadEmailSender(int postId, bool moderatorEmail)
            : this(postId, moderatorEmail, null, null)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadEmailSender"/> class.
        /// </summary>
        /// <param name="postId">The post ID</param>
        /// <param name="moderatorEmail">If set to <c>true</c>, email is sent to moderator</param>
        /// <param name="baseUrl">Default base URL</param>
        /// <param name="unsubscriptionUrl">Default unsubscription URL</param>
        public ThreadEmailSender(int postId, bool moderatorEmail, string baseUrl, string unsubscriptionUrl)
        {
            if (HttpContext.Current == null)
            {
                return;
            }

            // Get the post data
            mForumPostInfo = ForumPostInfoProvider.GetForumPostInfo(postId);
            if (mForumPostInfo == null)
            {
                return;
            }

            // Get the forum data
            mForumInfo = ForumInfoProvider.GetForumInfo(mForumPostInfo.PostForumID);
            if (mForumInfo == null)
            {
                return;
            }

            // If forum post is approved do nothing
            if ((moderatorEmail && mForumPostInfo.PostApproved) || (!moderatorEmail && !mForumPostInfo.PostApproved))
            {
                return;
            }

            // Get the forum group data
            mForumGroupInfo = ForumGroupInfoProvider.GetForumGroupInfo(mForumInfo.ForumGroupID);
            if (mForumGroupInfo == null)
            {
                return;
            }

            mModeratorEmail = moderatorEmail;

            // Set subject value
            mSubject = ResHelper.GetAPIString("Forum.EmailPostSubject", "New forum post: {%postsubject%}");

            mBaseUrl = baseUrl;
            mUnsubscriptionUrl = ForumSubscriptionInfoProvider.GetUnsubscriptionUrl(null, mForumInfo, mForumPostInfo, unsubscriptionUrl);

            mSiteInfo = SiteInfoProvider.GetSiteInfo(mForumGroupInfo.GroupSiteID);

            // Get the subscriptions
            if (mSiteInfo != null)
            {
                if (!moderatorEmail)
                {
                    string[] fpiArray = mForumPostInfo.PostIDPath.Split('/');

                    string where = string.Format("((SubscriptionApproved = 1) OR (SubscriptionApproved IS NULL)) AND (SubscriptionForumID = {0}) AND ((SubscriptionPostID IS NULL) OR (SubscriptionPostID IN (", mForumInfo.ForumID);
                    bool first = false;

                    foreach (string post in fpiArray)
                    {
                        if (!string.IsNullOrEmpty(post))
                        {
                            if (first)
                            {
                                where += ",";
                            }
                            where += post.TrimStart('0');
                            first = true;
                        }
                    }
                    where += ")))";

                    mRecipients = ForumSubscriptionInfoProvider.GetSubscriptions(where, null, 0, null);
                }
                else
                {
                    mRecipients = ForumInfoProvider.GetModerators(mForumInfo.ForumID);
                }

                // Start the sending thread
                CMSThread asyncEmailSend = new CMSThread(Run);
                asyncEmailSend.Start();
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Runs the thread.
        /// </summary>
        public void Run()
        {
            try
            {
                if (!DataHelper.DataSourceIsEmpty(mRecipients))
                {
                    // Prepare the macro resolver
                    MacroResolver resolver = MacroContext.CurrentResolver;

                    var data = new object[4];
                    data[0] = mForumPostInfo;
                    data[1] = mForumInfo;
                    data[2] = mForumGroupInfo;

                    resolver.SetAnonymousSourceData(data);

                    resolver.SetNamedSourceData("ForumPost", mForumPostInfo);
                    resolver.SetNamedSourceData("Forum", mForumInfo);
                    resolver.SetNamedSourceData("ForumGroup", mForumGroupInfo);

                    int thrId = ForumPostInfoProvider.GetPostRootFromIDPath(mForumPostInfo.PostIDPath);

                    resolver.SetNamedSourceData(new Dictionary<string, object>
                    {
                        { "forumdisplayname", mForumInfo.ForumDisplayName },
                        { "postsubject", mForumPostInfo.PostSubject },
                        { "link", MacroResolver.Resolve(string.Format("{0}?forumid={1}&threadid={2}", GetMailUrl(mForumInfo, mBaseUrl), mForumInfo.ForumID, thrId)) },
                        { "forumname", mForumInfo.ForumName },
                        { "posttext", PreparePostText(mForumInfo, mForumPostInfo) },
                        { "postusername", mForumPostInfo.PostUserName },
                        { "posttime", mForumPostInfo.PostTime.ToString() },
                        { "groupdisplayname", mForumGroupInfo.GroupDisplayName },
                        { "groupname", mForumGroupInfo.GroupName },
                        { "groupdescription", mForumGroupInfo.GroupDescription },
                        { "forumdescription", mForumInfo.ForumDescription },
                        { "posttextplain", DiscussionMacroResolver.RemoveTags(HTMLHelper.StripTags(mForumPostInfo.PostText)) }
                    }, isPrioritized: false);

                    var emails = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

                    // Send to all subscribers
                    foreach (DataRow dr in mRecipients.Tables[0].Rows)
                    {
                        try
                        {
                            ForumSubscriptionInfo forumSubscriptionInfo = new ForumSubscriptionInfo(dr);
                            if ((forumSubscriptionInfo != null) || mModeratorEmail)
                            {
                                resolver.SetNamedSourceData("Subscriber", forumSubscriptionInfo);
                                data[3] = forumSubscriptionInfo;

                                var unsubscriptionUrl = mUnsubscriptionUrl;

                                if (!mModeratorEmail)
                                {
                                    // Check whether email to the current email was already sent
                                    if (!emails.Add(forumSubscriptionInfo.SubscriptionEmail))
                                    {
                                        continue;
                                    }

                                    ForumPostInfo forumPostInfo = forumSubscriptionInfo.SubscriptionPostID > 0 ? ForumPostInfoProvider.GetForumPostInfo(forumSubscriptionInfo.SubscriptionPostID) : null;
                                    unsubscriptionUrl = URLHelper.RemoveQuery(unsubscriptionUrl) + URLHelper.GetQuery(ForumSubscriptionInfoProvider.GetUnsubscriptionUrl(forumSubscriptionInfo, mForumInfo, forumPostInfo, unsubscriptionUrl));
                                }

                                resolver.SetNamedSourceData("unsubscribelink", unsubscriptionUrl, false);

                                // Select e-mail template depend on type of post insert
                                EmailTemplateInfo emailTemplate;
                                if (mModeratorEmail)
                                {
                                    emailTemplate = EmailTemplateProvider.GetEmailTemplate("Forums.ModeratorNotice", mSiteInfo.SiteName);
                                }
                                else
                                {
                                    emailTemplate = EmailTemplateProvider.GetEmailTemplate("Forums.NewPost", mSiteInfo.SiteName);
                                }

                                if (emailTemplate != null)
                                {
                                    string from = EmailHelper.GetSender(emailTemplate, DataHelper.GetNotEmpty(SettingsKeyInfoProvider.GetValue(mSiteInfo.SiteName + ".CMSSendForumEmailsFrom"), String.Empty));

                                    if (!String.IsNullOrEmpty(from))
                                    {
                                        EmailMessage message = new EmailMessage
                                        {
                                            EmailFormat = EmailFormatEnum.Default,
                                            From = from,
                                            Subject = mSubject
                                        };

                                        if (mModeratorEmail)
                                        {
                                            message.Recipients = ValidationHelper.GetString(dr["Email"], string.Empty);
                                        }
                                        else
                                        {
                                            message.Recipients = ValidationHelper.GetString(dr["SubscriptionEmail"], string.Empty);
                                        }

                                        resolver.Settings.AvoidInjection = true;

                                        if (message.Recipients != null && message.Recipients.Trim() != string.Empty)
                                        {
                                            EmailSender.SendEmailWithTemplateText(mSiteInfo.SiteName, message, emailTemplate, resolver, false);
                                        }
                                    }
                                    else
                                    {
                                        EventLogProvider.LogEvent(EventType.ERROR, "Forums", "EmailSenderNotSpecified");
                                    }
                                }
                                else
                                {
                                    EventLogProvider.LogEvent(EventType.ERROR, "Forums", "GetEmailTemplate", eventUrl: RequestContext.RawURL);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            EventLogProvider.LogException("Forums", "FORUMEMAIL", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("Forums", "FORUMEMAIL", ex);
            }
        }


        private static string GetMailUrl(ForumInfo forum, string baseUrl)
        {
            // For Ad-Hoc forum set base url by document url
            if (forum.ForumDocumentID > 0)
            {
                TreeProvider treeProvider = new TreeProvider();
                TreeNode treeNode = treeProvider.SelectSingleDocument(forum.ForumDocumentID);
                if (treeNode != null)
                {
                    string url = DocumentURLProvider.GetUrl(treeNode);
                    if (!String.IsNullOrEmpty(url))
                    {
                        return URLHelper.GetAbsoluteUrl(url, RequestContext.FullDomain, URLHelper.GetFullApplicationUrl(), URLHelper.GetAbsoluteUrl(RequestContext.CurrentURL));
                    }
                }
            }

            return GetForumUrl(forum, baseUrl);
        }


        private static string GetForumUrl(ForumInfo forum, string baseUrl)
        {
            string mailUrl;
            if (!string.IsNullOrEmpty(baseUrl))
            {
                // Set forum url from webpart properties if specified
                mailUrl = URLHelper.ResolveUrl(baseUrl);
            }
            else
            {
                // Set forum url from forum base url setting
                mailUrl = URLHelper.ResolveUrl(DataHelper.GetNotEmpty(forum.ForumBaseUrl, RequestContext.CurrentURL));
            }

            // Remove query string if is present
            if (!string.IsNullOrEmpty(mailUrl))
            {
                mailUrl = URLHelper.RemoveQuery(mailUrl);

                // Make forum link absolute with domain
                mailUrl = URLHelper.GetAbsoluteUrl(mailUrl, RequestContext.FullDomain, URLHelper.GetFullApplicationUrl(), URLHelper.GetAbsoluteUrl(RequestContext.CurrentURL));
            }

            return mailUrl;
        }


        private static string PreparePostText(ForumInfo forumInfo, ForumPostInfo forumPostInfo)
        {
            string postText = forumPostInfo.PostText;
            if (!forumInfo.ForumHTMLEditor)
            {
                postText = postText.Replace("\r\n", " ");
            }
            return DiscussionMacroResolver.RemoveTags(postText);
        }

        #endregion
    }
}