using System.Collections;
using System.Collections.Specialized;
using System.Web;
using System.Web.Helpers;

using CMS.AspNet.Platform;
using CMS.Base;
using CMS.Core;
using CMS.EventLog;
using CMS.Helpers;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Handles the <see cref="VirtualContext"/> URLs and rewrites it to the original non-virtual context path.
    /// </summary>
    /// <remarks>
    /// Uses the collection of <see cref="IVirtualContextUrlProcessor"/> registered in the <see cref="VirtualContextUrlProcessorsRegister"/> to validate the request.
    /// </remarks>
    internal static class VirtualContextUrlRewriter
    {
        /// <summary>
        /// Registers virtual context rewriter to the request pipeline.
        /// </summary>
        /// <remarks>
        /// This method has to be called only once within the application life-cycle. Thread-safety must be ensured by caller.
        /// </remarks>
        public static void Initialize()
        {
            RequestEvents.Begin.Execute += (s, e) =>
            {
                var httpContextWrapper = new HttpContextWrapper(HttpContext.Current);

                ProcessVirtualContextRequest(httpContextWrapper);
            };
        }


        /// <summary>
        /// Processes every request to handle URLs containing <see cref="VirtualContext"/>.
        /// </summary>
        internal static void ProcessVirtualContextRequest(HttpContextBase httpContextWrapper)
        {
            var processors = VirtualContextUrlProcessorsRegister.Instance.Processors;
            if (processors.Count == 0)
            {
                return;
            }

            var relativeFilePath = httpContextWrapper.Request.AppRelativeCurrentExecutionFilePath?.TrimStart('~');

            NameValueCollection parsedValues = null;
            try
            {
                parsedValues = VirtualContext.LoadVirtualContextValues(ref relativeFilePath);
            }
            catch (InvalidVirtualContextException ex)
            {
                var eventLogService = Service.Resolve<IEventLogService>();
                eventLogService.LogException("VirtualContextHashOrUserValidationFailed", "VIRTUALCONTEXT", ex);

                HandleInvalidUrl(ex);
            }

            if (parsedValues != null)
            {
                if (!VirtualContext.ValidatePathHash(relativeFilePath))
                {
                    var eventLogService = Service.Resolve<IEventLogService>();
                    eventLogService.LogEvent(EventType.ERROR, "VirtualContextHashValidationFailed", "VIRTUALCONTEXT", "Relative path hash validation has failed.");

                    HandleInvalidUrl();
                }

                foreach (var processor in processors)
                {
                    var processed = processor.ValidateAndInitializeVirtualContext(relativeFilePath, parsedValues);
                    if (processed)
                    {
                        AntiForgeryConfig.SuppressXFrameOptionsHeader = true;

                        // Do not cache response (on the server) in a preview mode, it has to contain current data
                        httpContextWrapper.Response.Cache.SetNoServerCaching();

                        // Remove preview mode information from the request URL
                        httpContextWrapper.RewritePath("~" + relativeFilePath, httpContextWrapper.Request.PathInfo, httpContextWrapper.Request.GetEffectiveUrl().Query.TrimStart('?'));

                        // Remember the URL processor that validated this request
                        Service.Resolve<ICurrentUrlProcessorService>().SetProcessor(processor);

                        break;
                    }
                }
            }

            foreach (var processor in processors)
            {
                processor.PostVirtualContextInitialization(httpContextWrapper);
            }
        }


        private static void HandleInvalidUrl(InvalidVirtualContextException ex = null)
        {
            VirtualContext.Reset();
            throw new HttpException(404, "The preview link is not valid.", ex);
        }
    }
}
