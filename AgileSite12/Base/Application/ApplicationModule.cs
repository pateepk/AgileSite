using System;
using System.Web;

using CMS.Core;

#if NETFULLFRAMEWORK
namespace CMS.Base
{
    /// <summary>
    /// Application module for registering the application events globally
    /// </summary>
    public class ApplicationModule : IHttpModule
    {
        #region "Methods"

        /// <summary>
        /// Registers the event handler(s).
        /// </summary>
        /// <param name="app">The http application</param>
        public void Init(HttpApplication app)
        {
            // Initialize the handlers
            app.BeginRequest += BeginRequest;
            app.AuthenticateRequest += AuthenticateRequest;
            app.PostAuthenticateRequest += PostAuthenticateRequest;

            app.AuthorizeRequest += AuthorizeRequest;
            app.PostAuthorizeRequest += PostAuthorizeRequest;
            app.ResolveRequestCache += ResolveRequestCache;
            app.PostResolveRequestCache += PostResolveRequestCache;

            app.MapRequestHandler += MapRequestHandler;
            app.PostMapRequestHandler += PostMapRequestHandler;

            app.AcquireRequestState += AcquireRequestState;
            app.PostAcquireRequestState += PostAcquireRequestState;

            app.PreRequestHandlerExecute += PreRequestHandlerExecute;
            app.PostRequestHandlerExecute += PostRequestHandlerExecute;
            app.ReleaseRequestState += ReleaseRequestState;
            app.PostReleaseRequestState += PostReleaseRequestState;
            app.UpdateRequestCache += UpdateRequestCache;
            app.PostUpdateRequestCache += PostUpdateRequestCache;
            app.LogRequest += LogRequest;
            app.PostLogRequest += PostLogRequest;

            app.EndRequest += EndRequest;

            // Unordered events
            app.PreSendRequestHeaders += PreSendRequestHeaders;
            app.PreSendRequestContent += PreSendRequestContent;
            app.RequestCompleted += RequestCompleted;
        }


        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Dispose()
        {
        }

        #endregion


        #region "Static event handlers"

        private static void PostMapRequestHandler(object sender, EventArgs e)
        {
            RequestEvents.PostMapRequestHandler.StartEvent(e);
        }


        private static void MapRequestHandler(object sender, EventArgs e)
        {
            RequestEvents.MapRequestHandler.StartEvent(e);
        }


        private static void EndRequest(object sender, EventArgs e)
        {
            RequestEvents.End.StartEvent(e);

            try
            {
                // Do not allow redirects in further code
                using (new CMSActionContext { AllowLicenseRedirect = false })
                {
                    // Run post-request tasks
                    RequestEvents.RunEndRequestTasks.StartEvent(e);

                    // Finalize the request
                    RequestEvents.Finalize.StartEvent(e);
                }
            }
            catch (Exception ex)
            {
                // Catch and log the error to prevent further modification to the response
                CoreServices.EventLog.LogException("Application", "EndRequest", ex);
            }
        }


        private static void BeginRequest(object sender, EventArgs e)
        {
            RequestEvents.Prepare.StartEvent(e);

            RequestEvents.Begin.StartEvent(e);
        }


        private static void AuthorizeRequest(object sender, EventArgs e)
        {
            RequestEvents.Authorize.StartEvent(e);
        }


        private static void PostAuthorizeRequest(object sender, EventArgs e)
        {
            RequestEvents.PostAuthorize.StartEvent(e);
        }


        private void ResolveRequestCache(object sender, EventArgs e)
        {
            RequestEvents.ResolveRequestCache.StartEvent(e);
        }


        private void PostResolveRequestCache(object sender, EventArgs e)
        {
            RequestEvents.PostResolveRequestCache.StartEvent(e);
        }


        private static void AuthenticateRequest(object sender, EventArgs e)
        {
            RequestEvents.Authenticate.StartEvent(e);
        }


        private void PostAuthenticateRequest(object sender, EventArgs e)
        {
            RequestEvents.PostAuthenticate.StartEvent(e);
        }


        private static void AcquireRequestState(object sender, EventArgs e)
        {
            RequestEvents.AcquireRequestState.StartEvent(e);
        }


        private static void PostAcquireRequestState(object sender, EventArgs e)
        {
            RequestEvents.PostAcquireRequestState.StartEvent(e);
        }


        private void PreRequestHandlerExecute(object sender, EventArgs e)
        {
            RequestEvents.PreRequestHandlerExecute.StartEvent(e);
        }


        private void PostRequestHandlerExecute(object sender, EventArgs e)
        {
            RequestEvents.PostRequestHandlerExecute.StartEvent(e);
        }


        private void ReleaseRequestState(object sender, EventArgs e)
        {
            RequestEvents.ReleaseRequestState.StartEvent(e);
        }


        private void PostReleaseRequestState(object sender, EventArgs e)
        {
            RequestEvents.PostReleaseRequestState.StartEvent(e);
        }


        private void UpdateRequestCache(object sender, EventArgs e)
        {
            RequestEvents.UpdateRequestCache.StartEvent(e);
        }


        private void PostUpdateRequestCache(object sender, EventArgs e)
        {
            RequestEvents.PostUpdateRequestCache.StartEvent(e);
        }


        private void LogRequest(object sender, EventArgs e)
        {
            RequestEvents.LogRequest.StartEvent(e);
        }


        private void PostLogRequest(object sender, EventArgs e)
        {
            RequestEvents.PostLogRequest.StartEvent(e);
        }


        private void PreSendRequestHeaders(object sender, EventArgs e)
        {
            RequestEvents.PreSendRequestHeaders.StartEvent(e);
        }


        private void PreSendRequestContent(object sender, EventArgs e)
        {
            RequestEvents.PreSendRequestContent.StartEvent(e);
        }


        private void RequestCompleted(object sender, EventArgs e)
        {
            RequestEvents.RequestCompleted.StartEvent(e);
        }

        #endregion
    }
}
#endif
