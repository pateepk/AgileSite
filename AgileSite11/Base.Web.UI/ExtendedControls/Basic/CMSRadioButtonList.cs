using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Base control for radiobutton lists. Supports localization.
    /// </summary>
    [ToolboxData("<{0}:CMSRadioButtonList runat=server />"), Serializable()]
    public class CMSRadioButtonList : RadioButtonList
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
        public CMSRadioButtonList()
        {
            RepeatLayout = RepeatLayout.Flow;
            this.AddCssClass("radio");
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
                    this.AddCssClass("radio-list-vertical");
                }
                else
                {
                    this.AddCssClass("radio-list-horizontal");
                }
            }

            // Ensure class for radio buttons
            this.AddCssClass("radio");

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