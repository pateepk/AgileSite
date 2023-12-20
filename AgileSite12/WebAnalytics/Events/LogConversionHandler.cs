using System.Collections.Generic;

using CMS.Base;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Handler for the conversion logging.
    /// </summary>
    public class LogConversionHandler : AdvancedHandler<LogConversionHandler, CMSEventArgs<LogRecord>>
    {
        /// <summary>
        /// Initiates the event handling.
        /// </summary>
        /// <param name="conversion">Processed log record</param>
        public LogConversionHandler StartEvent(LogRecord conversion)
        {
            var e = new CMSEventArgs<LogRecord>()
                {
                    Parameter = conversion,
                };

            return StartEvent(e);
        }
    }
}
