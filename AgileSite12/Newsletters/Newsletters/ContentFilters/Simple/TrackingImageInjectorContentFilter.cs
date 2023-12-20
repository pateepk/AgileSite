using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Newsletters.Filters
{
    /// <summary>
    /// Filter for injecting tracking image to email content.
    /// </summary>
    internal sealed class TrackingImageInjectorContentFilter : IEmailContentFilter
    {
        private readonly SiteInfo site;
        private readonly IssueInfo issue;
        private readonly string baseUrl;


        /// <summary>
        /// Creates instance of <see cref="TrackingImageInjectorContentFilter"/> class.
        /// </summary>
        /// <param name="issue">Issue.</param>
        /// <param name="site">Issue site.</param>
        /// <param name="baseUrl">Base URL to use for resolving.</param>
        public TrackingImageInjectorContentFilter(IssueInfo issue, SiteInfo site, string baseUrl)
        {
            this.issue = issue;
            this.site = site;
            this.baseUrl = baseUrl;
        }


        /// <summary>
        /// Applies the filter to the given <paramref name="text"/>.
        /// </summary>
        /// <param name="text">Text to transform.</param>
        /// <returns>Text with applied filter transformation.</returns>
        public string Apply(string text)
        {
            return InjectTrackingImage(text);
        }


        private string InjectTrackingImage(string text)
        {
            var trackingLink = GetTrackingLink();
            var trackingImage = $"<img src=\"{trackingLink}\" alt=\"\" width=\"1\" height=\"1\" style=\"{GetTrackingImageStyles()}\" />";

            int insertAt = text.IndexOf("</body>", StringComparison.InvariantCultureIgnoreCase);

            // Closing body tag might not be found if HTML is malformed
            if (insertAt != -1)
            {
                text = text.Insert(insertAt, trackingImage);
            }
            else
            {
                // Fallback - even though the HTML is malformed, e-mail rendering engines might display it correctly outside any tags
                text = text + trackingImage;
            }

            return text;
        }


        private string GetTrackingLink()
        {
            var trackingLink = EmailTrackingLinkHelper.CreateOpenedEmailTrackingLink(baseUrl, site);
            trackingLink = URLHelper.AddParameterToUrl(trackingLink, "issueguid", issue.IssueGUID.ToString("D"));
            trackingLink = URLHelper.AddParameterToUrl(trackingLink, "email", "{%UrlEncode(Email)%}");
            trackingLink = URLHelper.AddParameterToUrl(trackingLink, "hash", "{%Hash%}");

            return HTMLHelper.HTMLEncode(trackingLink);
        }


        /// <summary>
        /// Gets string representation of styles suitable for the open email tracking image.
        /// Guarantees tracking image remains invisible even if there are some CSS styles present that e.g. declare image border color and width.
        /// </summary>
        private static string GetTrackingImageStyles()
        {
            var predefinedStyles = new List<string>
            {
                "position: absolute",
                "bottom: 0",
                "width: 1px",
                "height: 1px",
                "border: 0",
                "margin: 0",
                "padding: 0",
                "background-color: transparent",
            };

            // If there is !important suffix used within the email style sheet, styles above are overwritten.
            // However, some email browsers do not support !important styles (such declarations are skipped), therefore normal definitions has to be included as well.
            var importantStyles = predefinedStyles.Select(style => style + " !important");

            return string.Join(";", predefinedStyles.Union(importantStyles));
        }
    }
}
