using System.Web;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.AspNet.Platform
{
    /// <summary>
    /// Logs last application error.
    /// </summary>
    public static class ApplicationErrorLogger
    {
        /// <summary>
        /// Logs the last application error.
        /// </summary>
        public static void LogLastApplicationError()
        {
            if (ConnectionHelper.ConnectionAvailable && (CMSHttpContext.Current != null) && RequestContext.LogCurrentError)
            {
                var ex = CMSHttpContext.Current.Server.GetLastError();
                if (ex != null)
                {
                    const string EVENT_CODE = "EXCEPTION";
                    const string EVENT_TYPE = EventType.ERROR;

                    // Log request operation
                    RequestDebug.LogRequestOperation("OnError", ex.Message, 0);

                    // Page not found was already manually logged
                    var log = RequestContext.CurrentStatus != RequestStatusEnum.PageNotFound;

                    // Do not log page not found http exception... it's handled automatically
                    if ((ex is HttpException httpException) && (httpException.GetHttpCode() == 404))
                    {
                        log = false;
                    }

                    if (log)
                    {
                        // Initiate the event
                        var logException = true;
                        SystemEvents.Exception.StartEvent(ex, ref logException);

                        if (logException)
                        {
                            // Write error to Event log
                            try
                            {
                                EventLogProvider.LogEvent(EVENT_TYPE, "Application_Error", EVENT_CODE, EventLogProvider.GetExceptionLogMessage(ex));
                            }
                            catch
                            {
                                // can't write to log, do not process any code
                            }
                        }
                    }
                }
            }
        }
    }
}
