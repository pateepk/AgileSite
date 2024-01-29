using System.Collections.Generic;

using CMS.Base;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Handler for the log processing.
    /// </summary>
    public class ProcessLogRecordHandler : AdvancedHandler<ProcessLogRecordHandler, CMSEventArgs<LogRecord>>
    {
        /// <summary>
        /// Initiates the event handling.
        /// </summary>
        /// <param name="logRecord">Processed log record</param>
        public ProcessLogRecordHandler StartEvent(LogRecord logRecord)
        {
            var e = new CMSEventArgs<LogRecord>()
                {
                    Parameter = logRecord,
                };

            return StartEvent(e);
        }
    }
}