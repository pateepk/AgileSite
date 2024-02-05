using System;
using System.Web;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.Modules;
using CMS.Newsletters.Filters;
using CMS.Newsletters.Web.UI;
using CMS.Routing.Web;
using CMS.SiteProvider;

[assembly: RegisterHttpHandler("CMSPages/Newsletters/GetEmailBuilderContent.ashx", typeof(GetEmailBuilderContentHandler), Order = 1)]

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Provides HTML code of email builder for marketing email given by <c>issueid</c> query string parameter.
    /// </summary>
    internal sealed class GetEmailBuilderContentHandler : IHttpHandler
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

            var template = EmailTemplateInfoProvider.GetEmailTemplateInfo(issue.IssueTemplateID);

            if (template == null)
            {
                RequestHelper.Respond404();
            }

            CheckPermissions();

            var filter = new EmailBuilderContentFilter(issue, newsletter);
            var html = EmailBuilderContentGenerator.PrepareHtml(template.TemplateCode, filter);

            SendResponse(context, html);
        }


        private void CheckPermissions()
        {
            // Check the license
            if (!String.IsNullOrEmpty(RequestContext.CurrentDomain))
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

            // Check UI personalization
            if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerUIElement(ModuleName.NEWSLETTER, EmailBuilderHelper.EMAIL_BUILDER_UI_ELEMENT))
            {
                RequestHelper.Respond403();
            }
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
