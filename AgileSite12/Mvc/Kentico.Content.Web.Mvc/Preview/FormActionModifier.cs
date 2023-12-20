using System;
using System.Text.RegularExpressions;

using CMS.Helpers;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Class for transforming URL in action parameter of forms.
    /// </summary>
    internal static class FormActionModifier
    {
        /// <summary>
        /// Hash table of regular expressions to resolve URLs.
        /// </summary>
        private static Regex urlRegEx;


        /// <summary>
        /// Ensures that all URLs in action parameter of all form elements in <paramref name="html"/> has correct virtual context in order to be used in preview.
        /// </summary>
        /// <param name="html">HTML code.</param>
        /// <returns>HTML code with correct URLs for preview.</returns>
        public static string EnsurePreviewActionUrl(string html)
        {
            if ((html != null) && VirtualContext.IsInitialized)
            {
                MatchEvaluator evaluator = m => EnsurePreviewUrl(m);

                if (urlRegEx == null)
                {
                    urlRegEx = RegexHelper.GetRegex("(\\saction=[\"'])([^\"']*)([\"'])", RegexHelper.DefaultOptions | RegexOptions.IgnoreCase);
                }

                html = urlRegEx.Replace(html, evaluator);
            }

            return html;
        }


        /// <summary>
        /// Returns the preview URL in the given <paramref name="match"/>.
        /// </summary>
        private static string EnsurePreviewUrl(Match match)
        {
            var start = match.Groups[1].ToString();
            var url = match.Groups[2].ToString();
            var end = match.Groups[3].ToString();

            url = EnsureUrlPath(url, RequestContext.URL.LocalPath);

            if (!VirtualContext.ContainsVirtualContextPrefix(url))
            {
                Uri absoluteUri;
                if (Uri.TryCreate(url, UriKind.Absolute, out absoluteUri))
                {
                    if (String.Equals(absoluteUri.Host, RequestContext.CurrentDomain, StringComparison.OrdinalIgnoreCase))
                    {
                        var hostAndDomain = absoluteUri.AbsoluteUri.Replace(absoluteUri.PathAndQuery, "");
                        var pathWithVirtualContext = VirtualContext.GetPreviewPathFromVirtualContext(absoluteUri.PathAndQuery, VirtualContext.ReadonlyMode, embededInAdministration: false);

                        url = $"{hostAndDomain}{pathWithVirtualContext}";
                    }
                }
                else
                {
                    url = VirtualContext.GetPreviewPathFromVirtualContext(url, VirtualContext.ReadonlyMode);
                }
            }

            return $"{start}{url}{end}";
        }


        private static string EnsureUrlPath(string url, string currentRequestUrl)
        {
            // URL is empty
            if (String.IsNullOrEmpty(url))
            {
                return currentRequestUrl;
            }

            // URL consists only of query part
            if (String.IsNullOrEmpty(URLHelper.RemoveQuery(url)))
            {
                return $"{currentRequestUrl}{url}";
            }

            return url;
        }
    }
}
