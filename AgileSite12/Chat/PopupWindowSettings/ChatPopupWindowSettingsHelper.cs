using System;
using System.Linq;
using System.Text;
using System.Web.Caching;

using CMS.Helpers;

namespace CMS.Chat
{
    /// <summary>
    /// Helper class for managing ChatPopupWindowSettings.
    /// </summary>
    public static class ChatPopupWindowSettingsHelper
    {
        #region Public methods

        /// <summary>
        /// Stores settings in DB and cache for further use.
        /// </summary>
        /// <param name="messageTrans">Message transformation setting</param>
        /// <param name="userTrans">User transformation setting</param>
        /// <param name="errorTrans">Error transformation setting</param>
        /// <param name="errorClearTrans">Error clear transformation setting</param>
        /// <returns>Hash code required to retrieve stored settings.</returns>
        public static int Store(string messageTrans, string userTrans, string errorTrans, string errorClearTrans)
        {
            int hashCode = GetHashCode(messageTrans, userTrans, errorTrans, errorClearTrans);
            
            string cacheKey = BuildCacheKey(hashCode);


            object dummyValue;

            // If key exists in cache, it for sure exists also in DB and hashCode can be directly returned.
            if (CacheHelper.TryGetItem(cacheKey, out dummyValue))
            {
                return hashCode;
            }

            // Add to DB if not exists
            ChatPopupWindowSettingsInfo infoObject = ChatPopupWindowSettingsInfoProvider.Store(hashCode, messageTrans, userTrans, errorTrans, errorClearTrans);

            // Add to cache
            SaveToCache(cacheKey, infoObject);

            return hashCode;
        }


        /// <summary>
        /// Gets popup window settings from cache or database.
        /// </summary>
        /// <param name="hashCode">Hash code of this settings</param>
        /// <returns>Popup window settings</returns>
        public static ChatPopupWindowSettingsInfo GetPopupWindowSettings(int hashCode)
        {
            ChatPopupWindowSettingsInfo result;
            string cacheKey = BuildCacheKey(hashCode);

            if (!CacheHelper.TryGetItem(cacheKey, out result))
            {
                result = ChatPopupWindowSettingsInfoProvider.GetByHashCode(hashCode);

                SaveToCache(cacheKey, result);
            }

            return result;
        }

        #endregion


        #region "Private methods"

        private static void SaveToCache(string cacheKey, ChatPopupWindowSettingsInfo infoObject)
        {
            CacheHelper.Add(cacheKey, infoObject, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(5));
        }


        private static string BuildCacheKey(int hashCode)
        {
            return "chat|chatpopupwindowsettings|" + hashCode;
        }


        private static int GetHashCode(params string[] stringsToHash)
        {
            unchecked
            {
                int result = 37; // prime

                foreach (string str in stringsToHash)
                {
                    result *= 397;
                    result += GetStringHashCode(str);
                }

                return result;
            }
        }


        /// <summary>
        /// We can't use string's GetHashCode, because it is not guaranteed to be equal on different CLR versions.
        /// </summary>
        /// <param name="str">Input string</param>
        /// <returns>Hash code</returns>
        private static int GetStringHashCode(string str)
        {
            unchecked
            {
                int result = 37;

                foreach (char c in str)
                {
                    result *= 397;
                    result += c;
                }

                return result;
            }
        }

        #endregion
    }
}
