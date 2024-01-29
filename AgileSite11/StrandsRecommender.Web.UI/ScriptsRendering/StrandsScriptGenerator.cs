using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

using CMS.Base;
using CMS.Base.Web.UI;


namespace CMS.StrandsRecommender.Web.UI
{
    /// <summary>
    /// This class is able to generate JavaScript snippets which are required to be rendered on page in order to enable integration with Strands Recommender.
    /// </summary>
    public class StrandsScriptGenerator
    {
        #region "Public methods"

        /// <summary>
        /// Returns script which logs "visited" event to the Strands Recommender.
        /// </summary>
        /// <param name="itemID">ID of the catalog item which is being visited. Must correspond to the IDs send through catalog feed</param>
        /// <returns>JavaScript code which logs "visited" event</returns>
        public virtual string GetVisitedTrackingScript(string itemID)
        {
            return GetStrandsPushScript(new
            {
                item = itemID,
                @event = "visited",
            });
        }


        /// <summary>
        /// Returns script which logs "searched" event to the Strands Recommender.
        /// </summary>
        /// <param name="searchText">Text which was searched</param>
        /// <param name="itemID">ID of the catalog item which is being visited. Must correspond to the IDs send through catalog feed</param>
        /// <returns>Generated JavaScript</returns>
        public virtual string GetSearchedTrackingScript(string searchText, string itemID)
        {
            return GetStrandsPushScript(new
            {
                item = itemID,
                searchstring = searchText,
                @event = "searched",
            });
        }


        /// <summary>
        /// Returns script which logs "userlogged" event to the Strands Recommender.
        /// This script indicates which user is browsing the web. This event needs to be logged at every page user visits (see http://recommender.strands.com/developers/javascript/#ulogin"/). 
        /// Strands can later associate this user ID with the ID of the newsletter recipient and show appropriate recommendations.
        /// </summary>
        /// <param name="userID">ID of the user (needs to be the same as ID which is placed at the newsletter email with recommendation)</param>
        /// <returns>Generated JavaScript</returns>
        public virtual string GetUserLoggedTrackingScript(string userID)
        {
            return GetStrandsPushScript(new
            {
                user = userID,
                @event = "userlogged",
            });
        }


        /// <summary>
        /// Returns script which logs "updateschoppingcart" event to the Strands Recommender.
        /// </summary>
        /// <param name="itemsInCart">IDs of all items in shopping cart</param>
        /// <exception cref="ArgumentNullException"><paramref name="itemsInCart"/> is null</exception>
        /// <returns>Generated JavaScript</returns>
        public virtual string GetUpdateShoppingCartTrackingScript(IEnumerable<string> itemsInCart)
        {
            if (itemsInCart == null)
            {
                throw new ArgumentNullException("itemsInCart");
            }

            return GetStrandsPushScript(new
            {
                items = itemsInCart,
                @event = "updateshoppingcart",
            });
        }


        /// <summary>
        /// Returns script which logs "purchased" event to the Strands Recommender.
        /// </summary>
        /// <param name="orderID">ID of the order</param>
        /// <param name="orderData">Information about products which were purchased</param>
        /// <exception cref="ArgumentNullException"><paramref name="orderData"/> is null</exception>
        /// <returns>Generated JavaScript</returns>
        public virtual string GetPurchasedTrackingScript(string orderID, IEnumerable<StrandsPurchasedEventData> orderData)
        {
            if (orderData == null)
            {
                throw new ArgumentNullException("orderData");
            }

            // Order data need to be transformed to the format expected by Strands library
            var orderDataTransformed = orderData.Select(x => new
            {
                id = x.ItemID,
                price = x.Price,
                quantity = x.Quantity
            });

            return GetStrandsPushScript(new
            {
                orderid = orderID,
                items = orderDataTransformed,
                @event = "purchased",
            });
        }


        /// <summary>
        /// Returns script which includes Strands library file. Returned string contains html script tags.
        /// </summary>
        /// <returns>Generated JavaScript</returns>
        public virtual string GetLibraryIncludeScript()
        {
            // Using '//' as a protocol ensures that the browser loads the script using the protocol used to load the page itself. This is required due to the 'mixed content' security feature of modern browsers
            // which dictates that on a page loaded via HTTPS, all resources must be references via https as well.
            return ScriptHelper.GetScriptTag("//bizsolutions.strands.com/sbsstatic/js/sbsLib-1.0.min.js", false);
        }


        /// <summary>
        /// Returns script which starts execution of the Strands library. Must be included after Strands library.
        /// </summary>
        /// <param name="apiID">API ID of the connected Strands account</param>
        /// <returns>Generated JavaScript</returns>
        public virtual string GetInitializationScript(string apiID)
        {
            return @"var oldOnLoad = window.onload;
window.onload = function(evt) {
	try {
		SBS.Worker.go(""" + apiID + @""");
	} finally {
		if (typeof oldOnLoad == 'function') {
            oldOnLoad(evt);
        }
	}
};";
        }

        
        /// <summary>
        /// Returns code which registers custom rendering function and declaration of the rendering function. jQuery templates library has to be included in order for this code to work.
        /// </summary>
        /// <param name="jQueryTransformation">Code of the transformation which will be used to render recommendation</param>
        /// <param name="templateID">ID of the Strands template which will be rendered using custom transformation (i.e. HOME-1)</param>
        /// <param name="placementID">Client ID of the element where transformation will be rendered</param>
        /// <exception cref="ArgumentNullException"><paramref name="jQueryTransformation"/>, <paramref name="templateID"/> or <paramref name="placementID"/> is null</exception>
        /// <returns>Generated JavaScript</returns>
        public virtual string GetCustomizedRendererScript(string jQueryTransformation, string templateID, string placementID)
        {
            if (jQueryTransformation == null)
            {
                throw new ArgumentNullException("jQueryTransformation");
            }
            if (templateID == null)
            {
                throw new ArgumentNullException("templateID");
            }
            if (placementID == null)
            {
                throw new ArgumentNullException("placementID");
            }

            // There cannot be dashes in js function name
            var customRenderingFunctionName = string.Format("strandsCustomRenderer_{0}", templateID.Replace('-', '_'));

            StringBuilder scriptOutput = new StringBuilder();

            // Renders definition of custom renderer
            scriptOutput.AppendFormat("function {0}(recInfo) {{$cmsj.tmpl({1}, recInfo).appendTo(\"#{2}\");}} ", customRenderingFunctionName, ScriptHelper.GetString(jQueryTransformation), placementID);

            // Register custom renderer to strands
            scriptOutput.AppendFormat("try{{ SBS.Recs.setRenderer({0},{1}); }} catch (e){{}}", customRenderingFunctionName, ScriptHelper.GetString(templateID));

            return scriptOutput.ToString();
        }


        /// <summary>
        /// Returns script which sets current culture, so correct item captions can be shown.
        /// </summary>
        /// <param name="cultureCode">Culture code, e.g. en-US</param>
        /// <returns>Generated JavaScript</returns>
        /// <exception cref="ArgumentNullException"><paramref name="cultureCode"/> is null</exception>
        public virtual string GetCultureSettingsScript(string cultureCode)
        {
            if (cultureCode == null)
            {
                throw new ArgumentNullException("cultureCode");
            }

            return GetStrandsPushScript(new
            {
                setting = "pview",
                value = cultureCode.Replace("-", string.Empty).ToLowerCSafe(),
            });
        }


        /// <summary>
        /// Returns script which sets current currency, so correct prices can be shown.
        /// </summary>
        /// <param name="currencyCode">Current currency, e.g. EUR</param>
        /// <returns>Generated JavaScript</returns>
        public virtual string GetCurrencySettingsScript(string currencyCode)
        {
            return GetStrandsPushScript(new
            {
                setting = "cview",
                value = currencyCode.ToLowerCSafe(),
            });
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns script which pushes data into StrandsTrack JavaScript array. This array is used
        /// to communicate with Strands library.
        /// </summary>
        /// <param name="parameters">Parameters which will be pushed into array. This will be serialized to JavaScript</param>
        /// <returns>Push script</returns>
        private string GetStrandsPushScript(object parameters)
        {
            var serializer = new JavaScriptSerializer();

            string eventParametersJson = serializer.Serialize(parameters);

            string trackingScript = string.Format(
                @"if (typeof StrandsTrack==""undefined"") {{StrandsTrack=[];}}
StrandsTrack.push({0});", eventParametersJson);

            return trackingScript;
        }

        #endregion
    }
}
