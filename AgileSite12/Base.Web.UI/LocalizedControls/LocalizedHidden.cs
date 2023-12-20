using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Hidden field with localized value string.
    /// </summary>
    [ToolboxData("<{0}:LocalizedHidden runat=server></{0}:LocalizedHidden>"), Serializable()]
    public class LocalizedHidden : HiddenField
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
        /// Refreshes hidden field's text on pre-render.
        /// </summary>        
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Get the localized text
            Value = ControlsLocalization.GetLocalizedText(this, ResourceString, Source, Value);
        }
    }
}