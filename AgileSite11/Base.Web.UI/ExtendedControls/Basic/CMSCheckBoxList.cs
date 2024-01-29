using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Base control for checkbox lists. Supports localization.
    /// </summary>
    [ToolboxData("<{0}:CMSCheckBoxList runat=server />"), Serializable()]
    public class CMSCheckBoxList : CheckBoxList
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


        #region "Constructors"

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CMSCheckBoxList()
        {
            RepeatLayout = RepeatLayout.Flow;
            this.AddCssClass("checkbox");
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Set appropriate CSS class
            if (RepeatLayout == RepeatLayout.Flow)
            {
                if (RepeatDirection == RepeatDirection.Vertical)
                {
                    this.AddCssClass("checkbox-list-vertical");
                }
                else
                {
                    this.AddCssClass("checkbox-list-horizontal");
                }
            }

            // Ensure checkbox class
            this.AddCssClass("checkbox");

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
