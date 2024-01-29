using System;
using System.IO;

using CMS.DataEngine;
using CMS.HealthMonitoring;
using CMS.LicenseProvider;
using CMS.WinServiceEngine;

namespace CMS.HealthMonitoringService
{
    /// <summary>
    /// Health monitoring service.
    /// </summary>
    public partial class HealthMonitoringService : BaseService
    {
        #region "Variables"

        private FileSystemWatcher mCountersWatcher = null;
        private static bool mCategoriesInstalled = false;

        #endregion


        #region "Overridden properties"

        /// <summary>
        /// Service interval.
        /// </summary>
        protected override int Interval
        {
            get
            {
                return (DatabaseHelper.IsDatabaseAvailable ? HealthMonitoringHelper.ServiceMonitoringInterval : 0);
            }
        }


        /// <summary>
        /// Indicates if service is enabled.
        /// </summary>
        protected override bool Enabled
        {
            get
            {
                return DatabaseHelper.IsDatabaseAvailable && HealthMonitoringHelper.LogCounters && HealthMonitoringHelper.UseExternalService;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructors.
        /// </summary>
        /// <param name="webAppPath">Web application path</param>
        public HealthMonitoringService(string webAppPath)
            : base(webAppPath, WinServiceHelper.HM_SERVICE_BASENAME)
        {
            InitializeComponent();

            CMSApplication.WaitForDatabaseAvailable.Value = true;
            CMSApplication.Init();

            // Initialize watcher
            InitializeWatcher();
        }

        #endregion


        #region "Overridden methods"

        /// <summary>
        /// Execute service method.
        /// </summary>
        protected override void Execute()
        {
            // Check license
            var key = LicenseKeyInfoProvider.GetBestLicense();
            if ((key == null) || (key.Edition != ProductEditionEnum.EnterpriseMarketingSolution))
            {
                throw new LicenseException("License check failed. EMS license is required for health monitoring service.");
            }

            HealthMonitoringLogHelper.LogServiceCounters();

            if (!mCategoriesInstalled)
            {
                mCategoriesInstalled = true;

                // Install categories if don't exist
                InstallCategories();
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Installs categories.
        /// </summary>
        private void InstallCategories()
        {
            if (!HealthMonitoringManager.PerformanceCategoryExists(HealthMonitoringManager.GeneralCategoryName)
                || !HealthMonitoringManager.PerformanceCategoryExists(HealthMonitoringManager.SitesCategoryName))
            {
                HealthMonitoringManager.CreateCounterCategories();
            }
        }


        /// <summary>
        /// Initializes file system watcher.
        /// </summary>
        private void InitializeWatcher()
        {
            try
            {
                if (Directory.Exists(HealthMonitoringHelper.CountersStartDirectoryPath))
                {
                    // Watcher to the path where are system counters
                    mCountersWatcher = new FileSystemWatcher(HealthMonitoringHelper.CountersStartDirectoryPath, "*.xpc");
                    mCountersWatcher.IncludeSubdirectories = true;
                    mCountersWatcher.EnableRaisingEvents = true;
                    mCountersWatcher.Changed += new FileSystemEventHandler(Watcher_Event);
                    mCountersWatcher.Created += new FileSystemEventHandler(Watcher_Event);
                    mCountersWatcher.Deleted += new FileSystemEventHandler(Watcher_Event);
                }
            }
            catch (Exception ex)
            {
                // Log exception
                ServiceHelper.LogException(ServiceName, ex);
            }
        }


        /// <summary>
        /// Handles changed, created and deleted event of file system watcher.
        /// </summary>
        /// <param name="sender">File system watcher</param>
        /// <param name="e">File system event argument</param>
        private void Watcher_Event(object sender, FileSystemEventArgs e)
        {
            try
            {
                // Temporarily disable raising events because event OnChange is called twice when file is changed
                mCountersWatcher.EnableRaisingEvents = false;

                // Disable logging
                HealthMonitoringHelper.CanLog = false;

                string tempFile = HealthMonitoringHelper.CountersStartDirectoryPath + "\\HealthMonitoring\\Counters.tmp";
                FileStream fs = File.Create(tempFile);
                fs.Close();
                fs.Dispose();

                // Create categories
                HealthMonitoringManager.CreateCounterCategories();

                // Delete temporary file
                File.Delete(tempFile);
            }
            catch (Exception ex)
            {
                // Log exception
                ServiceHelper.LogException(ServiceName, ex);
            }
            finally
            {
                // Enable logging
                HealthMonitoringHelper.CanLog = true;

                // Enable raising events
                mCountersWatcher.EnableRaisingEvents = true;
            }
        }

        #endregion
    }
}