using System;
using System.ComponentModel;
using System.Globalization;

using CMS.Helpers;


namespace CMS.StrandsRecommender.Internal
{
    /// <summary>
    /// When it isn't right time to render Strands tracking event (for example the HTTP response status is 302, so JavaScripts wouldn't be executed), this class
    /// can remember the information about the event and when the time is right, it will provide information that event tracking is pending.
    /// </summary>
    /// <remarks>
    /// Class uses Cookies internally to remember pending events.
    /// This is needed, because there comes a redirect after items are purchased or shopping cart is updated. This means that HTTP response is 302 and is not rendered on the client side and thus JavaScript is not executed.
    /// Therefore information that event happened is stored to user's cookie and is retrieved later, when response is 200.
    /// </remarks>
    public static class StrandsEventsMemory
    {
        #region "Public methods"
        
        /// <summary>
        /// Stores information that cart update event should be rendered. After calling this method, <see cref="IsCartUpdateEventPending"/> method will return
        /// true once for the same user. Information will be stored in Cookies.
        /// </summary>
        public static void RememberCartUpdateEvent()
        {
            RememberEvent("CartUpdate", true);
        }


        /// <summary>
        /// Checks if cart update event should be rendered. This method returns true only once and after that assumes that event has been rendered.
        /// </summary>
        /// <returns>True if cart update event should be rendered</returns>
        public static bool IsCartUpdateEventPending()
        {
            bool storedValue;
            bool result = IsEventPending("CartUpdate", out storedValue);

            return result && storedValue;
        }


        /// <summary>
        /// Stores information that items purchased event should be rendered. After calling this method, <see cref="IsItemsPurchasedEventPending"/> method will return
        /// true once for the same user. Information will be stored in Cookies.
        /// </summary>
        /// <param name="lastOrderID">ID of the order which was made</param>
        public static void RememberItemsPurchasedEvent(int lastOrderID)
        {
            RememberEvent("ItemsPurchased", lastOrderID);
        }


        /// <summary>
        /// Checks if item purchased event should be rendered. This method returns true only once and after that assumes that event has been rendered.
        /// </summary>
        /// <param name="lastOrderID">Output parameter which will be set to the value passed to the <see cref="RememberItemsPurchasedEvent"/> method when storing information about pending event</param>
        /// <returns>True if cart update event should be rendered</returns>
        public static bool IsItemsPurchasedEventPending(out int lastOrderID)
        {
            return IsEventPending("ItemsPurchased", out lastOrderID);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Stores information about pending event to the cookie.
        /// </summary>
        /// <param name="trackingEventCode">Event code</param>
        /// <param name="eventData">Associated data which will be stored and returned when <see cref="IsEventPending{TEventType}"/> method succeeds. Has to implement IConvertible to be able to cast value to string with invariant culture</param>
        private static void RememberEvent(string trackingEventCode, IConvertible eventData)
        {
            string cookieKey = GetTrackingEventCookieKey(trackingEventCode);

            CookieHelper.SetValue(cookieKey, eventData.ToString(CultureInfo.InvariantCulture), DateTime.Now.AddMinutes(20));
        }


        /// <summary>
        /// Checks if the particular event is pending by checking data in the cookie. If this method returns true, it automatically removes data from cookie, so next time it returns false again.
        /// </summary>
        /// <typeparam name="TEventData">Type of associated data, has to implement IConvertible</typeparam>
        /// <param name="trackingEventCode">Code of the event</param>
        /// <param name="eventData">Associated data stored when event was set</param>
        /// <returns>True if event was pending, otherwise false</returns>
        private static bool IsEventPending<TEventData>(string trackingEventCode, out TEventData eventData) where TEventData : IConvertible
        {
            string cookieKey = GetTrackingEventCookieKey(trackingEventCode);
            string storedEvent = CookieHelper.GetValue(cookieKey);

            if (storedEvent == null)
            {
                eventData = default(TEventData);
                return false;
            }

            CookieHelper.Remove(cookieKey);

            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(TEventData));
                eventData = (TEventData)converter.ConvertFromString(storedEvent);
                return true;
            }
            catch
            {
                eventData = default(TEventData);
                return false;
            }
        }


        /// <summary>
        /// Gets key which will be used to store information about pending events.
        /// </summary>
        /// <param name="trackingEventCode">Code of the event</param>
        /// <returns>Generated key</returns>
        private static string GetTrackingEventCookieKey(string trackingEventCode)
        {
            return "CMSStrandsTrackEvent_" + trackingEventCode;
        }

        #endregion
    }
}