using System;
using System.Web;

using CMS.Core;

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

            app.AuthorizeRequest += AuthorizeRequest;
            app.PostAuthorizeRequest += PostAuthorizeRequest;
            app.PostResolveRequestCache += PostResolveRequestCache;

            // Try to map request handler - Integrated mode
            if (SystemContext.IsIntegratedMode)
            {
                app.MapRequestHandler += MapRequestHandler;
                app.PostMapRequestHandler += PostMapRequestHandler;
            }
            // Map the request handler to authorize request - Classic mode
            else
            {
                app.AuthorizeRequest += MapRequestHandler;
            }

            app.AcquireRequestState += AcquireRequestState;
            app.PostAcquireRequestState += PostAcquireRequestState;

            app.EndRequest += EndRequest;
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


        private void PostResolveRequestCache(object sender, EventArgs e)
        {
            RequestEvents.PostResolveRequestCache.StartEvent(e);
        }


        private static void AuthenticateRequest(object sender, EventArgs e)
        {
            RequestEvents.Authenticate.StartEvent(e);
        }


        private static void AcquireRequestState(object sender, EventArgs e)
        {
            RequestEvents.AcquireRequestState.StartEvent(e);
        }


        private static void PostAcquireRequestState(object sender, EventArgs e)
        {
            RequestEvents.PostAcquireRequestState.StartEvent(e);
        }

        #endregion
    }
}