using System;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Page mode enumeration.
    /// </summary>
    public enum ViewModeEnum
    {
        #region "Base page modes (available for cookie)"

        /// <summary>
        /// Live site mode.
        /// </summary>
        LiveSite = 0,

        /// <summary>
        /// Preview mode.
        /// </summary>
        Preview = 2,

        /// <summary>
        /// Edit mode - content editing.
        /// </summary>
        Edit = 3,

        /// <summary>
        /// Edit mode with disabled controls - when not authorized or cannot edit.
        /// </summary>
        Design = 6,

        /// <summary>
        /// Edit form mode - for portal engine pages.
        /// </summary>
        EditForm = 8,

        /// <summary>
        /// Unknown / does not matter
        /// </summary>
        Unknown = 9,
        
        /// <summary>
        /// On-Site edit mode
        /// </summary>
        EditLive = 18,

        /// <summary>
        /// Listing mode.
        /// </summary>
        Listing = 20,

        /// <summary>
        /// Section edit mode.
        /// </summary>
        SectionEdit = 21,

        /// <summary>
        /// UI Page
        /// </summary>
        UI = 22,

        /// <summary>
        /// Design mode for web parts
        /// </summary>
        DesignWebPart = 23,

        #endregion


        #region "Special modes"

        /// <summary>
        /// Edit mode - content editing (editing disabled).
        /// </summary>
        EditDisabled = 4,

        /// <summary>
        /// Edit mode - not current page.
        /// </summary>
        EditNotCurrent = 5,

        /// <summary>
        /// Design mode with disabled controls - for portal engine pages.
        /// </summary>
        DesignDisabled = 7,

        /// <summary>
        /// User widgets mode.
        /// </summary>
        UserWidgets = 12,

        /// <summary>
        /// User widgets mode with disabled functionality (for preview mode).
        /// </summary>
        UserWidgetsDisabled = 13,

        /// <summary>
        /// Group widgets mode.
        /// </summary>
        GroupWidgets = 14,

        /// <summary>
        /// Dashboard widgets mode.
        /// </summary>
        DashboardWidgets = 15

        #endregion
    }
}

