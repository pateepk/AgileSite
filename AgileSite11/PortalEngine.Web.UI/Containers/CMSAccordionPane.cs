using System;
using System.ComponentModel;

using AjaxControlToolkit;

using CMS.Base.Web.UI;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// CMSAccordionPane control extends the original <see cref="AjaxControlToolkit.AccordionPane"/> control with short ID and portal engine support.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSAccordionPane : AccordionPane, IShortID
    {
        #region "Properties"

        /// <summary>
        /// Short ID of the control.
        /// </summary>
        public string ShortID
        {
            get;
            set;
        }


        /// <summary>
        /// Web part zone covered by this pane
        /// </summary>
        public CMSWebPartZone WebPartZone
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            this.SetShortID();

            base.OnInit(e);
        }


        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            Page.PreRenderComplete += Page_PreRenderComplete;
        }


        /// <summary>
        /// PreRender complete event handler
        /// </summary>
        private void Page_PreRenderComplete(object sender, EventArgs e)
        {
            // Hide the pane if zone is not visible
            if ((WebPartZone != null) && !WebPartZone.ZoneIsVisible)
            {
                Visible = false;
            }
        }

        #endregion
    }
}