using System;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Scheduler;
using CMS.SiteProvider;


namespace CMS.SharePoint
{
    /// <summary>
    /// Class providing SharePointLibraryInfo management.
    /// </summary>
    public class SharePointLibraryInfoProvider : AbstractInfoProvider<SharePointLibraryInfo, SharePointLibraryInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public SharePointLibraryInfoProvider()
            : base(SharePointLibraryInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the SharePointLibraryInfo objects.
        /// </summary>
        public static ObjectQuery<SharePointLibraryInfo> GetSharePointLibraries()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns SharePointLibraryInfo with specified ID.
        /// </summary>
        /// <param name="id">SharePointLibraryInfo ID</param>
        public static SharePointLibraryInfo GetSharePointLibraryInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns SharePointLibraryInfo with specified name.
        /// </summary>
        /// <param name="name">SharePointLibraryInfo name</param>
        /// <param name="siteId">SharePointLibraryInfo's site identifier</param>
        public static SharePointLibraryInfo GetSharePointLibraryInfo(string name, SiteInfoIdentifier siteId)
        {
            return ProviderObject.GetInfoByCodeName(name, siteId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified SharePointLibraryInfo.
        /// </summary>
        /// <param name="infoObj">SharePointLibraryInfo to be set</param>
        public static void SetSharePointLibraryInfo(SharePointLibraryInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified SharePointLibraryInfo.
        /// </summary>
        /// <param name="infoObj">SharePointLibraryInfo to be deleted</param>
        public static void DeleteSharePointLibraryInfo(SharePointLibraryInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes SharePointLibraryInfo with specified ID.
        /// </summary>
        /// <param name="id">SharePointLibraryInfo ID</param>
        public static void DeleteSharePointLibraryInfo(int id)
        {
            SharePointLibraryInfo infoObj = GetSharePointLibraryInfo(id);
            DeleteSharePointLibraryInfo(infoObj);
        }


        /// <summary>
        /// Gets state of SharePoint library synchronization.
        /// </summary>
        /// <param name="libraryInfo">SharePoint library for which to get the synchronization state.</param>
        /// <returns>Current state of the synchronization.</returns>
        public static SharePointLibrarySynchronizationState GetSharePointLibrarySynchronizationState(SharePointLibraryInfo libraryInfo)
        {
            return ProviderObject.GetSharePointLibrarySynchronizationStateInternal(libraryInfo);
        }


        /// <summary>
        /// Schedules the SharePoint library synchronization for an immediate run and returns.
        /// Use <see cref="GetSharePointLibrarySynchronizationState"/> to see whether the synchronization
        /// is in the scheduler's queue (<see cref="SharePointLibrarySynchronizationState.NextRunTime"/> is from the past),
        /// is running (<see cref="SharePointLibrarySynchronizationState.IsRunning"/> is set), or will be run
        /// (<see cref="SharePointLibrarySynchronizationState.NextRunTime"/> is in the future).
        /// </summary>
        /// <param name="libraryInfo">SharePoint library to be synchronized immediately.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="libraryInfo"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the library can not be scheduled for synchronization (the scheduler's task is missing).</exception>
        public static void SynchronizeSharePointLibrary(SharePointLibraryInfo libraryInfo)
        {
            ProviderObject.SynchronizeSharePointLibraryInternal(libraryInfo);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(SharePointLibraryInfo info)
        {
            base.SetInfo(info);
            CreateOrUpdateSharePointLibrarySynchronizationTask(info);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(SharePointLibraryInfo info)
        {
            DeleteSharePointLibrarySynchronizationTask(info);
            base.DeleteInfo(info);
        }


        /// <summary>
        /// Gets state of SharePoint library synchronization.
        /// </summary>
        /// <param name="libraryInfo">SharePoint library for which to get the synchronization state.</param>
        /// <returns>Current state of the synchronization.</returns>
        protected virtual SharePointLibrarySynchronizationState GetSharePointLibrarySynchronizationStateInternal(SharePointLibraryInfo libraryInfo)
        {
            TaskInfo scheduledTask = GetSharePointLibrarySynchronizationTask(libraryInfo);
            if (scheduledTask != null)
            {
                DateTime? lastRunTime = scheduledTask.TaskLastRunTime;
                if (lastRunTime == TaskInfoProvider.NO_TIME)
                {
                    lastRunTime = null;
                }
                DateTime? nextRunTime = scheduledTask.TaskNextRunTime;
                if (nextRunTime == TaskInfoProvider.NO_TIME)
                {
                    nextRunTime = null;
                }

                return new SharePointLibrarySynchronizationState(lastRunTime, scheduledTask.TaskLastResult, nextRunTime);
            }

            return new SharePointLibrarySynchronizationState(null, ResHelper.GetString("sharepoint.library.scheduledtask.notfound"), null);
        }


        /// <summary>
        /// Schedules the SharePoint library synchronization for an immediate run and returns.
        /// Use <see cref="GetSharePointLibrarySynchronizationState"/> to see whether the synchronization
        /// is in the scheduler's queue (<see cref="SharePointLibrarySynchronizationState.NextRunTime"/> is from the past),
        /// is running (<see cref="SharePointLibrarySynchronizationState.IsRunning"/> is set), or will be run
        /// (<see cref="SharePointLibrarySynchronizationState.NextRunTime"/> is in the future).
        /// </summary>
        /// <param name="libraryInfo">SharePoint library to be synchronized immediately.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="libraryInfo"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the library can not be scheduled for synchronization (the scheduler's task is missing).</exception>
        protected virtual void SynchronizeSharePointLibraryInternal(SharePointLibraryInfo libraryInfo)
        {
            if (libraryInfo == null)
            {
                throw new ArgumentNullException("libraryInfo", "[SharePointLibraryInfoProvider.SynchronizeSharePointLibraryInternal]: The library can not be null.");
            }

            TaskInfo scheduledTask = GetSharePointLibrarySynchronizationTask(libraryInfo);
            if (scheduledTask == null)
            {
                throw new InvalidOperationException("[SharePointLibraryInfoProvider.SynchronizeSharePointLibraryInternal]: The SharePoint library synchronization can not be scheduled for running. " +
                                                    "The synchronization task is missing. Try calling SharePointLibraryInfoProvider.SetSharePointLibraryInfo on the library to recreate the task.");
            }

            SiteInfo si = SiteInfoProvider.GetSiteInfo(scheduledTask.TaskSiteID);
            
            // Verify that the task is not currently running - next run time would be set to zero time
            if ((scheduledTask.TaskNextRunTime != DateTimeHelper.ZERO_TIME))
            {
                // Schedule the task for an immediate run at the end of the request
                scheduledTask.TaskNextRunTime = DateTime.Now;
                TaskInfoProvider.SetTaskInfo(scheduledTask);

                // Run the task
                SchedulingTimer.RunSchedulerImmediately = true;
                SchedulingTimer.SchedulerRunImmediatelySiteName = si.SiteName;
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Creates or updates the scheduled synchronization task for given SharePoint library.
        /// The task is updated only when the synchronization period of the <paramref name="libraryInfo"/> has changed.
        /// The method can be called to recreate the task in case it has been incidentally deleted by someone.
        /// </summary>
        /// <param name="libraryInfo">SharePoint library whose task is created or updated.</param>
        private void CreateOrUpdateSharePointLibrarySynchronizationTask(SharePointLibraryInfo libraryInfo)
        {
            if (libraryInfo == null)
            {
                return;
            }

            TaskInfo scheduledTask = GetSharePointLibrarySynchronizationTask(libraryInfo);
            if (scheduledTask == null)
            {
                // Create new task for the library
                TaskInterval interval = new TaskInterval
                {
                    Period = SchedulingHelper.PERIOD_MINUTE,
                    StartTime = DateTime.Now,
                    Every = libraryInfo.SharePointLibrarySynchronizationPeriod,
                    BetweenEnd = DateTime.Today.AddMinutes(-1),
                    Days = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().ToList()
                };

                scheduledTask = new TaskInfo
                {
                    TaskDisplayName = String.Format("{{$sharepoint.library.scheduledtask.name$}} ({0})", libraryInfo.SharePointLibraryDisplayName),
                    TaskName = SharePointLibrarySynchronizationTask.GetTaskCodeName(libraryInfo.SharePointLibraryID),
                    TaskAssemblyName = "CMS.SharePoint",
                    TaskClass = "CMS.SharePoint.SharePointLibrarySynchronizationTask",
                    TaskDeleteAfterLastRun = false,
                    TaskSiteID = libraryInfo.SharePointLibrarySiteID,
                    TaskAllowExternalService = true,
                    TaskUseExternalService = true,
                    TaskRunInSeparateThread = true,
                    TaskEnabled = true,
                    TaskType = ScheduledTaskTypeEnum.System,
                    TaskData = libraryInfo.SharePointLibraryID.ToString(),
                    TaskInterval = SchedulingHelper.EncodeInterval(interval)
                };

                // Set task activation time - for a new library synchronize immediately
                scheduledTask.TaskNextRunTime = DateTime.Now;
                TaskInfoProvider.SetTaskInfo(scheduledTask);
            }
            else
            {
                // Update task's interval if needed - for an updated library synchronize after the new interval elapses
                TaskInterval interval = SchedulingHelper.DecodeInterval(scheduledTask.TaskInterval);
                if (interval.Every != libraryInfo.SharePointLibrarySynchronizationPeriod)
                {
                    interval.Every = libraryInfo.SharePointLibrarySynchronizationPeriod;
                    scheduledTask.TaskInterval = SchedulingHelper.EncodeInterval(interval);
                    if (scheduledTask.TaskNextRunTime != TaskInfoProvider.NO_TIME)
                    {
                        // If the task is currently running, do not schedule it for synchronization after the new interval elapses. Changing next run time for a running task allows the task to be run multiple times
                        scheduledTask.TaskNextRunTime = SchedulingHelper.GetNextTime(interval, scheduledTask.TaskNextRunTime);
                    }
                    TaskInfoProvider.SetTaskInfo(scheduledTask);
                }
            }
        }


        /// <summary>
        /// Deletes the scheduled synchronization task for given SharePoint library, if it exists.
        /// Does nothing when <paramref name="libraryInfo"/> is null or the task does not exist.
        /// </summary>
        /// <param name="libraryInfo">SharePoint library which no longer needs the task.</param>
        private void DeleteSharePointLibrarySynchronizationTask(SharePointLibraryInfo libraryInfo)
        {
            if (libraryInfo == null)
            {
                return;
            }

            TaskInfo scheduledTask = GetSharePointLibrarySynchronizationTask(libraryInfo);
            if (scheduledTask != null)
            {
                // Delete existing task of the application
                TaskInfoProvider.DeleteTaskInfo(scheduledTask);
            }
        }


        /// <summary>
        /// Gets synchronization task used for scheduled SharePoint library synchronization.
        /// </summary>
        /// <param name="libraryInfo">SharePoint library for which to return the scheduled task.</param>
        /// <returns>Synchronization task, or null if it does not exist for the specified library.</returns>
        private TaskInfo GetSharePointLibrarySynchronizationTask(SharePointLibraryInfo libraryInfo)
        {
            return TaskInfoProvider.GetTaskInfo(SharePointLibrarySynchronizationTask.GetTaskCodeName(libraryInfo.SharePointLibraryID), libraryInfo.SharePointLibrarySiteID);
        }

        #endregion
    }
}