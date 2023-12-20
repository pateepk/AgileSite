using System;

using CMS.Base;
using CMS.EventLog;

namespace CMS.WebAnalytics.Internal
{
    /// <summary>
    /// Worker thread for logging hits directly to database.
    /// </summary>
    internal sealed class HitLogDirectProcessor : ThreadQueueWorker<LogRecord, HitLogDirectProcessor>
    {
        protected override int DefaultInterval => 30000;


        /// <summary>
        /// Save single <paramref name="item"/> to database.
        /// </summary>
        protected override void ProcessItem(LogRecord item)
        {
            try
            {
                HitLogProcessor.SaveLogToDatabase(item);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("HitLogDirectProcessor", "PROCESSITEM", ex);
            }
        }


        protected override void Finish()
        {
        }
    }
}
