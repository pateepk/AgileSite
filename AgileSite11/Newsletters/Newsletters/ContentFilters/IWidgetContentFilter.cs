using CMS.Newsletters.Issues.Widgets.Configuration;

namespace CMS.Newsletters.Filters
{
    /// <summary>
    /// Filter able to transform widget content or email parts.
    /// </summary>
    public interface IWidgetContentFilter : IEmailContentFilter
    {
        /// <summary>
        /// Applies the filter to the given <paramref name="code"/>.
        /// </summary>
        /// <param name="code">Widget code to transform.</param>
        /// <param name="widget">Widget configuration.</param>
        /// <returns>Code with applied filter transformation.</returns>
        string Apply(string code, Widget widget);
    }
}