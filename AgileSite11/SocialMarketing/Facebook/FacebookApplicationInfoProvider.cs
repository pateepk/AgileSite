using System;
using System.Linq;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Modules;
using CMS.Scheduler;
using CMS.SiteProvider;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Provides management of Facebook applications.
    /// </summary>
    public class FacebookApplicationInfoProvider : AbstractInfoProvider<FacebookApplicationInfo, FacebookApplicationInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the FacebookApplicationInfoProvider class.
        /// </summary>
        public FacebookApplicationInfoProvider() 
            : base(FacebookApplicationInfo.TYPEINFO, new HashtableSettings
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
        /// Returns a query for all the FacebookApplicationInfo objects.
        /// </summary>
        public static ObjectQuery<FacebookApplicationInfo> GetFacebookApplications()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Retrieves a Facebook application with the specified identifier, and returns it.
        /// </summary>
        /// <param name="applicationId">Facebook application identifier.</param>
        /// <returns>A Facebook application with the specified identifier, if found; otherwise, null.</returns>
        public static FacebookApplicationInfo GetFacebookApplicationInfo(int applicationId)
        {
            return ProviderObject.GetInfoById(applicationId);
        }


        /// <summary>
        /// Retrieves a Facebook application matching the specified criteria, and returns it.
        /// </summary>
        /// <param name="applicationName">Facebook application name.</param>                
        /// <param name="siteName">Site name.</param>                
        /// <returns>A Facebook application matching the specified criteria, if found; otherwise, null.</returns>
        public static FacebookApplicationInfo GetFacebookApplicationInfo(string applicationName, SiteInfoIdentifier siteName)
        {
            return ProviderObject.GetInfoByCodeName(applicationName, siteName);
        }


        /// <summary>
        /// Returns FacebookApplicationInfo with specified GUID.
        /// </summary>
        /// <param name="guid">FacebookApplicationInfo GUID</param>                
        public static FacebookApplicationInfo GetFacebookApplicationInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Updates or creates the specified Facebook application.
        /// </summary>
        /// <param name="application">Facebook application to be updated or created.</param>
        public static void SetFacebookApplicationInfo(FacebookApplicationInfo application)
        {
            ProviderObject.SetInfo(application);
        }


        /// <summary>
        /// Deletes the specified Facebook application.
        /// </summary>
        /// <param name="application">Facebook application to be deleted.</param>
        public static void DeleteFacebookApplicationInfo(FacebookApplicationInfo application)
        {
            ProviderObject.DeleteInfo(application);
        }


        /// <summary>
        /// Deletes the Facebook application with specified identifier.
        /// </summary>
        /// <param name="applicationId">Facebook application identifier.</param>
        public static void DeleteFacebookApplicationInfo(int applicationId)
        {
            FacebookApplicationInfo application = GetFacebookApplicationInfo(applicationId);
            DeleteFacebookApplicationInfo(application);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Retrieves a dataset of Facebook applications for the specified site, and returns it.
        /// </summary>
        /// <param name="siteId">Site ID.</param>        
        /// <returns>A dataset of Facebook applications for the specified site.</returns>
        public static ObjectQuery<FacebookApplicationInfo> GetFacebookApplications(SiteInfoIdentifier siteId)
        {
            return ProviderObject.GetFacebookApplicationsInternal(siteId);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(FacebookApplicationInfo info)
        {
            using (var scope = BeginTransaction())
            {
                if (!CheckUniqueValues(info, "FacebookApplicationConsumerKey", "FacebookApplicationSiteID"))
                {
                    throw new Exception("[FacebookApplicationInfoProvider.SetInfo]: Facebook application object with the specified consumer key already exists.");
                }

                base.SetInfo(info);

                scope.Commit();
            }

            SetFacebookInsightsSchedulerStateInternal(info, true);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(FacebookApplicationInfo info)
        {
            ProviderHelper.ClearHashtables(FacebookAccountInfo.OBJECT_TYPE, true);
            ProviderHelper.ClearHashtables(FacebookPostInfo.OBJECT_TYPE, true);
            SetFacebookInsightsSchedulerStateInternal(info, false);
            base.DeleteInfo(info);
        }


        /// <summary>
        /// Creates or removes Facebook Insights collection scheduler.
        /// Called whenever condition for Insights data collection changes - new application is created or an existing is deleted.
        /// </summary>
        /// <param name="application">Facebook application for which the stats should be collected.</param>
        /// <param name="enabled">Enable the task after application creation, disable when deleting.</param>
        protected virtual void SetFacebookInsightsSchedulerStateInternal(FacebookApplicationInfo application, bool enabled)
        {
            if (application == null)
            {
                return;
            }

            TaskInfo scheduledTask = TaskInfoProvider.GetTaskInfo(String.Format(FacebookInsightsCollectionTask.TASK_CODENAME_FORMAT, application.FacebookApplicationID), application.FacebookApplicationSiteID);
            if (enabled && (scheduledTask == null))
            {
                // Create new task for the application
                TaskInterval interval = new TaskInterval
                {
                    Period = SchedulingHelper.PERIOD_HOUR,
                    StartTime = DateTime.Now,
                    Every = 1,
                    BetweenEnd = DateTime.Today.AddMinutes(-1),
                    Days = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().ToList()
                };

                var resource = ResourceInfoProvider.GetResourceInfo(ModuleName.SOCIALMARKETING);

                scheduledTask = new TaskInfo
                {
                    TaskDisplayName = String.Format("{{$sm.facebook.insights.scheduledtask.name$}} ({0})", application.FacebookApplicationDisplayName),
                    TaskName = String.Format(FacebookInsightsCollectionTask.TASK_CODENAME_FORMAT, application.FacebookApplicationID),
                    TaskAssemblyName = "CMS.SocialMarketing",
                    TaskClass = "CMS.SocialMarketing.FacebookInsightsCollectionTask",
                    TaskDeleteAfterLastRun = false,
                    TaskSiteID = application.FacebookApplicationSiteID,
                    TaskAllowExternalService = true,
                    TaskUseExternalService = true,
                    TaskRunInSeparateThread = true,
                    TaskEnabled = true,
                    TaskResourceID = resource.ResourceID,
                    TaskType = ScheduledTaskTypeEnum.System,
                    TaskData = FacebookInsightsCollectionTaskData.ToXmlString(new FacebookInsightsCollectionTaskData(application.FacebookApplicationID)),
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
        /// Returns a query for all the FacebookApplicationInfo objects of a specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual ObjectQuery<FacebookApplicationInfo> GetFacebookApplicationsInternal(SiteInfoIdentifier siteId)
        {
            return GetObjectQuery().OnSite(siteId);
        }  
        
        #endregion
    }
}