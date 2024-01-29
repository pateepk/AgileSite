using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

using CMS.Activities;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.DocumentEngine;
using CMS.SiteProvider;
using CMS.EmailEngine;
using CMS.WebAnalytics;
using CMS.MacroEngine;
using CMS.Core;

namespace CMS.MessageBoards
{
    /// <summary>
    /// Class providing BoardSubscriptionInfo management.
    /// </summary>
    public class BoardSubscriptionInfoProvider : AbstractInfoProvider<BoardSubscriptionInfo, BoardSubscriptionInfoProvider>
    {
        #region "Methods"

        /// <summary>
        /// Gets all the subscriptions for specified user and site.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="topN">Return top N records</param>
        public static DataSet GetSubscriptions(int userId, int siteId, int topN = 0)
        {
            // Prepare the  parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@UserID", userId);

            return ConnectionHelper.ExecuteQuery("board.subscription.selectforuser", parameters, "BoardSiteID = " + siteId, null, topN);
        }


        /// <summary>
        /// Gets all the subscriptions according specified conditions.
        /// </summary>
        /// <param name="where">Where condition to filter data</param>
        /// <param name="orderBy">Order by statement to use</param>        
        /// <param name="columns">Columns</param>
        public static DataSet GetSubscriptions(string where, string orderBy, string columns = null)
        {
            return GetSubscriptions()
                        .Columns(columns)
                        .Where(where)
                        .OrderBy(orderBy);
        }


        /// <summary>
        /// Returns board subscription object query.
        /// </summary>
        public static ObjectQuery<BoardSubscriptionInfo> GetSubscriptions()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the BoardSubscriptionInfo structure for the specified board id an user id.
        /// </summary>
        /// <param name="boardId">Board id</param>
        /// <param name="userId">User id</param>
        public static BoardSubscriptionInfo GetBoardSubscriptionInfo(int boardId, int userId)
        {
            return GetSubscriptions()
                .TopN(1)
                .WhereEquals("SubscriptionBoardID", boardId)
                .WhereEquals("SubscriptionUserID", userId)
                .FirstOrDefault();
        }


        /// <summary>
        /// Returns the BoardSubscriptionInfo structure for the specified boardSubscription.
        /// </summary>
        /// <param name="boardSubscriptionId">BoardSubscription id</param>
        public static BoardSubscriptionInfo GetBoardSubscriptionInfo(int boardSubscriptionId)
        {
            return ProviderObject.GetInfoById(boardSubscriptionId);
        }


        /// <summary>
        /// Returns the BoardSubscriptionInfo structure for the specified message board subscription hash code.
        /// </summary>
        /// <param name="subscriptionHash">Subscription hash.</param>
        public static BoardSubscriptionInfo GetBoardSubscriptionInfo(string subscriptionHash)
        {
            return GetSubscriptions()
                        .TopN(1)
                        .WhereEquals("SubscriptionApprovalHash", subscriptionHash)
                        .FirstOrDefault();
        }


        /// <summary>
        /// Returns the BoardSubscriptionInfo structure.
        /// </summary>
        /// <param name="subscriptionGuid">Board subscription GUID</param>
        public static BoardSubscriptionInfo GetBoardSubscriptionInfo(Guid subscriptionGuid)
        {
            return ProviderObject.GetInfoByGuid(subscriptionGuid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified boardSubscription.
        /// </summary>
        /// <param name="boardSubscription">BoardSubscription to set</param>
        public static void SetBoardSubscriptionInfo(BoardSubscriptionInfo boardSubscription)
        {
            if (boardSubscription == null)
            {
                throw new ArgumentNullException(nameof(boardSubscription));
            }

            if (boardSubscription.SubscriptionID > 0)
            {
                boardSubscription.Generalized.UpdateData();
            }
            else
            {
                boardSubscription.Generalized.InsertData();
            }
        }


        /// <summary>
        /// Deletes specified boardSubscription.
        /// </summary>
        /// <param name="infoObj">BoardSubscription object</param>
        public static void DeleteBoardSubscriptionInfo(BoardSubscriptionInfo infoObj)
        {
            infoObj?.Generalized.DeleteData();
        }


        /// <summary>
        /// Deletes specified board subscription.
        /// </summary>
        /// <param name="subscriptionId">Board subscription ID</param>
        public static void DeleteBoardSubscriptionInfo(int subscriptionId)
        {
            BoardSubscriptionInfo infoObj = GetBoardSubscriptionInfo(subscriptionId);
            DeleteBoardSubscriptionInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified board subscription.
        /// </summary>
        /// <param name="subscriptionGuid">Board subscription GUID</param>
        public static void DeleteBoardSubscriptionInfo(Guid subscriptionGuid)
        {
            BoardSubscriptionInfo infoObj = GetBoardSubscriptionInfo(0, subscriptionGuid);
            DeleteBoardSubscriptionInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified board subscription.
        /// </summary>
        /// <param name="boardId">Board ID</param>
        /// <param name="subscriptionGuid">Board subscription GUID</param>
        public static void DeleteBoardSubscriptionInfo(int boardId, Guid subscriptionGuid)
        {
            BoardSubscriptionInfo infoObj = GetBoardSubscriptionInfo(boardId, subscriptionGuid);
            DeleteBoardSubscriptionInfo(infoObj);
        }


        /// <summary>
        /// Sends subscription or unsubscription confirmation mail.
        /// </summary>
        /// <param name="boardSubscription">Board subscription info</param>
        /// <param name="isSubscription">If true subscription email is sent else unsubscription email is sent</param>
        public static void SendConfirmationEmail(BoardSubscriptionInfo boardSubscription, bool isSubscription)
        {
            SendConfirmationEmail(boardSubscription, isSubscription, false);
        }


        /// <summary>
        /// Sends double opt-in mail.
        /// </summary>
        /// <param name="boardSubscription">Board subscription info</param>
        public static void SendDoubleOptInEmail(BoardSubscriptionInfo boardSubscription)
        {
            SendConfirmationEmail(boardSubscription, false, true);
        }


        /// <summary>
        /// Subscribe method intend to subscribe user to message board and send confirmation/opt-in email 
        /// </summary>
        /// <param name="bsi">Subscription to be saved</param>
        /// <param name="when">Time of subscription</param>
        /// <param name="sendConfirmationEmail">Indicates if confirmation email should be send</param>
        /// <param name="sendOptInEmail">Indicates if opt-in email should be send</param>
        public static void Subscribe(BoardSubscriptionInfo bsi, DateTime when, bool sendConfirmationEmail, bool sendOptInEmail)
        {
            if (bsi == null)
            {
                return;
            }

            BoardInfo board = BoardInfoProvider.GetBoardInfo(bsi.SubscriptionBoardID);
            if (board == null)
            {
                return;
            }

            // Delete older not approved subscription
            DataSet dsBoards = GetSubscriptions("(SubscriptionApproved = 0) AND (SubscriptionBoardID=" + bsi.SubscriptionBoardID + ") AND (SubscriptionEmail=N'" + SqlHelper.GetSafeQueryString(bsi.SubscriptionEmail, false) + "')", null);
            if (!DataHelper.DataSourceIsEmpty(dsBoards))
            {
                foreach (DataRow dr in dsBoards.Tables[0].Rows)
                {
                    BoardSubscriptionInfo bsub = new BoardSubscriptionInfo(dr);
                    DeleteBoardSubscriptionInfo(bsub);
                }
            }

            // Check if sending double opt-in e-mail is required
            bool approved = !(sendOptInEmail && board.BoardEnableOptIn);
                    
            bsi.SubscriptionApproved = approved;
            SetBoardSubscriptionInfo(bsi);

            // Send double opt-in if not approved
            if (!approved)
            {
                // Send activation link
                SendDoubleOptInEmail(bsi);
            }
                // Send message board confirmation e-mail
            else if (sendConfirmationEmail)
            {
                SendConfirmationEmail(bsi, true);
            }
        }


        /// <summary>
        /// Validates request hash and checks if request was approved in needed interval.  
        /// </summary>
        /// <param name="bsi">Board subscription info.</param>
        /// <param name="requestHash">Hash parameter representing specific subscription</param>
        /// <param name="datetime">Date and time of request.</param>
        /// <param name="siteName">Site name.</param>
        public static OptInApprovalResultEnum ValidateHash(BoardSubscriptionInfo bsi, string requestHash, string siteName, DateTime datetime)
        {
            if (bsi == null)
            {
                return OptInApprovalResultEnum.NotFound;
            }

            DateTime now = DateTime.Now;
            TimeSpan span = now.Subtract(datetime);

            // Get interval from settings
            double interval = BoardInfoProvider.DoubleOptInInterval(siteName);

            // Check interval
            if ((interval > 0) && (span.TotalHours > interval))
            {
                return OptInApprovalResultEnum.TimeExceeded;
            }

            // Get message board
            BoardInfo bi = BoardInfoProvider.GetBoardInfo(bsi.SubscriptionBoardID);

            if (bi != null)
            {
                // Validate hash
                if (!SecurityHelper.ValidateConfirmationEmailHash(requestHash, bi.BoardGUID + "|" + bsi.SubscriptionEmail, datetime))
                {
                    return OptInApprovalResultEnum.Failed;
                }
            }

            return OptInApprovalResultEnum.Success;
        }



        /// <summary>
        /// Returns unsubscription URL which is used to cancel existing subscription
        /// </summary>
        /// <param name="subscription">Message board subscription object</param>
        /// <param name="bi">Message board object</param>
        /// <param name="unsubscriptionUrl">Custom unsubscription page URL</param>
        public static string GetUnsubscriptionUrl(BoardSubscriptionInfo subscription, BoardInfo bi, string unsubscriptionUrl)
        {
            string unsubscribeLink;
            if (!String.IsNullOrEmpty(unsubscriptionUrl))
            {
                unsubscribeLink = unsubscriptionUrl;
            }
            else
            {
                unsubscribeLink = DataHelper.GetNotEmpty((bi != null) ? bi.BoardUnsubscriptionURL : null, "~/CMSModules/MessageBoards/CMSPages/Unsubscribe.aspx");
            }

            // Sets absolute unsubscribe link
            unsubscribeLink = URLHelper.ResolveUrl(unsubscribeLink);
            unsubscribeLink = URLHelper.GetAbsoluteUrl(unsubscribeLink, RequestContext.FullDomain, URLHelper.GetFullApplicationUrl(), URLHelper.GetAbsoluteUrl(RequestContext.CurrentURL));

            if (subscription == null)
            {
                return MacroResolver.Resolve(unsubscribeLink);
            }

            // Add parameters to unsubscribe URL
            if (bi == null || !bi.BoardEnableOptIn)
            {
                unsubscribeLink = string.Format("{0}?boardsubguid={1}&boardid={2}",
                    unsubscribeLink,
                    subscription.SubscriptionGUID,
                    subscription.SubscriptionBoardID);
            }
            else
            {
                // Generate subscription hash and save it to the database
                DateTime time = DateTime.Now;

                string hash = SecurityHelper.GenerateConfirmationEmailHash(bi.BoardGUID + "|" + subscription.SubscriptionEmail, time);
                subscription.SubscriptionApprovalHash = hash;
                SetBoardSubscriptionInfo(subscription);

                unsubscribeLink = string.Format("{0}?boardsubscriptionhash={1}&datetime={2}",
                    unsubscribeLink,
                    subscription.SubscriptionApprovalHash,
                    DateTimeUrlFormatter.Format(time));
            }

            return MacroResolver.Resolve(unsubscribeLink);
        }


        /// <summary>
        /// Approves existing subscription - sets SubscriptionApproved to true. 
        /// Checks if subscription wasn't already approved. Confirmation e-mail may be sent optionally.
        /// </summary>
        /// <param name="bsi">Subscription object</param>
        /// <param name="subscriptionHash">Hash parameter representing specific subscription</param>
        /// <param name="sendConfirmationEmail">Indicates if confirmation e-mail should be sent. Confirmation e-mail may also be sent if blog settings requires so</param>
        /// <param name="datetime">Date and time of request.</param>
        /// <param name="siteName">Site name.</param>
        /// <returns>Returns TRUE if subscription found and not already approved. Returns FALSE if subscription not found or already approved.</returns>
        public static OptInApprovalResultEnum ApproveSubscription(BoardSubscriptionInfo bsi, string subscriptionHash, bool sendConfirmationEmail, string siteName, DateTime datetime)
        {
            // Get subscription info
            if (bsi == null)
            {
                bsi = GetBoardSubscriptionInfo(subscriptionHash);
            }

            // Validate hash 
            OptInApprovalResultEnum result = ValidateHash(bsi, subscriptionHash, siteName, datetime);

            // If hash validation succeeded then approve subscription
            if (result == OptInApprovalResultEnum.Success)
            {
                // Check if subscription is not already approved
                if (bsi.SubscriptionApproved)
                {
                    return OptInApprovalResultEnum.NotFound;
                }

                // Set subscription to be approved
                bsi.SubscriptionApproved = true;
                SetBoardSubscriptionInfo(bsi);

                // Get board object
                BoardInfo bi = BoardInfoProvider.GetBoardInfo(bsi.SubscriptionBoardID);
                bool boardOptInConf = (bi != null) && bi.BoardSendOptInConfirmation;

                // Check if message board requires sending confirmation message
                if (sendConfirmationEmail || boardOptInConf)
                {
                    SendConfirmationEmail(bsi, true);
                }
            }
            else if (result == OptInApprovalResultEnum.TimeExceeded)
            {
                DeleteBoardSubscriptionInfo(bsi);
            }

            return result;
        }


        /// <summary>
        /// Unsubscribes user from message board.
        /// </summary>
        /// <param name="bsi">Board subscription object.</param>
        /// <param name="sendConfirmationEmail">Indicates whether send confirmation e-mail</param>
        public static OptInApprovalResultEnum Unsubscribe(BoardSubscriptionInfo bsi, bool sendConfirmationEmail)
        {
            if (bsi == null)
            {
                return OptInApprovalResultEnum.NotFound;
            }

            if (sendConfirmationEmail)
            {
                SendConfirmationEmail(bsi, false);
            }

            DeleteBoardSubscriptionInfo(bsi);

            return OptInApprovalResultEnum.Success;
        }

        /// <summary>
        /// Unsubscribes user from message board.
        /// </summary>
        /// <param name="subscriptionHash">Subscription hash.</param>
        /// <param name="sendConfirmationEmail">Indicates whether send confirmation e-mail</param>
        /// <param name="datetime">Date and time of request</param>
        /// <param name="siteName">Site name.</param>
        public static OptInApprovalResultEnum Unsubscribe(string subscriptionHash, bool sendConfirmationEmail, string siteName, DateTime datetime)
        {
            // Get subscription info
            BoardSubscriptionInfo bsi = GetBoardSubscriptionInfo(subscriptionHash);

            if (bsi == null)
            {
                return OptInApprovalResultEnum.NotFound;
            }

            // Validate hash 
            var result = ValidateHash(bsi, subscriptionHash, siteName, datetime);

            // Unsubscribe if hash validation was successful
            if (result == OptInApprovalResultEnum.Success)
            {
                Unsubscribe(bsi, sendConfirmationEmail);
            }

            return result;
        }


        /// <summary>
        /// Log activity (subscribing/unsubscribing) using context values.
        /// </summary>
        /// <param name="bsi">Board subscription object</param>
        /// <param name="bi">Board object</param>
        /// <param name="activityType">Activity type to log</param>
        /// <param name="checkViewMode">Indicates if view mode should be checked</param>
        public static void LogSubscriptionActivity(BoardSubscriptionInfo bsi, BoardInfo bi, string activityType, bool checkViewMode)
        {
            LogSubscriptionActivity(bsi, bi, ModuleCommands.OnlineMarketingGetCurrentContactID(), SiteContext.CurrentSiteID, RequestContext.CurrentRelativePath, (DocumentContext.CurrentDocument != null) ? DocumentContext.CurrentDocument.DocumentID : 0, Service.Resolve<ICampaignService>().CampaignCode, activityType, checkViewMode);
        }


        /// <summary>
        /// Log activity (subscribing/unsubscribing)
        /// </summary>
        /// <param name="bsi">Board subscription object</param>
        /// <param name="bi">Board object</param>
        /// <param name="contactId">Contact ID</param>
        /// <param name="campaign">Campaign</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="documentId">Document ID</param>
        /// <param name="url">URL</param>
        /// <param name="activityType">Activity type to log</param>
        /// <param name="checkViewMode">Indicates if view mode should be checked</param>
        public static void LogSubscriptionActivity(BoardSubscriptionInfo bsi, BoardInfo bi, int contactId, int siteId, string url, int documentId, string campaign, string activityType, bool checkViewMode)
        {
            if ((bsi != null) && (bi == null))
            {
                bi = BoardInfoProvider.GetBoardInfo(bsi.SubscriptionBoardID);
            }

            var currentDoc = DocumentHelper.GetDocument(documentId, null);
            
            if (bi.BoardLogActivity && bsi != null && currentDoc != null)
            {
                var activityInitializer = new MessageBoardSubscriptionActivityInitializer(bi, currentDoc, bsi.SubscriptionID)
                    .WithSiteId(siteId)
                    .WithContactId(contactId)
                    .WithCampaign(campaign)
                    .WithUrl(url);

                Service.Resolve<IActivityLogService>().Log(activityInitializer, CMSHttpContext.Current.Request, checkViewMode);
            }
        }

        #endregion


        #region "Private methods"
        
        /// <summary>
        /// Returns the BoardSubscriptionInfo structure.
        /// </summary>
        /// <param name="boardId">Board ID</param>
        /// <param name="subscriptionGuid">Board subscription GUID</param>
        private static BoardSubscriptionInfo GetBoardSubscriptionInfo(int boardId, Guid subscriptionGuid)
        {
            // Build WHERE condition	
            var condition = new WhereCondition().WhereEquals("SubscriptionGUID", subscriptionGuid);

            if (boardId > 0)
            {
                condition.WhereEquals("SubscriptionBoardID", boardId);
            }

            return GetSubscriptions()
                        .Where(condition)
                        .FirstObject;
        }


        /// <summary>
        /// Sends subscription or unsubscription confirmation mail.
        /// </summary>
        /// <param name="boardSubscription">Board subscription info</param>
        /// <param name="isSubscription">If true subscription email is sent else unsubscription email is sent</param>
        /// <param name="isOptIn">Indicates if opt-in email should be sent</param>
        private static void SendConfirmationEmail(BoardSubscriptionInfo boardSubscription, bool isSubscription, bool isOptIn)
        {
            // Check whether context exists
            if ((HttpContext.Current == null) || (boardSubscription == null))
            {
                return;
            }

            // Get message board
            BoardInfo board = BoardInfoProvider.GetBoardInfo(boardSubscription.SubscriptionBoardID);
            if (board == null)
            {
                return;
            }

            // Get site object
            SiteInfo si = SiteInfoProvider.GetSiteInfo(board.BoardSiteID);
            if (si == null)
            {
                return;
            }

            // Prepare the macro resolver and enable HTML encoding
            MacroResolver resolver = MacroContext.CurrentResolver;
            resolver.Settings.EncodeResolvedValues = true;
            resolver.SetNamedSourceData(new Dictionary<string, object>
            {
                { "DocumentLink", BoardInfoProvider.GetMessageBoardUrl(board, si.SiteName) },
                { "SubscriptionLink", isOptIn ? GetActivationUrl(boardSubscription, board) : string.Empty },
                { "OptInInterval", isOptIn ? BoardInfoProvider.DoubleOptInInterval(si.SiteName).ToString() : string.Empty },
                { "UnsubscriptionLink", (isOptIn || isSubscription) ? GetUnsubscriptionUrl(boardSubscription, board, null) : string.Empty }
            }, isPrioritized: false);
            resolver.SetNamedSourceData("Board", board);

            // Get email template
            EmailTemplateInfo template;
            if (isOptIn)
            {
                template = EmailTemplateProvider.GetEmailTemplate("Boards.SubscriptionRequest", si.SiteName);
            }
            else if (isSubscription)
            {
                template = EmailTemplateProvider.GetEmailTemplate("Boards.SubscribeConfirmation", si.SiteName);
            }
            else
            {
                template = EmailTemplateProvider.GetEmailTemplate("Boards.UnsubscribeConfirmation", si.SiteName);
            }

            if (template == null)
            {
                return;
            }

            // Send message
            EmailMessage message = new EmailMessage();
            message.EmailFormat = EmailFormatEnum.Default;
            message.From = DataHelper.GetNotEmpty(SettingsKeyInfoProvider.GetValue(si.SiteName + ".CMSSendBoardEmailsFrom"), EmailHelper.DEFAULT_EMAIL_SENDER);
            message.Recipients = boardSubscription.SubscriptionEmail;

            if ((message.Recipients != null) && (message.Recipients.Trim() != ""))
            {
                EmailSender.SendEmailWithTemplateText(si.SiteName, message, template, resolver, false);
            }
        }


        /// <summary>
        /// Returns approval page URL to confirm message board subscription.
        /// </summary>
        /// <param name="subscription">Message board subscription object</param>
        /// <param name="board">Message board object</param>
        private static string GetActivationUrl(BoardSubscriptionInfo subscription, BoardInfo board)
        {
            // Get activation page from message board
            string activationPage = DataHelper.GetNotEmpty(board.BoardOptInApprovalURL, "~/CMSModules/MessageBoards/CMSPages/SubscriptionApproval.aspx");

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
            string hash = SecurityHelper.GenerateConfirmationEmailHash(board.BoardGUID + "|" + subscription.SubscriptionEmail, time);
            subscription.SubscriptionApprovalHash = hash;
            SetBoardSubscriptionInfo(subscription);

            string url = string.Format("{0}?boardsubscriptionhash={1}&datetime={2}&cid={3}&siteid={4}&url={5}&docid={6}&camp={7}",
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

        #endregion
    }
}