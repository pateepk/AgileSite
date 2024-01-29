using System;
using System.ComponentModel;

using AjaxControlToolkit;

using CMS.Base.Web.UI;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// CMSAccordion control extends the original <see cref="AjaxControlToolkit.Accordion"/> control with short ID support.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSAccordion : Accordion, IShortID
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

        #endregion


        #region "Methods"

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            this.SetShortID();

            base.OnInit(e);
        }

        #endregion
    }
}