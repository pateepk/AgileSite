using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Security;
using System.Web;
using System.Web.Security;

using CMS.Core;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.Search
{
    /// <summary>
    /// Search HTML crawler for indexing content from the output of the web site
    /// </summary>
    public class SearchCrawler : ISearchCrawler
    {
        #region "Variables"

        private static readonly ConcurrentDictionary<string, CookieContainer> credentials = new ConcurrentDictionary<string, CookieContainer>(StringComparer.InvariantCultureIgnoreCase);
        private static ICredentials icredentials;
        private static bool? mCrawlerAllowSiteAliasRedirect;
        private static string mCrawlerUserAgent = "Kentico CMS Smart Search Crawler";
        private static string mCrawlerID;

        private string mCrawlerDomain = String.Empty;
        private string mCrawlerUserName;
        private string mCrawlerPassword;
        private bool? mAcceptInvalidServerCertificate;

        #endregion


        #region "Events"

        /// <summary>
        /// Html to plain text version delegate.
        /// </summary>
        /// <param name="plainText">Plain text version</param>
        /// <param name="originalHtml">Original HTML version</param>
        public delegate string HtmlToPlainTextHandler(string plainText, string originalHtml);


        /// <summary>
        /// Fires when html to plain text process is required.
        /// </summary>
        public static event HtmlToPlainTextHandler OnHtmlToPlainText;

        #endregion


        #region "Properties"


        /// <summary>
        /// Gets the domain specified in app keys which should be used for crawler indexer.
        /// </summary>
        public static string CrawlerDomainName
        {
            get
            {
                return ValidationHelper.GetString(SettingsHelper.AppSettings["CMSCrawlerSearchDomain"], String.Empty);
            }
        }


        /// <summary>
        /// Gets or sets the user agent which should be used within crawler search.
        /// </summary>
        public static string CrawlerUserAgent
        {
            get
            {
                return mCrawlerUserAgent;
            }
            set
            {
                mCrawlerUserAgent = value;
            }
        }


        /// <summary>
        /// Determines whether to allow redirection within site aliases when crawler gets redirection HTTP response from main alias.
        /// </summary>
        private static bool CrawlerAllowSiteAliasRedirect
        {
            get
            {
                if (mCrawlerAllowSiteAliasRedirect == null)
                {
                    mCrawlerAllowSiteAliasRedirect = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSCrawlerAllowSiteAliasRedirect"], true);
                }
                return mCrawlerAllowSiteAliasRedirect.Value;
            }
        }


        /// <summary>
        /// Gets the user name which should be used for crawler search.
        /// </summary>
        public string CrawlerFormsUserName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the crawler user name which should be used for current search.
        /// </summary>
        public string CrawlerUserName
        {
            get
            {
                if (mCrawlerUserName == null)
                {
                    // Get username from application settings
                    string username = DataHelper.GetNotEmpty(SettingsHelper.AppSettings["CMSCrawlerDomainUserName"], String.Empty);

                    username = ExtractUserDomain(username);

                    // Save username into the global variable
                    mCrawlerUserName = username;
                }
                return mCrawlerUserName;
            }
            set
            {
                mCrawlerUserName = ExtractUserDomain(value);
            }
        }


        /// <summary>
        /// Gets the crawler user password.
        /// </summary>
        public string CrawlerPassword
        {
            get
            {
                if (mCrawlerPassword == null)
                {
                    mCrawlerPassword = DataHelper.GetNotEmpty(SettingsHelper.AppSettings["CMSCrawlerDomainPassword"], String.Empty);
                }
                return mCrawlerPassword;
            }
            set
            {
                mCrawlerPassword = value;
            }
        }


        /// <summary>
        ///  Gets or sets the crawler id which is sent within the request.
        /// </summary>
        public static string CrawlerID
        {
            get
            {
                if (mCrawlerID == null)
                {
                    mCrawlerID = Guid.NewGuid().ToString();
                }
                return mCrawlerID;
            }
            set
            {
                mCrawlerID = value;
            }
        }


        /// <summary>
        /// Gets the crawler user domain.
        /// </summary>
        private string CrawlerDomain
        {
            get
            {
                if (mCrawlerDomain == null)
                {
                    mCrawlerDomain = DataHelper.GetNotEmpty(SettingsHelper.AppSettings["CMSCrawlerSearchDomain"], String.Empty);
                }
                return mCrawlerDomain;
            }
        }


        /// <summary>
        /// Indicates whether crawler is allowed to browse web site secured via TLS/SSL even though the web site server doesn't provide a valid certificate. For example, self-signed or expired certificate is the case. 
        /// Default is false because allowing it implicitly may represent security vulnerability.
        /// </summary>
        private bool AcceptInvalidServerCertificate
        {
            get
            {
                if (mAcceptInvalidServerCertificate == null)
                {
                    mAcceptInvalidServerCertificate = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSSearchCrawlerAcceptAllCertificates"], false) || ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSAcceptAllCertificates"], false);
                }

                return mAcceptInvalidServerCertificate.Value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Extracts the user domain from given username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private string ExtractUserDomain(string username)
        {
            // Check whether username contains domain
            int slashIndex = username.IndexOf("\\", StringComparison.Ordinal);
            if (slashIndex > 0)
            {
                // Set domain name
                mCrawlerDomain = username.Substring(0, slashIndex);
                // Set username
                username = username.Substring(slashIndex + 1);
            }
            return username;
        }


        /// <summary>
        /// Converts html to the plain text (body part).
        /// </summary>
        /// <param name="htmlCode">HTML code</param>
        public static string HtmlToPlainText(string htmlCode)
        {
            // Keep original html code
            string originalCode = htmlCode;

            // Remove new lines
            htmlCode = HTMLHelper.HtmlToPlainText(htmlCode);

            // Handle conversion to plain text if is required
            if (OnHtmlToPlainText != null)
            {
                htmlCode = OnHtmlToPlainText(htmlCode, originalCode);
            }

            // Return plain html code
            return htmlCode;
        }


        /// <summary>
        /// Returns authentication cookie for specific user and url (domain) which is used for crawling.
        /// </summary>
        /// <param name="user">Username</param>
        /// <param name="url">Full url to get domain</param>
        private static CookieContainer GetCrawlerCookieContainer(string user, string url)
        {
            // Try get domain from URL
            string domain = URLHelper.GetDomain(url);

            string key = domain + ";" + user;
            CookieContainer cont = null;

            // Try get cached credentials and if not found create a new one
            if (!credentials.TryGetValue(key, out cont))
            {
                // Create cookie container
                cont = new CookieContainer();

                // Public user should not be authenticated
                IUserInfo userInfo = Service.Resolve<IAuthenticationService>().GetUser(user);
                if ((userInfo != null) && (!userInfo.IsPublic()))
                {
                    // Get authentication cookie
                    HttpCookie authCookie = FormsAuthentication.GetAuthCookie(user, true);

                    // Remove port as cookies are not port-specific
                    domain = URLHelper.RemovePort(domain);

                    // Create cookie object and add it to the container
                    Cookie cookie = new Cookie(authCookie.Name, authCookie.Value, authCookie.Path, domain);
                    cont.Add(cookie);
                }

                // Cache the container
                credentials[key] = cont;
            }

            return cont;
        }


        /// <summary>
        /// Returns network credential for crawler.
        /// </summary>
        private ICredentials GetCrawlerCredentials()
        {
            // Check whether credentials are cached
            if (icredentials == null)
            {
                // Check whether username is defined
                if (!String.IsNullOrEmpty(CrawlerUserName))
                {
                    // Create credential object
                    NetworkCredential nc = new NetworkCredential();
                    nc.UserName = CrawlerUserName;
                    nc.Password = CrawlerPassword;
                    nc.Domain = CrawlerDomain;
                    icredentials = nc;
                }
                // Use default credentials if username is not defined
                else
                {
                    icredentials = CredentialCache.DefaultCredentials;
                }
            }
            return icredentials;
        }


        /// <summary>
        /// Creates HttpWebRequest for specific URL.
        /// </summary>
        /// <param name="url">Target URL</param>
        private HttpWebRequest CreateRequest(string url)
        {
            // Create web request
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);

            if (!string.IsNullOrEmpty(CrawlerFormsUserName))
            {
                // Set cookie authentication for forms authentication
                wr.CookieContainer = GetCrawlerCookieContainer(CrawlerFormsUserName, url);
            }

            // Set crawler credentials
            wr.Credentials = GetCrawlerCredentials();
            // Set crawler agent
            wr.UserAgent = CrawlerUserAgent;
            // Disable auto redirect  
            wr.AllowAutoRedirect = false;

            ConfigureHttpsRequest(wr);

            return wr;
        }


        /// <summary>
        /// Downloads the HTML code for the specified URL.
        /// </summary>
        /// <param name="url">Page URL</param>
        public string DownloadHtmlContent(string url)
        {
            // Add crawler identifier to the request
            url = AddCrawlerIdToUrl(url);

            // Keep original domain
            string originalDomain = URLHelper.GetDomain(url);

            // Create web request
            HttpWebRequest wr = CreateRequest(url);

            // Check whether Web request is defined
            if (wr != null)
            {
                // Indicates whether request was processed correctly
                bool isComplete = false;
                int counter = 0;

                // Response object
                HttpWebResponse wres = null;

                // Try get response till page is ok or max request value reached
                while (!isComplete && (counter < 10))
                {
                    // Increment counter
                    counter++;

                    // Try get response
                    wres = wr.GetResponse() as HttpWebResponse;
                    // Check redirect response
                    if ((wres.StatusCode == HttpStatusCode.Moved) ||
                        (wres.StatusCode == HttpStatusCode.MovedPermanently) ||
                        (wres.StatusCode == HttpStatusCode.Redirect))
                    {
                        // Try get location redirect
                        string location = wres.Headers["location"];
                        if (!String.IsNullOrEmpty(location))
                        {
                            // Get absolute url
                            location = URLHelper.GetAbsoluteUrl(location, originalDomain, URLHelper.GetFullApplicationUrl(), URLHelper.GetAbsoluteUrl(RequestContext.CurrentURL));

                            // Check whether domain is correct
                            string locationDomain = URLHelper.GetDomain(location);
                            if (!String.IsNullOrEmpty(locationDomain))
                            {
                                bool domainAllowed = locationDomain.Equals(originalDomain, StringComparison.InvariantCultureIgnoreCase);
                                if (!domainAllowed && CrawlerAllowSiteAliasRedirect)
                                {
                                    // Check if the domain changed only within the domain aliases of the same site
                                    SiteInfo si = SiteInfoProvider.GetRunningSiteInfo(originalDomain, null);
                                    if (si != null)
                                    {
                                        if (si.SiteDomainAliases[locationDomain] != null)
                                        {
                                            domainAllowed = true;
                                        }
                                    }
                                }

                                // Domain cannot be different than original domain
                                if (domainAllowed)
                                {
                                    // Add crawler identifier to the request
                                    location = AddCrawlerIdToUrl(location);

                                    // Create new request
                                    wr = CreateRequest(location);
                                }
                            }
                        }
                    }
                    else
                    {
                        isComplete = true;
                    }
                }

                // If page is ok => get response
                if (isComplete)
                {
                    // Read response
                    using (StreamReader sr = StreamReader.New(wres.GetResponseStream()))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            return String.Empty;
        }


        internal static string GetCrawlerHashString()
        {
            return ValidationHelper.GetHashString(CrawlerID, new HashSettings());
        }


        /// <summary>
        /// Adds "crawlerid" parameter to given URL along with the signature of such extended URL.
        /// </summary>
        /// <param name="url">URL to add crawlerid to</param>
        private static string AddCrawlerIdToUrl(string url)
        {
            url = URLHelper.UpdateParameterInUrl(url, "crawlerid", CrawlerID);
            url = URLHelper.RemoveParameterFromUrl(url, "cidhash");
            url = URLHelper.AddParameterToUrl(url, "cidhash", GetCrawlerHashString());
            return url;
        }


        /// <summary>
        /// Returns true if current request was initialized by smart search crawler.
        /// </summary>
        public static bool IsCrawlerRequest()
        {
            if (SearchContext.IsCrawler != null)
            {
                return SearchContext.IsCrawler.Value;
            }

            bool isCrawler = false;

            // Try get crawler id from querystring
            string query = QueryHelper.GetString("crawlerid", String.Empty);

            // Check whether query string value is defined
            if (!String.IsNullOrEmpty(query))
            {
                var hash = QueryHelper.GetString("cidhash", String.Empty);

                // Validate crawler id signature
                if (ValidationHelper.ValidateHash(query, hash, new HashSettings()) && query.Equals(CrawlerID, StringComparison.OrdinalIgnoreCase))
                {
                    isCrawler = true;
                }
            }

            // Request is not crawler by default
            SearchContext.IsCrawler = isCrawler;
            return isCrawler;
        }


        private void ConfigureHttpsRequest(HttpWebRequest request)
        {
            // By default, crawler's web client accepts only valid server certificate. This behavior may be suppressed by application settings. In order to reflect these settings there needs to add custom validation callback for request.
            if (request != null)
            {
                request.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                {
                    return AcceptInvalidServerCertificate ? true : (sslPolicyErrors == SslPolicyErrors.None);
                };
            }
        }

        #endregion
    }
}