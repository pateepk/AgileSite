using System;
using System.Linq;
using System.Text;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Scheduler;
using CMS.SiteProvider;

namespace CMS.SharePoint
{
    /// <summary>
    /// Handles SharePoint library synchronization when activated by the scheduler.
    /// </summary>
    public class SharePointLibrarySynchronizationTask : ITask
    {
        #region "Constants - public"

        /// <summary>
        /// Scheduled task name prefix for SharePoint library synchronization.
        /// </summary>
        internal const string TASK_CODENAME_PREFIX = "SharePointLibrarySync";


        /// <summary>
        /// Scheduled task name format for SharePoint library synchronization.
        /// Usage: <code>var completeTaskCodeName = String.Format(SharePointLibrarySynchronizationTask.TASK_CODENAME_FORMAT, {SharePointLibraryID});</code>
        /// </summary>
        internal const string TASK_CODENAME_FORMAT = TASK_CODENAME_PREFIX + "_{0}";

        #endregion


        #region "Methods"

        /// <summary>
        /// Executes synchronization of <see cref="SharePointLibraryInfo"/>.
        /// </summary>
        /// <param name="taskInfo">Contains task data with information related to the SharePoint library.</param>
        /// <returns>Null on success, error description otherwise.</returns>
        public string Execute(TaskInfo taskInfo)
        {
            if (!IsFeatureAvailable(taskInfo))
            {
                return null;
            }

            int libraryId = ValidationHelper.GetInteger(taskInfo.TaskData, 0);
            SharePointLibraryInfo libraryInfo = SharePointLibraryInfoProvider.GetSharePointLibraryInfo(libraryId);
            if (libraryInfo == null)
            {
                return "SharePoint library with ID " + libraryId + " not found.";
            }

            try
            {
                SharePointLibraryHelper.SynchronizeLibrary(libraryInfo);
            }
            catch (SharePointConnectionNotFoundException)
            {
                // The library is read-only
                return String.Format("Share point library '{0}' with ID {1} has no connection and thus can not be synchronized.", libraryInfo.SharePointLibraryName, libraryId);
            }

            return null;
        }


        /// <summary>
        /// Gets synchronization task code name based on SharePointLibraryInfo identifier.
        /// </summary>
        /// <param name="sharePointLibraryId">ID of SharePointLibraryInfo</param>
        /// <returns>Task code name</returns>
        internal static string GetTaskCodeName(int sharePointLibraryId)
        {
            return String.Format(TASK_CODENAME_FORMAT, sharePointLibraryId);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Checks whether SharePoint is available.
        /// </summary>
        /// <param name="taskInfo">Task info.</param>
        /// <returns>True if SharePoint is available, false otherwise.</returns>
        private bool IsFeatureAvailable(TaskInfo taskInfo)
        {
            SiteInfo siteInfo = SiteInfoProvider.GetSiteInfo(taskInfo.TaskSiteID);
            if (!LicenseKeyInfoProvider.IsFeatureAvailable(siteInfo.DomainName, FeatureEnum.SharePoint))
            {
                return false;
            }
            return true;
        }

        #endregion
    }
}
