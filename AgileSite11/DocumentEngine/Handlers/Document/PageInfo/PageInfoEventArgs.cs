using CMS.PortalEngine;
using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// PageInfo event arguments
    /// </summary>
    public class PageInfoEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Page info
        /// </summary>
        public PageInfo PageInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Page template instance which is currently processed
        /// </summary>
        public PageTemplateInstance PageTemplateInstance
        {
            get;
            set;
        }
    }
}