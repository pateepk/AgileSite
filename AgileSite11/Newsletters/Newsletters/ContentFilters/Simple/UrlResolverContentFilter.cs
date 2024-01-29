namespace CMS.Newsletters.Filters
{
    /// <summary>
    /// Filter for resolving the relative URLs in the email content to absolute.
    /// </summary>
    /// <remarks>Uses regular expression to find URLs in the email content.</remarks>
    internal sealed class UrlResolverContentFilter : IEmailContentFilter
    {
        private readonly string baseUrl;


        /// <summary>
        /// Creates an instance of <see cref="UrlResolverContentFilter"/> class.
        /// </summary>
        /// <param name="baseUrl">Base URL to use for resolving.</param>
        public UrlResolverContentFilter(string baseUrl)
        {
            this.baseUrl = baseUrl;
        }


        /// <summary>
        /// Applies the filter to the given <paramref name="text"/>.
        /// </summary>
        /// <param name="text">Text to transform.</param>
        /// <returns>Text with applied filter transformation.</returns>
        public string Apply(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            return LinkConverter.ConvertToAbsolute(text, baseUrl);
        }
    }
}
