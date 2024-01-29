using System;

using CMS.UIControls;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Base page for the variant dialog pages.
    /// </summary>
    public abstract class CMSVariantDialogPage : CMSContentPage
    {
        /// <summary>
        /// OnInit
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Disable confirm changes checking
            DocumentManager.RegisterSaveChangesScript = false;
        }
    }
}