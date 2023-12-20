using System.Linq;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Extends <see cref="AbstractSynchronizationSettings"/> class.
    /// </summary>
    public static class AbstractSynchronizationSettingsExtensionMethods
    {
        /// <summary>
        /// Sets <see cref="AbstractSynchronizationSettings.TaskGroups"/> and <see cref="AbstractSynchronizationSettings.User"/> for current instance of
        /// <see cref="AbstractSynchronizationSettings"/>, so the <see cref="StagingTaskInfo"/> is logged with <see cref="IUserInfo"/> and grouped
        /// in <see cref="TaskGroupInfo"/>s.
        /// </summary>
        /// <remarks>
        /// Properties are set only in the main thread, they mustn't be set when processed by <see cref="SimpleQueueWorker{T}"/>.
        /// </remarks>
        /// <param name="settings">Current instance of <see cref="AbstractSynchronizationSettings"/></param>
        public static void InitUserAndTaskGroups(this AbstractSynchronizationSettings settings)
        {
            if (!settings.WorkerCall)
            {
                settings.User = SynchronizationActionContext.CurrentLogUserWithTask ? CMSActionContext.CurrentUser : null;
                settings.TaskGroups = SynchronizationActionContext.CurrentTaskGroups.ToArray();
            }
        }
    }
}
