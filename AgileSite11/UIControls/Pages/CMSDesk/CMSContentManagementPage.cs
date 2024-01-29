using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the MyDesk pages to apply global settings to the pages.
    /// </summary>
    public abstract class CMSContentManagementPage : CMSDeskPage
    {
        /// <summary>
        /// PreInit event handler
        /// </summary>
        protected override void OnPreInit(EventArgs e)
        {
            CheckDocPermissions = false;
            base.OnPreInit(e);
        }
    }
}