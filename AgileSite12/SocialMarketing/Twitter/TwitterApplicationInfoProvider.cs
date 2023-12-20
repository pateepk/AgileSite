using System;
using System.Linq;

using CMS.DataEngine;
using CMS.Scheduler;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Provides management of Twitter applications.
    /// </summary>
    public class TwitterApplicationInfoProvider : AbstractInfoProvider<TwitterApplicationInfo, TwitterApplicationInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the TwitterApplicationInfoProvider class.
        /// </summary>
        public TwitterApplicationInfoProvider() 
            : base(TwitterApplicationInfo.TYPEINFO, new HashtableSettings
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
        /// Returns a query for all the TwitterApplicationInfo objects.
        /// </summary>
        public static ObjectQuery<TwitterApplicationInfo> GetTwitterApplications()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Retrieves a Twitter application with the specified identifier, and returns it.
        /// </summary>
        /// <param name="applicationId">Twitter application identifier.</param>
        /// <returns>A Twitter application with the specified identifier, if found; otherwise, null.</returns>
        public static TwitterApplicationInfo GetTwitterApplicationInfo(int applicationId)
        {
            return ProviderObject.GetInfoById(applicationId);
        }


        /// <summary>
        /// Retrieves a Twitter application matching the specified criteria, and returns it.
        /// </summary>
        /// <param name="applicationName">Twitter application name.</param>                
        /// <param name="siteName">Site name.</param>                
        /// <returns>A Twitter application matching the specified criteria, if found; otherwise, null.</returns>
        public static TwitterApplicationInfo GetTwitterApplicationInfo(string applicationName, SiteInfoIdentifier siteName)
        {
            return ProviderObject.GetInfoByCodeName(applicationName, siteName);
        }


        /// <summary>
        /// Returns TwitterApplicationInfo with specified GUID.
        /// </summary>
        /// <param name="guid">TwitterApplicationInfo GUID</param>                
        public static TwitterApplicationInfo GetTwitterApplicationInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Updates or creates the specified Twitter application.
        /// </summary>
        /// <param name="application">Twitter application to be updated or created.</param>
        public static void SetTwitterApplicationInfo(TwitterApplicationInfo application)
        {
            ProviderObject.SetInfo(application);
        }


        /// <summary>
        /// Deletes the specified Twitter application.
        /// </summary>
        /// <param name="application">Twitter application to be deleted.</param>
        public static void DeleteTwitterApplicationInfo(TwitterApplicationInfo application)
        {
            ProviderObject.DeleteInfo(application);
        }


        /// <summary>
        /// Deletes the Twitter application with specified identifier.
        /// </summary>
        /// <param name="applicationId">Twitter application identifier.</param>
        public static void DeleteTwitterApplicationInfo(int applicationId)
        {
            TwitterApplicationInfo application = GetTwitterApplicationInfo(applicationId);
            DeleteTwitterApplicationInfo(application);
        }

        #endregion


        #region "Public methods - Advanced"
        

        /// <summary>
        /// Retrieves a dataset of Twitter applications for the specified site, and returns it.
        /// </summary>
        /// <param name="siteId">Site ID.</param>        
        /// <returns>A dataset of Twitter applications for the specified site.</returns>
        public static ObjectQuery<TwitterApplicationInfo> GetTwitterApplications(SiteInfoIdentifier siteId)
        {
            return ProviderObject.GetTwitterApplicationsInternal(siteId);
        }
        
        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(TwitterApplicationInfo info)
        {
            using (var scope = BeginTransaction())
            {
                if (!CheckUniqueValues(info, "TwitterApplicationConsumerKey", "TwitterApplicationSiteID"))
                {
                    throw new Exception("[TwitterApplicationInfoProvider.SetInfo]: Twitter application object with the specified consumer key already exists.");
                }

                base.SetInfo(info);

                EnsureScheduledTask(info.TwitterApplicationSiteID);

                scope.Commit();
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(TwitterApplicationInfo info)
        {
            ProviderHelper.ClearHashtables(TwitterAccountInfo.OBJECT_TYPE, true);
            ProviderHelper.ClearHashtables(TwitterPostInfo.OBJECT_TYPE, true);
            base.DeleteInfo(info);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns a query for all the TwitterApplicationInfo objects of a specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual ObjectQuery<TwitterApplicationInfo> GetTwitterApplicationsInternal(int siteId)
        {
            return GetObjectQuery().OnSite(siteId);
        }  
        
        #endregion
        

        #region "Private methods"

        /// <summary>
        /// Determines whether there is a scheduled task to collect Twitter insights. If it does not, creates the task.
        /// </summary>
        /// <param name="siteId">The identifier of the site where the task will be created.</param>
        private void EnsureScheduledTask(int siteId)
        {
            string taskName = "SocialMarketing.TwitterInsightsCollection";
            TaskInfo task = TaskInfoProvider.GetTaskInfo(taskName, siteId);
            if (task != null)
            {
                return;
            }

            TaskInterval interval = new TaskInterval()
            {
                Period = SchedulingHelper.PERIOD_HOUR,
                StartTime = DateTime.Now,
                Every = 1,
                BetweenEnd = DateTime.Today.AddMinutes(-1),
                Days = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().ToList()
            };
            task = new TaskInfo
            {
                TaskDisplayName = "{$sm.twitter.insights.scheduledtask.name$}",
                TaskName = taskName,
                TaskAssemblyName = "CMS.SocialMarketing",
                TaskClass = "CMS.SocialMarketing.TwitterInsightsCollectionTask",
                TaskDeleteAfterLastRun = false,
                TaskSiteID = siteId,
                TaskAllowExternalService = true,
                TaskUseExternalService = true,
                TaskRunInSeparateThread = true,
                TaskEnabled = true,
                TaskType = ScheduledTaskTypeEnum.System,
                TaskData = "<data></data>",
                TaskInterval = SchedulingHelper.EncodeInterval(interval)
            };
            task.TaskNextRunTime = SchedulingHelper.GetFirstRunTime(interval, DateTime.Now.AddHours(1));
            TaskInfoProvider.SetTaskInfo(task);
        }

        #endregion
    }
}