using System;
using System.Collections.Generic;

using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Base page for the CMS Newsletter pages to apply global settings to the pages.
    /// </summary>
    public abstract class CMSNewsletterPage : CMSDeskPage
    {
        private const string URL_PARAMETER_PING = "ping";

        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Check the license
            if (!string.IsNullOrEmpty(DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "")))
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Newsletters);
            }

            // Check site availability
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.Newsletter", SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.Newsletter");
            }

            CurrentUserInfo user = MembershipContext.AuthenticatedUser;

            // Check UI elements for CMS Desk - CMS Desk -> Online Marketing
            bool authorizedOnlineMarketing = user.IsAuthorizedPerUIElement("CMS", "CMSDesk.OnlineMarketing");
            if (!authorizedOnlineMarketing)
            {
                RedirectToUIElementAccessDenied("CMS", "CMSDesk.OnlineMarketing");
            }

            // Check UI elements for CMS Desk -> Tools -> Newsletter OR CMS Desk -> Online marketing -> Newsletter
            bool authorizedToolsNewsletter = user.IsAuthorizedPerUIElement("CMS.Newsletter", "Newsletter");
            bool authorizedOMNewlsetter = user.IsAuthorizedPerUIElement("CMS.OnlineMarketing", "Newsletters");
            if (!authorizedToolsNewsletter && !authorizedOMNewlsetter)
            {
                RedirectToUIElementAccessDenied("CMS.Newsletter", "Newsletter");
            }

            // Check if newsletter is specified
            int newsletterId = QueryHelper.GetInteger("newsletterId", 0);
            int issueId = QueryHelper.GetInteger("issueid", 0);
            if ((newsletterId > 0) || (issueId > 0))
            {
                NewsletterInfo newsletterObj = null;
                if (issueId > 0)
                {
                    IssueInfo issue = IssueInfoProvider.GetIssueInfo(issueId);
                    if (issue != null)
                    {
                        newsletterObj = NewsletterInfoProvider.GetNewsletterInfo(issue.IssueNewsletterID);
                    }
                }
                else
                {
                    newsletterObj = NewsletterInfoProvider.GetNewsletterInfo(newsletterId);
                }

                // Check that newsletter is placed on current site
                if ((newsletterObj != null) && (newsletterObj.NewsletterSiteID != SiteContext.CurrentSiteID))
                {
                    RedirectToAccessDenied(GetString("editnewsletters.differentsite"));
                }
            }

            // Check if newsletter template is on current site
            int templateId = QueryHelper.GetInteger("templateid", 0);
            if (templateId > 0)
            {
                EmailTemplateInfo emailTemplateObj = EmailTemplateInfoProvider.GetEmailTemplateInfo(templateId);
                if ((emailTemplateObj != null) && (emailTemplateObj.TemplateSiteID != SiteContext.CurrentSiteID))
                {
                    RedirectToAccessDenied(GetString("editnewsletters.differentsitetemplate"));
                }
            }

            // Check if newsletter subscriber specified is on current site
            int subscriberId = QueryHelper.GetInteger("subscriberid", 0);
            if (subscriberId > 0)
            {
                SubscriberInfo subscriberObj = SubscriberInfoProvider.GetSubscriberInfo(subscriberId);
                if ((subscriberObj != null) && (subscriberObj.SubscriberSiteID != SiteContext.CurrentSiteID))
                {
                    RedirectToAccessDenied(GetString("editnewsletters.differentsitesubscriber"));
                }
            }

            // Check 'NewsletterRead' permission
            if (!user.IsAuthorizedPerResource("CMS.Newsletter", "Read"))
            {
                RedirectToAccessDenied("CMS.Newsletter", "Read");
            }
        }


        /// <summary>
        /// Adds notifier, that checks for broken URLs used in newsletter emails
        /// and displays alert when these URLs are broken.
        /// The notifier applies only for ContentOnly sites.
        /// </summary>
        /// <param name="newsletter">Newsletter</param>
        /// <param name="alertLabel">Alert label control, where notification will be rendered</param>
        protected void AddBrokenEmailUrlNotifier(NewsletterInfo newsletter, AlertLabel alertLabel)
        {
            if (SiteContext.CurrentSite.SiteIsContentOnly)
            {
                alertLabel.Visible = true;
                alertLabel.Text = GetString("newsletter.liveSiteNotAvailable.text") + GetString("newsletter.liveSiteNotAvailable.description");

                ScriptHelper.RegisterModule(this, "CMS/BrokenEmailUrlNotifier", new
                {
                    shownElementIdOnError = alertLabel.ClientID,
                    helpUrl = DocumentationHelper.GetDocumentationTopicUrl("email_troubleshooting_mvc"),
                    checkedUrls = GetUrlsToCheck(newsletter)
                });
            }
        }


        /// <summary>
        /// Gets the URLs to check. 
        /// Includes only URLs for features enabled by the <paramref name="newsletter"/>
        /// </summary>
        /// <param name="newsletter">Newsletter</param>
        /// <returns>Array of objects prepared to conversion to JSON</returns>
        private object[] GetUrlsToCheck(NewsletterInfo newsletter)
        {
            var urlsToCheck = new List<object>();
            var basePresentationUrl = SiteContext.CurrentSite.SitePresentationURL.TrimEnd('/');
            var urlService = Service.Resolve<IIssueUrlService>();

            var random = new Random();

            // Unsubscription feature can't be disabled. Including always.
            urlsToCheck.Add(new
            {
                name = GetString("newsletter.liveSiteNotAvailable.unsubscriptionUrl"),
                url = urlService.GetUnsubscriptionBaseUrl(newsletter)
            });

            if (newsletter.NewsletterEnableOptIn)
            {
                urlsToCheck.Add(new
                {
                    name = GetString("newsletter.liveSiteNotAvailable.activationUrl"),
                    url = urlService.GetActivationBaseUrl(newsletter)
                });
            }

            if (newsletter.NewsletterTrackOpenEmails)
            {
                var testContent = random.Next().ToString();

                var openedEmailTrackingUrl = basePresentationUrl + "/" + EmailTrackingLinkHelper.GetOpenedEmailTrackingPage(SiteContext.CurrentSite).TrimStart('/');
                openedEmailTrackingUrl = URLHelper.AddParameterToUrl(openedEmailTrackingUrl, URL_PARAMETER_PING, testContent);

                urlsToCheck.Add(new
                {
                    name = GetString("newsletter.liveSiteNotAvailable.openedEmailUrl"),
                    url = openedEmailTrackingUrl,
                    expectedContent = testContent
                });
            }

            if (newsletter.NewsletterTrackClickedLinks)
            {
                var testContent = random.Next().ToString();

                var clickedLinkTrackingUrl = basePresentationUrl + "/" + EmailTrackingLinkHelper.GetClickedLinkTrackingPageUrl(SiteContext.CurrentSite).TrimStart('/');
                clickedLinkTrackingUrl = URLHelper.AddParameterToUrl(clickedLinkTrackingUrl, URL_PARAMETER_PING, testContent);

                urlsToCheck.Add(new
                {
                    name = GetString("newsletter.liveSiteNotAvailable.clickedLinkUrl"),
                    url = clickedLinkTrackingUrl,
                    expectedContent = testContent
                });
            }

            return urlsToCheck.ToArray();
        }
    }
}