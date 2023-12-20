using System;
using System.Collections.Specialized;
using System.Net;
using System.Web;

using CMS.Base;
using CMS.Core;
using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Newsletters
{
    /// <summary>
    /// Tracks subscribers who clicked on a link in a newsletter issue.
    /// </summary>
    /// <remarks>
    /// Handles all modified links that were converted to the tracking ones via <see cref="LinkConverter.ConvertToTracking"/>.
    /// Besides guids related to the email, every link has subscriber's email and its salted hash, which is used to validate the link.
    /// Emails sent prior to version 9 won't be logging contact activities, nor set up Online marketing context.
    /// A redirect to the original URL is sent in the response.
    /// </remarks>
    public class LinkTracker : IHttpHandler
    {
        #region "Variables"

        /// <summary>
        /// Object for locking.
        /// </summary>
        private static readonly object lockObj = new object();

        #endregion


        #region "Properties"

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
        /// Gets link GUID from query string parameter.
        /// </summary>
        protected Guid LinkGuid
        {
            get
            {
                return QueryHelper.GetGuid("linkguid", Guid.Empty);
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
        /// Processes the tracking data and redirects request to original URL.
        /// </summary>
        /// <param name="context">An HttpContext associated with this request</param>
        public virtual void ProcessRequest(HttpContext context)
        {
            var baseContext = new HttpContextWrapper(context);

            ProcessRequest(baseContext);
        }


        /// <summary>
        /// Processes the tracking data and redirects request to original URL.
        /// </summary>
        /// <param name="context">An HttpContext associated with this request</param>
        protected internal virtual void ProcessRequest(HttpContextBase context)
        {
            // Log the click and try to get original URL
            string originalUrl = GetLinkUrl();
            int ping;

            if (!string.IsNullOrEmpty(originalUrl))
            {
                context.Response.StatusCode = (int)HttpStatusCode.SeeOther;
                context.Response.RedirectLocation = URLHelper.GetAbsoluteUrl(originalUrl);
            }
            else if ((ping = Ping) != 0)
            {
                ActivityTrackingHelper.RespondToPing(context, ping);
            }
            else
            {
                if (SystemContext.IsCMSRunningAsMainApplication)
                {
                    context.Server.Transfer(AdministrationUrlHelper.GetErrorPageUrl("Error.Header", "newsletter.linktrackingerror"));
                }
                else
                {
                    RequestHelper.Respond404();
                }
            }
        }


        /// <summary>
        /// Retrieves the original URL given the arguments and logs the request.
        /// </summary>
        /// <returns>Original URL or null if not found</returns>
        protected virtual string GetLinkUrl()
        {
            return GetLinkUrlInternal(Service.Resolve<IEmailHashValidator>());
        }


        /// <summary>
        /// Retrieves the original URL given the arguments and logs the request.
        /// </summary>
        /// <param name="emailHashValidator">Service for validating link hash</param>
        /// <returns>Original URL or null if not found</returns>
        internal string GetLinkUrlInternal(IEmailHashValidator emailHashValidator)
        {
            if (emailHashValidator == null)
            {
                throw new ArgumentNullException("emailHashValidator");
            }

            Guid linkGuid = LinkGuid;
            var email = Email;

            if (linkGuid != Guid.Empty)
            {
                // Load additional info
                LinkInfo link = LinkInfoProvider.GetLinkInfo(linkGuid);
                if (link == null)
                {
                    return null;
                }

                // Get issue
                IssueInfo issue = IssueInfoProvider.GetIssueInfo(link.LinkIssueID);
                if (issue == null)
                {
                    return null;
                }

                NewsletterInfo newsletter = NewsletterInfoProvider.GetNewsletterInfo(issue.IssueNewsletterID);
                if (newsletter == null)
                {
                    return null;
                }

                string hash = Hash;
                var validEmail = hash != null && emailHashValidator.ValidateEmailHash(hash, email);

                if (newsletter.NewsletterTrackOpenEmails && validEmail)
                {
                    lock (lockObj)
                    {
                        // Try to log opened email if it is not already logged
                        int readIssueID = OpenedEmailInfoProvider.LogOpenedEmail(email, issue.IssueID);

                        // Increase opened email counter in the issue and clear it from cache to avoid miscounts
                        IssueInfoProvider.AddOpenedEmails(readIssueID);
                    }
                }

                // Log the click and get original URL
                var originalUrl = link.LinkTarget;

                if (!string.IsNullOrEmpty(originalUrl))
                {
                    if (validEmail)
                    {
                        LinkInfoProvider.LogClick(link.LinkID, email);

                        var additionalParameters = new NameValueCollection(CMSHttpContext.Current.Request.QueryString);
                        additionalParameters.Add("originalURL", originalUrl);

                        NewsletterEvents.SubscriberClicksTrackedLink.StartEvent(new LinksEventArgs
                        {
                            IssueInfo = issue,
                            NewsletterInfo = newsletter,
                            AdditionalParameters = additionalParameters,
                        });
                    }

                    // Return original URL
                    return ResolveUrl(originalUrl, email, newsletter, issue);
                }

                // Log warning to the event log
                EventLogProvider.LogEvent(EventType.WARNING, "Newsletter", "LinkTracker", "Original link was not found!", RequestContext.CurrentURL);
            }

            return null;
        }


        /// <summary>
        /// Resolves subscriber macros in the specified URL.
        /// </summary>
        /// <param name="url">URL containing subscriber macros</param>
        /// <param name="email">Subscriber's email</param>
        /// <param name="newsletter">Text which is being resolved belongs to the issue in this newsletter</param>
        /// <param name="issue">Text which is being resolved belongs to this issue</param>
        /// <returns>Resolved URL</returns>
        private static string ResolveUrl(string url, string email, NewsletterInfo newsletter, IssueInfo issue)
        {
            var subscriber = SubscriberInfoProvider.GetSubscriberByEmail(email, issue.IssueSiteID);

            var resolver = new EmailContentMacroResolver(new EmailContentMacroResolverSettings
            {
                Subscriber = subscriber,
                Newsletter = newsletter,
                Issue = issue,
                Site = SiteContext.CurrentSiteName
            });

            return resolver.Resolve(url);
        }

        #endregion
    }
}