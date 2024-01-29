namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document events
    /// </summary>
    public static class PageInfoEvents
    {
        /// <summary>
        /// Fires when page template instance is combined with other instances
        /// </summary>
        public static PageInfoHandler CombinePageTemplateInstance = new PageInfoHandler { Name = "PageInfoEvents.CombinePageTemplateInstance" };
    }
}