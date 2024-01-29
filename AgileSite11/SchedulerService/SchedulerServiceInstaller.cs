using System;
using CMS.DataEngine;
using CMS.Base;
using CMS.WinServiceEngine;

namespace CMS.SchedulerService
{
    /// <summary>
    /// Class for installing of Scheduler service.
    /// </summary>
    public partial class SchedulerServiceInstaller : BaseServiceInstaller
    {
        #region "Constructors

        /// <summary>
        /// Base constructor.
        /// </summary>
        public SchedulerServiceInstaller()
            : base(WinServiceHelper.SCHEDULER_SERVICE_BASENAME)
        {
            InitializeComponent();
        }

        #endregion
    }
}