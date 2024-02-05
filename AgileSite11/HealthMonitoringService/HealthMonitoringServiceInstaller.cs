using System;
using CMS.DataEngine;
using CMS.Base;
using CMS.WinServiceEngine;

namespace CMS.HealthMonitoringService
{
    /// <summary>
    /// Class for installing of Health monitoring service.
    /// </summary>
    public partial class HealthMonitoringServiceInstaller : BaseServiceInstaller
    {
        #region "Constructors"

        /// <summary>
        /// Base constructor.
        /// </summary>
        public HealthMonitoringServiceInstaller()
            : base(WinServiceHelper.HM_SERVICE_BASENAME)
        {
            InitializeComponent();
        }

        #endregion
    }
}