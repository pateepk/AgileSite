using System;

using CMS.Base;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the debug tab of administration -> system pages to apply global settings to the pages.
    /// </summary>
    public abstract class CMSDebugPage : GlobalAdminPage
    {
        /// <summary>
        /// PreInit event handler
        /// </summary>
        protected override void OnPreInit(EventArgs e)
        {
            // Disable debugging
            DisableDebugging();

            base.OnPreInit(e);
        }


        /// <summary>
        /// Loads the log control
        /// </summary>
        /// <param name="log">Request log</param>
        /// <param name="path">Control path</param>
        /// <param name="index">Control index</param>
        protected LogControl LoadLogControl(RequestLog log, string path, int index)
        {
            // Load the control
            LogControl logCtrl = (LogControl)LoadUserControl(path);

            logCtrl.ID = "L" + index;
            logCtrl.EnableViewState = false;
            logCtrl.DisplayHeader = false;
            logCtrl.Log = log;
                            
            return logCtrl;
        }
    }
}