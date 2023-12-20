using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Newsletters;
using CMS.SiteProvider;

[assembly: RegisterImplementation(typeof(IIssueUrlService), typeof(IssueUrlService), Priority = RegistrationPriority.SystemDefault)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Service retrieving URLs for issue content.
    /// </summary>
    internal class IssueUrlService : IIssueUrlService
    {
        private readonly DateTime? time;


        /// <summary>
        /// Creates an instance of <see cref="IssueUrlService"/> class.
        /// </summary>
        public IssueUrlService()
        {

        }


        /// <summary>
        /// Creates an instance of <see cref="IssueUrlService"/> class.
        /// </summary>
        /// <param name="time">Custom time used to evaluate activation URL hash.</param>
        internal IssueUrlService(DateTime time)
        {
            this.time = time;
        }


        /// <summary>
        /// Gets the base URL for given <paramref name="newsletter"/> if defined. Otherwise it returns full application URL of newsletter site. 
        /// </summary>
        /// <param name="newsletter">Newsletter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="newsletter"/> is <c>null</c></exception>
        /// <remarks>The result URL doesn't contain trailing slash.</remarks>
        public string GetBaseUrl(NewsletterInfo newsletter)
        {
            if (newsletter == null)
            {
                throw new ArgumentNullException(nameof(newsletter));
            }

            if (!string.IsNullOrEmpty(newsletter.NewsletterBaseUrl))
            {
                return newsletter.NewsletterBaseUrl.TrimEnd('/');
            }

            var site = SiteInfoProvider.GetSiteInfo(newsletter.NewsletterSiteID);
            if (site == null)
            {
                return String.Empty;
            }

            if (site.SiteIsContentOnly)
            {
                return site.SitePresentationURL.TrimEnd('/');
            }

            return URLHelper.GetFullApplicationUrl(site.DomainName);
        }


        /// <summary>
        /// Gets unsubscription base URL for given <paramref name="newsletter"/>.
        /// </summary>
        /// <param name="newsletter">Newsletter.</param>
        public string GetUnsubscriptionBaseUrl(NewsletterInfo newsletter)
        {
            var pageUrl = GetUnsubscriptionUrlFromNewsletter(newsletter);
            var baseUrl = GetBaseUrl(newsletter);

            // Combine base URL and unsubscription link
            if (string.IsNullOrEmpty(pageUrl))
            {
                return $"{baseUrl}/CMSModules/Newsletters/CMSPages/unsubscribe.aspx";
            }

            return FormatUrl(pageUrl, baseUrl);
        }


        /// <summary>
        /// Gets unsubscription URL for given <paramref name="newsletter"/>, <paramref name="issue"/>, and <paramref name="subscriber"/>.
        /// </summary>
        /// <param name="newsletter">Newsletter.</param>
        /// <param name="issue">Issue.</param>
        /// <param name="subscriber">Subscriber.</param>
        public string GetUnsubscriptionUrl(NewsletterInfo newsletter, IssueInfo issue, SubscriberInfo subscriber)
        {
            var baseUrl = GetUnsubscriptionBaseUrl(newsletter);
            var queryCollection = GetUnsubscriptionUrlQuery(newsletter, subscriber, issue);

            return $"{baseUrl}?{queryCollection}";
        }


        /// <summary>
        /// Gets activation base URL for the newsletter opt-in feature.
        /// </summary>
        /// <param name="newsletter">Newsletter.</param>
        public string GetActivationBaseUrl(NewsletterInfo newsletter)
        {
            var pageUrl = GetActivationUrlFromNewsletter(newsletter);
            var baseUrl = GetBaseUrl(newsletter);

            // Combine base URL and activation link
            if (string.IsNullOrEmpty(pageUrl))
            {
                return $"{baseUrl}/CMSModules/Newsletters/CMSPages/SubscriptionApproval.aspx";
            }

            return FormatUrl(pageUrl, baseUrl);
        }


        /// <summary>
        /// Creates activation URL for the newsletter opt-in feature.
        /// </summary>
        /// <param name="newsletter">Newsletter.</param>
        /// <param name="subscriber">Subscriber.</param>
        /// <param name="subscription">Subscription.</param>
        /// <returns>Activation URL for given <paramref name="newsletter"/> and <paramref name="subscriber"/>.</returns>
        /// <remarks>When the activation URL is created, the hash used for the activation URL is stored to the related <paramref name="subscription"/>.</remarks>
        public string CreateActivationUrl(NewsletterInfo newsletter, SubscriberInfo subscriber, SubscriberNewsletterInfo subscription)
        {
            var baseUrl = GetActivationBaseUrl(newsletter);
            var hashTime = time ?? DateTime.Now;
            var hash = SecurityHelper.GenerateConfirmationEmailHash(newsletter.NewsletterGUID + "|" + subscriber.SubscriberGUID, hashTime);

            SetSubscriptionHash(subscription, hash);

            var activationUrl = $"{baseUrl}?subscriptionhash={hash}&datetime={DateTimeUrlFormatter.Format(hashTime)}";

            foreach (var definition in DoubleOptInExtensionDefinitionRegister.Instance.Items)
            {
                var parameters = definition.GetQueryParameters?.Invoke();
                if (parameters != null && parameters.HasKeys())
                {
                    foreach (string key in parameters)
                    {
                        activationUrl = URLHelper.AddParameterToUrl(activationUrl, key, parameters[key]);
                    }
                }
            }

            return activationUrl;
        }


        /// <summary>
        /// Gets base URL to view email content in a browser.
        /// </summary>
        /// <param name="newsletter">Newsletter.</param>

        public string GetViewInBrowserBaseUrl(NewsletterInfo newsletter)
        {
            var baseUrl = GetBaseUrl(newsletter);
            return $"{baseUrl}/CMSPages/Newsletters/GetEmailBrowserContent.ashx";
        }


        /// <summary>
        /// Gets URL to view email content in a browser.
        /// </summary>
        /// <param name="newsletter">Newsletter.</param>
        /// <param name="issue">Issue.</param>
        /// <param name="subscriber">Subscriber.</param>

        public string GetViewInBrowserUrl(NewsletterInfo newsletter, IssueInfo issue, SubscriberInfo subscriber)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            var baseUrl = GetViewInBrowserBaseUrl(newsletter);
            var url = $"{baseUrl}?issueGuid={issue.IssueGUID}&recipientEmail={HttpUtility.UrlEncode(subscriber.SubscriberEmail)}";

            return URLHelper.AddParameterToUrl(url, "hash", QueryHelper.GetHash(url));
        }


        private static Guid GetIssueGuid(IssueInfo issue)
        {
            if (issue == null)
            {
                return Guid.Empty;
            }

            Guid issueGuid = Guid.Empty;
            if (issue.IssueIsVariant)
            {
                // If a variant of A/B test issue is sent get parent issue GUID
                var parentIssue = IssueInfoProvider.GetIssueInfo(issue.IssueVariantOfIssueID);
                if (parentIssue != null)
                {
                    issueGuid = parentIssue.IssueGUID;
                }
            }
            else
            {
                // Get current issue GUID
                issueGuid = issue.IssueGUID;
            }

            return issueGuid;
        }


        private static NameValueCollection GetUnsubscriptionUrlQuery(NewsletterInfo newsletter, SubscriberInfo subscriber, IssueInfo issue)
        {
            // Use HttpUtility.ParseQueryString method since it internally uses HttpValueCollection which handles URL encoding of values
            var queryCollection = HttpUtility.ParseQueryString(string.Empty);

            var issueGuid = GetIssueGuid(issue);
            if (issueGuid != Guid.Empty)
            {
                queryCollection.Add("issueguid", issueGuid.ToString());
            }
            queryCollection.Add("email", subscriber.SubscriberEmail);
            queryCollection.Add("hash", Service.Resolve<IEmailHashGenerator>().GetEmailHash(subscriber.SubscriberEmail));
            queryCollection.Add("newsletterguid", newsletter.NewsletterGUID.ToString());

            return queryCollection;
        }


        private static void SetSubscriptionHash(SubscriberNewsletterInfo subscription, string hash)
        {
            subscription.SubscriptionApprovalHash = hash;
            SubscriberNewsletterInfoProvider.SetSubscriberNewsletterInfo(subscription);
        }


        private static string GetActivationUrlFromNewsletter(NewsletterInfo newsletter)
        {
            // Get activation page from newsletter or fall back to page defined in settings
            return string.IsNullOrEmpty(newsletter.NewsletterOptInApprovalURL) ?
                SettingsKeyInfoProvider.GetValue("CMSOptInApprovalURL", newsletter.NewsletterSiteID) :
                newsletter.NewsletterOptInApprovalURL;
        }


        private static string GetUnsubscriptionUrlFromNewsletter(NewsletterInfo newsletter)
        {
            // Get unsubscription page from newsletter or fall back to page defined in settings
            return string.IsNullOrEmpty(newsletter.NewsletterUnsubscribeUrl) ?
                SettingsKeyInfoProvider.GetValue("CMSNewsletterUnsubscriptionURL", newsletter.NewsletterSiteID) :
                newsletter.NewsletterUnsubscribeUrl;
        }


        private static string FormatUrl(string url, string baseUrl)
        {
            url = url.Trim();

            if (URLHelper.ContainsProtocol(url))
            {
                return url.Trim('/');
            }

            if (url.StartsWith("~", StringComparison.Ordinal))
            {
                url = url.Remove(0, 1);
            }

            if (!url.StartsWith("/", StringComparison.Ordinal))
            {
                url = "/" + url;
            }

            url = EnsureFriendlyExtension(url);

            return baseUrl + url;
        }


        private static string EnsureFriendlyExtension(string url)
        {
            var urlExtSettings = URLHelper.GetFriendlyExtensions(SiteContext.CurrentSiteName).Split(';');
            if (url.EndsWithAny(StringComparison.OrdinalIgnoreCase, urlExtSettings))
            {
                return url;
            }

            if (urlExtSettings.Length > 0)
            {
                // Just set the first one
                url += urlExtSettings[0];
            }

            return url;
        }


        /// <summary>
        /// Gets resolved source URL of dynamic newsletter.
        /// </summary>
        /// <param name="newsletter">Newsletter.</param>
        public string GetDynamicNewsletterUrl(NewsletterInfo newsletter)
        {
            if (newsletter == null)
            {
                throw new ArgumentNullException(nameof(newsletter));
            }

            var site = SiteInfoProvider.GetSiteInfo(newsletter.NewsletterSiteID);
            var url = newsletter.NewsletterDynamicURL;

            // Resolve relative URL to full URL
            if (url.StartsWith("~/", StringComparison.Ordinal))
            {
                url = URLHelper.GetAbsoluteUrl(url, site?.DomainName);
            }

            // Check if protocol is contained in checked URL and if not complete it
            if (!URLHelper.ContainsProtocol(url))
            {
                url = URLHelper.AddHTTPToUrl(url);
            }

            return url;
        }
    }
}