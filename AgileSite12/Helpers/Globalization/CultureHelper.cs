using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;

using CMS.Core;
using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Provides the global methods for culture helping operations.
    /// </summary>
    public class CultureHelper : AbstractHelper<CultureHelper>
    {
        #region "Variables"

        /// <summary>
        /// English culture code
        /// </summary>
        private const string ENGLISH_CULTURE_CODE = "en-us";


        /// <summary>
        /// Default culture.
        /// </summary>
        private static CultureInfo mDefaultCulture;


        /// <summary>
        /// List of cultures which are supported by facebook separated by semicolon.
        /// </summary>
        private static string mFacebookCultures;


        /// <summary>
        /// Default UI culture code (e.g., "en-US").
        /// </summary>
        private readonly StringAppSetting mDefaultUICultureCode = new StringAppSetting("CMSDefaultUICulture", ENGLISH_CULTURE_CODE);


        #endregion


        #region "Properties"

        /// <summary>
        /// English culture (default culture for storing data).
        /// </summary>
        public static CultureInfo EnglishCulture
        {
            get
            {
                return SystemContext.EnglishCulture;
            }
        }


        /// <summary>
        /// Default culture (default culture of user interface).
        /// </summary>
        public static CultureInfo DefaultUICulture
        {
            get
            {
                return mDefaultCulture ?? (mDefaultCulture = GetCultureInfo(DefaultUICultureCode));
            }
        }


        /// <summary>
        /// Returns default UI culture code.
        /// </summary>
        public static string DefaultUICultureCode
        {
            get
            {
                return HelperObject.DefaultUICultureCodeInternal;
            }
        }


        /// <summary>
        /// Returns preferred UI culture code.
        /// </summary>
        public static string PreferredUICultureCode
        {
            get
            {
                return GetPreferredUICultureCode();
            }
            set
            {
                SetPreferredUICultureCode(value);
            }
        }


        /// <summary>
        /// Preferred UI culture info object.
        /// </summary>
        public static CultureInfo PreferredUICultureInfo
        {
            get
            {
                return GetCultureInfo(PreferredUICultureCode);
            }
        }


        /// <summary>
        /// List of cultures which are supported by facebook separated by semicolon.
        /// </summary>
        [Obsolete]
        private static string FacebookCultures
        {
            get
            {
                if (mFacebookCultures == null)
                {
                    var doc = new XmlDocument();

                    try
                    {
                        doc.Load("http://www.facebook.com/translations/FacebookLocales.xml");
                    }
                    catch
                    {
                        // Service is not available 
                        return string.Empty;
                    }

                    var root = doc.DocumentElement;
                    if (root != null)
                    {
                        var locales = root.SelectNodes("//standard");
                        if (locales != null)
                        {
                            foreach (XmlNode node in locales)
                            {
                                mFacebookCultures += node.ChildNodes[1].InnerText + ";";
                            }
                        }
                    }
                }

                return mFacebookCultures;
            }
        }


        /// <summary>
        /// Returns default UI culture code.
        /// </summary>
        protected string DefaultUICultureCodeInternal
        {
            get
            {
                return mDefaultUICultureCode.Value.ToLowerInvariant();
            }
            set
            {
                mDefaultUICultureCode.Value = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the culture info for specified culture.
        /// If culture code is invalid, then english culture is returned.
        /// </summary>
        /// <param name="cultureCode">Culture code</param>
        public static CultureInfo GetCultureInfo(string cultureCode)
        {
            if (string.IsNullOrEmpty(cultureCode))
            {
                return EnglishCulture;
            }

            try
            {
                return CultureInfo.GetCultureInfo(cultureCode);
            }
            catch (CultureNotFoundException)
            {
                return EnglishCulture;
            }
        }


        /// <summary>
        /// Returns the user preferred culture (reflects the virtual context).
        /// </summary>
        public static string GetPreferredCulture()
        {
            // Try to get from virtual context first
            string virtualCulture = (string)VirtualContext.GetItem(VirtualContext.PARAM_CULTURE);
            if (virtualCulture != null)
            {
                return virtualCulture;
            }

            return GetOriginalPreferredCulture();
        }


        /// <summary>
        /// Returns the user original preferred culture.
        /// </summary>
        public static string GetOriginalPreferredCulture()
        {
            return HTMLHelper.HTMLEncode((string)ContextHelper.GetItem(CookieName.PreferredCulture, true, false, true));
        }


        /// <summary>
        /// Sets the user preferred culture.
        /// </summary>
        public static void SetPreferredCulture(string cultureCode)
        {
            SetPreferredCulture(cultureCode, true, true);
        }


        /// <summary>
        /// Sets the user preferred culture.
        /// </summary>
        /// <param name="cultureCode">Culture code to set</param>
        /// <param name="setCookie">Set the culture to a cookie as well</param>
        /// <param name="handleLifeTime">If true, the life time support of the culture is allowed</param>
        public static void SetPreferredCulture(string cultureCode, bool setCookie, bool handleLifeTime)
        {
            bool useCookies = true;

            // Handle the life time
            if (handleLifeTime)
            {
                // Set cookies only if OblectLifeTime query parameter is not set to request
                ObjectLifeTimeEnum lifeTime = ObjectLifeTimeFunctions.GetCurrentObjectLifeTime("Lang");
                useCookies = (setCookie && (lifeTime == ObjectLifeTimeEnum.Cookies) && !VirtualContext.ItemIsSet("Culture"));
            }

            ContextHelper.Add(CookieName.PreferredCulture, cultureCode, true, false, useCookies, DateTime.Now.AddYears(1));
        }


        /// <summary>
        /// Returns the user preferred UI Culture code.
        /// </summary>
        public static string GetPreferredUICultureCode()
        {
            // Try to get from virtual context first
            string virtualCulture = (string)VirtualContext.GetItem("UICulture");
            if (virtualCulture != null)
            {
                return virtualCulture;
            }

            return GetPreferredUICultureCodeWithCheck();
        }


        /// <summary>
        /// Sets the user preferred UI Culture code.
        /// </summary>
        public static void SetPreferredUICultureCode(string cultureCode)
        {
            bool useCookies = !VirtualContext.ItemIsSet("UICulture");

            ContextHelper.Add(CookieName.PreferredUICulture, cultureCode, true, false, useCookies, DateTime.Now.AddYears(1));
        }


        /// <summary>
        /// Returns true is current content culture is right to left culture.
        /// </summary>
        public static bool IsPreferredCultureRTL()
        {
            return IsCultureRTL(GetPreferredCulture());
        }


        /// <summary>
        /// Returns true if current UI culture is right to left culture.
        /// </summary>
        public static bool IsUICultureRTL()
        {
            return IsCultureRTL(GetPreferredUICultureCode());
        }


        /// <summary>
        /// Returns true if specified culture is right to left culture.
        /// </summary>
        public static bool IsCultureRTL(string cultureCode)
        {
            if (string.IsNullOrEmpty(cultureCode))
            {
                return false;
            }

            try
            {
                return CultureInfo.GetCultureInfo(cultureCode).TextInfo.IsRightToLeft;
            }
            catch(CultureNotFoundException)
            {
                return false;
            }
        }


        /// <summary>
        /// Returns the default culture code.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetDefaultCultureCode(string siteName)
        {
            // Get default culture from settings
            string cultureCode = CoreServices.Settings[siteName + ".CMSDefaultCultureCode"];

            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = DefaultUICultureCode;
            }
            return cultureCode;
        }


        /// <summary>
        /// Returns the short culture part, just the main culture identifier ("en-us" => "en").
        /// </summary>
        /// <param name="cultureCode">Culture code</param>
        public static string GetShortCultureCode(string cultureCode)
        {
            if (string.IsNullOrEmpty(cultureCode))
            {
                return cultureCode;
            }

            try
            {
                return CultureInfo.GetCultureInfo(cultureCode).TwoLetterISOLanguageName;
            }
            catch (CultureNotFoundException)
            {
                return cultureCode;
            }
        }


        /// <summary>
        /// Converts culture code to Facebook culture code.
        /// </summary>
        /// <param name="cultureCode">Culture code</param>
        /// <returns>Facebook culture code</returns>
        [Obsolete("The member was never intended for public use. Please contact Kentico support if you need to find a replacement.")]
        public static string GetFacebookCulture(string cultureCode)
        {
            // If given language is supported by Facebook
            cultureCode = cultureCode.Replace("-", "_");
            if (FacebookCultures.Contains(cultureCode + ";"))
            {
                return cultureCode;
            }

            // Try to find another language from same family
            Regex re = RegexHelper.GetRegex("(" + cultureCode.Substring(0, 3) + "[A-Z][A-Z])");
            Match m = re.Match(FacebookCultures);
            if (m.Success)
            {
                return m.Groups[1].Value;
            }

            // Return default language
            return "en_US";
        }


        /// <summary>
        /// Check if culture code is valid DotNet culture code 
        /// (All cultures that ship with the .NET Framework, including neutral and specific 
        /// cultures, cultures installed in the Windows operating system, and 
        /// custom cultures created by the user).
        /// </summary>
        /// <param name="cultureCode">Culture code</param>
        public static bool IsValidCultureInfoName(string cultureCode)
        {
            // No culture code given = invalid
            if (String.IsNullOrEmpty(cultureCode))
            {
                return false;
            }

            try
            {
                var cultureInfo = CultureInfo.GetCultureInfo(cultureCode);
                return true;
            }
            catch (CultureNotFoundException)
            {
                return false;
            }
        }


        /// <summary>
        /// Checks if culture code is neutral culture code.
        /// </summary>
        /// <param name="cultureCode">Culture code</param>
        public static bool IsNeutralCulture(string cultureCode)
        {
            if (string.IsNullOrEmpty(cultureCode))
            {
                return false;
            }

            try
            {
                return CultureInfo.GetCultureInfo(cultureCode).IsNeutralCulture;
            }
            catch(CultureNotFoundException)
            {
                return false;
            }
        }


        private static string GetPreferredUICultureCodeWithCheck()
        {
            var cultureCode = (string)ContextHelper.GetItem(CookieName.PreferredUICulture, true, false, true);
            if (Service.Resolve<ICultureService>().IsUICulture(cultureCode))
            {
                return cultureCode;
            }
            else if(!String.IsNullOrEmpty(cultureCode))
            {
                ContextHelper.Remove(CookieName.PreferredUICulture, true, false, true);
            }

            return null;
        }

        #endregion
    }
}