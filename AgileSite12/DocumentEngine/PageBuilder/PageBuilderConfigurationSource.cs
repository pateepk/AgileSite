namespace CMS.DocumentEngine.PageBuilder.Internal
{
    /// <summary>
    /// Represents source for Page builder configuration.
    /// </summary>
    public sealed class PageBuilderConfigurationSource
    {
        /// <summary>
        /// Source of widgets configuration in JSON format.
        /// </summary>
        public string WidgetsConfiguration { get; set; }


        /// <summary>
        /// Source of page template configuration in JSON format.
        /// </summary>
        public string PageTemplateConfiguration { get; set; }
    }
}
