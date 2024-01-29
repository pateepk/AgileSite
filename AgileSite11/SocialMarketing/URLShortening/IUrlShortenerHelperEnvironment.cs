using System;

using CMS.DataEngine;

namespace CMS.SocialMarketing.URLShortening
{
    /// <summary>
    /// This interface is designed purely to separate URLShortener helper from the CMS environment.
    /// </summary>
    public interface IURLShortenerHelperEnvironment
    {
        /// <summary>
        /// Provides settings for URLShortenerHelper.
        /// </summary>
        /// <param name="key">SetteingsKey</param>
        /// <param name="site">Site which you want the settings for</param>
        /// <returns>Site's settings for given key or global settings. Empty string if key does not exist.</returns>
        string GetStringSettingValue(string key, SiteInfoIdentifier site);


        /// <summary>
        /// Provides URL-Encoding function. ' ' => %20 etc.
        /// </summary>
        /// <param name="str">string to be URL-Encoded</param>
        /// <returns>URL-Encoded original string</returns>
        string URLEncode(string str);


        /// <summary>
        /// Logs a warning if an URLshortening failed.
        /// </summary>
        /// <param name="source">Source of the event (Content, Administration, etc.)</param>
        /// <param name="eventCode">Event code (UPDATEDOC, SENDEMAIL, DELETE, etc.)</param>
        /// <param name="eventException">Exception to be logged</param>
        /// <param name="site">Current site</param>
        void LogWarning(string source, string eventCode, Exception eventException, SiteInfoIdentifier site);
    }
}
