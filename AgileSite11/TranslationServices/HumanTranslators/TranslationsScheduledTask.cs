using System;

using CMS.Scheduler;
using CMS.SiteProvider;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Provides an ITask interface to process translated documents from translation services
    /// </summary>
    public class TranslationsScheduledTask : ITask
    {
        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                string siteName = SiteInfoProvider.GetSiteName(task.TaskSiteID);
                return TranslationServiceHelper.CheckAndDownloadTranslations(siteName);
            }
            catch (Exception ex)
            {
                TranslationServiceHelper.LogEvent(ex);
                return (ex.Message);
            }
        }
    }
}
