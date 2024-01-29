using System;
using System.Web;

using CMS.EventLog;
using CMS.Base;

using ITHit.WebDAV.Logger;
using ITHit.WebDAV.Server.Response;

namespace CMS.WebDAV
{
    /// <summary>
    /// WebDAV handler.
    /// </summary>
    public class WebDAVHandler : IHttpHandler
    {
        #region "Properties"

        /// <summary>
        /// Gets file path of WebDAV log file.
        /// </summary>
        public static string LogFilePath
        {
            get
            {
                // Get file name from web.config
                string fileName = SettingsHelper.AppSettings["CMSWebDAVLogFilePath"];

                return !string.IsNullOrEmpty(fileName) ? HttpContext.Current.Request.PhysicalApplicationPath + fileName : null;
            }
        }


        /// <summary>
        /// Gets log level.
        /// </summary>
        public static LogLevel LogLevel
        {
            get
            {
                LogLevel logLevel = LogLevel.Off;
                string level = SettingsHelper.AppSettings["CMSWebDAVLogLevel"];

                if (!string.IsNullOrEmpty(level))
                {
                    try
                    {
                        object value = Enum.Parse(typeof(LogLevel), level, true);
                        logLevel = (LogLevel)value;
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogException("WebDAVHandler_LogLevel", "WebDAV", ex);
                    }
                }

                return logLevel;
            }
        }

        #endregion


        #region "IHttpHandler Members"

        /// <summary>
        /// Processes WebDAV request.
        /// </summary>
        /// <param name="context">HTTP context to work with</param>
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                // WebDAV is enabled
                if (WebDAVHelper.IsWebDAVEnabled())
                {
                    context.Response.BufferOutput = false;

                    WebDAVEngine engine = new WebDAVEngine();
                    engine.IgnoreExceptions = false;

                    // Create custom handlers
                    GetHandler getHandler = new GetHandler();
                    PutHandler putHandler = new PutHandler();

                    // Register handlers
                    getHandler.OriginalHandler = engine.RegisterMethodHandler("GET", getHandler);
                    putHandler.OriginalHandler = engine.RegisterMethodHandler("PUT", putHandler);

                    engine.Run(HttpContext.Current);
                }
                // WebDAV is disabled
                else
                {
                    AccessDeniedResponse adr = new AccessDeniedResponse();
                    context.Response.StatusCode = adr.Code;
                    context.Response.StatusDescription = adr.Description;
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("WebDAVHandler_ProcessRequest", "WebDAV", ex);
            }
        }


        /// <summary>
        /// Gets a value indicating whether another request can use the IHttpHandler instance.
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        #endregion
    }
}