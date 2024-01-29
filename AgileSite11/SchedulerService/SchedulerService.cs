using System;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Scheduler;
using CMS.SiteProvider;
using CMS.WinServiceEngine;

namespace CMS.SchedulerService
{
    /// <summary>
    /// Scheduler service.
    /// </summary>
    public partial class SchedulerService : BaseService
    {
        #region "Overridden properties"

        /// <summary>
        /// Service interval.
        /// </summary>
        protected override int Interval
        {
            get
            {
                return SchedulingHelper.ServiceInterval;
            }
        }


        /// <summary>
        /// Indicates if service is enabled.
        /// </summary>
        protected override bool Enabled
        {
            get
            {
                return SchedulingHelper.UseExternalService;
            }
        }


        /// <summary>
        /// Maximum service interval. Default value is 30 seconds.
        /// </summary>
        protected override int MaxInterval
        {
            get
            {
                return 30;
            }
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
                throw new LicenseException("License check failed. EMS license is required for scheduler service.");
            }

            // Get running sites
            var sites = SiteInfoProvider.GetSites()
                                        .WhereEquals("SiteStatus", SiteStatusEnum.Running.ToStringRepresentation())
                                        .Columns("SiteName")
                                        .Select(s => s.SiteName)
                                        .ToList();

            // There are no running sites
            if (sites.Count <= 0)
            {
                return;
            }

            // Execute tasks in separate thread for each site
            foreach (var thread in sites.Select(siteName => new CMSThread(() => Run(siteName))))
            {
                thread.Start();
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Run tasks execution
        /// </summary>
        /// <param name="siteName">Site name of the scheduled tasks to be executed</param>
        private void Run(string siteName)
        {
            SchedulingExecutor.ExecuteScheduledTasks(siteName, WebFarmHelper.ServerName);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructors.
        /// </summary>
        /// <param name="webAppPath">Web application path</param>
        public SchedulerService(string webAppPath)
            : base(webAppPath, WinServiceHelper.SCHEDULER_SERVICE_BASENAME)
        {
            InitializeComponent();

            CMSApplication.WaitForDatabaseAvailable.Value = true;
            CMSApplication.Init();
        }

        #endregion
    }
}