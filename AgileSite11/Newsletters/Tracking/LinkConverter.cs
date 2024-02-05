using System;
using System.Text.RegularExpressions;
using System.Web;

using CMS.Helpers;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.SiteProvider;

namespace CMS.Newsletters
{
    /// <summary>
    /// Converts links in newsletter issue text.
    /// </summary>
    public class LinkConverter
    {
        #region "Constants"

        /// <summary>
        /// Name of the HTML anchor's attribute that determines whether tracking is enabled or not.
        /// </summary>
        private const string TRACKING_ATTRIBUTE = "tracking";

        #endregion


        #region "Variables"

        // Regular expression that matches a link.
        private static readonly Regex linkRegex = RegexHelper.GetRegex(@"(?<prefix>\s(src|href)\s*=\s*)((?<quote>[""'])(?<link>.+?)\k<quote>)");

        // Regular expression that matches an anchor hyperlink.
        private static readonly Regex hyperlinkRegex = RegexHelper.GetRegex(@"(?<prefix><a[\s|\S]*?href(\s)*?=(\s)*?)(?<quote>[""'])(?<url>.*?)\k<quote>(?<suffix>[\s|\S]*?>)(?<name>[\s|\S]*?)</a>");

        #endregion


        #region "Methods"

        /// <summary>
        /// Replaces <paramref name="oldValue"/> with <paramref name="newValue"/> in all link URLs in the <paramref name="input"/> string.
        /// </summary>
        /// <param name="input">Text containing links</param>
        /// <param name="oldValue">The string to be replaced</param>
        /// <param name="newValue">The string to replace all occurrences of oldValue</param>
        public static string ReplaceLinkUrls(string input, string oldValue, string newValue)
        {
            return linkRegex.Replace(input, match => FormatLink(match, match.Groups["link"].Value.Replace(oldValue, newValue)));
        }


        /// <summary>
        /// Converts links from relative to absolute.
        /// </summary>
        /// <param name="input">Text containing links with relative URLs</param>
        /// <param name="baseUrl">Base URL to use when resolving links</param>
        /// <returns>Text with all links converted to absolute form</returns>
        internal static string ConvertToAbsolute(string input, string baseUrl)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return linkRegex.Replace(input, match => FormatLink(match, ConvertUrlToAbsolute(match.Groups["link"].Value, baseUrl)));
        }


        /// <summary>
        /// Creates an absolute URL for use in newsletter.
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="baseUrl">Base URL</param>
        /// <returns>Absolute URL</returns>
        public static string ConvertUrlToAbsolute(string url, string baseUrl)
        {
            // Do not convert absolute URL and pseudo protocol or anchor
            if (ContainsProtocol(url) || url.StartsWith("#", StringComparison.InvariantCulture) || IsMacro(url))
            {
                return url;
            }

            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ArgumentException("Cannot convert url to absolute on relative link without baseUrl", nameof(baseUrl));
            }

            // Result should contain protocol too
            if (!ContainsProtocol(baseUrl))
            {
                baseUrl = "http://" + baseUrl;
            }

            if (url.StartsWith("/", StringComparison.InvariantCulture))
            {
                baseUrl = URLHelper.GetProtocol(baseUrl) + "://" + URLHelper.GetDomain(baseUrl);
            }
            else if (url.StartsWith("~", StringComparison.InvariantCulture))
            {
                url = url.TrimStart('~');
            }

            return baseUrl.TrimEnd('/') + "/" + url.TrimStart('/');
        }


        /// <summary>
        /// Converts links to tracking links.
        /// </summary>
        /// <param name="input">Text containing links with relative URLs</param>
        /// <param name="issue">Newsletter issue that contains the text to resolve</param>
        /// <param name="newsletter">Newsletter</param>
        /// <param name="site">Site for which clicked links are tracked</param>
        /// <param name="emailAddress">Email address to be used for the links.</param>
        /// <returns>Text with all links converted to tracking links</returns>
        internal static string ConvertToTracking(string input, IssueInfo issue, NewsletterInfo newsletter, SiteInfo site, string emailAddress)
        {
            string output;
            var trackingPageUrl = EmailTrackingLinkHelper.GetClickedLinkTrackingPageUrl(site);

            var urlService = Service.Resolve<IIssueUrlService>();
            var baseUrl = urlService.GetBaseUrl(newsletter);

            var trackingLinkUrl = ConvertUrlToAbsolute(trackingPageUrl, baseUrl);
            var systemUrls = GetEmailSystemLinks(newsletter, urlService);

            // Disable new version creation and logging
            using (new CMSActionContext { CreateVersion = false, LogExport = false, LogSynchronization = false })
            {
                // Convert all links in the text to tracking links
                output = hyperlinkRegex.Replace(input,
                    match => CreateTrackingLink(match, issue, trackingLinkUrl, emailAddress, systemUrls));
            }

            return output;
        }


        private static string[] GetEmailSystemLinks(NewsletterInfo newsletter, IIssueUrlService urlService)
        {
            return new[]
            {
                urlService.GetUnsubscriptionBaseUrl(newsletter),
                urlService.GetActivationBaseUrl(newsletter),
                urlService.GetViewInBrowserBaseUrl(newsletter)
            };
        }


        /// <summary>
        /// Creates a link from the regular expression match and link URL.
        /// </summary>
        /// <param name="match">Regex match result</param>
        /// <param name="url">Link after resolution</param>
        /// <returns>Full hyperlink with attributes, URL and text</returns>
        private static string FormatLink(Match match, string url)
        {
            return match.Groups["prefix"].Value +
                   match.Groups["quote"].Value + url + match.Groups["quote"].Value;
        }


        /// <summary>
        /// Creates a tracking hyperlink link from a given regex match.
        /// </summary>
        /// <param name="match">Regular expression matching result</param>
        /// <param name="issue">Newsletter issue</param>
        /// <param name="trackingLink">Tracking link without query parameters</param>
        /// <param name="emailAddress">Email address to be used for the links.</param>
        /// <param name="systemLinks">Set of system links which should not be tracked</param>
        /// <returns>A tracking hyperlink</returns>
        private static string CreateTrackingLink(Match match, IssueInfo issue, string trackingLink, string emailAddress, string[] systemLinks)
        {
            string url = match.Groups["url"].Value.Replace("&amp;", "&").Trim();

            if (url.StartsWithAny(StringComparison.OrdinalIgnoreCase, systemLinks) || !IsTrackable(match, url, trackingLink))
            {
                return FormatHyperlink(match, url);
            }

            // When new link found save it if it doesn't exist yet
            var linkFullName = ObjectHelper.BuildFullName(issue.IssueID.ToString(), url);
            var link = LinkInfoProvider.GetLinkInfo(linkFullName);
            if (link == null)
            {
                // Maximal permitted length of link description is 400 characters (limited by DB column definition)
                string description = GetDescription(match).Truncate(400);

                link = new LinkInfo
                {
                    LinkIssueID = issue.IssueID,
                    LinkTarget = url,
                    LinkDescription = description
                };

                LinkInfoProvider.SetLinkInfo(link);
            }
            
            string trackingUrl = URLHelper.AddParameterToUrl(trackingLink, "linkguid", link.LinkGUID.ToString("D"));
            trackingUrl = URLHelper.AddParameterToUrl(trackingUrl, "email", HttpUtility.UrlEncode(emailAddress));
            trackingUrl = URLHelper.AddParameterToUrl(trackingUrl, "hash", Service.Resolve<IEmailHashGenerator>().GetEmailHash(emailAddress));

            trackingUrl = HTMLHelper.HTMLEncode(trackingUrl);

            return FormatHyperlink(match, trackingUrl);
        }


        /// <summary>
        /// Checks whether given URL can and should be tracked.
        /// </summary>
        /// <param name="match">Regex match result for a hyperlink</param>
        /// <param name="url">URL to check</param>
        /// <param name="trackingLink">Path the link will be transformed to after tracking (../Redirect.ashx by default)</param>
        /// <returns>true if specified URL can be tracked, otherwise false</returns>
        private static bool IsTrackable(Match match, string url, string trackingLink)
        {
            bool? trackingEnabled = ValidationHelper.GetNullableBoolean(GetAttributeValue(match.Value, TRACKING_ATTRIBUTE), null);

            // Invalid/malformed URIs cannot be tracked as well as where explicitly prohibited using attribute
            // Links that already contains trackingLink will not be tracked either
            return
                Uri.IsWellFormedUriString(url, UriKind.Absolute) &&
                !url.Contains(trackingLink) &&
                (trackingEnabled ?? IsSupportedUriScheme(url));
        }


        /// <summary>
        /// Determines whether the URI is of a supported scheme.
        /// </summary>
        /// <param name="url">The URL</param>
        /// <returns><c>true</c> if URI scheme is supported, otherwise, <c>false</c></returns>
        /// <remarks>Currently, only HTTP/HTTPS is supported because of the need to use redirects.</remarks>
        private static bool IsSupportedUriScheme(string url)
        {
            Uri uri = new Uri(url);
            return (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }


        /// <summary>
        /// Gets a description for tracked link either from title attribute or anchor's element text.
        /// </summary>
        /// <param name="match">Regex match result for a hyperlink</param>
        /// <returns>Link's description</returns>
        private static string GetDescription(Match match)
        {
            string title = GetAttributeValue(match.Value, "title");

            if (!string.IsNullOrEmpty(title))
            {
                return title;
            }

            return HTMLHelper.StripTags(match.Groups["name"].Value);
        }


        /// <summary>
        /// Creates a tracking link from the regular expression match and link URL.
        /// </summary>
        /// <param name="match">Regex match result for a hyperlink</param>
        /// <param name="url">Link after resolution</param>
        /// <returns>Full tracking hyperlink with attributes, URL and text</returns>
        private static string FormatHyperlink(Match match, string url)
        {
            string hyperlink =
                match.Groups["prefix"].Value +
                match.Groups["quote"].Value + url + match.Groups["quote"].Value +
                match.Groups["suffix"].Value +
                match.Groups["name"] + "</a>";

            // Remove tracking attribute from hyper link if exists            
            return RemoveTagAttribute(hyperlink, TRACKING_ATTRIBUTE);
        }


        /// <summary>
        /// Removes an attribute of a specified name inside an HTML element.
        /// </summary>
        /// <param name="input">Element's outer HTML</param>
        /// <param name="attributeName">Name of the attribute to replace</param>
        internal static string RemoveTagAttribute(string input, string attributeName)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(attributeName))
            {
                return input;
            }

            Regex attributeRegex =
                RegexHelper.GetRegex($@"\s{attributeName}=(?<quote>[""'])(?<value>.*?)\k<quote>", true);

            return attributeRegex.Replace(input, string.Empty);
        }


        /// <summary>
        /// Gets the value of the HTML element's attribute.
        /// </summary>
        /// <param name="input">Element's outer HTML</param>
        /// <param name="attributeName">Name of the attribute</param>
        /// <returns>Value of the element's attribute</returns>
        private static string GetAttributeValue(string input, string attributeName)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(attributeName))
            {
                return null;
            }

            Regex attributeRegex =
                RegexHelper.GetRegex($@"\s{attributeName}=(?<quote>[""'])(?<value>.*?)\k<quote>", true);

            Match match = attributeRegex.Match(input);
            return match.Groups["value"].Value;
        }


        /// <summary>
        /// Checks if URL contains protocol, including pseudo-protocol.
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>true, if URL contains protocol, otherwise false</returns>
        private static bool ContainsProtocol(string url)
        {
            return URLHelper.ContainsProtocol(url) || Regex.IsMatch(url, "^[a-zA-Z]+:");
        }


        private static bool IsMacro(string url)
        {
            return url.StartsWith("{%", StringComparison.InvariantCulture);
        }

        #endregion
    }
}