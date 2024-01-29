using System;

using CMS.Base;
using CMS.Base.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Panel that displays only if the development mode is enabled
    /// </summary>
    public class DevelopmentMode : CMSPlaceHolder
    {
        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            // Display only in development mode
            Visible = SystemContext.DevelopmentMode;

            base.OnInit(e);
        }
    }
}