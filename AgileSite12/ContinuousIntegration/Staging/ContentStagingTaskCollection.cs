using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.Synchronization;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Holds content related objects with <see cref="TaskTypeEnum"/> actions based on changes during continuous integration restore operation.
    /// </summary>
    public class ContentStagingTaskCollection
    {
        private readonly List<StagingTask> tasks = new List<StagingTask>();


        /// <summary>
        /// Event raised when Continuous Integration restore action is done 
        /// and any task was collected in the collection to be processed.
        /// </summary>
        public static readonly SimpleHandler<TasksCollectedEventArgs> TasksCollected = new SimpleHandler<TasksCollectedEventArgs>();


        /// <summary>
        /// Adds staging task of type <paramref name="taskType"/> for given <paramref name="baseInfo"/>.
        /// If the baseInfo has <see cref="SynchronizationTypeEnum.LogSynchronization"/> set, or Content staging is disabled,
        /// staging task for the baseInfo is NOT added into the collection.
        /// </summary>
        /// <param name="baseInfo">Info object.</param>
        /// <param name="taskType">Task type.</param>
        public void Add(BaseInfo baseInfo, TaskTypeEnum taskType)
        {
            if (HasLogSynchronizationSet(baseInfo))
            {
                // Do not collect tasks for objects, which are synchronized by system
                return;
            }

            if (IsContentStagingEnabled(baseInfo.Generalized.ObjectSiteName) && IsAnyStagingServerEnabled(baseInfo.Generalized.ObjectSiteID))
            {
                // Collect tasks for objects only if content staging and any staging server for the site are enabled
                tasks.Add(new StagingTask(baseInfo, taskType));
            }
        }


        private static bool IsContentStagingEnabled(string siteName)
        {
            return StagingTaskInfoProvider.LogContentChanges(siteName);
        }


        private static bool IsAnyStagingServerEnabled(int siteId)
        {
            return ServerInfoProvider.IsEnabledServer(siteId);
        }


        private static bool HasLogSynchronizationSet(BaseInfo info)
        {
            return info.TypeInfo.SynchronizationSettings.LogSynchronization == SynchronizationTypeEnum.LogSynchronization;
        }


        /// <summary>
        /// Raises the <see cref="TasksCollected"/> event, if there are any tasks to log.
        /// </summary>
        public void RaiseTasksCollected()
        {
            if (!tasks.Any())
            {
                return;
            }

            var args = new TasksCollectedEventArgs
            {
                Tasks = tasks.AsReadOnly()
            };

            if (TasksCollected.IsBound)
            {
                TasksCollected.StartEvent(args);
            }
        }
    }
}
