using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.Synchronization;

[assembly: ThreadRequiredContext(typeof(SynchronizationActionContext))]

namespace CMS.Synchronization
{
    /// <summary>
    /// Context for synchronization and task processing.
    /// </summary>
    [RegisterAllProperties]
    public sealed class SynchronizationActionContext : AbstractActionContext<SynchronizationActionContext>
    {
        #region "Variables"

        private bool? mLogUserWithTask;
        private IEnumerable<TaskGroupInfo> mTaskGroups;
        private IDictionary<int, IEnumerable<TaskGroupInfo>> mUserTaskGroups;

        #endregion


        #region "Static Properties"

        /// <summary>
        /// Gets task group collection for current user context.
        /// </summary>
        /// <remarks>
        /// The same task group collection is used for all users if collection of task groups is set through <see cref="TaskGroups"/> property.  
        /// </remarks>
        public static IEnumerable<TaskGroupInfo> CurrentTaskGroups
        {
            get
            {
                if (Current.mTaskGroups == null)
                {
                    return GetCurrentGroupInfo();
                }

                return Current.mTaskGroups;
            }
            private set
            {
                Current.mTaskGroups = value;
            }
        }


        /// <summary>
        /// Indicates whether user should be logged within the task creation.
        /// </summary>
        /// <remarks>
        /// By default tasks are logged with information about the current user.
        /// </remarks>
        public static bool CurrentLogUserWithTask
        {
            get
            {
                return Current.mLogUserWithTask ?? true;
            }
            private set
            {
                Current.mLogUserWithTask = value;
            }
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Sets the collection of tasks used within the current task processing.
        /// </summary>
        /// <remarks>
        /// The collection of task groups defined in <see cref="CurrentTaskGroups"/> property is returning currently  set value for all users.
        /// </remarks>
        public IEnumerable<TaskGroupInfo> TaskGroups
        {
            set
            {
                StoreOriginalValue(ref OriginalData.mTaskGroups, CurrentTaskGroups);

                CurrentTaskGroups = value;
            }
        }


        /// <summary>
        /// Sets the value that indicates whether user should be logged  within the task creation. 
        /// </summary>
        /// <remarks>
        /// By default tasks are logged with and information about user.
        /// If disabled, task are not logged with information about user.
        /// </remarks>
        public bool LogUserWithTask
        {
            set
            {
                StoreOriginalValue(ref OriginalData.mLogUserWithTask, CurrentLogUserWithTask);

                CurrentLogUserWithTask = value;
            }
        }


        /// <summary>
        /// Sets the value that indicates whether user should be logged  within the task creation. 
        /// </summary>
        /// <remarks>
        /// By default tasks are logged with and information about user.
        /// If disabled, task are not logged with information about user.
        /// </remarks>
        private IDictionary<int, IEnumerable<TaskGroupInfo>> UserTaskGroups
        {
            set
            {
                StoreOriginalValue(ref OriginalData.mUserTaskGroups, Current.mUserTaskGroups);

                Current.mUserTaskGroups = value;
            }
        }

        #endregion


        #region Constructor

        /// <summary>
        /// Creates context that affects logging additional information with staging task within using block.
        /// </summary>
        /// <example>
        /// <code>
        /// using (new SynchronizationActionContext() { LogUserWithTask = false })
        /// { 
        ///     // Within this block, information about user who caused staging task creation won't be logged
        /// }
        /// </code>
        /// </example>
        public SynchronizationActionContext()
        {
        }


        /// <summary>
        /// Copies internal state of injected context to new one.
        /// Which causes the static SynchronizationActionContext to return values set by injected context.
        /// </summary>
        /// <param name="sac">Context to be injected</param>
        internal SynchronizationActionContext(SynchronizationActionContext sac)
        {
            UserTaskGroups = sac.mUserTaskGroups;
            LogUserWithTask = sac.mLogUserWithTask ?? true;
            TaskGroups = sac.mTaskGroups;
        }

        #endregion


        /// <summary>
        /// Restores original values.
        /// </summary>
        protected override void RestoreOriginalValues()
        {
            // Restore settings
            var o = OriginalData;

            if (o.mTaskGroups != null)
            {
                CurrentTaskGroups = o.mTaskGroups;
            }

            if (o.mUserTaskGroups != null)
            {
                Current.mUserTaskGroups = o.mUserTaskGroups;
            }

            if (o.mLogUserWithTask.HasValue)
            {
                CurrentLogUserWithTask = (bool)o.mLogUserWithTask;
            }

            base.RestoreOriginalValues();
        }


        /// <summary>
        /// Initialize context, ensures that values will be cached for current thread and will not be changed by another thread.
        /// </summary>
        internal static void EnsureRequiredValues()
        {
            string siteName = CMSActionContext.CurrentSite == null ? String.Empty : CMSActionContext.CurrentSite.SiteName;

            if (StagingTaskInfoProvider.LoggingOfStagingTasksEnabled(siteName))
            {
                Ensure(CurrentTaskGroups);
            }
        }


        /// <summary>
        /// Ensures values in current thread for context and clones synchronization action context object for new thread.     
        /// </summary>
        public override object CloneForNewThread()
        {
            EnsureRequiredValues();
            return base.CloneForNewThread();
        }


        /// <summary>
        /// Returns the clone of current <see cref="SynchronizationActionContext"/> instance.
        /// </summary>
        internal static SynchronizationActionContext CloneCurrent()
        {
            return (SynchronizationActionContext)Current.CloneForNewThread();
        }


        /// <summary>
        /// Returns collection of <see cref="TaskGroupInfo"/> objects for current user.
        /// </summary>
        private static IEnumerable<TaskGroupInfo> GetCurrentGroupInfo()
        {
            if (CMSActionContext.CurrentUser == null)
            {
                return new TaskGroupInfo[0];
            }

            IEnumerable<TaskGroupInfo> result = null;
            var userId = CMSActionContext.CurrentUser.UserID;

            if (Current.mUserTaskGroups == null)
            {
                Current.mUserTaskGroups = new Dictionary<int, IEnumerable<TaskGroupInfo>>();
            }

            if (!Current.mUserTaskGroups.TryGetValue(userId, out result))
            {
                var userGroups = TaskGroupUserInfoProvider.GetTaskGroupUsers()
                    .WhereEquals("UserID", userId)
                    .Column("TaskGroupID");

                var userGroup = TaskGroupInfoProvider.GetTaskGroups()
                    .WhereIn("TaskGroupID", userGroups)
                    .TopN(1);

                result = userGroup.ToArray();

                Current.mUserTaskGroups[userId] = result;
            }

            return result;
        }
    }
}
