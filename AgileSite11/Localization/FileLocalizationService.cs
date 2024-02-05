using System;

using CMS.Core;

namespace CMS.Localization
{
    /// <summary>
    /// Default localization service
    /// </summary>
    public class FileLocalizationService : ILocalizationService
    {
        /// <summary>
        /// Returns specified string.
        /// </summary>
        /// <param name="stringName">Key of the string</param>
        /// <param name="culture">Culture</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        public virtual string GetString(string stringName, string culture = null, bool useDefaultCulture = true)
        {
            return LocalizationHelper.GetFileString(stringName, culture, useDefaultCulture);
        }


        /// <summary>
        /// Returns specified string.
        /// </summary>
        /// <param name="stringName">Key of the string</param>
        /// <param name="culture">Culture</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        public virtual string GetFileString(string stringName, string culture = null, bool useDefaultCulture = true)
        {
            return LocalizationHelper.GetFileString(stringName, culture, useDefaultCulture);
        }
        

        /// <summary>
        /// Returns specified string.
        /// </summary>
        /// <param name="stringName">Key of the string</param>
        /// <param name="culture">Culture</param>
        /// <param name="defaultValue">Default value</param>
        public virtual string GetAPIString(string stringName, string culture, string defaultValue)
        {
            return LocalizationHelper.GetFileString(stringName, culture, defaultValue);
        }


        /// <summary>
        /// Localizes the given expression, handles two types of expressions:
        /// 
        /// stringkey - Simple localization
        /// 
        /// =default string|cs-cz=localized string - advanced localization
        /// </summary>
        /// <param name="expression">Expression to localize</param>
        /// <param name="culture">Culture to use for localization</param>
        /// <param name="encode">If true, translations (replacements) are HTML encoded</param>
        /// <param name="getStringMethod">Method to get the resource string for localization</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        public string LocalizeExpression(string expression, string culture = null, bool encode = false, Func<string, string, bool, string> getStringMethod = null, bool useDefaultCulture = true)
        {
            return LocalizationHelper.LocalizeExpression(expression, culture, encode, getStringMethod, useDefaultCulture);
        }


        /// <summary>
        /// Clears the cached resource strings and forces them to load again on next localization request.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        public virtual void ClearCache(bool logTasks)
        {
            // Do nothing
        }
    }
}
