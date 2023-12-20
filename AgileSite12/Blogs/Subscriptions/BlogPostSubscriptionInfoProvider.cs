using System;
using System.Data;
using System.Linq;
using System.Web;

using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EmailEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.SiteProvider;
using CMS.WebAnalytics;

namespace CMS.Blogs
{
    /// <summary>
    /// Class providing BlogPostSubscriptionInfo management.
    /// </summary>
    public class BlogPostSubscriptionInfoProvider : AbstractInfoProvider<BlogPostSubscriptionInfo, BlogPostSubscriptionInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the BlogPostSubscriptionInfo structure for the specified blogPostSubscriptionId.
        /// </summary>
        /// <param name="blogPostSubscriptionId">Blog post subscription ID</param>
        public static BlogPostSubscriptionInfo GetBlogPostSubscriptionInfo(int blogPostSubscriptionId)
        {
            return ProviderObject.GetInfoById(blogPostSubscriptionId);
        }


        /// <summary>
        /// Returns the BlogPostSubscriptionInfo structure for the subscriber specified by its GUID.
        /// </summary>
        /// <param name="guid">Subscriber GUID</param>
        public static BlogPostSubscriptionInfo GetBlogPostSubscriptionInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Gets the query for all blog post subscriptions.
        /// </summary>
        public static ObjectQuery<BlogPostSubscriptionInfo> GetBlogPostSubscriptions()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns blog post subscription info for specified email and blog post.
        /// </summary>
        /// <param name="email">Subscriber e-mail</param>
        /// <param name="documentId">Blog post document ID</param>
        public static BlogPostSubscriptionInfo GetBlogPostSubscriptionInfo(string email, int documentId)
        {
            if (String.IsNullOrEmpty(email) || (documentId <= 0))
            {
                return null;
            }

            return GetBlogPostSubscriptions().WhereEqualsOrNull("SubscriptionApproved", true)
                                             .WhereEquals("SubscriptionEmail", email)
                                             .WhereEquals("SubscriptionPostDocumentID", documentId)
                                             .FirstOrDefault();
        }


        /// <summary>
        /// Returns blog post subscription info for specified subscription hash.
        /// </summary>
        /// <param name="subscriptionHash">Subscription hash</param>
        public static BlogPostSubscriptionInfo GetBlogPostSubscriptionInfo(string subscriptionHash)
        {
            return GetBlogPostSubscriptions().WhereEquals("SubscriptionApprovalHash", subscriptionHash)
                                             .FirstOrDefault();
        }


        /// <summary>
        /// Returns all blog post subscriptions for specified user which were created on specified site. Blog post document name is attached for each subscription.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="where">WHERE condition</param>
        /// <param name="topN">TOP N query parameter</param>
        public static DataSet GetBlogPostSubscriptions(int userId, int siteId, string where, int topN)
        {
            int totalRecords = 0;
            return GetBlogPostSubscriptions(userId, siteId, where, topN, null, null, 0, 0, ref totalRecords);
        }


        /// <summary>
        /// Returns all blog post subscriptions for specified user which were created on specified site. Blog post document name is attached for each subscription.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="where">WHERE condition</param>
        /// <param name="topN">TOP N query parameter</param>
        /// <param name="columns">Columns expression</param>
        /// <param name="order">Order by</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total records</param>
        public static DataSet GetBlogPostSubscriptions(int userId, int siteId, string where, int topN, string order, string columns, int offset, int maxRecords, ref int totalRecords)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@UserID", userId);
            parameters.Add("@SiteID", siteId);

            return ConnectionHelper.ExecuteQuery("blog.postsubscription.selectforuser", parameters, where, order, topN, columns, offset, maxRecords, ref totalRecords);
        }


        /// <summary>
        /// Returns all subscriptions info with specified parameters.
        /// </summary>
        /// <param name="where">WHERE condition</param>
        /// <param name="orderby">ORDER BY</param>
        /// <param name="columns">Selected columns</param>
        public static DataSet GetBlogPostSubscriptions(string where, string orderby, string columns = null)
        {
            return GetBlogPostSubscriptions()
                            .Columns(columns)
                            .Where(where)
                            .OrderBy(orderby);
        }


        /// <summary>
        /// Sets (updates or inserts) specified blogPostSubscription.
        /// </summary>
        /// <param name="blogPostSubscription">BlogPostSubscription to set</param>
        public static void SetBlogPostSubscriptionInfo(BlogPostSubscriptionInfo blogPostSubscription)
        {
            ProviderObject.SetInfo(blogPostSubscription);
        }


        /// <summary>
        /// Deletes specified blog post subscription.
        /// </summary>
        /// <param name="infoObj">BlogPostSubscription object</param>
        public static void DeleteBlogPostSubscriptionInfo(BlogPostSubscriptionInfo infoObj)
        {
            if (infoObj != null)
            {
                ProviderObject.DeleteInfo(infoObj);
            }
        }


        /// <summary>
        /// Deletes specified blogPostSubscription.
        /// </summary>
        /// <param name="blogPostSubscriptionId">BlogPostSubscription id</param>
        public static void DeleteBlogPostSubscriptionInfo(int blogPostSubscriptionId)
        {
            BlogPostSubscriptionInfo infoObj = GetBlogPostSubscriptionInfo(blogPostSubscriptionId);
            DeleteBlogPostSubscriptionInfo(infoObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Sends subscription or unsubscription confirmation mail.
        /// </summary>
        /// <param name="blogSubscription">Blog post subscription info</param>
        /// <param name="isSubscription">If true subscription email is sent else unsubscription email is sent</param>
        public static void SendConfirmationEmail(BlogPostSubscriptionInfo blogSubscription, bool isSubscription)
        {
            ProviderObject.SendConfirmationEmailInternal(blogSubscription, isSubscription, false);
        }


        /// <summary>
        /// Sends double opt-in mail.
        /// </summary>
        /// <param name="blogSubscription">Blog post subscription info</param>
        public static void SendDoubleOptInEmail(BlogPostSubscriptionInfo blogSubscription)
        {
            ProviderObject.SendConfirmationEmailInternal(blogSubscription, false, true);
        }


        /// <summary>
        /// Subscribe method intend to subscribe user to blog post and send confirmation/opt-in email 
        /// </summary>
        /// <param name="bpsi">Subscription to be saved</param>
        /// <param name="when">Time of subscription</param>
        /// <param name="sendConfirmationEmail">Indicates if confirmation email should be send</param>
        /// <param name="sendOptInEmail">Indicates if opt-in email should be send</param>
        public static void Subscribe(BlogPostSubscriptionInfo bpsi, DateTime when, bool sendConfirmationEmail, bool sendOptInEmail)
        {
            ProviderObject.SubscribeInternal(bpsi, when, sendConfirmationEmail, sendOptInEmail);
        }


        /// <summary>
        /// Validates request hash and checks if request was approved in needed interval.  
        /// </summary>
        /// <param name="bpsi">Blog post subscription info.</param>
        /// <param name="requestHash">Hash parameter representing specific subscription</param>
        /// <param name="datetime">Date and time of request.</param>
        /// <param name="siteName">Site name.</param>
        public static OptInApprovalResultEnum ValidateHash(BlogPostSubscriptionInfo bpsi, string requestHash, string siteName, DateTime datetime)
        {
            return ProviderObject.ValidateHashInternal(bpsi, requestHash, siteName, datetime);
        }


        /// <summary>
        /// Returns unsubscription URL which is used to cancel existing subscription
        /// </summary>
        /// <param name="subscription">Blog post subscription object</param>
        /// <param name="blog">Blog document</param>
        /// <param name="blogPost">Blog post to which user was subscribed</param>
        /// <param name="unsubscriptionUrl">Custom unsubscription page URL</param>
        public static string GetUnsubscriptionUrl(BlogPostSubscriptionInfo subscription, TreeNode blog, TreeNode blogPost, string unsubscriptionUrl)
        {
            return ProviderObject.GetUnsubscriptionUrlInternal(subscription, blog, blogPost, unsubscriptionUrl);
        }


        /// <summary>
        /// Returns approval page URL to confirm blog post subscription.
        /// </summary>
        /// <param name="subscription">Forum subscription object</param>
        /// <param name="blog">Blog document</param>
        /// <param name="blogPost">Blog post to which user was subscribed</param>
        private static string GetActivationUrl(BlogPostSubscriptionInfo subscription, TreeNode blog, TreeNode blogPost)
        {
            return ProviderObject.GetActivationUrlInternal(subscription, blog, blogPost);
        }


        /// <summary>
        /// Approves existing subscription - sets SubscriptionApproved to true. 
        /// Checks if subscription wasn't already approved. Confirmation e-mail may be sent optionally.
        /// </summary>
        /// <param name="bpsi">Blog post subscription</param>
        /// <param name="subscriptionHash">Hash parameter representing specific subscription</param>
        /// <param name="sendConfirmationEmail">Indicates if confirmation e-mail should be sent. Confirmation e-mail may also be sent if blog settings requires so</param>
        /// <param name="datetime">Date and time of request.</param>
        /// <param name="siteName">Site name.</param>
        /// <returns>Returns TRUE if subscription found and not already approved. Returns FALSE if subscription not found or already approved.</returns>
        public static OptInApprovalResultEnum ApproveSubscription(BlogPostSubscriptionInfo bpsi, string subscriptionHash, bool sendConfirmationEmail, string siteName, DateTime datetime)
        {
            return ProviderObject.ApproveSubscriptionInternal(bpsi, subscriptionHash, sendConfirmationEmail, siteName, datetime);
        }


        /// <summary>
        /// Unsubscribes user from blog post.
        /// </summary>
        /// <param name="subscriptionHash">Subscription hash.</param>
        /// <param name="sendConfirmationEmail">Indicates whether send confirmation e-mail</param>
        /// <param name="datetime">Date and time of request</param>
        /// <param name="siteName">Site name.</param>
        public static OptInApprovalResultEnum Unsubscribe(string subscriptionHash, bool sendConfirmationEmail, string siteName, DateTime datetime)
        {
            return ProviderObject.UnsubscribeInternal(subscriptionHash, sendConfirmationEmail, siteName, datetime);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(BlogPostSubscriptionInfo info)
        {
            if (info != null)
            {
                base.DeleteInfo(info);
            }
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Sends subscription or unsubscription confirmation mail.
        /// </summary>
        /// <param name="blogSubscription">Blog post subscription info</param>
        /// <param name="isSubscription">If true subscription email is sent else unsubscription email is sent</param>
        /// <param name="isOptIn">Indicates if opt-in email should be sent</param>
        protected virtual void SendConfirmationEmailInternal(BlogPostSubscriptionInfo blogSubscription, bool isSubscription, bool isOptIn)
        {
            // Check whether context exists
            if ((CMSHttpContext.Current == null) || (blogSubscription == null))
            {
                return;
            }

            // Get blog post info
            TreeProvider tp = new TreeProvider();
            TreeNode blogPost = tp.SelectSingleDocument(blogSubscription.SubscriptionPostDocumentID);
            if (blogPost == null)
            {
                return;
            }

            // Get blog
            TreeNode blog = BlogHelper.GetParentBlog(blogSubscription.SubscriptionPostDocumentID, true);
            if (blog == null)
            {
                return;
            }

            SiteInfo si = SiteInfoProvider.GetSiteInfo(blogPost.NodeSiteID);
            if (si == null)
            {
                return;
            }

            var siteName = si.SiteName;

            var resolver = CreateMacroResolver(blogSubscription, blog, blogPost, siteName, isSubscription, isOptIn);

            // Get email template
            EmailTemplateInfo template;
            if (isOptIn)
            {
                template = EmailTemplateProvider.GetEmailTemplate("Blogs.SubscriptionRequest", siteName);
            }
            else if (isSubscription)
            {
                template = EmailTemplateProvider.GetEmailTemplate("Blogs.SubscribeConfirmation", siteName);
            }
            else
            {
                template = EmailTemplateProvider.GetEmailTemplate("Blogs.UnsubscribeConfirmation", siteName);
            }

            if (template == null)
            {
                return;
            }

            // Send message
            EmailMessage message = new EmailMessage();
            message.EmailFormat = EmailFormatEnum.Default;
            message.From = DataHelper.GetNotEmpty(SettingsKeyInfoProvider.GetValue(si.SiteName + ".CMSSendBlogEmailsFrom"), EmailHelper.DEFAULT_EMAIL_SENDER);
            message.Recipients = blogSubscription.SubscriptionEmail;

            if (message.Recipients != null && message.Recipients.Trim() != "")
            {
                EmailSender.SendEmailWithTemplateText(si.SiteName, message, template, resolver, false);
            }
        }


        /// <summary>
        /// Subscribe method intend to subscribe user to blog post and send confirmation/opt-in email 
        /// </summary>
        /// <param name="bpsi">Subscription to be saved</param>
        /// <param name="when">Time of subscription</param>
        /// <param name="sendConfirmationEmail">Indicates if confirmation email should be send</param>
        /// <param name="sendOptInEmail">Indicates if opt-in email should be send</param>
        protected virtual void SubscribeInternal(BlogPostSubscriptionInfo bpsi, DateTime when, bool sendConfirmationEmail, bool sendOptInEmail)
        {
            TreeNode blog = BlogHelper.GetParentBlog(bpsi.SubscriptionPostDocumentID, true);
            if (blog != null)
            {
                // Delete older not approved subscription
                BlogPostSubscriptionInfo bsub = GetBlogPostSubscriptionInfo(bpsi.SubscriptionEmail, bpsi.SubscriptionPostDocumentID);
                DeleteBlogPostSubscriptionInfo(bsub);

                // Check if sending double opt-in e-mail is required
                bool optInRequired = sendOptInEmail && BlogHelper.BlogOptInEnabled(blog);

                bpsi.SubscriptionApproved = !optInRequired;
                SetBlogPostSubscriptionInfo(bpsi);

                // Send double opt-in activation email if required
                if (optInRequired)
                {
                    // Send activation link
                    SendDoubleOptInEmail(bpsi);
                }
                // Send confirmation e-mail
                else if (sendConfirmationEmail)
                {
                    SendConfirmationEmail(bpsi, true);
                }
            }
        }


        /// <summary>
        /// Validates request hash and checks if request was approved in needed interval.  
        /// </summary>
        /// <param name="bpsi">Blog post subscription info.</param>
        /// <param name="requestHash">Hash parameter representing specific subscription</param>
        /// <param name="datetime">Date and time of request.</param>
        /// <param name="siteName">Site name.</param>
        protected virtual OptInApprovalResultEnum ValidateHashInternal(BlogPostSubscriptionInfo bpsi, string requestHash, string siteName, DateTime datetime)
        {
            if (bpsi == null)
            {
                return OptInApprovalResultEnum.NotFound;
            }

            DateTime now = DateTime.Now;
            TimeSpan span = now.Subtract(datetime);

            // Get interval from settings
            double interval = BlogHelper.GetBlogDoubleOptInInterval(siteName);

            // Check interval
            if ((interval > 0) && (span.TotalHours > interval))
            {
                return OptInApprovalResultEnum.TimeExceeded;
            }

            // Get blog post
            TreeProvider tp = new TreeProvider();
            TreeNode blogPost = tp.SelectSingleDocument(bpsi.SubscriptionPostDocumentID);

            if (blogPost != null)
            {
                // Validate hash
                if (!SecurityHelper.ValidateConfirmationEmailHash(requestHash, blogPost.NodeGUID + "|" + bpsi.SubscriptionEmail, datetime))
                {
                    return OptInApprovalResultEnum.Failed;
                }
            }

            return OptInApprovalResultEnum.Success;
        }


        /// <summary>
        /// Returns unsubscription URL which is used to cancel existing subscription
        /// </summary>
        /// <param name="subscription">Blog post subscription object</param>
        /// <param name="blog">Blog document</param>
        /// <param name="blogPost">Blog post to which user was subscribed</param>
        /// <param name="unsubscriptionUrl">Custom unsubscription page URL</param>
        protected virtual string GetUnsubscriptionUrlInternal(BlogPostSubscriptionInfo subscription, TreeNode blog, TreeNode blogPost, string unsubscriptionUrl)
        {
            string unsubscribeLink;
            if (!String.IsNullOrEmpty(unsubscriptionUrl))
            {
                unsubscribeLink = unsubscriptionUrl;
            }
            else
            {
                unsubscribeLink = ValidationHelper.GetString(blog.GetValue("BlogUnsubscriptionUrl"), string.Empty);
                if (unsubscribeLink == string.Empty)
                {
                    unsubscribeLink = DataHelper.GetNotEmpty(SettingsKeyInfoProvider.GetValue(blog.NodeSiteName + ".CMSBlogsUnsubscriptionUrl"), "~/CMSModules/Blogs/CMSPages/Unsubscribe.aspx");
                }
            }

            // Sets absolute unsubscribe link
            unsubscribeLink = URLHelper.ResolveUrl(unsubscribeLink);
            unsubscribeLink = URLHelper.GetAbsoluteUrl(unsubscribeLink, RequestContext.FullDomain, URLHelper.GetFullApplicationUrl(), URLHelper.GetAbsoluteUrl(RequestContext.CurrentURL));

            // Add parameters to unsubscribe URL
            if ((subscription != null) && (blogPost != null))
            {
                if (!BlogHelper.BlogOptInEnabled(blog))
                {
                    unsubscribeLink = string.Format("{0}?blogsubguid={1}&postdocid={2}",
                                      unsubscribeLink,
                                      subscription.SubscriptionGUID,
                                      subscription.SubscriptionPostDocumentID);
                }
                else
                {
                    // Generate subscription hash and save it to the database
                    DateTime time = DateTime.Now;

                    string hash = SecurityHelper.GenerateConfirmationEmailHash(blogPost.NodeGUID + "|" + subscription.SubscriptionEmail, time);
                    subscription.SubscriptionApprovalHash = hash;
                    SetBlogPostSubscriptionInfo(subscription);

                    unsubscribeLink = string.Format("{0}?blogsubscriptionhash={1}&datetime={2}",
                                      unsubscribeLink,
                                      subscription.SubscriptionApprovalHash,
                                      DateTimeUrlFormatter.Format(time));
                }
            }

            return MacroResolver.Resolve(unsubscribeLink);
        }


        /// <summary>
        /// Returns approval page URL to confirm blog post subscription.
        /// </summary>
        /// <param name="subscription">Forum subscription object</param>
        /// <param name="blog">Blog document</param>
        /// <param name="blogPost">Blog post to which user was subscribed</param>
        protected virtual string GetActivationUrlInternal(BlogPostSubscriptionInfo subscription, TreeNode blog, TreeNode blogPost)
        {
            // Get activation page from blog or fall back to page defined in settings
            string activationPage = ValidationHelper.GetString(blog.GetValue("BlogOptInApprovalURL"), string.Empty);
            if (activationPage == string.Empty)
            {
                activationPage = DataHelper.GetNotEmpty(SettingsKeyInfoProvider.GetValue(blog.NodeSiteName + ".CMSBlogOptInApprovalPath"), "~/CMSModules/Blogs/CMSPages/SubscriptionApproval.aspx");
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
            string hash = SecurityHelper.GenerateConfirmationEmailHash(blogPost.NodeGUID + "|" + subscription.SubscriptionEmail, time);
            subscription.SubscriptionApprovalHash = hash;
            SetBlogPostSubscriptionInfo(subscription);

            string url = string.Format("{0}?blogsubscriptionhash={1}&datetime={2}&cid={3}&siteid={4}&url={5}&camp={6}",
                                        activationPage,
                                        subscription.SubscriptionApprovalHash,
                                        DateTimeUrlFormatter.Format(time),
                                        ModuleCommands.OnlineMarketingGetCurrentContactID(),
                                        SiteContext.CurrentSiteID,
                                        HttpUtility.UrlEncode(RequestContext.CurrentRelativePath),
                                        Service.Resolve<ICampaignService>().CampaignCode);

            return URLHelper.AddParameterToUrl(url, "hash", QueryHelper.GetHash(url, false));
        }


        /// <summary>
        /// Approves existing subscription - sets SubscriptionApproved to true. 
        /// Checks if subscription wasn't already approved. Confirmation e-mail may be sent optionally.
        /// </summary>
        /// <param name="bpsi">Blog post subscription</param>
        /// <param name="subscriptionHash">Hash parameter representing specific subscription</param>
        /// <param name="sendConfirmationEmail">Indicates if confirmation e-mail should be sent. Confirmation e-mail may also be sent if blog settings requires so</param>
        /// <param name="datetime">Date and time of request.</param>
        /// <param name="siteName">Site name.</param>
        /// <returns>Returns TRUE if subscription found and not already approved. Returns FALSE if subscription not found or already approved.</returns>
        protected virtual OptInApprovalResultEnum ApproveSubscriptionInternal(BlogPostSubscriptionInfo bpsi, string subscriptionHash, bool sendConfirmationEmail, string siteName, DateTime datetime)
        {
            // Get subscription info
            if (bpsi == null)
            {
                bpsi = GetBlogPostSubscriptionInfo(subscriptionHash);
            }

            // Validate hash 
            OptInApprovalResultEnum result = ValidateHash(bpsi, subscriptionHash, siteName, datetime);

            // If hash validation succeeded then approve subscription
            if (result == OptInApprovalResultEnum.Success)
            {
                // Check if subscription is not already approved
                if (bpsi.SubscriptionApproved)
                {
                    return OptInApprovalResultEnum.NotFound;
                }

                // Set subscription to be approved
                bpsi.SubscriptionApproved = true;
                SetBlogPostSubscriptionInfo(bpsi);

                // Get blog
                TreeNode blog = BlogHelper.GetParentBlog(bpsi.SubscriptionPostDocumentID, true);
                bool blogOptInConf = false;
                if (blog != null)
                {
                    int optInConf = blog.GetValue("BlogSendOptInConfirmation", -1);
                    if (optInConf < 0)
                    {
                        blogOptInConf = SettingsKeyInfoProvider.GetBoolValue(blog.NodeSiteName + ".CMSBlogEnableOptInConfirmation");
                    }
                    else
                    {
                        blogOptInConf = optInConf > 0;
                    }
                }

                // Check if blog requires sending confirmation message
                if (sendConfirmationEmail || blogOptInConf)
                {
                    SendConfirmationEmail(bpsi, true);
                }
            }
            else if (result == OptInApprovalResultEnum.TimeExceeded)
            {
                DeleteBlogPostSubscriptionInfo(bpsi);
            }

            return result;
        }


        /// <summary>
        /// Unsubscribes user from blog post.
        /// </summary>
        /// <param name="subscriptionHash">Subscription hash.</param>
        /// <param name="sendConfirmationEmail">Indicates whether send confirmation e-mail</param>
        /// <param name="datetime">Date and time of request</param>
        /// <param name="siteName">Site name.</param>
        protected virtual OptInApprovalResultEnum UnsubscribeInternal(string subscriptionHash, bool sendConfirmationEmail, string siteName, DateTime datetime)
        {
            OptInApprovalResultEnum result = OptInApprovalResultEnum.NotFound;

            // Get subscription info
            BlogPostSubscriptionInfo bpsi = GetBlogPostSubscriptionInfo(subscriptionHash);

            if (bpsi != null)
            {
                // Validate hash 
                result = ValidateHash(bpsi, subscriptionHash, siteName, datetime);

                // Unsubscribe if hash validation was successful
                if (result == OptInApprovalResultEnum.Success)
                {
                    if (sendConfirmationEmail)
                    {
                        SendConfirmationEmail(bpsi, false);
                    }
                    DeleteBlogPostSubscriptionInfo(bpsi);
                }
            }

            return result;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Creates and initializes macro resolver.
        /// </summary>
        /// <param name="blogSubscription">Blog post subscription object</param>
        /// <param name="blog">Blog tree node</param>
        /// <param name="blogPost">Blog post tree node</param>
        /// <param name="siteName">Site name of the blog post</param>
        /// <param name="isSubscription">Indicates if resolver is created for subscription</param>
        /// <param name="isOptIn">Indicates if resolver is created for double opt-in</param>
        private static MacroResolver CreateMacroResolver(BlogPostSubscriptionInfo blogSubscription, TreeNode blog, TreeNode blogPost, string siteName, bool isSubscription, bool isOptIn)
        {
            var resolver = BlogHelper.CreateMacroResolver(blog, blogPost);
            resolver.Settings.EncodeResolvedValues = true;

            resolver.SetNamedSourceData("SubscriptionLink", isOptIn ? GetActivationUrl(blogSubscription, blog, blogPost) : string.Empty);
            resolver.SetNamedSourceData("OptInInterval", isOptIn ? BlogHelper.GetBlogDoubleOptInInterval(siteName).ToString() : string.Empty);

            // Macro for unsubscription link - keep this macro at the end !!!
            resolver.SetNamedSourceData("UnsubscriptionLink", (isOptIn || isSubscription) ? GetUnsubscriptionUrl(blogSubscription, blog, blogPost, null) : string.Empty);

            return resolver;
        }

        #endregion
    }
}