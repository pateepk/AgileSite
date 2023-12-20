using System;

using CMS.Base;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
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
            SiteEvents.Delete.Before += Delete_Before;
            SiteEvents.Delete.After += Delete_After;
        }


        private static void Delete_After(object sender, SiteDeletionEventArgs e)
        {
            var settings = e.Settings;
            if (!settings.DeleteAttachments)
            {
                return;
            }

            var path = AttachmentBinaryHelper.GetFilesFolderPath(settings.Site.SiteName);
            var progressLog = e.ProgressLog;

            LogMessage(progressLog, LogStatusEnum.Info, ResHelper.GetAPIString("Site_Delete.DeletingAttachments", "Deleting files folder"));

            try
            {
                if (!Directory.Exists(path))
                {
                    return;
                }

                WebFarmHelper.CreateIOTask(new DeleteFolderWebFarmTask {
                    Path = path.Substring(SystemContext.WebApplicationPhysicalPath.Length).TrimStart('\\'),
                    TaskFilePath = path
                });
                DirectoryHelper.DeleteDirectory(path, true);
            }
            catch
            {
                LogMessage(progressLog, LogStatusEnum.Warning, String.Format(ResHelper.GetAPIString("Site_Delete.DeletingAttachmentsWarning", "Error deleting folder '{0}'. Please delete the folder manually."), path));
            }
        }


        private static void Delete_Before(object sender, SiteDeletionEventArgs e)
        {
            var settings = e.Settings;
            var progressLog = e.ProgressLog;

            // Delete records in CMS_Tree and CMS_Document
            var tree = new TreeProvider(MembershipContext.AuthenticatedUser);
            tree.LogSynchronization = false;
            tree.LogEvents = false;
            tree.EnableNotifications = false;
            tree.TouchCacheDependencies = false;
            tree.AllowAsyncActions = false;
            tree.LogWebFarmTasks = false;

            // Delete site tree
            DocumentHelper.DeleteSiteTree(settings.Site.SiteName, tree, node => LogMessage(progressLog, LogStatusEnum.Info, String.Format(ResHelper.GetAPIString("Site_Delete.DeletingDocument", "Deleting page '{0}' ({1})"), node.NodeAliasPath, node.DocumentCulture)));
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
