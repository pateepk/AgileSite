﻿using System;
using System.Text.RegularExpressions;
using System.Threading;

using CMS.Base;
using CMS.Core;

namespace CMS.Helpers
{
    /// <summary>
    /// Provides methods to provide localized resource strings
    /// </summary>
    public static class ResHelper
    {
        #region "Variables"

        private static char mStringSeparator = '|';
        private static char mCultureSeparator = '=';

        /// <summary>
        /// Regular expression for finding the localized strings.
        /// </summary>
        private static Regex mRegExLocalize = null;

        #endregion


        #region "Symbols"

        /// <summary>
        /// Colon symbol.
        /// </summary>
        private static string mColon = null;


        /// <summary>
        /// Semicolon symbol.
        /// </summary>
        private static string mSemicolon = null;


        /// <summary>
        /// Comma symbol.
        /// </summary>
        private static string mComma = null;


        /// <summary>
        /// Dash symbol.
        /// </summary>
        private static string mDash = null;


        /// <summary>
        /// Slash symbol.
        /// </summary>
        private static string mSlash = null;


        /// <summary>
        /// Dot symbol.
        /// </summary>
        private static string mDot = null;


        /// <summary>
        /// Star symbol.
        /// </summary>
        private static string mRequiredMark = null;

        #endregion


        #region "Symbol properties"

        /// <summary>
        /// Colon symbol.
        /// </summary>
        public static string Colon
        {
            get
            {
                return mColon ?? (mColon = GetAPIString("general.colon", ":"));
            }
            set
            {
                mColon = value;
            }
        }


        /// <summary>
        /// Semicolon symbol.
        /// </summary>
        public static string Semicolon
        {
            get
            {
                return mSemicolon ?? (mSemicolon = GetAPIString("general.semicolon", ";"));
            }
            set
            {
                mSemicolon = value;
            }
        }


        /// <summary>
        /// Comma symbol.
        /// </summary>
        public static string Comma
        {
            get
            {
                return mComma ?? (mComma = GetAPIString("general.comma", ","));
            }
            set
            {
                mComma = value;
            }
        }


        /// <summary>
        /// Dash symbol.
        /// </summary>
        public static string Dash
        {
            get
            {
                return mDash ?? (mDash = GetAPIString("general.dash", "-"));
            }
            set
            {
                mDash = value;
            }
        }


        /// <summary>
        /// Slash symbol.
        /// </summary>
        public static string Slash
        {
            get
            {
                return mSlash ?? (mSlash = GetAPIString("general.slash", "/"));
            }
            set
            {
                mSlash = value;
            }
        }


        /// <summary>
        /// Dot symbol.
        /// </summary>
        public static string Dot
        {
            get
            {
                return mDot ?? (mDot = GetAPIString("general.dot", "."));
            }
            set
            {
                mDot = value;
            }
        }


        /// <summary>
        /// RequiredMark symbol.
        /// </summary>
        public static string RequiredMark
        {
            get
            {
                return mRequiredMark ?? (mRequiredMark = GetAPIString("general.requiredmark", "*"));
            }
            set
            {
                mRequiredMark = value;
            }
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// String separator.
        /// </summary>
        public static char StringSeparator
        {
            get
            {
                return mStringSeparator;
            }
            set
            {
                mStringSeparator = value;
            }
        }


        /// <summary>
        /// Culture separator.
        /// </summary>
        public static char CultureSeparator
        {
            get
            {
                return mCultureSeparator;
            }
            set
            {
                mCultureSeparator = value;
            }
        }


        /// <summary>
        /// Localization regular expression.
        /// </summary>
        public static Regex RegExLocalize
        {
            get
            {
                // Expressing groups:                                            (1:selector                             )
                return mRegExLocalize ?? (mRegExLocalize = RegexHelper.GetRegex("(?:\\{\\$)((?:[^\\$]|(?:\\$(?=[^\\}])))*)(?:\\$\\})"));
            }
            set
            {
                mRegExLocalize = value;
            }
        }

        #endregion


        #region "GetString methods"

        /// <summary>
        /// Gets the given string and formats it with the standard String.Format method
        /// </summary>
        /// <param name="stringName">String name</param>
        /// <param name="parameters">Parameters for the formatting</param>
        public static string GetStringFormat(string stringName, params object[] parameters)
        {
            return String.Format(GetString(stringName), parameters);
        }


        /// <summary>
        /// Returns specified string.
        /// </summary>
        /// <param name="stringName">Key of the string</param>
        /// <param name="culture">Culture</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        public static string GetString(string stringName, string culture = null, bool useDefaultCulture = true)
        {
            return CoreServices.Localization.GetString(stringName, culture, useDefaultCulture);
        }


        /// <summary>
        /// Returns specified string.
        /// </summary>
        /// <param name="stringName">Key of the string</param>
        /// <param name="culture">Culture</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        public static string GetFileString(string stringName, string culture = null, bool useDefaultCulture = true)
        {
            return CoreServices.Localization.GetFileString(stringName, culture, useDefaultCulture);
        }


        /// <summary>
        /// Returns specified string for the API usage (the default value is used when string is not found).
        /// </summary>
        /// <param name="stringName">Key of the string</param>
        /// <param name="defaultValue">Value to return in case string not found</param>
        public static string GetAPIString(string stringName, string defaultValue)
        {
            string culture = Thread.CurrentThread.CurrentUICulture.ToString();

            return GetAPIString(stringName, culture, defaultValue);
        }


        /// <summary>
        /// Returns specified string.
        /// </summary>
        /// <param name="stringName">Key of the string</param>
        /// <param name="culture">Culture</param>
        /// <param name="defaultValue">Default value</param>
        public static string GetAPIString(string stringName, string culture, string defaultValue)
        {
            return CoreServices.Localization.GetAPIString(stringName, culture, defaultValue);
        }

        #endregion


        #region "Localize methods"

        /// <summary>
        /// Detects whether translation can be found for the given localized string
        /// </summary>
        /// <param name="inputText">Input text with localizable expressions</param>
        public static bool TranslationFoundForLocalizedString(string inputText)
        {
            bool found = false;

            LocalizeString(inputText, (key, culture, useDefaultCulture) =>
                {
                    // Check localization
                    if (GetString(key, culture, useDefaultCulture) != key)
                    {
                        found = true;
                    }

                    return key;
                });

            return found;
        }


        /// <summary>
        /// Replaces "{$stringname$}" expressions in given text with localized strings using given culture.
        /// </summary>
        /// <param name="inputText">Input text with localizable expressions</param>
        /// <param name="culture">Culture</param>
        /// <param name="encode">If true, translations (replacements) are HTML encoded</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        public static string LocalizeString(string inputText, string culture = null, bool encode = false, bool useDefaultCulture = true)
        {
            return LocalizeString(inputText, GetString, culture, useDefaultCulture, encode);
        }


        /// <summary>
        /// Replaces "{$stringname$}" expressions in given text with localized strings.
        /// </summary>
        /// <param name="inputText">Input text with localizable expressions</param>
        public static string LocalizeFileString(string inputText)
        {
            return LocalizeString(inputText, GetFileString);
        }


        /// <summary>
        /// Replaces "{$stringname$}" expressions in given text with localized strings. 
        /// </summary>
        /// <param name="inputText">Input text with localizable expressions</param>
        /// <param name="getStringMethod">Method to get the resource string for localization</param>
        /// <param name="culture">Culture</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        /// <param name="encode">If true, translations (replacements) are HTML encoded</param>
        private static string LocalizeString(string inputText, Func<string, string, bool, string> getStringMethod = null, string culture = null, bool useDefaultCulture = true, bool encode = false)
        {
            if (String.IsNullOrEmpty(inputText))
            {
                return inputText;
            }

            if (!ContainsLocalizationMacro(inputText))
            {
                return inputText;
            }

            if (getStringMethod == null)
            {
                getStringMethod = GetString;
            }

            return RegExLocalize.Replace(inputText, m => LocalizedStringMatch(m, culture, encode, getStringMethod, useDefaultCulture));
        }


        /// <summary>
        /// Returns true, if the given text contains a localization macro
        /// </summary>
        /// <param name="inputText">Text to check</param>
        public static bool ContainsLocalizationMacro(string inputText)
        {
            if (String.IsNullOrEmpty(inputText))
            {
                return false;
            }

            return inputText.Contains("{$");
        }


        /// <summary>
        /// Match evaluator for the string localization.
        /// </summary>
        /// <param name="m">Regular expression match</param>
        /// <param name="culture">Culture to use for localization</param>
        /// <param name="encode">If true, translations (replacements) are HTML encoded</param>
        /// <param name="getStringMethod">Method to provide the resource string in case of localization</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        private static string LocalizedStringMatch(Match m, string culture, bool encode, Func<string, string, bool, string> getStringMethod, bool useDefaultCulture)
        {
            string expression = m.Groups[1].ToString();

            return LocalizeExpression(expression, culture, encode, getStringMethod, useDefaultCulture);
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
        public static string LocalizeExpression(string expression, string culture = null, bool encode = false, Func<string, string, bool, string> getStringMethod = null, bool useDefaultCulture = true)
        {
            return CoreServices.Localization.LocalizeExpression(expression, culture, encode, getStringMethod, useDefaultCulture);
        }

        #endregion
    }
}
