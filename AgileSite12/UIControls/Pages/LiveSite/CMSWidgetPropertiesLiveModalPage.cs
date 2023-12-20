using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the properties pages to apply global settings to the pages.
    /// </summary>
    public abstract class CMSWidgetPropertiesLiveModalPage : CMSWidgetPropertiesLivePage
    {
        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Modal dialog registration
            SetLiveRTL();
            RegisterEscScript();
        }


        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            RegisterModalPageScripts();
        }
    }
}