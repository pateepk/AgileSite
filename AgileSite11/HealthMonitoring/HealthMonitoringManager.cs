using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Base;
using CMS.IO;
using CMS.LicenseProvider;
using CMS.SiteProvider;

namespace CMS.HealthMonitoring
{
    /// <summary>
    /// The class that provides methods to work with performance counters.
    /// </summary>
    public static class HealthMonitoringManager
    {
        #region "Constants"

        /// <summary>
        /// Description of general category.
        /// </summary>
        private const string GENERAL_CATEGORY_DESCRIPTION = "The set of general counters that are logged per instance.";

        /// <summary>
        /// Description of sites category.
        /// </summary>
        private const string SITES_CATEGORY_DESCRIPTION = "The set of site counters that are logged per sites.";

        #endregion


        #region "Variables"

        /// <summary>
        /// Counter list.
        /// </summary>
        private static List<Counter> mCounters = null;

        /// <summary>
        /// General category name.
        /// </summary>
        private static string mGeneralCategoryName = null;

        /// <summary>
        /// Sites category name.
        /// </summary>
        private static string mSitesCategoryName = null;

        /// <summary>
        /// Collection for sites name.
        /// </summary>
        private static IEnumerable<string> mSites;


        /// <summary>
        /// Collection for all sites name.
        /// </summary>
        private static IEnumerable<string> mAllSites;

        /// <summary>
        /// Collection of system counter keys.
        /// </summary>
        private static ISet<string> mSystemCounterKeys;

        /// <summary>
        /// Collection of system counter keys with special treatment.
        /// </summary>
        private static ISet<string> mSystemSpecialCounterKeys;

        /// <summary>
        /// Collection of system database counter keys.
        /// </summary>
        private static ISet<string> mSystemDatabaseCounterKeys;

        /// <summary>
        /// Locker object.
        /// </summary>
        private static readonly object locker = new object();

        #endregion


        #region "Events and delegates"

        /// <summary>
        /// Progress log event handler.
        /// </summary>
        public delegate void OnProgressLogEventHandler(string message);

        /// <summary>
        /// Progress log event.
        /// </summary>
        public static event OnProgressLogEventHandler OnProgressLog;

        #endregion


        #region "Properties"

        /// <summary>
        /// Collection of system counter keys based on CMSPerformanceCounter.
        /// </summary>
        private static ISet<string> SystemCounterKeys
        {
            get
            {
                return mSystemCounterKeys ?? (mSystemCounterKeys = new HashSet<string>
                       {
                           CounterName.ALLOCATED_MEMORY,
                           CounterName.VIEW_OF_CONTENT_PAGES_PER_SECOND,
                           CounterName.PENDING_REQUESTS_PER_SECOND,
                           CounterName.FILE_DOWNLOADS_AND_VIEWS_PER_SECOND,
                           CounterName.NOT_FOUND_PAGES_PER_SECOND,
                           CounterName.ROBOT_TXT_PER_SECOND,
                           CounterName.VIEW_OF_SYSTEM_PAGES_PER_SECOND,
                           CounterName.NON_PAGES_REQUESTS_PER_SECOND,
                           CounterName.CACHE_REMOVED_ITEMS_PER_SECOND,
                           CounterName.CACHE_UNDERUSED_ITEMS_PER_SECOND,
                           CounterName.CACHE_EXPIRED_ITEMS_PER_SECOND,
                           CounterName.RUNNING_THREADS,
                           CounterName.RUNNING_SQL_QUERIES,
                           CounterName.EVENTLOG_WARNINGS,
                           CounterName.EVENTLOG_ERRORS,
                           CounterName.RUNNING_TASKS
                       });
            }
        }


        /// <summary>
        /// Collection of system counter keys based on special values treatment with sites collection.
        /// </summary>
        private static ISet<string> SystemSpecialCounterKeys
        {
            get
            {
                return mSystemSpecialCounterKeys ?? (mSystemSpecialCounterKeys = new HashSet<string>
                       {
                           CounterName.ONLINE_USERS,
                           CounterName.AUTHENTICATED_USERS,
                           CounterName.ANONYMOUS_VISITORS
                       });
            }
        }


        /// <summary>
        /// Collection of system counter keys based on database values.
        /// </summary>
        private static ISet<string> SystemDatabaseCounterKeys
        {
            get
            {
                return mSystemDatabaseCounterKeys ?? (mSystemDatabaseCounterKeys = new HashSet<string>
                       {
                           CounterName.ALL_EMAILS_IN_QUEUE,
                           CounterName.ERROR_EMAILS_IN_QUEUE,
                           CounterName.TASKS_IN_QUEUE
                       });
            }
        }


        /// <summary>
        /// Gets counter list.
        /// </summary>
        public static List<Counter> Counters
        {
            get
            {
                // Load counters from XML files
                LoadCounters();

                return mCounters;
            }
        }


        /// <summary>
        /// Gets name of general category.
        /// </summary>
        public static string GeneralCategoryName
        {
            get
            {
                if (mGeneralCategoryName == null)
                {
                    InitializeCategoryNames();
                }

                return mGeneralCategoryName;
            }
        }


        /// <summary>
        /// Gets sites category name.
        /// </summary>
        public static string SitesCategoryName
        {
            get
            {
                if (mSitesCategoryName == null)
                {
                    InitializeCategoryNames();
                }

                return mSitesCategoryName;
            }
        }


        /// <summary>
        /// Gets or sets error flag.
        /// </summary>
        public static bool Error
        {
            get;
            set;
        }


        /// <summary>
        /// Gets collection of site names.
        /// </summary>
        public static IEnumerable<string> Sites
        {
            get
            {
                if ((mSites == null) && DatabaseHelper.IsDatabaseAvailable)
                {
                    // Get all running sites
                    mSites = SiteInfoProvider.GetSites()
                                             .WhereEquals("SiteStatus", "RUNNING")
                                             .Columns("SiteName")
                                             .GetListResult<string>();
                }

                return mSites;
            }
        }


        /// <summary>
        /// Gets collection of site names.
        /// </summary>
        public static IEnumerable<string> AllSites
        {
            get
            {
                if ((mAllSites == null) && DatabaseHelper.IsDatabaseAvailable)
                {
                    // Get all sites
                    mAllSites = SiteInfoProvider.GetSites()
                                                .Columns("SiteName")
                                                .GetListResult<string>();
                }

                return mAllSites;
            }
        }

        #endregion


        #region "Methods for working with counters"

        /// <summary>
        /// Creates new performance categories and counters.
        /// </summary>
        public static void CreateCounterCategories()
        {
            // Clear hashtables
            ClearCounters();

            // Create general category
            CreateCounterCategory(GeneralCategoryName, CategoryType.General);

            // Create sites category
            CreateCounterCategory(SitesCategoryName, CategoryType.Sites);

            // Reset counters
            ResetCounters();
        }


        /// <summary>
        /// Creates new performance category and counters.
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="categoryType">Category type</param>
        public static void CreateCounterCategory(string categoryName, CategoryType categoryType)
        {
            // Delete existing category
            DeleteCounterCategory(categoryName);

            try
            {
                if (categoryType == CategoryType.General)
                {
                    // Select enabled counters
                    var generalCounters = Counters.Where(c => c.Enabled);
                    // Create general category
                    PerformanceCounterCategory.Create(categoryName, GENERAL_CATEGORY_DESCRIPTION, PerformanceCounterCategoryType.SingleInstance, GetCustomCounters(generalCounters));
                    // Log progress message
                    LogProgress(string.Format("Performance counter category '{0}' was created.", categoryName));

                }
                else if (categoryType == CategoryType.Sites)
                {
                    // Select sites and enabled counters
                    var sitesCounters = Counters.Where(c => !c.OnlyGlobal && c.Enabled);
                    // Create site category
                    PerformanceCounterCategory.Create(categoryName, SITES_CATEGORY_DESCRIPTION, PerformanceCounterCategoryType.MultiInstance, GetCustomCounters(sitesCounters));
                    // Log progress message
                    LogProgress(string.Format("Performance category '{0}' was created.", categoryName));

                }
            }
            catch (Exception ex)
            {
                if (DatabaseHelper.IsDatabaseAvailable)
                {
                    EventLogProvider.LogException("HealthMonitoringManager", "CreateCounterCategory", ex, additionalMessage: string.Format("Error creating category '{0}'.", categoryName));
                    return;
                }
                // Throw original exception
                throw;
            }
        }


        /// <summary>
        /// Indicates if performance category exists.
        /// </summary>
        /// <param name="categoryName">Category name</param>
        public static bool PerformanceCategoryExists(string categoryName)
        {
            return PerformanceCounterCategory.Exists(categoryName);
        }


        /// <summary>
        /// Deletes existing performance category.
        /// </summary>
        /// <param name="categoryName">Category name</param>
        public static void DeleteCounterCategory(string categoryName)
        {
            try
            {
                // Delete existing category
                if (PerformanceCategoryExists(categoryName))
                {
                    PerformanceCounterCategory.Delete(categoryName);
                    // Log progress message
                    LogProgress(string.Format("Performance counter category '{0}' was removed.", categoryName));
                }
            }
            catch (Exception ex)
            {
                if (DatabaseHelper.IsDatabaseAvailable)
                {
                    EventLogProvider.LogException("HealthMonitoringManager", "DeleteCounterCategory", ex, additionalMessage: string.Format("Error deleting category '{0}'.", categoryName));
                    return;
                }
                // Throw original exception
                throw;
            }
        }


        /// <summary>
        /// Deletes performance categories.
        /// </summary>
        public static void DeleteCounterCategories()
        {
            // Delete general category
            DeleteCounterCategory(GeneralCategoryName);

            // Delete sites category
            DeleteCounterCategory(SitesCategoryName);
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Sets raw value to the counter.
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="counter">Counter</param>
        /// <param name="value">Value</param>
        public static void SetCounterValue(string categoryName, Counter counter, long value)
        {
            SetCounterValue(categoryName, counter, null, value);
        }


        /// <summary>
        /// Sets raw value to the instance.
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="counter">Counter</param>
        /// <param name="instanceName">Instance name</param>
        /// <param name="value">Value</param>
        public static void SetCounterValue(string categoryName, Counter counter, string instanceName, long value)
        {
            try
            {
                CheckLicense();

                PerformanceCounter pc = GetCounter(categoryName, counter, instanceName);
                if (pc != null)
                {
                    lock (LockHelper.GetLockObject(counter.Key))
                    {
                        pc.RawValue = value;
                        pc.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                SetCounterErrorMessage(counter, instanceName, categoryName, "writting to", ex);
                DisableCounter(counter);
            }
        }


        /// <summary>
        /// Increments raw value of the counter.
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="counter">Counter</param>
        public static void IncrementCounter(string categoryName, Counter counter)
        {
            IncrementCounter(categoryName, counter, null);
        }


        /// <summary>
        /// Increments raw value of the counter instance.
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="counter">Counter</param>
        /// <param name="instanceName">Instance name</param>
        public static void IncrementCounter(string categoryName, Counter counter, string instanceName)
        {
            try
            {
                CheckLicense();

                PerformanceCounter pc = GetCounter(categoryName, counter, instanceName);
                if (pc != null)
                {
                    pc.Increment();
                    pc.Close();
                }
            }
            catch (Exception ex)
            {
                SetCounterErrorMessage(counter, instanceName, categoryName, "incrementing", ex);
                DisableCounter(counter);
            }
        }


        /// <summary>
        /// Decrements raw value of the counter.
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="counter">Counter</param>
        public static void DecrementCounter(string categoryName, Counter counter)
        {
            DecrementCounter(categoryName, counter, null);
        }


        /// <summary>
        /// Decrements raw value of the counter instance.
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="counter">Counter</param>
        /// <param name="instanceName">Instance name</param>
        public static void DecrementCounter(string categoryName, Counter counter, string instanceName)
        {
            try
            {
                CheckLicense();

                PerformanceCounter pc = GetCounter(categoryName, counter, instanceName);
                if (pc != null)
                {
                    pc.Decrement();
                    pc.Close();
                }
            }
            catch (Exception ex)
            {
                SetCounterErrorMessage(counter, instanceName, categoryName, "decrementing", ex);
                DisableCounter(counter);
            }
        }


        /// <summary>
        /// Clears generic counter list.
        /// </summary>
        public static void ClearCounters()
        {
            mSitesCategoryName = null;
            mGeneralCategoryName = null;
            mCounters = null;
            mSites = null;
        }


        /// <summary>
        /// Resets values of all counters.
        /// </summary>
        public static void ResetCounters()
        {
            foreach (Counter counter in Counters)
            {
                // Clear last log time
                counter.PerformanceCounter.ClearLastLog();

                // Reset global value
                SetCounterValue(GeneralCategoryName, counter, 0);

                if (AllSites != null)
                {
                    // Reset sites value
                    foreach (string siteName in AllSites)
                    {
                        SetCounterValue(SitesCategoryName, counter, siteName, 0);
                    }
                }
            }
        }


        /// <summary>
        /// Indicates if counter is system.
        /// </summary>
        /// <param name="counterKey">Counter key</param>
        public static bool IsSystemCounter(string counterKey)
        {
            return SystemCounterKeys.Contains(counterKey) || IsSystemSpecialCounter(counterKey) || IsSystemDatabaseCounter(counterKey);
        }


        /// <summary>
        /// Indicates if counter is system counter based on special values treatment.
        /// </summary>
        /// <param name="counterKey">Counter key</param>
        public static bool IsSystemSpecialCounter(string counterKey)
        {
            return SystemSpecialCounterKeys.Contains(counterKey);
        }


        /// <summary>
        /// Indicates if counter is system counter based on special values treatment.
        /// </summary>
        /// <param name="counterKey">Counter key</param>
        public static bool IsSystemDatabaseCounter(string counterKey)
        {
            return SystemDatabaseCounterKeys.Contains(counterKey);
        }


        /// <summary>
        /// Logs the message progress.
        /// </summary>
        /// <param name="message">Progress message</param>
        public static void LogProgress(string message)
        {
            if (OnProgressLog != null)
            {
                OnProgressLog(message);
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Loads counters definition from xml files and set generic counter list.
        /// </summary>
        private static void LoadCounters()
        {
            try
            {
                if (mCounters == null)
                {
                    lock (locker)
                    {
                        if (mCounters == null)
                        {
                            mCounters = new List<Counter>();

                            // Start directory path 
                            string directoryPath = HealthMonitoringHelper.CountersStartDirectoryPath;
                            if (Directory.Exists(directoryPath))
                            {
                                LoadCountersFromSubDirectories(directoryPath);
                            }
                            else
                            {
                                throw new Exception(string.Format("Directory '{0}' doesn't exist.", directoryPath));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Error = true;
                EventLogProvider.LogException("HealthMonitoringManager", "LoadCounters", ex);
            }
        }


        /// <summary>
        /// Loads counters from subdirectories.
        /// </summary>
        /// <param name="directoryPath">Start directory path</param>
        private static void LoadCountersFromSubDirectories(string directoryPath)
        {
            foreach (string subDirectoryPath in Directory.GetDirectories(directoryPath))
            {
                // Get all files with extension '.xpc'
                foreach (string filePath in Directory.GetFiles(subDirectoryPath, "*.xpc"))
                {
                    // Load counter list from file
                    List<Counter> list = GetCountersFromXmlDefinition(filePath);

                    if ((list != null) && (list.Count > 0))
                    {
                        mCounters.AddRange(list);
                    }
                }

                LoadCountersFromSubDirectories(subDirectoryPath);
            }
        }


        /// <summary>
        /// Gets counters from xml definition.
        /// </summary>
        /// <param name="path">File path</param>
        private static List<Counter> GetCountersFromXmlDefinition(string path)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(path);

                // Select nodes 'Counter'
                XmlNodeList nodes = document.SelectNodes("//Counter");

                if ((nodes != null) && (nodes.Count > 0))
                {
                    return nodes.Cast<XmlNode>()
                                .Select(node => new Counter(node))
                                .ToList();
                }
            }
            catch (Exception ex)
            {
                Error = true;
                EventLogProvider.LogException("HealthMonitoringManager", "GetCountersFromXmlDefinition", ex);
            }

            return null;
        }


        /// <summary>
        /// Gets collection of custom creation data.
        /// </summary>
        /// <param name="counters">Counter list</param>
        private static CounterCreationDataCollection GetCustomCounters(IEnumerable<Counter> counters)
        {
            CounterCreationDataCollection counterCollection = new CounterCreationDataCollection();

            foreach (Counter counter in counters)
            {
                // Create custom counter objects and add to the collection
                CounterCreationData counterData = new CounterCreationData(counter.Name, counter.Description, counter.Type);
                counterCollection.Add(counterData);

                // Log progress message
                LogProgress(string.Format("Creating counter '{0}'.", counter.Name));
            }

            return counterCollection;
        }


        /// <summary>
        /// Sets properties GeneralCategoryName and SitesCategoryName.
        /// </summary>
        private static void InitializeCategoryNames()
        {
            string applicationName = SystemHelper.ApplicationName;
            mGeneralCategoryName = HealthMonitoringHelper.GetCategoryName(applicationName, CategoryType.General);
            mSitesCategoryName = HealthMonitoringHelper.GetCategoryName(applicationName, CategoryType.Sites);
        }


        /// <summary>
        /// Gets performance counter.
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="counter">Counter</param>
        /// <param name="instanceName">Instance name</param>
        private static PerformanceCounter GetCounter(string categoryName, Counter counter, string instanceName)
        {
            // Check category name and counter
            if (!string.IsNullOrEmpty(categoryName) && (counter != null))
            {
                return new PerformanceCounter(categoryName, counter.Name, instanceName ?? String.Empty, false);
            }

            return null;
        }


        /// <summary>
        /// Disables and sets error property of counter.
        /// </summary>
        /// <param name="counter">Counter</param>
        private static void DisableCounter(Counter counter)
        {
            if (counter != null)
            {
                counter.Enabled = false;
                counter.Error = true;
            }
        }


        /// <summary>
        /// Checks the license.
        /// </summary>
        private static void CheckLicense()
        {
            var key = LicenseKeyInfoProvider.GetBestLicense();

            if(!LicenseKeyInfoProvider.IsFeatureAvailable(key, FeatureEnum.HealthMonitoring))
            {
                throw new LicenseException("License check failed. EMS license is required for health monitoring service.");
            }
        }


        /// <summary>
        /// Sets error message of counter.
        /// </summary>
        /// <param name="counter">Counter</param>
        /// <param name="instanceName">Instance name of counter</param>
        /// <param name="categoryName">Performance category name</param>
        /// <param name="counterAction">Counter action</param>
        /// <param name="originalException">Original exception</param>
        private static void SetCounterErrorMessage(Counter counter, string instanceName, string categoryName, string counterAction, Exception originalException)
        {
            if (counter != null)
            {
                StringBuilder sb = new StringBuilder();

                string instanceStr = string.IsNullOrEmpty(instanceName) ? null : " (instance '" + instanceName + "')";
                sb.Append($"Error {counterAction} counter '{counter.Name}'{instanceStr} in category '{categoryName}'.\n");
                sb.Append("Make sure you have the counter correctly registered.\n");

                // Add original message
                if (originalException != null)
                {
                    sb.Append(" Original message: ", originalException.ToString(), "\n");
                }

                counter.LastErrorMessage = sb.ToString();
            }
        }

        #endregion
    }
}