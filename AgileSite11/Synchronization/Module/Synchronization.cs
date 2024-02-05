using System.Collections.Generic;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Synchronization actions
    /// </summary>
    internal class Synchronization
    {
        #region "Methods"

        /// <summary>
        /// Initializes the actions for synchronization
        /// </summary>
        public static void Init()
        {
            SynchronizationActionManager.RegisterAction(LogStaging, CheckLogStaging);
            SynchronizationActionManager.RegisterAction(LogIntegration, CheckLogIntegration);
            SynchronizationActionManager.RegisterAction(CreateVersion, CheckCreateVersion);
        }


        private static bool CheckCreateVersion(LogObjectChangeSettings settings)
        {
            if (!settings.WorkerCall)
            {
                settings.CreateVersion &= SynchronizationHelper.CheckCreateVersion(settings.InfoObj, settings.TaskType);
            }

            return settings.CreateVersion;
        }


        private static IEnumerable<ISynchronizationTask> CreateVersion(LogObjectChangeSettings settings)
        {
            if (settings.CreateVersion)
            {
                // Ensure version for object deletion
                if (settings.TaskType == TaskTypeEnum.DeleteObject)
                {
                    ObjectVersionManager.EnsureDeletedVersion(settings.InfoObj);
                }
                // Create version for object
                else
                {
                    var userId = GetUserId(settings);
                    ObjectVersionManager.CreateVersion(settings.InfoObj, userId);
                }
            }

            return null;
        }


        private static bool CheckLogIntegration(LogObjectChangeSettings settings)
        {
            if (!settings.WorkerCall)
            {
                settings.LogIntegration &= SynchronizationHelper.CheckIntegrationLogging(settings.InfoObj);
            }

            return settings.LogIntegration;
        }


        private static IEnumerable<ISynchronizationTask> LogIntegration(LogObjectChangeSettings settings)
        {
            List<ISynchronizationTask> tasks = null;

            if (settings.LogIntegration)
            {
                tasks = new List<ISynchronizationTask>();

                // Log simple tasks
                if (settings.LogIntegrationSimpleTasks)
                {
                    var intSimpleTasks = IntegrationHelper.LogIntegration(settings.InfoObj, settings.TaskType, settings.SiteID, TaskProcessTypeEnum.AsyncSimple, TaskProcessTypeEnum.AsyncSimpleSnapshot);
                    tasks.AddRange(intSimpleTasks);
                }

                if (!settings.IsTouchParent)
                {
                    // Log SnapShot tasks
                    var intTasks = IntegrationHelper.LogIntegration(settings.InfoObj, settings.TaskType, settings.SiteID, TaskProcessTypeEnum.AsyncSnapshot);
                    tasks.AddRange(intTasks);
                }
            }

            return tasks;
        }


        private static bool CheckLogStaging(LogObjectChangeSettings settings)
        {
            if (!settings.WorkerCall)
            {
                settings.LogStaging &= SynchronizationHelper.CheckStagingLogging(settings.InfoObj);
            }

            return settings.LogStaging;
        }


        private static IEnumerable<ISynchronizationTask> LogStaging(LogObjectChangeSettings settings)
        {
            List<ISynchronizationTask> tasks = null;

            if (settings.LogStaging)
            {
                tasks = new List<ISynchronizationTask> { StagingTaskInfoProvider.LogSynchronization(settings.InfoObj, settings.TaskType, settings.InfoObj.ObjectSiteName, settings.SiteID, settings.ServerID) };
            }

            return tasks;
        }


        /// <summary>
        /// Returns user id obtained from settings or from <see cref="CMSActionContext"/> when settings does not have assigned user
        /// </summary>
        private static int GetUserId(AbstractSynchronizationSettings settings)
        {
            return (settings.User ?? CMSActionContext.CurrentUser).UserID;
        }

        #endregion
    }
}
