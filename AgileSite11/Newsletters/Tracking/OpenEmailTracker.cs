using System;
using System.Net;
using System.Web;

using CMS.Core;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Newsletters
{
    /// <summary>
    /// Tracks subscribers who opened a newsletter issues sent by e-mail.
    /// </summary>
    /// <remarks>
    /// An e-mail containing a newsletter issue contains a link to image.
    /// This link contains a subscriber's guid and the guid of the newsletter issue.
    /// The image is sent in response and the request is logged.
    /// </remarks>
    public class OpenEmailTracker : IHttpHandler
    {
        #region "Fields"

        // Object for locking.
        private static readonly object lockObj = new object();

        // Byte array representing a 1x1 transparent gif
        private static readonly byte[] mImage = Convert.FromBase64String("R0lGODlhAQABAIAAANvf7wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==");

        #endregion


        #region "Properties"

        /// <summary>
        /// The tracking image that is sent in response.
        /// </summary>
        public static byte[] Image
        {
            get
            {
                return mImage;
            }
        }


        /// <summary>
        /// Gets whether this handler can be reused for other request; always returns true.
        /// </summary>
        /// <value>Always true</value>
        public virtual bool IsReusable
        {
            get
            {
                return true;
            }
        }


        /// <summary>
        /// Gets email from query string parameter.
        /// </summary>
        protected string Email
        {
            get
            {
                return QueryHelper.GetString("email", String.Empty);
            }
        }


        /// <summary>
        ///  Gets hash from query string parameter.
        /// </summary>
        protected string Hash
        {
            get
            {
                return QueryHelper.GetString("hash", null);
            }
        }


        /// <summary>
        /// Gets issue GUID from query string parameter.
        /// </summary>
        protected Guid IssueGuid
        {
            get
            {
                return QueryHelper.GetGuid("issueguid", Guid.Empty);
            }
        }


        /// <summary>
        /// Gets ping from query string parameter.
        /// </summary>
        /// <remarks>
        /// The ping parameter serves for endpoint availability checking.
        /// </remarks>
        protected int Ping
        {
            get
            {
                return QueryHelper.GetInteger("ping", 0);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Processes the tracking data and serves a dummy image in response.
        /// </summary>
        /// <param name="context">An HttpContext associated with this request</param>
        public virtual void ProcessRequest(HttpContext context)
        {
            var baseContext = new HttpContextWrapper(context);
            CMSHttpContext.Current = baseContext;

            ProcessRequest(baseContext);
        }


        /// <summary>
        /// Processes the tracking data and serves a dummy image in response.
        /// </summary>
        /// <param name="context">An HttpContext associated with this request</param>
        protected internal virtual void ProcessRequest(HttpContextBase context)
        {
            int ping;

            if (UseCached(context.Request))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                context.Response.SuppressContent = true;
            }
            else if ((ping = Ping) != 0)
            {
                ActivityTrackingHelper.RespondToPing(context, ping);
            }
            else
            {
                var emailTrackingHashValidator = Service.Resolve<IEmailHashValidator>();

                Log(emailTrackingHashValidator, context);

                context.Response.ContentType = "image/gif";
                context.Response.Cache.SetLastModified(DateTime.Now.AddSeconds(-1));
                context.Response.Cache.SetCacheability(HttpCacheability.Private);

                context.Response.OutputStream.Write(Image, 0, Image.Length);

                context.Response.End();
            }
        }


        /// <summary>
        /// Checks if the same request for this image hasn't arrived in last 24 hours.
        /// </summary>
        /// <param name="request">An HttpRequest object</param>
        /// <returns>true if the same request was already received in last 24 hours, otherwise false</returns>
        private static bool UseCached(HttpRequestBase request)
        {
            string ifModified = ValidationHelper.GetString(request.Headers["If-Modified-Since"], String.Empty);
            DateTime modified = ValidationHelper.GetDateTime(ifModified, DateTime.MinValue);

            return (modified.AddHours(24) >= DateTime.Now);
        }


        /// <summary>
        /// Logs a request - extracts the subscriber and issue and counts the e-mail as opened.
        /// </summary>
        /// <param name="emailHashValidator">Service for validating link hash</param>
        /// <param name="context">Http context in which, this method is processed</param>
        /// <exception cref="ArgumentNullException"><paramref name="emailHashValidator"/> is null.</exception>
        internal void Log(IEmailHashValidator emailHashValidator, HttpContextBase context)
        {
            if (emailHashValidator == null)
            {
                throw new ArgumentNullException("emailHashValidator");
            }

            Guid issueGuid = IssueGuid;
            string subscriberEmail = Email;

            // If the guids are valid, log the email with newsletter issue as read
            if ((issueGuid != Guid.Empty) && !String.IsNullOrEmpty(subscriberEmail))
            {
                int siteId = ValidationHelper.GetInteger(SiteContext.CurrentSiteID, 0);

                // Get issue
                IssueInfo issue = IssueInfoProvider.GetIssueInfo(issueGuid, siteId);
                if (issue == null)
                {
                    return;
                }

                var newsletter = NewsletterInfoProvider.GetNewsletterInfo(issue.IssueNewsletterID);
                if (newsletter == null)
                {
                    return;
                }

                var hash = Hash;
                if ((hash == null) || !emailHashValidator.ValidateEmailHash(hash, subscriberEmail))
                {
                    return;
                }

                lock (lockObj)
                {
                    int readIssueID = OpenedEmailInfoProvider.LogOpenedEmail(subscriberEmail, issue.IssueID);

                    // Increase opened email counter in the issue and clear it from cache to avoid miscounts
                    IssueInfoProvider.AddOpenedEmails(readIssueID);
                }

                NewsletterEvents.SubscriberOpensEmail.StartEvent(new LinksEventArgs
                {
                    IssueInfo = issue,
                    NewsletterInfo = newsletter,
                    AdditionalParameters = context.Request.QueryString,
                });
            }
        }

        #endregion
    }
}