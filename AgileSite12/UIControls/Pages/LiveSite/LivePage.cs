using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the live site pages to apply global settings to the pages.
    /// </summary>
    public abstract class LivePage : CMSPage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        protected LivePage()
        {
            SetLiveCulture();

            RegisterPageLoadedScript = false;

            Load += BasePage_Load;
        }


        /// <summary>
        /// PageLoad event handle
        /// </summary>
        protected void BasePage_Load(object sender, EventArgs e)
        {
            SetLiveRTL();
            SetBrowserClass();
            AddNoCacheTag();
        }
    }
}