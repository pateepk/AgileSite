using System;

using CMS.Base;
using CMS.DocumentEngine.Web.UI;
using CMS.PortalEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// View state log control for debug purposes.
    /// </summary>
    public class ViewStateLog : LogControl
    {
        #region "Variables"

        private bool mLogProcessed;
        private bool mDisplayTotalSize = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Display total size of view state.
        /// </summary>
        public bool DisplayTotalSize
        {
            get
            {
                return mDisplayTotalSize;
            }
            set
            {
                mDisplayTotalSize = value;
            }
        }


        /// <summary>
        /// Display only dirty items.
        /// </summary>
        public bool DisplayOnlyDirty
        {
            get;
            set;
        }


        /// <summary>
        /// Debug settings for this particular log
        /// </summary>
        public override DebugSettings Settings
        {
            get
            {
                return ViewStateDebug.Settings;
            }
        }
        
        #endregion

        
        #region "Methods"

        /// <summary>
        /// Gets the current log from the context
        /// </summary>
        protected override RequestLog GetCurrentLog()
        {
            if (!mLogProcessed && (PortalContext.ViewMode.IsLiveSite() || (PortalContext.ViewMode == ViewModeEnum.DashboardWidgets)))
            {
                // Log view state
                RequestLog log = ViewStateDebug.GetViewStates(Page);
                mLogProcessed = true;

                // Display live
                if (ViewStateDebug.Settings.Live && (PortalContext.ViewMode != ViewModeEnum.DashboardWidgets))
                {
                    return log;
                }
            }

            return null;
        }

        #endregion
    }
}