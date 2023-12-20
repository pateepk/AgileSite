using System;

namespace CMS.Core
{
    /// <summary>
    /// Localization service interface
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Gets the string by the specified resource key
        /// </summary>
        /// <param name="resourceKey">Resource key</param>
        /// <param name="culture">Culture</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        string GetString(string resourceKey, string culture = null, bool useDefaultCulture = true);


        /// <summary>
        /// Gets the string by the specified resource key from resource file
        /// </summary>
        /// <param name="resourceKey">Resource key</param>
        /// <param name="culture">Culture</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        string GetFileString(string resourceKey, string culture = null, bool useDefaultCulture = true);


        /// <summary>
        /// Returns specified string from the resource files.
        /// </summary>
        /// <param name="stringName">Name of the string</param>
        /// <param name="culture">Culture</param>
        /// <param name="defaultValue">Default value to return in case no string found</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exists</param>
        string GetFileString(string stringName, string culture, string defaultValue, bool useDefaultCulture = true);


        /// <summary>
        /// Returns specified string.
        /// </summary>
        /// <param name="resourceKey">Key of the string</param>
        /// <param name="culture">Culture</param>
        /// <param name="defaultValue">Default value</param>
        string GetAPIString(string resourceKey, string culture, string defaultValue);


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
        string LocalizeExpression(string expression, string culture = null, bool encode = false, Func<string, string, bool, string> getStringMethod = null, bool useDefaultCulture = true);


        /// <summary>
        /// Replaces "{$stringname$}" expressions in given text with localized strings using given culture.
        /// </summary>
        /// <param name="inputText">Input text with localizable expressions</param>
        /// <param name="culture">Culture</param>
        /// <param name="encode">If true, translations (replacements) are HTML encoded</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        string LocalizeString(string inputText, string culture = null, bool encode = false, bool useDefaultCulture = true);


        /// <summary>
        /// Replaces "{$stringname$}" expressions in given text with localized strings.
        /// </summary>
        /// <param name="inputText">Input text with localizable expressions</param>
        string LocalizeFileString(string inputText);


        /// <summary>
        /// Clears the cached resource strings and forces them to load again on next localization request.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        void ClearCache(bool logTasks);
    }
}
