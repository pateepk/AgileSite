using CMS.DataEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Interface for SyncManager
    /// </summary>
    public interface ISyncManager
    {
        #region "Properties"

        /// <summary>
        /// Indicates if logging staging tasks is enabled.
        /// </summary>
        bool LogTasks
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets current site name.
        /// </summary>
        string SiteName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets site ID.
        /// </summary>
        int SiteID
        {
            get;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Determines whether to continue with translation operations.
        /// </summary>
        /// <param name="th">Current translation helper object</param>
        /// <returns>TRUE if OperationType is other than Integration. When OperationType is  Integration there also have to be some translations present.</returns>
        bool ProceedWithTranslations(TranslationHelper th);

        #endregion
    }
}
