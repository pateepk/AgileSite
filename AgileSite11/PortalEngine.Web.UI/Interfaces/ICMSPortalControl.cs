using System;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Portal control interface.
    /// </summary>
    public interface ICMSPortalControl
    {
        /// <summary>
        /// Parent page placeholder.
        /// </summary>
        CMSPagePlaceholder PagePlaceholder
        {
            get;
            set;
        }


        /// <summary>
        /// Parent portal manager.
        /// </summary>
        CMSPortalManager PortalManager
        {
            get;
            set;
        }
    }
}