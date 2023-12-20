using System;

using CMS.Core;
using CMS.Core.Internal;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Scheduler;

namespace CMS.DocumentEngine.PageBuilder
{
    /// <summary>
    /// Provides an scheduled task for the temporary PageBuilder widgets configuration deletion.
    /// </summary>
    public sealed class TempPageBuilderWidgetsCleaner : ITask
    {
        /// <summary>
        /// Executes the purge action.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                var where = GetDeletionThresholdWhereCondition();
                TempPageBuilderWidgetsInfoProvider.BulkDeleteData(where);
                return null;
            }
            catch (Exception e)
            {
                EventLogProvider.LogException("PageBuilder", "EXCEPTION", e);
                return e.Message;
            }
        }


        internal WhereCondition GetDeletionThresholdWhereCondition()
        {
            var dateTimeService = Service.Resolve<IDateTimeNowService>();
            var date = dateTimeService.GetDateTimeNow().Subtract(TimeSpan.FromHours(GetDeletionThreshold()));

            return new WhereCondition().WhereLessThan(TempPageBuilderWidgetsInfo.TYPEINFO.TimeStampColumn, date);
        }


        private int GetDeletionThreshold()
        {
            var olderThan = Service.Resolve<IAppSettingsService>()["CMSDeleteTempPageBuilderWidgetsOlderThan"];
            return ValidationHelper.GetInteger(olderThan, 1);
        }
    }
}
