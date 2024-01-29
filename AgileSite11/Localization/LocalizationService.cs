using System;
using System.Linq;
using System.Text;

using CMS.DataEngine;

namespace CMS.Localization
{
    /// <summary>
    /// Localization service including the database operations
    /// </summary>
    public class LocalizationService : FileLocalizationService
    {
        /// <summary>
        /// Returns specified string.
        /// </summary>
        /// <param name="stringName">Key of the string</param>
        /// <param name="culture">Culture</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        public override string GetString(string stringName, string culture = null, bool useDefaultCulture = true)
        {
            return LocalizationHelper.GetString(stringName, culture, useDefaultCulture);
        }


        /// <summary>
        /// Returns specified string.
        /// </summary>
        /// <param name="stringName">Key of the string</param>
        /// <param name="culture">Culture</param>
        /// <param name="defaultValue">Default value</param>
        public override string GetAPIString(string stringName, string culture, string defaultValue)
        {
            return LocalizationHelper.GetAPIString(stringName, culture, defaultValue);
        }


        /// <summary>
        /// Clears the cached resource strings and forces them to load again on next localization request.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        public override void ClearCache(bool logTasks)
        {
            ProviderHelper.ClearHashtables(ResourceStringInfo.OBJECT_TYPE, logTasks);
        }
    }
}
