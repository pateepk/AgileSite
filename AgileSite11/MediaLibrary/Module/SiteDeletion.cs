using System;

using CMS.Helpers;
using CMS.IO;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Site deletion actions
    /// </summary>
    internal class SiteDeletion
    {
        #region "Methods"

        /// <summary>
        /// Initializes the actions for deletion
        /// </summary>
        public static void Init()
        {
            SiteEvents.Delete.After += Delete_After;
        }


        private static void Delete_After(object sender, SiteDeletionEventArgs e)
        {
            var settings = e.Settings;
            if (!settings.DeleteMediaFiles)
            {
                return;
            }

            var path = MediaLibraryHelper.GetMediaRootFolderPath(settings.Site.SiteName);
            var progressLog = e.ProgressLog;

            LogMessage(progressLog, LogStatusEnum.Info, ResHelper.GetAPIString("Site_Delete.DeletingMediaFiles", "Deleting files folder"));

            try
            {
                if (!Directory.Exists(path))
                {
                    return;
                }

                WebFarmHelper.CreateIOTask(StorageTaskType.DeleteFolder, path, null, "deletefolder", path.Substring(SystemContext.WebApplicationPhysicalPath.Length).TrimStart('\\'));
                DirectoryHelper.DeleteDirectory(path, true);
            }
            catch
            {
                LogMessage(progressLog, LogStatusEnum.Warning, string.Format(ResHelper.GetAPIString("Site_Delete.DeletingMediaFilesWarning", "Error deleting folder '{0}'. Please delete the folder manually."), path));
            }
        }


        private static void LogMessage(IProgress<SiteDeletionStatusMessage> progressLog, LogStatusEnum status, string message)
        {
            if (progressLog == null)
            {
                return;
            }

            progressLog.Report(new SiteDeletionStatusMessage
            {
                Status = status,
                Message = message
            });
        }

        #endregion
    }
}
