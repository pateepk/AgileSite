using System;
using System.Web.UI;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Button control with localized text string.
    /// </summary>
    [ToolboxData("<{0}:LocalizedButton runat=server />"), Serializable()]
    public class LocalizedButton : CMSButton
    {
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


        /// <summary>
        /// Refreshes buttons's text on pre-render.
        /// </summary>        
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Get the localized text
            Text = ControlsLocalization.GetLocalizedText(this, ResourceString, Source, Text);
            ToolTip = ControlsLocalization.GetLocalizedText(this, ToolTipResourceString, Source, ToolTip);
            
        }
    }
}