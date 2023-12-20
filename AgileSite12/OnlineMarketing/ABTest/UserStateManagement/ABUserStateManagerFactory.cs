using System;

using CMS;
using CMS.Core;
using CMS.OnlineMarketing.Internal;

[assembly: RegisterImplementation(typeof(IABUserStateManagerFactory), typeof(ABUserStateManagerFactory), Priority = RegistrationPriority.SystemDefault)]

namespace CMS.OnlineMarketing.Internal
{
    /// <summary>
    /// Factory for <see cref="IABUserStateManager{TIdentifier}"/>.
    /// </summary>
    internal class ABUserStateManagerFactory : IABUserStateManagerFactory
    {
        /// <summary>
        /// Creates a new user state manager for A/B test identified by <paramref name="abTestName"/>.
        /// </summary>
        /// <typeparam name="TIdentifier">Type of the A/B variant identifier.</typeparam>
        /// <param name="abTestName">Name of A/B test to create a manager for.</param>
        /// <returns>Returns a new instance of A/B test manager for a test.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="abTestName"/> is null or an empty string.</exception>
        public IABUserStateManager<TIdentifier> Create<TIdentifier>(string abTestName)
        {
            if (String.IsNullOrEmpty(abTestName))
            {
                throw new ArgumentException(nameof(abTestName));
            }

            return new ABUserStateManager<TIdentifier>(abTestName);
        }
    }
}
