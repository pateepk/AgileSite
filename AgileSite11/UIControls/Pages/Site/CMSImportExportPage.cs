using System;

using CMS.Base;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the Import/Export pages.
    /// </summary>
    public abstract class CMSImportExportPage : GlobalAdminPage
    {
        /// <summary>
        /// PreInit event handler
        /// </summary>
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            if (!DebugHelper.DebugImportExport)
            {
                DisableDebugging();
            }
        }
    }
}