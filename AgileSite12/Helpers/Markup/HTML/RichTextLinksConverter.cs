using System;
using System.Text.RegularExpressions;

namespace CMS.Helpers
{
    /// <summary>
    /// Converts local links in a rich text HTML to a relative form.
    /// </summary>
    /// <remarks>
    /// Replaces all links in the format "/applicationPath/xxx*" to "~/xxx*". 
    /// External link modification is available via the <see cref="RichTextLinksConverter.RichTextLinksConverter(string, Func{string, string})"/>.
    /// </remarks>
    internal class RichTextLinksConverter
    {
        /// <summary>
        /// Function that is called after each of the absolute URL paths is unresolved.
        /// </summary>
        private readonly Func<string, string> pathModifier;

        /// <summary>
        /// The regular expression that searches for all local links (absolute path without a domain).
        /// </summary>
        private readonly Regex linksRegex;

        private const string ATTRIBUTE_PATH = "PATH";
        private const string ATTRIBUTE_BEGIN = "BEGIN";
        private const string ATTRIBUTE_END = "END";


        /// <summary>
        /// Initializes a new instance of the <see cref="RichTextLinksConverter"/> class.
        /// </summary>
        /// <param name="applicationPath">The application path of links to be unresolved</param>
        /// <param name="pathModifier">Function that is called after each of the absolute URL paths is unresolved.</param>
        public RichTextLinksConverter(string applicationPath, Func<string, string> pathModifier)
        {
            applicationPath = applicationPath ?? throw new ArgumentNullException(nameof(applicationPath));

            this.pathModifier = pathModifier;

            linksRegex = GetLinksRegex(applicationPath);
        }


        /// <summary>
        /// Unresolves URL paths in the given <paramref name="html"/> if used in "src=" or "href=" or "background=" or "url(" HTML attributes.
        /// </summary>
        /// <param name="html">HTML code to unresolve.</param>
        public string Unresolve(string html)
        {
            // Unresolve the URLs
            return linksRegex.Replace(html, UnresolvePathEvaluator);
        }


        /// <summary>
        /// Gets a regular expression that searches for all local links (absolute path without a domain).
        /// </summary>
        /// <param name="applicationPath">The application path.</param>
        private Regex GetLinksRegex(string applicationPath)
        {
            const string QUOTES = "[\"']?";

            if (!applicationPath.EndsWith("/", StringComparison.Ordinal))
            {
                applicationPath += "/";
            }

            // Find all paths starting with the given application path. Omit double slash URLs (protocol-relative URLs)
            string applicationPathPrefix = $"/(?!/){applicationPath.TrimStart('/')}";

            string urlPath = $"{applicationPathPrefix}(?<{ATTRIBUTE_PATH}>[^\"'>\\s]*)";

            return RegexHelper.GetRegex(
                            $@"((?<{ATTRIBUTE_BEGIN}>\s(background|src|href)={QUOTES}){urlPath}(?<{ATTRIBUTE_END}>{QUOTES}(\s|>|/>)))|((?<{ATTRIBUTE_BEGIN}>url\({QUOTES}){urlPath}(?<{ATTRIBUTE_END}>{QUOTES}\)))",
                            RegexHelper.DefaultOptions | RegexOptions.IgnoreCase);
        }


        private string UnresolvePathEvaluator(Match m)
        {
            string attributeWithUri;

            if (m.Groups[ATTRIBUTE_BEGIN].Success && m.Groups[ATTRIBUTE_PATH].Success && m.Groups[ATTRIBUTE_END].Success)
            {
                string relativePath = "~/" + m.Groups[ATTRIBUTE_PATH].ToString().TrimStart('/');

                if (pathModifier != null)
                {
                    relativePath = pathModifier(relativePath);
                }

                // 'background=' or 'src=' or 'href='
                // 'url('
                attributeWithUri = m.Groups[ATTRIBUTE_BEGIN] + relativePath + m.Groups[ATTRIBUTE_END];
            }
            else
            {
                // If no main group succeeded, return original value (→ non-destructive)
                attributeWithUri = m.Groups[0].Value;
            }

            return attributeWithUri;
        }
    }
}
