using CMS.Base;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Arguments for the template cloning handler.
    /// </summary>
    public class PageTemplateCloneEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Original (the one which is being cloned) page template.
        /// </summary>
        public PageTemplateInfo OriginalPageTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Newly created template (a clone).
        /// </summary>
        public PageTemplateInfo NewPageTemplate
        {
            get;
            set;
        }
    }
}