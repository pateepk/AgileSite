using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.DocumentEngine;
using CMS.PortalEngine;


namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// Class for events requesting A/B test pages
    /// </summary>
    public class ProcessABTestEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Page's site name
        /// </summary>
        public String SiteName
        {
            get;
            set;
        }


        /// <summary>
        /// View mode
        /// </summary>
        public ViewModeEnum ViewMode
        {
            get;
            set;
        }


        /// <summary>
        /// Current page info
        /// </summary>
        public PageInfo PageInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Page info returned after A/B test is processed
        /// </summary>
        public PageInfo ReturnedPageInfo
        {
            get;
            set;
        }
    }
}
