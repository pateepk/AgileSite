using System;

using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Base;

namespace CMS.Scheduler
{
    /// <summary>
    /// Class for RentedLicenseUpdaterTask scheduled task, which renews rented licenses keys.
    /// </summary>
    public class RentedLicenseUpdaterTask : ITask
    {
        #region "Methods"

        /// <summary>
        /// Executes license update
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                if (String.IsNullOrEmpty(task.TaskData))
                {
                    return ResHelper.GetString("RentedLicense.NoInput");
                }

                // Each keycode on new line
                string[] rentedKeys = task.TaskData.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                // Service url can be set explicitly from settings, for special cases only. 
                string serviceUrl = ValidationHelper.GetString(SettingsHelper.AppSettings["RentedLicenseServiceUrl"], null);

                RentedUpdater updater = new RentedUpdater(serviceUrl);

                // Renews license keys for domains specified by rented keys.
                return updater.RenewLicenseKeys(rentedKeys);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(RentedUpdater.LOG_SOURCE, RentedUpdater.EVENT_CODE, ex);
                return ex.Message;
            }
        }

        #endregion
    }
}