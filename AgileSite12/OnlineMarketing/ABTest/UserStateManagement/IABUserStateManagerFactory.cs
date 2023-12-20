using System;

namespace CMS.OnlineMarketing.Internal
{
    /// <summary>
    /// Factory for <see cref="IABUserStateManager{TIdentifier}"/>.
    /// </summary>
    public interface IABUserStateManagerFactory
    {
        /// <summary>
        /// Creates a new user state manager for A/B test identified by <paramref name="abTestName"/>.
        /// </summary>
        /// <typeparam name="TIdentifier">Type of the A/B variant identifier.</typeparam>
        /// <param name="abTestName">Name of A/B test to create a manager for.</param>
        /// <returns>Returns a new instance of A/B test manager for a test.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="abTestName"/> is null or an empty string.</exception>
        IABUserStateManager<TIdentifier> Create<TIdentifier>(string abTestName);
    }
}
