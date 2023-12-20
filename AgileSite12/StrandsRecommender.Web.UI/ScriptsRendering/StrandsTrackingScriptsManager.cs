using System;
using System.Linq;
using System.Web.UI;

using CMS.ContactManagement;
using CMS.DocumentEngine;
using CMS.Ecommerce;
using CMS.Ecommerce.Web.UI;
using CMS.Helpers;
using CMS.StrandsRecommender.Internal;

namespace CMS.StrandsRecommender.Web.UI
{
    /// <summary>
    /// Handles rendering Strands tracking scripts on the live site. This class is able to recognize which scripts shall be rendered and renders them if needed.
    /// </summary>
    internal class StrandsTrackingScriptsManager
    {
        #region "Private fields"

        private readonly Page mPage;
        private readonly string mStrandsApiID;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructs Strands scripts delayed renderer. 
        /// </summary>
        /// <param name="page">Scripts will be rendered to this page</param>
        /// <param name="strandsApiID">API ID of the connected Strands account</param>
        /// <exception cref="ArgumentNullException"><paramref name="page"/> or <paramref name="strandsApiID"/> is null</exception>
        public StrandsTrackingScriptsManager(Page page, string strandsApiID)
        {
            if (page == null)
            {
                throw new ArgumentNullException("page");
            }

            if (strandsApiID == null)
            {
                throw new ArgumentNullException("strandsApiID");
            }

            mPage = page;
            mStrandsApiID = strandsApiID;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Renders strands scripts for all supported Strands events. This method recognizes based on context which events shall be logged and renders appropriate scripts.
        /// This method must be called when the Session is ready (typically on the Page's PreRender event).
        /// </summary>
        public void RenderStrandsScripts()
        {
            // Don't render when status isn't 200 (javascript wouldn't be called anyway)
            if (mPage.Response.StatusCode != 200)
            {
                return;
            }

            StrandsScriptRenderer strandsScriptRenderer = new StrandsScriptRenderer(mPage, mStrandsApiID);

            RenderUserLoggedScript(strandsScriptRenderer);
            RenderItemVisitedScript(strandsScriptRenderer);
            RenderItemSearchedScript(strandsScriptRenderer);
            RenderPurchasedScript(strandsScriptRenderer);
            RenderUpdateShoppingCartScript(strandsScriptRenderer);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Renders event indicating which user is browsing the web. This event needs to be logged at every page user visits (see http://recommender.strands.com/developers/javascript/#ulogin"/). 
        /// Strands can later associate this user ID with the ID of the newsletter recipient and show appropriate recommendations.
        /// User logged event is rendered only if current contact is known.
        /// </summary>
        /// <param name="strandsScriptRenderer">Renderer</param>
        private void RenderUserLoggedScript(StrandsScriptRenderer strandsScriptRenderer)
        {
            // Render user logged event only if current contact is known. ContactGuid is sent as a user ID to the Strands.
            if (ContactManagementContext.CurrentContact != null)
            {
                strandsScriptRenderer.RenderUserLoggedTrackingScript(ContactManagementContext.CurrentContact.ContactGUID.ToString());
            }
        }


        /// <summary>
        /// Renders JavaScript which tracks event when user visits product. 
        /// </summary>
        /// <param name="strandsScriptRenderer">Renderer</param>
        private void RenderItemVisitedScript(StrandsScriptRenderer strandsScriptRenderer)
        {
            var currentDocument = DocumentContext.CurrentDocument;

            // Render item visited tracking script only if user is viewing a product
            if ((currentDocument != null) && currentDocument.IsProduct())
            {
                strandsScriptRenderer.RenderItemVisitedTrackingScript(StrandsCatalogPropertiesMapper.GetItemID(currentDocument));
            }
        }


        /// <summary>
        /// Renders script tracking event when user opens product from the search results.
        /// Method finds out that user comes from search page by checking http referrer. If previous page was displaying search results, it had searchtext parameter in url.
        /// </summary>
        /// <param name="strandsScriptRenderer">Renderer</param>
        private void RenderItemSearchedScript(StrandsScriptRenderer strandsScriptRenderer)
        {
            var currentDocument = DocumentContext.CurrentDocument;

            // Render event only if product is visited and user comes from search results
            if ((currentDocument != null) && currentDocument.IsProduct())
            {
                // If referrer contains query parameter searchtext, track search event
                var referrer = mPage.Request.UrlReferrer;
                if (referrer != null)
                {
                    var searchText = URLHelper.GetUrlParameter(referrer.ToString(), "searchtext");
                    if (searchText != null)
                    {
                        strandsScriptRenderer.RenderSearchedTrackingScript(searchText, StrandsCatalogPropertiesMapper.GetItemID(currentDocument));
                    }
                }
            }
        }


        /// <summary>
        /// Tracks event when user places an order. All items in order are included as event parameters.
        /// </summary>
        /// <param name="strandsScriptRenderer">Renderer</param>
        private void RenderPurchasedScript(StrandsScriptRenderer strandsScriptRenderer)
        {
            int lastOrderID;

            // Render tracking script only if it is stored in user's cookie that it should be rendered
            if (StrandsEventsMemory.IsItemsPurchasedEventPending(out lastOrderID))
            {
                var orderedItems = OrderItemInfoProvider.GetOrderItems(lastOrderID);
                var itemsToRender = from orderItem in orderedItems
                                    let treeNode = EcommerceTransformationFunctions.GetSKUNode(orderItem.OrderItemSKU, "NodeAlias")
                                    where treeNode != null
                                    select new StrandsPurchasedEventData
                                    {
                                        ItemID = StrandsCatalogPropertiesMapper.GetItemID(treeNode),
                                        Price = orderItem.OrderItemUnitPrice,
                                        Quantity = orderItem.OrderItemUnitCount
                                    };

                strandsScriptRenderer.RenderPurchasedTrackingScript(lastOrderID.ToString(), itemsToRender);
            }
        }


        /// <summary>
        /// Render script tracking event when user updates his shopping cart (adds or removes a product).
        /// </summary>
        /// <param name="strandsScriptRenderer">Renderer</param>
        private void RenderUpdateShoppingCartScript(StrandsScriptRenderer strandsScriptRenderer)
        {
            // Render tracking script only if it is stored in user's cookie that it should be rendered
            if (StrandsEventsMemory.IsCartUpdateEventPending())
            {
                strandsScriptRenderer.RenderUpdateShoppingCartTrackingScript(StrandsProductsProvider.GetItemIDsFromCurrentShoppingCart());
            }
        }

        #endregion
    }
}