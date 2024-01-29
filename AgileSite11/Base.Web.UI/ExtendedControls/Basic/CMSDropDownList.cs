using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Drop down list with localized text string.
    /// </summary>
    [ToolboxData("<{0}:CMSDropDownList runat=server />"), Serializable()]
    public class CMSDropDownList : DropDownList
    {
        #region "Properties"

        /// <summary>
        /// Localized string source property - may be 'database' or 'file'.
        /// </summary>
        public string Source
        {
            get;
            set;
        }


        /// <summary>
        /// Use resource strings instead of item texts.
        /// </summary>
        public bool UseResourceStrings
        {
            get;
            set;
        }


        /// <summary>
        /// Name of a resource string used for tooltip.
        /// </summary>
        public string ToolTipResourceString
        {
            get;
            set;
        }

        #endregion


        #region "Page events"
        
        /// <summary>
        /// Initializes control
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.AddCssClass("form-control");
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Localize tooltip
            ToolTip = ControlsLocalization.GetLocalizedText(this, ToolTipResourceString, Source, ToolTip);

            // Localize the items
            foreach (ListItem item in Items)
            {
                string resourceString = UseResourceStrings ? item.Text : string.Empty;
                item.Text = ControlsLocalization.GetLocalizedText(this, resourceString, Source, item.Text);
            }
        }

        #endregion
    }
}
