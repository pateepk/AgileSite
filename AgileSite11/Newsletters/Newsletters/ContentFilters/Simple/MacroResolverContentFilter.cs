using System;

using CMS.Newsletters.Issues.Widgets.Configuration;

namespace CMS.Newsletters.Filters
{
    /// <summary>
    /// Filter for resolving supported macros in email parts.
    /// </summary>
    internal sealed class MacroResolverContentFilter : IWidgetContentFilter
    {
        private readonly IEmailContentMacroResolver resolver;


        /// <summary>
        /// Creates an instance of <see cref="MacroResolverContentFilter"/> class.
        /// </summary>
        /// <param name="resolver">Macro resover to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="resolver"/> is null.</exception>
        public MacroResolverContentFilter(IEmailContentMacroResolver resolver)
        {
            if (resolver == null)
            {
                throw new ArgumentNullException(nameof(resolver));
            }
            this.resolver = resolver;
        }


        /// <summary>
        /// Applies the filter to the given <paramref name="text"/>.
        /// </summary>
        /// <param name="text">Text to transform.</param>
        /// <returns>Text with applied filter transformation.</returns>
        public string Apply(string text)
        {
            return resolver.Resolve(text);
        }


        /// <summary>
        /// Applies the filter to the given <paramref name="code"/>.
        /// </summary>
        /// <param name="code">Widget code to transform.</param>
        /// <param name="widget">Widget configuration.</param>
        /// <returns>Code with applied filter transformation.</returns>
        public string Apply(string code, Widget widget)
        {
            var widgetResolver = resolver.GetWidgetContentMacroResolver(widget);

            return widgetResolver.Resolve(code);
        }
    }
}
