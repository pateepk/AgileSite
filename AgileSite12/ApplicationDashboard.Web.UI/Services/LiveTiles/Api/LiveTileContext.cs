using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.SiteProvider;

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Context of the live tile passed to the live tile model providers.
    /// </summary>
    public sealed class LiveTileContext
    {
        /// <summary>
        /// Gets or sets the site for which the model is requested.
        /// </summary>
        public SiteInfo SiteInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the user for which the model is requested.
        /// </summary>
        public IUserInfo UserInfo
        {
            get;
            set;
        }
    }
}
