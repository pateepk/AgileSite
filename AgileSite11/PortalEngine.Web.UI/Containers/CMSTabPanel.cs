using System;
using System.Linq;
using System.Text;

using AjaxControlToolkit;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// CMSTabPanel control extends the original <see cref="AjaxControlToolkit.TabPanel"/> control with short ID and portal engine support.
    /// </summary>
    public class CMSTabPanel : TabPanel
    {
        #region "Properties"

        /// <summary>
        /// Web part zone covered by this panel
        /// </summary>
        public CMSWebPartZone WebPartZone 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// If true, the tab hides if the underlying web part zone is empty
        /// </summary>
        public bool HideIfZoneEmpty 
        { 
            get; 
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            Page.PreRenderComplete += new EventHandler(Page_PreRenderComplete);
        }


        /// <summary>
        /// PreRender complete event handler
        /// </summary>
        protected void Page_PreRenderComplete(object sender, EventArgs e)
        {
            if (WebPartZone != null)
            {
                // Set visibility based on the underlying zone content visibility
                if (!WebPartZone.ZoneIsVisible || (!WebPartZone.ZoneHeaderVisible && HideIfZoneEmpty && WebPartZone.IsEmpty))
                {
                    Visible = false;
                }
            }
        }

        #endregion
    }
}
