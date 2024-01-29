using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;

using CMS.Base;
using CMS.Helpers;
using CMS.Core;

namespace CMS.StrandsRecommender
{
    /// <summary>
    /// Client for communication with Strands Recommender web services. API ID is always needed to communicate with service.
    /// </summary>
    public class StrandsApiClient
    {
        #region "Constants"

        /// <summary>
        /// Strands API URL for obtaining all available web templates.
        /// </summary>
        private const string WEB_TEMPLATES_URL = "https://bizsolutions.strands.com/api2/templates/list.sbs?apid={0}";


        /// <summary>
        /// Strands API URL for automation of settings.
        /// </summary>
        private const string SETUP_CALL_URL = "https://recommender.strands.com/account/plugin/setup/";


        /// <summary>
        /// Status text sent by Strands Recommender when successful response.
        /// </summary>
        private const string SUCCESS_RESPONSE_STATUS = "SUCCESS";


        /// <summary>
        /// Key determining recommendation is of type Home.
        /// </summary>
        private const string TEMPLATE_TYPE_HOME = "HOME";


        /// <summary>
        /// Key determining recommendation is of type Order confirmation.
        /// </summary>
        private const string TEMPLATE_TYPE_CONFIRMATION = "CHECKOUT";


        /// <summary>
        /// Key determining recommendation is of type Shopping cart/wishlist.
        /// </summary>
        private const string TEMPLATE_TYPE_CART = "BASKET";


        /// <summary>
        /// Key determining recommendation is of type Category.
        /// </summary>
        private const string TEMPLATE_TYPE_CATEGORY = "CATEGORY";


        /// <summary>
        /// Key determining recommendation is of type Product.
        /// </summary>
        private const string TEMPLATE_TYPE_PRODUCT = "PRODUCT";


        /// <summary>
        /// Key determining recommendation is of type Miscellaneous.
        /// </summary>
        private const string TEMPLATE_TYPE_MISCELLANEOUS = "UNDEFINED";


        /// <summary>
        /// Maximal number of attempts to get XML document from Strands API if HTTP status 404 is returned.
        /// </summary>
        private const int MAX_NUMBER_OF_ATTEMPTS = 2;


        /// <summary>
        /// Defines the time span application will be waiting before making next attempt to load templates from Strands API.
        /// </summary>
        private readonly TimeSpan DELAY_BEFORE_NEXT_ATTEMPT = TimeSpan.FromSeconds(1);

        #endregion


        #region "Private variables"

        /// <summary>
        /// Strands API ID.
        /// </summary>
        private readonly string mApiID;

        #endregion


        #region "Public methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="apiID">Strands API ID</param>
        /// <exception cref="ArgumentNullException"><paramref name="apiID"/> is null</exception>
        public StrandsApiClient(string apiID)
        {
            if (apiID == null)
            {
                throw new ArgumentNullException("apiID");
            }

            mApiID = apiID;
        }


        /// <summary>
        /// Gets all available web templates for given API ID from Strands API.
        /// </summary>
        /// <returns>Collection containing all available web templates</returns>
        public IEnumerable<StrandsWebTemplateData> GetAvailableWebTemplates()
        {
            // Constructing API URL for specified API ID
            string url = string.Format(WEB_TEMPLATES_URL, mApiID);

            // Loads XML document from Strands API
            var doc = DownloadXDocument(url);

            // Loading template details in object
            return from template in doc.Descendants("template")
                   let type = ResolveRecommendationType(GetElementValue(template, "placement"))
                   where type != StrandsWebTemplateTypeEnum.Miscellaneous
                   select new StrandsWebTemplateData
                   {
                       ID = GetElementValue(template, "name"),
                       Title = GetElementValue(template, "title"),
                       Type = type,
                   };
        }


        /// <summary>
        /// The call sets up fields in Strands Recommender settings. Eg. number of products per page, frequency of catalog update, 
        /// URL from which should Strands download the catalog, mapping between catalog feed elements and Strands fields etc.
        /// Most of the values are predefined and cannot be changed by user. Only exceptions are enabling/disabling of automatic 
        /// download of catalog from Strands and frequency of this download.
        /// </summary>
        /// <param name="data">Setup data</param>
        /// <returns>Task with result of asynchronous call</returns>
        public Task<StrandsSetupCallStatusCodeEnum> DoSetupCallAsync(StrandsSetupCallData data)
        {
            string url = URLHelper.AppendQuery(SETUP_CALL_URL, BuildSetupCallQuery(data));

            var request = (HttpWebRequest)WebRequest.Create(url);
            var task = Task.Factory.FromAsync(
               request.BeginGetResponse,
               asyncResult => request.EndGetResponse(asyncResult),
               null
            );

            return task.ContinueWith(CMSThread.WrapFunc<Task<WebResponse>, StrandsSetupCallStatusCodeEnum>(GetResponseStatusCode));
        }


        /// <summary>
        /// Gets the setup call response status code
        /// </summary>
        /// <param name="task">Setup task</param>
        private static StrandsSetupCallStatusCodeEnum GetResponseStatusCode(Task<WebResponse> task)
        {
            using (var responseStream = task.Result.GetResponseStream())
            {
                if (responseStream != null)
                {
#pragma warning disable BH1014 // Do not use System.IO
                    using (var sr = new StreamReader(responseStream))
                    {
                        string result = sr.ReadToEnd();
                        XDocument doc = XDocument.Parse(result);

                        return GetSetupCallStatusCode(doc);
                    }
#pragma warning restore BH1014 // Do not use System.IO
                }
            }

            return StrandsSetupCallStatusCodeEnum.UnknownStatus;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets HTTP status code from given WebException.
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <returns>HTTP status code or 0 if not available</returns>
        private static int GetExceptionStatusCode(WebException exception)
        {
            if (exception.Status == WebExceptionStatus.ProtocolError)
            {
                var response = exception.Response as HttpWebResponse;
                if (response != null)
                {
                    // There is HTTP status code available
                    return (int)response.StatusCode;
                }
            }
            // Exception has different reason, return default value
            return 0;
        }


        /// <summary>
        /// Checks, whether response from Strands API when using template call is valid (Strands is returning status code 0).
        /// </summary>
        /// <param name="document">Document to be checked</param>
        /// <exception cref="StrandsException">Response from Strands API is not successful</exception>
        private static void CheckForTemplateCallSuccessfulResponse(XDocument document)
        {
            var status = GetTemplateCallStatusCode(document);
            if (status == StrandsTemplateCallStatusCodeEnum.Success)
            {
                return;
            }

            string localizedMessage = null;

            // Localized message is used only in some cases, when this method is called within AJAX call. Therefore user excepts some response, even when error occurs.
            // In other cases non-localized message is sufficient, since they are displayed only in the event log.
            if (status == StrandsTemplateCallStatusCodeEnum.ErrorValidatingCustomersApiID)
            {
                localizedMessage = ResHelper.GetString("strands.invalidapiid");
            }
            else if (status == StrandsTemplateCallStatusCodeEnum.EmailRecommendationsUnavailable)
            {
                localizedMessage = ResHelper.GetString("strands.emailrecommendationsunavailable");
            }

            throw new StrandsException("[StrandsApiClient.CheckForTemplateCallSuccessfulResponse]: " + status.ToStringRepresentation(), localizedMessage);
        }


        /// <summary>
        /// Gets status response from XML document when using template call.
        /// </summary>
        /// <param name="document">XML document obtained from Strands Recommender</param>
        /// <returns>Status of given document</returns>
        private static StrandsTemplateCallStatusCodeEnum GetTemplateCallStatusCode(XDocument document)
        {
            // In most cases there should be code element present in XML document
            var root = document.Root;
            if (root != null)
            {
                var codeXmlElement = root.XPathSelectElement("//code");
                if (codeXmlElement != null)
                {
                    return (StrandsTemplateCallStatusCodeEnum)ValidationHelper.GetInteger(codeXmlElement.Value, -1);
                }
            }

            throw new StrandsException("[StrandsApiClient.GetStatusCode]: Unable to get status code from XML document loaded from Strands Recommender.");
        }


        /// <summary>
        /// Gets status response from XML document when using setup call.
        /// </summary>
        /// <param name="document">XML document obtained from Strands Recommender</param>
        /// <returns>Status of given document</returns>
        private static StrandsSetupCallStatusCodeEnum GetSetupCallStatusCode(XDocument document)
        {
            // When request is successful, no status code is sent, available is only the text message 'SUCCESS'. First check for this message and then, if message is not present, try to get status code (should be present in this case)
            var root = document.Root;
            if (root != null)
            {
                var statusXmlElement = root.XPathSelectElement("//status");
                if (statusXmlElement != null)
                {
                    if (statusXmlElement.Value == SUCCESS_RESPONSE_STATUS)
                    {
                        return StrandsSetupCallStatusCodeEnum.Success;
                    }

                    var codeXmlElement = root.XPathSelectElement("//code");
                    if (codeXmlElement != null)
                    {
                        return (StrandsSetupCallStatusCodeEnum)ValidationHelper.GetInteger(codeXmlElement.Value, -1);
                    }
                }
            }

            throw new StrandsException("[StrandsApiClient.GetStatusCode]: Unable to get status code from XML document loaded from Strands Recommender.");
        }


        /// <summary>
        /// Gets XML document loading it from given API URL.
        /// </summary>
        /// <param name="url">API URL of XML document</param>
        /// <returns>XDocument representing XML document located on given API URL</returns>
        /// <exception cref="StrandsException">API request fails (i.e. due to limited connectivity) or Strands returns code indicating error (i.e. invalid API ID)</exception>
        private XDocument DownloadXDocument(string url)
        {
            // Loads content from URL
            string xml = string.Empty;

            using (WebClient client = new WebClient())
            {
                for (int i = 0; i < MAX_NUMBER_OF_ATTEMPTS; i++)
                {
                    // Sometimes Strands gives 404 error when accessing Strands API, several attempts are made
                    try
                    {
                        xml = client.DownloadString(url);
                    }
                    catch (WebException ex)
                    {
                        if (GetExceptionStatusCode(ex) != 404)
                        {
                            // "Unknown" error, pass further
                            throw new StrandsException("[StrandsApiClient.GetXDocument]: Unknown error while loading document from Strands API: " + ex.Message, CoreServices.Localization.GetString("strands.exception"), ex);
                        }
                        if (i != (MAX_NUMBER_OF_ATTEMPTS - 1)) // In this case waiting is useless since no more attempt will be made
                        {
                            // Try it again after short delay
                            Thread.Sleep(DELAY_BEFORE_NEXT_ATTEMPT);
                        }
                    }
                }
            }
            // If XML is still empty, Strands API is unavailable thus XML document could not be loaded
            if (xml == string.Empty)
            {
                throw new StrandsException("[StrandsApiClient.GetXDocument]: Unable to load document from Strands API (Strands returns 404)");
            }

            XDocument doc = XDocument.Parse(xml);

            // Checks, whether XML document loaded successfully. Throws exception if not.
            CheckForTemplateCallSuccessfulResponse(doc);

            return doc;
        }


        /// <summary>
        /// Gets value of first element in container with tag name specified by <paramref name="element"/>.
        /// </summary>
        /// <param name="container">Container of element</param>
        /// <param name="element">Desired element</param>
        /// <returns>Value of given element</returns>
        /// <exception cref="StrandsException">XML document does not contain expected element</exception>
        private static string GetElementValue(XElement container, string element)
        {
            var node = container.Element(element);

            // Node should not be null, if it exists in XML document
            if (node == null)
            {
                throw new StrandsException("[StrandsApiClient.GetElementValue]: Parsed XML document is in bad format");
            }

            return node.Value;
        }


        /// <summary>
        /// Determines type of recommendation template based on placement attribute sent by Strands. 
        /// </summary>
        /// <param name="tpl">Template ID</param>
        private static StrandsWebTemplateTypeEnum ResolveRecommendationType(string tpl)
        {
            switch (tpl)
            {
                case TEMPLATE_TYPE_HOME:
                    return StrandsWebTemplateTypeEnum.Home;
                case TEMPLATE_TYPE_PRODUCT:
                    return StrandsWebTemplateTypeEnum.Product;
                case TEMPLATE_TYPE_CART:
                    return StrandsWebTemplateTypeEnum.Cart;
                case TEMPLATE_TYPE_CONFIRMATION:
                    return StrandsWebTemplateTypeEnum.Order;
                case TEMPLATE_TYPE_CATEGORY:
                    return StrandsWebTemplateTypeEnum.Category;
                case TEMPLATE_TYPE_MISCELLANEOUS:
                    return StrandsWebTemplateTypeEnum.Miscellaneous;
                default:
                    return StrandsWebTemplateTypeEnum.Home;
            }
        }


        /// <summary>
        /// Builds query for Strands setup call from given settings.
        /// </summary>
        /// <param name="data">Setup settings</param>
        /// <returns>Query containing every single setting as parameter</returns>
        private string BuildSetupCallQuery(StrandsSetupCallData data)
        {
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);

            queryString["apid"] = mApiID;
            queryString["token"] = data.ValidationToken;

            // Strands requires the type 'kentico' to be sent
            queryString["type"] = data.Type;

            // This field is useless as well, however required by Strands. CMS version is sent.
            queryString["version"] = data.Version;

            // Mapping of the fields (id, image_link, etc.)
            foreach (var field in data.Fields)
            {
                queryString[field.Key] = field.Value;
            }

            // Url of catalog feed page
            queryString["feed"] = data.FeedUrl;

            // Url of catalog feed page
            queryString["feedactive"] = data.FeedActive.ToString(CultureInfo.InvariantCulture).ToLowerInvariant();

            // Number of products per page
            queryString["feedpagination"] = data.Pagination.ToString(CultureInfo.InvariantCulture);

            // Frequency with which catalog feed is uploaded to Strands Recommender. Has to be in special format (see FeedFrequency field for details)
            queryString["feedfrequency"] = data.FeedFrequency;

            queryString["feeduser"] = data.CatalogFeedUsername;
            queryString["feedpassword"] = data.CatalogFeedPassword;

            // URLHelper.AddParameterToUrl method first ensures there are no parameters with the same key as the given one. If any is present, it is removed.
            // Since Strands requires multiple values for one key, URLHelper cannot be used.
            string query = queryString + string.Concat(data.Tracking.Select(t => string.Format("&tracking={0}", t)));

            return query;
        }

        #endregion
    }
}
