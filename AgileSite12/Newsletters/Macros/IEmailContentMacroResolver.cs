using CMS.Newsletters.Issues.Widgets.Configuration;

namespace CMS.Newsletters
{
    /// <summary>
    /// Interface for email content macro resolver.
    /// </summary>
    public interface IEmailContentMacroResolver
    {
        /// <summary>
        /// Resolves the dynamic field macros, replaces the {%dynamicfieldname%} macros with the dynamic field values.
        /// </summary>
        /// <param name="text">Text containing dynamic fields to resolve.</param>
        string Resolve(string text);


        /// <summary>
        /// Creates child instance of current resolver with data context of given <paramref name="widget"/> configuration properties.
        /// </summary>
        /// <param name="widget">Widget configuration.</param>
        IEmailContentMacroResolver GetWidgetContentMacroResolver(Widget widget);
    }
}