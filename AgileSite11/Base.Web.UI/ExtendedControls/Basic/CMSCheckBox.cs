using System;
using System.ComponentModel;
using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// CMSCheckBox with support for localization.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSCheckBox : CheckBox
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


        #region "Methods"

        /// <summary>
        /// Add control default CSS class and get localized text for control.
        /// </summary>        
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            this.AddCssClass("checkbox");

            // Get the localized text. Hack is used to render label tag even if no text is available.
            Text = DataHelper.GetNotEmpty(ControlsLocalization.GetLocalizedText(this, ResourceString, Source, Text), "&nbsp;");
            ToolTip = ControlsLocalization.GetLocalizedText(this, ToolTipResourceString, Source, ToolTip);
        }

        #endregion
    }
}
