using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.Newsletters.Internal
{
    /// <summary>
    /// Extension methods for <see cref="EmailTemplateInfo"/> class.
    /// </summary>
    public static class EmailTemplateInfoExtensions
    {
        /// <summary>
        /// Indicates whether the template code contains preheader macro.
        /// </summary>
        /// <remarks>
        /// The method doesn't work correctly in certain cases such as open macros, macro in comments...
        /// </remarks>
        public static bool ContainsPreheaderMacro(this EmailTemplateInfo template)
        {
            var macros = MacroProcessor.GetMacros(template.TemplateCode).ToLowerInvariant();

            return RegexHelper.GetRegex(@"(^|[^\w.])(email\.preheader|advanced\.issueinfo\.issuepreheader)([^\w.]|$)").IsMatch(macros);
        }
    }
}
