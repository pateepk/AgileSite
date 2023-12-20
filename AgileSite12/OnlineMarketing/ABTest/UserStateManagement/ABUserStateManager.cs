using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Core;
using CMS.Helpers;

using Newtonsoft.Json;

namespace CMS.OnlineMarketing.Internal
{
    /// <summary>
    /// Manages current user's A/B test state - can assign an A/B variant to him, save which conversions he has done, or read saved state. Internally uses a cookie for permanent state
    /// (see <see cref="ABCookieValue{TIdentifier}"/>) and session for session state(see <see cref="SessionHelper"/>).
    /// </summary>
    /// <typeparam name="TIdentifier">Type of the A/B variant identifier.</typeparam>
    internal class ABUserStateManager<TIdentifier> : IABUserStateManager<TIdentifier>
    {
        #region "Variables"

        private readonly string mPermanentCookieName;
        private readonly string mSessionKey;
        private readonly DateTime mPermanentExpiration;
        private ABCookieValue<TIdentifier> mCookieValue;
        private readonly IABResponseCookieProvider cookieProvider;
        private readonly string testName;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets cached cookie value.
        /// </summary>
        private ABCookieValue<TIdentifier> CookieValue
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
        /// Indicates whether is visitor included in A/B test or not.
        /// </summary>
        public bool IsExcluded
        {
            get
            {
                return CookieValue?.ExcludedFromTest ?? false;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes a new instance of <see cref="ABUserStateManager{TIdentifier}"/>.
        /// </summary>
        /// <param name="testName">Name of the test for which the user's state is managed</param>
        /// <exception cref="ArgumentException">testName is null or empty</exception>
        public ABUserStateManager(string testName)
            : this(testName, Service.Resolve<IABResponseCookieProvider>())
        {
        }


        /// <summary>
        /// Initializes a new instance of <see cref="ABUserStateManager{TIdentifier}"/>.
        /// </summary>
        internal ABUserStateManager(string testName, IABResponseCookieProvider cookieProvider)
        {
            if (String.IsNullOrEmpty(testName))
            {
                throw new ArgumentException("Test name cannot be null or empty.", nameof(testName));
            }

            mSessionKey = "ABConversions" + testName;
            mPermanentCookieName = ABTestConstants.AB_COOKIE_PREFIX + testName;
            mPermanentExpiration = DateTime.Now.AddMonths(2);
            this.cookieProvider = cookieProvider;
            this.testName = testName;
        }


        /// <summary>
        /// Sets a A/B test visit for current user so that from now on, conversions can be logged also as A/B test session conversions.
        /// </summary>
        /// <returns>True if visit was assigned successfully (wasn't already assigned). False if a visit is already set.</returns>
        public bool SetVisit()
        {
            if (IsABVisit())
            {
                return false;
            }

            if (!ABVisitRequestHelper.SetABVisitRequestTestName(testName))
            {
                SetVisitToSession();
            }

            return true;
        }


        /// <summary>
        /// Sets current A/B test visit directly to the session store.
        /// </summary>
        public void SetVisitToSession()
        {
            if (IsABVisit())
            {
                return;
            }

            SessionHelper.SetValue(mSessionKey, new List<string>());
        }


        /// <summary>
        /// Returns true if user has visited A/B test page and has visit session.
        /// </summary>
        public bool IsABVisit()
        {
            return (SessionHelper.GetValue(mSessionKey) is List<string>);
        }


        /// <summary>
        /// Gets name of the A/B variant assigned to the current user. Returns null, if the user hasn't got any variant assigned yet.
        /// </summary>
        /// <returns>Code name of the variant that is assigned to the current user</returns>
        public TIdentifier GetVariantIdentifier()
        {
            if (CookieValue != null)
            {
                return CookieValue.VariantIdentifier;
            }
            return default(TIdentifier);
        }


        /// <summary>
        /// Assigns an A/B variant to current user so that it can be used on next visit. From now on, permanent conversions can be also saved to
        /// user's state.
        /// </summary>
        /// <param name="variantIdentifier">Identifier of the variant to assign to current user.</param>
        public void AssignVariant(TIdentifier variantIdentifier)
        {
            if (CookieValue != null)
            {
                if (!(CookieValue.VariantIdentifier?.Equals(variantIdentifier) ?? (variantIdentifier == null)))
                {
                    // Inconsistent cookie exists, override variant name
                    CookieValue.VariantIdentifier = variantIdentifier;
                    SetCookie(mPermanentCookieName, CookieValue, mPermanentExpiration);
                }
            }
            else
            {
                // Cookie does not exist, create a new cookie
                CookieValue = new ABCookieValue<TIdentifier> { VariantIdentifier = variantIdentifier };
                SetCookie(mPermanentCookieName, CookieValue, mPermanentExpiration);
            }
        }


        /// <summary>
        /// Excludes visitor from A/B test.
        /// </summary>
        public void Exclude()
        {
            CookieValue = new ABCookieValue<TIdentifier> { ExcludedFromTest = true };
            SetCookie(mPermanentCookieName, CookieValue, mPermanentExpiration);
        }


        /// <summary>
        /// Gets permanent conversions that are done by current user.
        /// </summary>
        /// <returns>Permanent conversions that are done by current user</returns>
        public IEnumerable<string> GetPermanentConversions()
        {
            return CookieValue?.Conversions ?? Enumerable.Empty<string>();
        }


        /// <summary>
        /// Gets session conversions that are done by current user.
        /// </summary>
        /// <returns>Session conversions that are done by current user</returns>
        public IEnumerable<string> GetSessionConversions()
        {
            if (SessionHelper.GetValue(mSessionKey) is List<string> sessionConversions)
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
                throw new ArgumentException("Conversion name can't be null or empty.", nameof(conversion));
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
            if (CookieValue != null && !CookieValue.Conversions.Contains(conversion))
            {
                CookieValue.Conversions.Add(conversion);
                SetCookie(mPermanentCookieName, CookieValue, mPermanentExpiration);
            }
        }


        /// <summary>
        /// Adds specified conversion to session cookie.
        /// </summary>
        /// <param name="conversion">Conversion name</param>
        private void TryAddSessionConversion(string conversion)
        {
            if (SessionHelper.GetValue(mSessionKey) is List<string> sessionConversions && !sessionConversions.Contains(conversion))
            {
                sessionConversions.Add(conversion);
            }
        }


        /// <summary>
        /// Returns deserialized value of specified cookie.
        /// </summary>
        /// <param name="cookieName">Cookie name</param>
        private ABCookieValue<TIdentifier> GetCookieValue(string cookieName)
        {
            string serialized = cookieProvider.GetValue(cookieName);
            if (!String.IsNullOrWhiteSpace(serialized))
            {
                try
                {
                    return JsonConvert.DeserializeObject<ABCookieValue<TIdentifier>>(serialized);
                }
                catch
                {
                    // Ignore deserialization issues
                }
            }

            return null;
        }


        /// <summary>
        /// Serializes object to a cookie as a JSON object.
        /// </summary>
        /// <param name="cookieName">Name of the cookie to save object to</param>
        /// <param name="cookieValue">Object to serialize to the cookie</param>
        /// <param name="expiration">Expiration of the cookie</param>
        private void SetCookie(string cookieName, ABCookieValue<TIdentifier> cookieValue, DateTime expiration)
        {
            string serialized = JsonConvert.SerializeObject(cookieValue, Formatting.None, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });

            cookieProvider.SetValue(cookieName, serialized, null, expiration, null, null);
        }

        #endregion
    }
}
