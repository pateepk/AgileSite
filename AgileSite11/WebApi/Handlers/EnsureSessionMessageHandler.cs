using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using CMS.EventLog;
using CMS.SiteProvider;

namespace CMS.WebApi
{
    /// <summary>
    /// HTTP handler ensuring that the request has available session.
    /// </summary>
    internal class EnsureSessionMessageHandler : DelegatingHandler
    {
        /// <summary>
        /// Ensures the request has session available. Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="request"/> is null.</exception>
        /// <returns>
        /// The task object representing the asynchronous operation.
        /// </returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var currentContext = (HttpContextWrapper)request.Properties["MS_HttpContext"];
            if (currentContext.Session == null)
            {
                EventLogProvider.LogWarning("WebApi", "MISSINGSESSION", null, SiteContext.CurrentSiteID,
                    "Session is empty for web API call on url " + request.RequestUri + ", although it is required for the used route."
                );
                var task = new TaskCompletionSource<HttpResponseMessage>();
                task.SetResult(new HttpResponseMessage(HttpStatusCode.BadRequest));
                return task.Task;
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}