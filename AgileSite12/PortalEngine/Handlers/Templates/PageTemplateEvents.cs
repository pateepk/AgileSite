using System;
using System.Linq;
using System.Text;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Page templates events.
    /// </summary>
    public static class PageTemplateEvents
    {
        /// <summary>
        /// Fires when template is cloned as ad-hoc.
        /// </summary>
        public static PageTemplateCloneHandler PageTemplateCloneAsAdHoc = new PageTemplateCloneHandler { Name = "PageTemplateEvents.PageTemplateCloneAsAdHoc" };
    }
}