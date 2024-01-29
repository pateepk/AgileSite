using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Base class for helpers
    /// </summary>
    public class CoreMethods
    {
        /// <summary>
        /// Gets the string by the specified resource key
        /// </summary>
        /// <param name="resourceKey">Resource key</param>
        /// <param name="culture">Culture</param>
        protected static string GetString(string resourceKey, string culture = null)
        {
            return CoreServices.Localization.GetString(resourceKey, culture);
        }


        /// <summary>
        /// Gets the string by the specified resource key
        /// </summary>
        /// <param name="resourceKey">Key of the string</param>
        /// <param name="culture">Culture</param>
        /// <param name="defaultValue">Default value</param>
        protected static string GetAPIString(string resourceKey, string culture, string defaultValue)
        {
            return CoreServices.Localization.GetAPIString(resourceKey, culture, defaultValue);
        }
    }
}
