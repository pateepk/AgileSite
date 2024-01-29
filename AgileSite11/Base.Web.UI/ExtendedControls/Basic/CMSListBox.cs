using System;

using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// List box control with localized text string.
    /// </summary>
    [ToolboxData("<{0}:CMSListBox runat=server />"), Serializable()]
    public class CMSListBox : ListBox
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


        /// <summary>
        /// Indicates if the control should render <c>disabled="disabled"</c> attribute when the control's <see cref="WebControl.IsEnabled"/> property is false.
        /// </summary>
        /// <remarks>ListBox control does not render disabled attribute since ASP.NET 4.</remarks>
        public override bool SupportsDisabledAttribute
        {
            get
            {
                return true;
            }
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
