using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Summary description for CMSLiveModalPage.
    /// </summary>
    public abstract class CMSLiveModalPage : LivePage
    {
        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public CMSLiveModalPage()
        {
            Load += CMSLiveModalPage_Load;
        }


        /// <summary>
        /// Page load event
        /// </summary>
        protected void CMSLiveModalPage_Load(object sender, EventArgs e)
        {
            SetLiveCulture();
            SetLiveRTL();
            SetLiveDialogClass();
            RegisterEscScript();
        }


        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            RegisterModalPageScripts();
            RegisterDialogCSSLink();
        }

        #endregion
    }
}