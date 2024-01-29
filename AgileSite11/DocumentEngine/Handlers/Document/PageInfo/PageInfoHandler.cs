using CMS.PortalEngine;
using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Page info handler
    /// </summary>
    public class PageInfoHandler : AdvancedHandler<PageInfoHandler, PageInfoEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="pi">Page info</param>
        /// <param name="pti">Page template instance</param>
        public PageInfoHandler StartEvent(PageInfo pi, PageTemplateInstance pti)
        {
            PageInfoEventArgs e = new PageInfoEventArgs()
            {
                PageInfo = pi,
                PageTemplateInstance = pti
            };

            return StartEvent(e);
        }
    }
}