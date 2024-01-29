using System;
using System.Linq;

using CMS.DataEngine;
using CMS.Scheduler;


namespace CMS.SocialMarketing
{
    /// <summary>
    /// Provides management of LinkedIn applications.
    /// </summary>
    public class LinkedInApplicationInfoProvider : AbstractInfoProvider<LinkedInApplicationInfo, LinkedInApplicationInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the LinkedInApplicationInfoProvider class.
        /// </summary>
        public LinkedInApplicationInfoProvider()
            : base(LinkedInApplicationInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true,
                    Name = true,
                    Load = LoadHashtableEnum.All
                })
        {

        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the LinkedInApplicationInfo objects.
        /// </summary>
        public static ObjectQuery<LinkedInApplicationInfo> GetLinkedInApplications()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Retrieves a LinkedIn application with the specified identifier, and returns it.
        /// </summary>
        /// <param name="applicationId">LinkedIn application identifier.</param>
        /// <returns>A LinkedIn application with the specified identifier, if found; otherwise, null.</returns>
        public static LinkedInApplicationInfo GetLinkedInApplicationInfo(int applicationId)
        {
            return ProviderObject.GetInfoById(applicationId);
        }


        /// <summary>
        /// Retrieves a LinkedIn application matching the specified criteria, and returns it.
        /// </summary>
        /// <param name="applicationName">LinkedIn application name.</param>                
        /// <param name="siteName">Site name.</param>                
        /// <returns>A LinkedIn application matching the specified criteria, if found; otherwise, null.</returns>
        public static LinkedInApplicationInfo GetLinkedInApplicationInfo(string applicationName, SiteInfoIdentifier siteName)
        {
            return ProviderObject.GetInfoByCodeName(applicationName, siteName);
        }


        /// <summary>
        /// Returns LinkedInApplicationInfo with specified GUID.
        /// </summary>
        /// <param name="guid">LinkedInApplicationInfo GUID</param>                
        public static LinkedInApplicationInfo GetLinkedInApplicationInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Updates or creates the specified LinkedIn application.
        /// </summary>
        /// <param name="application">LinkedIn application to be updated or created.</param>
        public static void SetLinkedInApplicationInfo(LinkedInApplicationInfo application)
        {
            ProviderObject.SetInfo(application);
        }


        /// <summary>
        /// Deletes the specified LinkedIn application.
        /// </summary>
        /// <param name="application">LinkedIn application to be deleted.</param>
        public static void DeleteLinkedInApplicationInfo(LinkedInApplicationInfo application)
        {
            ProviderObject.DeleteInfo(application);
        }


        /// <summary>
        /// Deletes the LinkedIn application with specified identifier.
        /// </summary>
        /// <param name="applicationId">LinkedIn application identifier.</param>
        public static void DeleteLinkedInApplicationInfo(int applicationId)
        {
            LinkedInApplicationInfo application = GetLinkedInApplicationInfo(applicationId);
            DeleteLinkedInApplicationInfo(application);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Retrieves a dataset of LinkedIn applications for the specified site, and returns it.
        /// </summary>
        /// <param name="siteId">Site ID.</param>        
        /// <returns>A dataset of LinkedIn applications for the specified site.</returns>
        public static ObjectQuery<LinkedInApplicationInfo> GetLinkedInApplications(SiteInfoIdentifier siteId)
        {
            return ProviderObject.GetLinkedInApplicationsInternal(siteId);
        }
        
        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(LinkedInApplicationInfo info)
        {
            using (var scope = BeginTransaction())
            {
                if (!CheckUniqueValues(info, "LinkedInApplicationConsumerKey", "LinkedInApplicationSiteID"))
                {
                    throw new Exception("[LinkedInApplicationInfoProvider.SetInfo]: LinkedIn application object with the specified consumer key already exists.");
                }

                base.SetInfo(info);

                scope.Commit();
            }

            SetLinkedInInsightsSchedulerStateInternal(info, true);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(LinkedInApplicationInfo info)
        {
            ProviderHelper.ClearHashtables(LinkedInAccountInfo.OBJECT_TYPE, true);
            ProviderHelper.ClearHashtables(LinkedInPostInfo.OBJECT_TYPE, true);
            SetLinkedInInsightsSchedulerStateInternal(info, false);
            base.DeleteInfo(info);
        }


        /// <summary>
        /// Creates or removes LinkedIn Insights collection scheduler.
        /// Called whenever condition for Insights data collection changes - new application is created or an existing is deleted.
        /// </summary>
        /// <param name="application">LinkedIn application for which the stats should be collected.</param>
        /// <param name="enabled">Enable the task after application creation, disable when deleting.</param>
        protected virtual void SetLinkedInInsightsSchedulerStateInternal(LinkedInApplicationInfo application, bool enabled)
        {
            if (application == null)
            {
                return;
            }

            TaskInfo scheduledTask = TaskInfoProvider.GetTaskInfo(String.Format(LinkedInInsightsCollectionTask.TASK_CODENAME_FORMAT, application.LinkedInApplicationID), application.LinkedInApplicationSiteID);
            if (enabled && (scheduledTask == null))
            {
                // Create new task for the application
                TaskInterval interval = new TaskInterval()
                {
                    Period = SchedulingHelper.PERIOD_DAY,
                    StartTime = DateTime.Now,
                    Every = 1,
                    BetweenEnd = DateTime.Today.AddMinutes(-1),
                    Days = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().ToList()
                };

                scheduledTask = new TaskInfo()
                {
                    TaskDisplayName = String.Format("{{$sm.linkedin.insights.scheduledtask.name$}} ({0})", application.LinkedInApplicationDisplayName),
                    TaskName = String.Format(LinkedInInsightsCollectionTask.TASK_CODENAME_FORMAT, application.LinkedInApplicationID),
                    TaskAssemblyName = "CMS.SocialMarketing",
                    TaskClass = "CMS.SocialMarketing.LinkedInInsightsCollectionTask",
                    TaskDeleteAfterLastRun = false,
                    TaskSiteID = application.LinkedInApplicationSiteID,
                    TaskAllowExternalService = true,
                    TaskUseExternalService = true,
                    TaskRunInSeparateThread = true,
                    TaskEnabled = true,
                    TaskType = ScheduledTaskTypeEnum.System,
                    TaskData = LinkedInInsightsCollectionTaskData.ToXmlString(new LinkedInInsightsCollectionTaskData(application.LinkedInApplicationID)),
                    TaskInterval = SchedulingHelper.EncodeInterval(interval)
                };

                // Set task activation time
                scheduledTask.TaskNextRunTime = SchedulingHelper.GetFirstRunTime(interval);

                TaskInfoProvider.SetTaskInfo(scheduledTask);
            }
            else if (!enabled && (scheduledTask != null))
            {
                // Delete existing task of the application
                TaskInfoProvider.DeleteTaskInfo(scheduledTask);
            }
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns a query for all the LinkedInApplicationInfo objects of a specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual ObjectQuery<LinkedInApplicationInfo> GetLinkedInApplicationsInternal(SiteInfoIdentifier siteId)
        {
            return GetObjectQuery().OnSite(siteId);
        }  
        
        #endregion
    }
}