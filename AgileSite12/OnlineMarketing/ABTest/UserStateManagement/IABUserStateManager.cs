using System;
using System.Collections.Generic;

using CMS.Helpers;

namespace CMS.OnlineMarketing.Internal
{
    /// <summary>
    /// Manages current user's A/B test state - can assign an A/B variant to him, save which conversions he has done, or read saved state. Internally uses a cookie for permanent state
    /// (see <see cref="ABCookieValue{TIdentifier}"/>) and session for session state(see <see cref="SessionHelper"/>).
    /// </summary>
    /// <typeparam name="TIdentifier">Type of the A/B variant identifier.</typeparam>
    public interface IABUserStateManager<TIdentifier>
    {
        /// <summary>
        /// Indicates whether is visitor included in A/B test or not.
        /// </summary>
        bool IsExcluded { get; }


        /// <summary>
        /// Sets a A/B test visit for current user so that from now on, conversions can be logged also as A/B test session conversions.
        /// </summary>
        /// <returns>True if visit was assigned successfully (wasn't already assigned). False if a visit is already set.</returns>
        bool SetVisit();


        /// <summary>
        /// Sets current A/B test visit to the session store.
        /// </summary>
        void SetVisitToSession();


        /// <summary>
        /// Returns true if user has visited A/B test page and has visit session.
        /// </summary>
        bool IsABVisit();


        /// <summary>
        /// Gets name of the A/B variant assigned to the current user. Returns null, if the user hasn't got any variant assigned yet.
        /// </summary>
        /// <returns>Code name of the variant that is assigned to the current user</returns>
        TIdentifier GetVariantIdentifier();


        /// <summary>
        /// Assigns an A/B variant to current user so that it can be used on next visit. From now on, permanent conversions can be also saved to
        /// user's state.
        /// </summary>
        /// <param name="variantIdentifier">Identifier of the variant to assign to current user.</param>
        void AssignVariant(TIdentifier variantIdentifier);


        /// <summary>
        /// Excludes visitor from A/B test.
        /// </summary>
        void Exclude();


        /// <summary>
        /// Gets permanent conversions that are done by current user.
        /// </summary>
        /// <returns>Permanent conversions that are done by current user</returns>
        IEnumerable<string> GetPermanentConversions();


        /// <summary>
        /// Gets session conversions that are done by current user.
        /// </summary>
        /// <returns>Session conversions that are done by current user</returns>
        IEnumerable<string> GetSessionConversions();


        /// <summary>
        /// Adds a conversion to session and permanent conversions if possible.
        /// </summary>
        /// <param name="conversion">Conversion to add to current user</param>
        /// <exception cref="ArgumentException">conversion is null or empty</exception>
        void AddConversion(string conversion);


        /// <summary>
        /// Returns true when A/B test cookie is defined.
        /// </summary>
        bool IsABTestCookieDefined();
    }
}
