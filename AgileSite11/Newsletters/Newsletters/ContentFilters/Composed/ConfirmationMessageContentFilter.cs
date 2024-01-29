namespace CMS.Newsletters.Filters
{
    /// <summary>
    /// Composed filter for transforming the confirmation email content right before the sending.
    /// </summary>
    internal sealed class ConfirmationMessageContentFilter : IEmailContentFilter
    {
        private readonly IWidgetContentFilter macroFilter;
        private readonly IEmailContentFilter urlResolverFilter;
        private readonly IEmailContentFilter replacerFilter;


        /// <summary>
        /// Creates instance of <see cref="ConfirmationMessageContentFilter"/> class.
        /// </summary>
        /// <param name="resolver">Macro resover to use.</param>
        /// <param name="baseUrl">Base URL to use for resolving.</param>
        /// <param name="subscriber">Subscriber.</param>
        public ConfirmationMessageContentFilter(IEmailContentMacroResolver resolver, string baseUrl, SubscriberInfo subscriber)
        {
            macroFilter = new MacroResolverContentFilter(resolver);
            urlResolverFilter = new UrlResolverContentFilter(baseUrl);
            if (subscriber != null)
            {
                replacerFilter = new MacroReplacerContentFilter(subscriber.SubscriberEmail);
            }
        }


        /// <summary>
        /// Applies the filter to the given <paramref name="text"/>.
        /// </summary>
        /// <param name="text">Text to transform.</param>
        /// <returns>Text with applied filter transformation.</returns>
        public string Apply(string text)
        {
            text = replacerFilter?.Apply(text) ?? text;
            text = macroFilter.Apply(text);
            text = urlResolverFilter.Apply(text);

            return text;
        }
    }
}
