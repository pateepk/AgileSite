using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Helpers;

using Newtonsoft.Json;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Manages current user's AB test state - can assign an AB variant to him, save which conversions he has done, or read saved state. Internally uses a cookie for permanent state 
    /// (see <see cref="ABCookieValue"/>) and session for session state(see <see cref="SessionHelper"/>).
    /// </summary>
    internal class ABUserStateManager
    {
        #region "Variables"

        private readonly string mPermanentCookieName;
        private readonly string mSessionKey;
        private readonly DateTime mPermanentExpiration;
        private ABCookieValue mCookieValue;

        #endregion


        #region Constants

        /// <summary>
        /// Prefix of AB test cookie names.
        /// </summary>
        public const string ABCOOKIE_PREFIX = "CMSAB";

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets cached cookie value.
        /// </summary>
        private ABCookieValue CookieValue
        {
            get
            {
                return mCookieValue ?? (mCookieValue = GetCookieValue(mPermanentCookieName));
            }
            set
            {
                mCookieValue = value;
            }
        }


        /// <summary>
        /// Indicates whether is visitor included in AB test or not.
        /// </summary>
        public bool IsExcluded
        {
            get
            {
                if (CookieValue != null)
                {
                    return CookieValue.ExcludedFromTest;
                }

                return false;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="testName">Name of the test for which the user's state is managed</param>
        /// <exception cref="ArgumentException">testName is null or empty</exception>
        public ABUserStateManager(string testName)
        {
            if (String.IsNullOrEmpty(testName))
            {
                throw new ArgumentException("[ABUserStateManager]: Test name cannot be null or empty.", "testName");
            }

            mSessionKey = "ABConversions" + testName;
            mPermanentCookieName = ABCOOKIE_PREFIX + testName;
            mPermanentExpiration = DateTime.Now.AddMonths(2);
        }


        /// <summary>
        /// Sets a AB test visit for current user so that from now on, conversions can be logged also as AB test session conversions.
        /// </summary>
        /// <returns>True if visit was assigned successfully (wasn't already assigned). False if a visit is already set.</returns>
        public bool SetVisit()
        {
            if (IsABVisit())
            {
                return false;
            }

            SessionHelper.SetValue(mSessionKey, new List<string>());

            return true;
        }


        /// <summary>
        /// Returns true if user has visited AB test page and has visit session.
        /// </summary>
        public bool IsABVisit()
        {
            var session = SessionHelper.GetValue(mSessionKey) as List<string>;
            return (session != null);
        }


        /// <summary>
        /// Gets names of the current user's AB tests.
        /// </summary>
        public static IEnumerable<string> GetUsersTests()
        {
            // Get names of all user's cookies
            var cookieNames = CookieHelper.GetDistinctCookieNames();

            // Return AB test names
            return cookieNames.Where(c => c.StartsWithCSafe(ABCOOKIE_PREFIX, true))
                              .Select(c => c.Substring(ABCOOKIE_PREFIX.Length));
        }


        /// <summary>
        /// Gets name of the AB variant assigned to the current user. Returns null, if the user hasn't got any variant assigned yet.
        /// </summary>
        /// <returns>Code name of the variant that is assigned to the current user</returns>
        public string GetVariantName()
        {
            if (CookieValue != null)
            {
                return CookieValue.VariantName;
            }
            return null;
        }


        /// <summary>
        /// Assigns an AB variant to current user so that it can be used on next visit. From now on, permanent conversions can be also saved to
        /// user's state.
        /// </summary>
        /// <param name="variantName">Name of the variant to assign current user to</param>
        public void AssignVariant(string variantName)
        {
            if (String.IsNullOrEmpty(variantName))
            {
                throw new ArgumentException("[ABUserStateManager]: Variant name cannot be null or empty.", "variantName");
            }

            if (CookieValue != null)
            {
                if (CookieValue.VariantName != variantName)
                {
                    // Inconsistent cookie exists, override variant name
                    CookieValue.VariantName = variantName;
                    SetCookie(mPermanentCookieName, CookieValue, mPermanentExpiration);
                }
            }
            else
            {
                // Cookie does not exist, create a new cookie
                CookieValue = new ABCookieValue { VariantName = variantName };
                SetCookie(mPermanentCookieName, CookieValue, mPermanentExpiration);
            }
        }


        /// <summary>
        /// Excludes visitor from AB test.
        /// </summary>
        public void Exclude()
        {
            CookieValue = new ABCookieValue { ExcludedFromTest = true };
            SetCookie(mPermanentCookieName, CookieValue, mPermanentExpiration);
        }


        /// <summary>
        /// Gets permanent conversions that are done by current user.
        /// </summary>
        /// <returns>Permanent conversions that are done by current user</returns>
        public IEnumerable<string> GetPermanentConversions()
        {
            if (CookieValue != null)
            {
                return CookieValue.Conversions;
            }
            return Enumerable.Empty<string>();
        }


        /// <summary>
        /// Gets session conversions that are done by current user.
        /// </summary>
        /// <returns>Session conversions that are done by current user</returns>
        public IEnumerable<string> GetSessionConversions()
        {
            var sessionConversions = SessionHelper.GetValue(mSessionKey) as List<string>;
            if (sessionConversions != null)
            {
                return sessionConversions;
            }
            return Enumerable.Empty<string>();
        }


        /// <summary>
        /// Adds a conversion to session and permanent conversions if possible.
        /// </summary>
        /// <param name="conversion">Conversion to add to current user</param>
        /// <exception cref="ArgumentException">conversion is null or empty</exception>
        public void AddConversion(string conversion)
        {
            if (String.IsNullOrEmpty(conversion))
            {
                throw new ArgumentException("[ABUserStateManager.AddConversion]: Conversion name can't be null or empty.", "conversion");
            }
            TryAddPermanentConversion(conversion);
            TryAddSessionConversion(conversion);
        }


        /// <summary>
        /// Returns true when A/B test cookie is defined.
        /// </summary>
        public bool IsABTestCookieDefined()
        {
            return CookieValue != null;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Adds specified conversion to permanent cookie.
        /// </summary>
        /// <param name="conversion">Conversion name</param>
        private void TryAddPermanentConversion(string conversion)
        {
            if (CookieValue != null)
            {
                if (!CookieValue.Conversions.Contains(conversion))
                {
                    CookieValue.Conversions.Add(conversion);
                    SetCookie(mPermanentCookieName, CookieValue, mPermanentExpiration);
                }
            }
        }


        /// <summary>
        /// Adds specified conversion to session cookie.
        /// </summary>
        /// <param name="conversion">Conversion name</param>
        private void TryAddSessionConversion(string conversion)
        {
            List<string> sessionConversions = SessionHelper.GetValue(mSessionKey) as List<string>;
            if (sessionConversions != null)
            {
                if (!sessionConversions.Contains(conversion))
                {
                    sessionConversions.Add(conversion);
                }
            }
        }


        /// <summary>
        /// Returns deserialized value of specified cookie.
        /// </summary>
        /// <param name="cookieName">Cookie name</param>
        private ABCookieValue GetCookieValue(string cookieName)
        {
            string serialized = CookieHelper.GetValue(cookieName);
            try
            {
                return JsonConvert.DeserializeObject<ABCookieValue>(serialized);
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// Serializes object to a cookie as a JSON object.
        /// </summary>
        /// <param name="cookieName">Name of the cookie to save object to</param>
        /// <param name="cookieValue">Object to serialize to the cookie</param>
        /// <param name="expiration">Expiration of the cookie</param>
        private void SetCookie(string cookieName, ABCookieValue cookieValue, DateTime expiration)
        {
            string serialized = JsonConvert.SerializeObject(cookieValue, Formatting.None, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
            CookieHelper.SetValue(cookieName, serialized, expiration);
        }

        #endregion
    }
}
