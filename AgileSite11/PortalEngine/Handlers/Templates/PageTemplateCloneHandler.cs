using CMS.Base;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Handler for the template cloning.
    /// </summary>
    public class PageTemplateCloneHandler : AdvancedHandler<PageTemplateCloneHandler, PageTemplateCloneEventArgs>
    {
        /// <summary>
        /// Initiates event handling.
        /// </summary>
        /// <param name="originalPageTemplate">Original (the one which is being cloned) page template</param>
        /// <returns>Event handler</returns>
        public PageTemplateCloneHandler StartEvent(PageTemplateInfo originalPageTemplate)
        {
            var e = new PageTemplateCloneEventArgs()
            {
                OriginalPageTemplate = originalPageTemplate,
                NewPageTemplate = null,
            };

            return StartEvent(e);
        }
    }
}