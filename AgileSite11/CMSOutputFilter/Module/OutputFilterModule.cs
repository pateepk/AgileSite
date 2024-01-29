using System;
using System.Web;
using System.Web.UI;

using CMS.Helpers;

namespace CMS.OutputFilter
{
    /// <summary>
    /// HttpModule wrapper for the output filter.
    /// </summary>
    public class OutputFilterModule : IHttpModule
    {
        #region "Methods"

        /// <summary>
        /// Registers the event handler(s).
        /// </summary>
        /// <param name="app">The http application</param>
        public void Init(HttpApplication app)
        {
            // Hook the handlers
            app.ReleaseRequestState += ReleaseRequestState;
        }


        /// <summary>
        /// Output filter assignment handler.
        /// </summary>
        private static void ReleaseRequestState(object sender, EventArgs e)
        {
            if (RequestContext.ResponseIsCompressed)
            {
                // Do not apply output filter on a compressed response
                return;
            }

            bool logToFile;
            bool logToDebug;
            var outputFilterRequested = IsOutputFilterRequestedByDebug(out logToDebug, out logToFile);

            ApplyFilterIfRequired(outputFilterRequested, logToFile, logToDebug);
        }


        private static bool IsOutputFilterRequestedByDebug(out bool debugCurrent, out bool logCurrent)
        {
            logCurrent = false;
            debugCurrent = false;

            // Log and debug is available only when using output filter cache
            if (!ResponseOutputFilter.UseOutputFilterCache)
            {
                return false;
            }

            var outputFilterRequested = (OutputFilterContext.LogCurrentOutputToFile || OutputDebug.DebugCurrentRequest) && IsCurrentHandlerPage();

            logCurrent = outputFilterRequested && OutputFilterContext.LogCurrentOutputToFile;
            debugCurrent = outputFilterRequested && OutputDebug.DebugCurrentRequest;

            return outputFilterRequested;
        }


        private static bool IsOutputFilterRequestedByCompression()
        {
            var contentType = CMSHttpContext.Current.Response.ContentType;
            return RequestContext.UseGZip && IsCurrentHandlerPage() && string.Equals(contentType, "text/html", StringComparison.OrdinalIgnoreCase);
        }


        private static bool IsCurrentHandlerPage()
        {
            return CMSHttpContext.Current.Handler is Page;
        }


        private static void ApplyFilterIfRequired(bool outputFilterRequested, bool logToFile, bool logToDebug)
        {
            // Ensure virtual context prefixes for not excluded URLs
            var ensurePrefixes = VirtualContext.IsInitialized && (RequestContext.CurrentExcludedStatus != ExcludedSystemEnum.Excluded);

            var applyFilter = OutputFilterContext.ApplyOutputFilter;
            var useGZip = IsOutputFilterRequestedByCompression();

            if (applyFilter || useGZip || ensurePrefixes || outputFilterRequested)
            {
                var filter = ResponseOutputFilter.EnsureOutputFilter();
                filter.ApplyFilter = applyFilter;
                filter.UseGZip = useGZip;

                filter.LogToFile = logToFile;
                filter.LogToDebug = logToDebug;

                // Set the filter for the response
                var response = HttpContext.Current.Response;
                if (response.Filter != filter)
                {
                    response.Filter = filter;
                }
            }
        }


        void IHttpModule.Dispose()
        {
        }

        #endregion
    }
}