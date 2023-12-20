using System;
using System.Linq;
using System.Web;

using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.Modules;
using CMS.Newsletters.Web.UI;
using CMS.Routing.Web;
using CMS.SiteProvider;

using HttpCacheability = System.Web.HttpCacheability;

[assembly: RegisterHttpHandler("CMSPages/Newsletters/GetEmailPreviewContent.ashx", typeof(GetEmailPreviewContentHandler), Order = 1)]

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Provides HTML code of email given by <c>issueGuid</c> query string parameter.
    /// </summary>
    internal sealed class GetEmailPreviewContentHandler : IHttpHandler
    {
        /// <summary>
        /// Gets whether this handler can be reused for other request; always returns <c>false</c>.
        /// </summary>
        /// <remarks>
        /// Although the handler could be re-used by another requests, always create a new instance as allocating memory 
        /// is cheap and also initialization costs are minimal.
        /// </remarks>
        public bool IsReusable => false;


        /// <summary>
        /// Parses query parameters, check permissions and returns newsletter email template code.
        /// </summary>
        /// <param name="context">Current HTTP context</param>
        public void ProcessRequest(HttpContext context)
        {
            int issueId = QueryHelper.GetInteger("issueid", 0);
            if (issueId <= 0)
            {
                RequestHelper.Respond404();
            }

            var issue = IssueInfoProvider.GetIssueInfo(issueId);
            if (issue == null)
            {
                RequestHelper.Respond404();
            }

            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(issue.IssueNewsletterID);
            if (newsletter == null)
            {
                RequestHelper.Respond404();
            }

            CheckPermissions();

            string html;
            var culture = CultureHelper.GetDefaultCultureCode(SiteContext.CurrentSiteName);
            using (new CMSActionContext { ThreadCulture = CultureHelper.GetCultureInfo(culture) })
            {
                html = GetPreviewHTML(issue, newsletter);
            }

            SendResponse(context, html);
        }


        private static string GetPreviewHTML(IssueInfo issue, NewsletterInfo newsletter)
        {
            var subscriber = GetSubscriber(newsletter);

            string output = String.Empty;
            using (var h = NewsletterEvents.GeneratePreview.Start(issue, subscriber, output))
            {
                if (h.CanContinue())
                {
                    if (!String.IsNullOrEmpty(h.EventArguments.PreviewHtml))
                    {
                        return h.EventArguments.PreviewHtml;
                    }

                    output = new EmailViewer(issue, newsletter, subscriber, true).GetBody();

                    h.EventArguments.PreviewHtml = output;
                }
                h.FinishEvent();

                return h.EventArguments.PreviewHtml;
            }
        }


        private void CheckPermissions()
        {
            // Check the license
            if (!string.IsNullOrEmpty(RequestContext.CurrentDomain))
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Newsletters);
            }

            // Check site availability
            if (!ResourceSiteInfoProvider.IsResourceOnSite(ModuleName.NEWSLETTER, SiteContext.CurrentSiteName))
            {
                RequestHelper.Respond403();
            }

            // Check user permissions for the module
            if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource(ModuleName.NEWSLETTER, "Read", SiteContext.CurrentSiteName))
            {
                RequestHelper.Respond403();
            }
        }


        private static SubscriberInfo GetSubscriber(NewsletterInfo newsletter)
        {
            var recipientEmail = QueryHelper.GetString("recipientEmail", string.Empty);
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
