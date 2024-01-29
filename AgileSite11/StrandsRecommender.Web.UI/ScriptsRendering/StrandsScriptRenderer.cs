using System;
using System.Collections.Generic;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.MacroEngine;
using CMS.PortalEngine;

namespace CMS.StrandsRecommender.Web.UI
{
    /// <summary>
    /// Class capable of rendering various JavaScripts needed by Strands recommendation engine.
    /// </summary>
    public class StrandsScriptRenderer
    {
        #region "Private fields"

        private readonly Page mPage;
        private readonly string mApiID;
        private readonly StrandsScriptGenerator mStrandsScriptGenerator;

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructs class with default Scripts Generator.
        /// </summary>
        /// <param name="page">Scripts will be rendered on this page</param>
        /// <param name="apiID">API ID of the associated Strands account</param>
        public StrandsScriptRenderer(Page page, string apiID) : this(page, apiID, new StrandsScriptGenerator())
        {
        }


        /// <summary>
        /// Constructs class.
        /// </summary>
        /// <param name="page">Scripts will be rendered on this page</param>
        /// <param name="apiID">API ID of the associated Strands account</param>
        /// <param name="strandsScriptGenerator">This instance will be used to generate scripts rendered to the page</param>
        /// <exception cref="ArgumentNullException"><paramref name="page"/>, <paramref name="apiID"/> or <paramref name="strandsScriptGenerator"/> is null</exception>
        public StrandsScriptRenderer(Page page, string apiID, StrandsScriptGenerator strandsScriptGenerator)
        {
            if (page == null)
            {
                throw new ArgumentNullException("page");
            }
            if (apiID == null)
            {
                throw new ArgumentNullException("apiID");
            }
            if (strandsScriptGenerator == null)
            {
                throw new ArgumentNullException("strandsScriptGenerator");
            }

            mPage = page;
            mApiID = apiID;
            mStrandsScriptGenerator = strandsScriptGenerator;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Renders script for the item visited event tracking.
        /// </summary>
        /// <param name="itemID">ID of the catalog item which is being visited. Must correspond to the IDs send through catalog feed</param>
        public void RenderItemVisitedTrackingScript(string itemID)
        {
            string script = mStrandsScriptGenerator.GetVisitedTrackingScript(itemID);

            RenderSettingsScript(script);
        }


        /// <summary>
        /// Renders script for the item searched event tracking.
        /// </summary>
        /// <param name="searchText">Search text</param>
        /// <param name="itemID">ID of the catalog item which is being visited. Must correspond to the IDs send through catalog feed</param>
        public void RenderSearchedTrackingScript(string searchText, string itemID)
        {
            string script = mStrandsScriptGenerator.GetSearchedTrackingScript(searchText, itemID);

            RenderSettingsScript(script);
        }


        /// <summary>
        /// Renders event indicating which user is browsing the web. This event needs to be logged at every page user visits (see http://recommender.strands.com/developers/javascript/#ulogin"/). 
        /// Strands can later associate this user ID with the ID of the newsletter recipient and show appropriate recommendations.        
        /// </summary>
        /// <param name="userID">ID of the user (needs to be the same as ID which is placed at the newsletter email with recommendation)</param>
        public void RenderUserLoggedTrackingScript(string userID)
        {
            string script = mStrandsScriptGenerator.GetUserLoggedTrackingScript(userID);

            RenderSettingsScript(script);
        }


        /// <summary>
        /// Renders script for updateschoppingcart tracking event.
        /// </summary>
        /// <param name="itemsInCart">IDs of all items in shopping cart</param>
        /// <exception cref="ArgumentNullException"><paramref name="itemsInCart"/> is null</exception>
        public void RenderUpdateShoppingCartTrackingScript(IEnumerable<string> itemsInCart)
        {
            if (itemsInCart == null)
            {
                throw new ArgumentNullException("itemsInCart");
            }

            string script = mStrandsScriptGenerator.GetUpdateShoppingCartTrackingScript(itemsInCart);

            RenderSettingsScript(script);
        }


        /// <summary>
        /// Renders purchased script with informations about all purchased items.
        /// </summary>
        /// <param name="orderID">ID of the order</param>
        /// <param name="orderData">Informations about items to render</param>
        /// <exception cref="ArgumentNullException"><paramref name="orderData"/> is null</exception>
        public void RenderPurchasedTrackingScript(string orderID, IEnumerable<StrandsPurchasedEventData> orderData)
        {
            if (orderData == null)
            {
                throw new ArgumentNullException("orderData");
            }

            string script = mStrandsScriptGenerator.GetPurchasedTrackingScript(orderID, orderData);

            RenderSettingsScript(script);
        }


        /// <summary>
        /// Includes Strands library in the page and initializes it by calling SBS.Worker.go. This is needed to perform any other actions (event tracking or getting recommendations).
        /// Strands library and initialization call are rendered at the end of the page, so every other script (tracking, settings, etc.) is before it.
        /// </summary>
        public void RenderLibraryScript()
        {
            // Get include script has to be used here, because registering scripts at the end of the page does not support including external files
            string includeScript = mStrandsScriptGenerator.GetLibraryIncludeScript();

            ScriptHelper.RegisterStartupScript(mPage, typeof (string), "StrandsLibrary", includeScript, false);

            string strandsLibraryInitialization = mStrandsScriptGenerator.GetInitializationScript(mApiID);

            // RegisterStartupScript is used here because Strands library must be the last Strands script on the page (other scripts are rendered by RegisterClientScriptBlock)
            ScriptHelper.RegisterStartupScript(mPage, typeof (string), "StrandsLibraryInitialization", strandsLibraryInitialization, true);
        }


        /// <summary>
        /// Renders script from custom layout of recommendation template.
        /// </summary>
        /// <param name="transformation">Transformation provided by webpart settings. JQuery type is required</param>
        /// <param name="templateID">ID of Strands template to be customized</param>
        /// <param name="placementID">Client ID of element, where the template should be placed. Should by ID of 'Strands div' by default</param>
        /// <exception cref="ArgumentNullException"><paramref name="transformation"/>, <paramref name="templateID"/> or <paramref name="placementID"/> is null</exception>
        /// <exception cref="ArgumentException">Type of <paramref name="transformation"/> is not jQuery</exception>
        public void RenderCustomizedRendererScript(TransformationInfo transformation, string templateID, string placementID)
        {
            if (transformation == null)
            {
                throw new ArgumentNullException("transformation");
            }
            if (templateID == null)
            {
                throw new ArgumentNullException("templateID");
            }
            if (placementID == null)
            {
                throw new ArgumentNullException("placementID");
            }

            if (transformation.TransformationType != TransformationTypeEnum.jQuery)
            {
                throw new ArgumentException("[StrandsScriptRenderer:RenderCustomizedRendererScript]: Transformation type has to be of type jQuery.", "placementID");
            }

            RenderLibraryScript();

            // Ensures templates library is loaded
            ScriptHelper.RegisterJQueryTemplates(mPage);

            ScriptHelper.RegisterJQuery(mPage);

            // Register transformation styles sheet
            CssRegistration.RegisterCssBlock(mPage, "StrandsCustomizedRenderer|" + transformation.TransformationID, transformation.TransformationCSS);

            string customRendererScript = mStrandsScriptGenerator.GetCustomizedRendererScript(
                MacroResolver.Resolve(transformation.TransformationCode), // Resolving macros in transformation to obtain currency format 
                templateID, 
                placementID);

            ScriptHelper.RegisterStartupScript(mPage, typeof (string), "StrandsCustomizedRenderer|" + templateID, customRendererScript, true);
        }


        /// <summary>
        /// Includes information about current culture in the page. Strands is then able to show recommendations in given culture.
        /// </summary>
        /// <param name="cultureCode">Culture code</param>
        public void RenderCultureScript(string cultureCode)
        {
            string script = mStrandsScriptGenerator.GetCultureSettingsScript(cultureCode);

            RenderSettingsScript(script);
        }


        /// <summary>
        /// Includes information about current currency in the page. Strands is then able to show recommendations with price in given currency.
        /// </summary>
        /// <param name="currencyCode">currency code</param>
        public void RenderCurrencyScript(string currencyCode)
        {
            string script = mStrandsScriptGenerator.GetCurrencySettingsScript(currencyCode);

            RenderSettingsScript(script);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Renders Strands JS script to the beginning of the page.
        /// </summary>
        /// <remarks>
        /// This method can be used to render:
        /// - tracking scripts (such as user logged tracking event)
        /// - settings scripts (cview or pview) 
        /// - custom rendering function.
        /// </remarks>
        /// <param name="script">Script which will be rendered</param>
        private void RenderSettingsScript(string script)
        {
            // Use script as key, so it is never included more than once
            // RegisterClientScriptBlock is used because it must be rendered before the Strands library script (which is rendered with RegisterStartupScript)
            ScriptHelper.RegisterClientScriptBlock(mPage, typeof (string), script, script, true);

            RenderLibraryScript();
        }

        #endregion
    }
}
