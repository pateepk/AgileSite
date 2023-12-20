using System;
using System.Web;

using CMS.Base;
using CMS.ContactManagement;
using CMS.Helpers;
using CMS.Newsletters;
using CMS.Routing.Web;
using CMS.SiteProvider;

using HttpCacheability = System.Web.HttpCacheability;

[assembly: RegisterHttpHandler("CMSPages/Newsletters/GetEmailBrowserContent.ashx", typeof(GetEmailBrowserContentHandler), Order = 1)]

namespace CMS.Newsletters
{
    internal sealed class GetEmailBrowserContentHandler : IHttpHandler
    {
        /// <summary>
        /// Gets whether this handler can be reused for other request; always returns <c>false</c>.
        /// </summary>
        /// <remarks>
        /// Although the handler could be re-used by another requests, always create a new instance as allocating memory 
        /// is cheap and also initialization costs are minimal.
        /// </remarks>
        public bool IsReusable => false;


        public void ProcessRequest(HttpContext context)
        {
            // View in browser link's hash is created only from email and issue guid parameters. 
            // If issue is using utm parameters they are added to link in send filter after the
            // view in browser link macro was resolved and hash computed therefore we need to exclude utm parameters here.
            if (!QueryHelper.ValidateHash("hash", "utm_source;utm_campaign;utm_medium"))
            {
                RequestHelper.Respond404();
            }

            var issueGuid = QueryHelper.GetGuid("issueGuid", Guid.Empty);
            if (issueGuid == Guid.Empty)
            {
                RequestHelper.Respond404();
            }

            var issue = IssueInfoProvider.GetIssueInfo(issueGuid, SiteContext.CurrentSiteID);
            if (issue == null)
            {
                RequestHelper.Respond404();
            }

            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(issue.IssueNewsletterID);
            if (newsletter == null)
            {
                RequestHelper.Respond404();
            }

            var subscriber = GetSubscriber(newsletter);

            string html;
            var culture = CultureHelper.GetDefaultCultureCode(SiteContext.CurrentSiteName);
            using (new CMSActionContext { ThreadCulture = CultureHelper.GetCultureInfo(culture) })
            {
                html = GetEmailHTML(issue, newsletter, subscriber);
            }

            SendResponse(context, html);
        }


        private static string GetEmailHTML(IssueInfo issue, NewsletterInfo newsletter, SubscriberInfo subscriber)
        {
            var preview = subscriber == null;

            return new EmailViewer(issue, newsletter, subscriber, preview).GetBody();
        }


        private static SubscriberInfo GetSubscriber(NewsletterInfo newsletter)
        {
            var recipientEmail = QueryHelper.GetString("recipientemail", string.Empty);
            if (newsletter.NewsletterType == EmailCommunicationTypeEnum.Newsletter)
            {
                return SubscriberInfoProvider.GetFirstSubscriberWithSpecifiedEmail(newsletter.NewsletterID, recipientEmail);
            }

            var recipient = ContactInfoProvider.GetContactInfo(recipientEmail);
            return recipient?.ToContactSubscriber();
        }


        private void SendResponse(HttpContext context, string templateCode)
        {
            var response = context.Response;

            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();
            response.Cache.SetExpires(DateTime.MinValue);
            response.Clear();

            response.Write(templateCode);

            RequestHelper.CompleteRequest();
        }
    }
}
