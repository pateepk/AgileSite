using CMS.Newsletters.Internal;

namespace CMS.Newsletters.Filters
{
    /// <summary>
    /// Filter for disabling the links in email content to not to be clickable.
    /// </summary>
    internal sealed class LinksPacifierContentFilter : IEmailContentFilter
    {
        private readonly bool isFragment;
        private readonly IEmailHtmlModifier customModifier;


        /// <param name="isFragment">Indicates if the transformed text is HTML fragment, not complete HTML document.</param>
        /// <param name="customModifier">Custom HTML modifier. If not provided, default <see cref="EmailHtmlModifier"/> is used.</param>
        public LinksPacifierContentFilter(bool isFragment, IEmailHtmlModifier customModifier = null)
        {
            this.isFragment = isFragment;
            this.customModifier = customModifier;
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

            var modifier = customModifier ?? new EmailHtmlModifier(text, isFragment);
            modifier.DisableLinks();

            return modifier.GetHtml();
        }
    }
}
