using System;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// RadioButton control with localized text string.
    /// </summary>
    [ToolboxData("<{0}:CMSRadioButton runat=server></{0}:CMSRadioButton>"), Serializable()]
    public class CMSRadioButton : RadioButton
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
        /// Name of a resource string used for text.
        /// </summary>
        public string ResourceString
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


        /// <summary>
        /// Add control default CSS class.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.AddCssClass("radio");
        }


        /// <summary>
        /// Get localized text for control.
        /// </summary>        
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Get the localized text. Hack is used to render label tag even if no text is available.
            Text = DataHelper.GetNotEmpty(ControlsLocalization.GetLocalizedText(this, ResourceString, Source, Text), "&nbsp;");
            ToolTip = ControlsLocalization.GetLocalizedText(this, ToolTipResourceString, Source, ToolTip);
        }
    }
}
