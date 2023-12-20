using System;
using System.Web;

using CMS.DataEngine;
using CMS.EventLog;

namespace CMS.SocialMarketing.URLShortening
{
    internal sealed class URLShortenerHelperEnvironment : IURLShortenerHelperEnvironment
    {

        /// <summary>
        /// Provides URL-Encoding function. ' ' => %20 etc.
        /// </summary>
        /// <param name="str">string to be URL-Encoded</param>
        /// <returns>URL-Encoded original string</returns>
        public string URLEncode(string str)
        {
            return HttpUtility.UrlEncode(str);
        }


        /// <summary>
        /// Provides settings for URLShortenerHelper.
        /// </summary>
        /// <param name="key">SetteingsKey</param>
        /// <param name="site">Site which you want the settings for</param>
        /// <returns>Site's settings for given key or global settings. Empty string if key does not exist.</returns>
        public string GetStringSettingValue(string key, SiteInfoIdentifier site)
        {
            return SettingsKeyInfoProvider.GetValue(key, site);
        }


        /// <summary>
        /// Logs a warning if an URLshortening failed.
        /// </summary>
        /// <param name="source">Source of the event (Content, Administration, etc.)</param>
        /// <param name="eventCode">Event code (UPDATEDOC, SENDEMAIL, DELETE, etc.)</param>
        /// <param name="eventException">Exception to be logged</param>
        /// <param name="site">Current site</param>
        public void LogWarning(string source,string eventCode,Exception eventException, SiteInfoIdentifier site)
        {
            EventLogProvider.LogWarning(source, eventCode, eventException, site.ObjectID, String.Empty);
        }
    }
}
