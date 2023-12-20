using System;

using CMS.Activities;
using CMS.Base;
using CMS.SiteProvider;
using CMS.StrandsRecommender.Internal;

namespace CMS.StrandsRecommender
{
    /// <summary>
    /// Contains handlers of the Strands recommendation module.
    /// </summary>
    internal class StrandsRecommenderHandlers
    {
        #region "Public methods"

        /// <summary>
        /// Connects handlers to events.
        /// </summary>
        public static void Init()
        {
            // ActivityLogged event is used because EC doesn't have purchased/shopping cart updated events yet
            ActivityEvents.ActivityProcessedInLogService.Execute += RememberDelayedRendering;

            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                // Change response status to 401 Unauthorized (prompts user to enter credentials) at the end of the request.
                // Status cannot be set at this point, because it would be handled by ASP.NET and user would be redirected to login page. 
                // By setting response status at the end of the request it is possible to return 401.
                RequestEvents.End.Execute += SetUnauthorizedStatus;
            }
        }

        #endregion


        #region "Handlers"

        /// <summary>
        /// Remembers information that purchased or cart update events happened, so tracking script can be rendered later.
        /// </summary>
        /// <remarks>
        /// This is needed, because there comes a redirect after mentioned activities are performed. This means that HTTP response is 302 and is not rendered on the client side and thus JavaScript is not executed.
        /// Therefore information that event happened is stored to user's cookie and is retrieved later, when response is 200.
        /// </remarks>
        private static void RememberDelayedRendering(object sender, CMSEventArgs<IActivityInfo> e)
        {
            if (!StrandsSettings.IsStrandsEnabled(SiteContext.CurrentSiteName))
            {
                return;
            }

            var activity = e.Parameter;

            if (activity == null)
            {
                return;
            }

            switch (activity.ActivityType)
            {
                case PredefinedActivityType.PURCHASE:
                {
                    StrandsEventsMemory.RememberItemsPurchasedEvent(activity.ActivityItemID);
                    break;
                }
                case PredefinedActivityType.PRODUCT_ADDED_TO_SHOPPINGCART:
                case PredefinedActivityType.PRODUCT_REMOVED_FROM_SHOPPINGCART:
                {
                    StrandsEventsMemory.RememberCartUpdateEvent();
                    break;
                }
            }
        }


        /// <summary>
        /// Checks whether current status code is 319. If true, sets response status code to 401 Unauthorized, so the browser will prompt user to enter credentials.
        /// </summary>
        private static void SetUnauthorizedStatus(object sender, EventArgs e)
        {
            StrandsCatalogFeedSecurity.SetUnauthorizedStatus();
        }
    }

    #endregion
}
