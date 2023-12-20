namespace CMS.Newsletters.Filters
{
    /// <summary>
    /// Filter to transform email content.
    /// </summary>
    public interface IEmailContentFilter
    {
        /// <summary>
        /// Applies the filter to the given <paramref name="text"/>.
        /// </summary>
        /// <param name="text">Text to transform.</param>
        /// <returns>Text with applied filter transformation.</returns>
        string Apply(string text);
    }
}