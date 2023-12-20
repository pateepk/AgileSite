using CMS;
using CMS.Core;
using CMS.OnlineMarketing.Internal;

[assembly: RegisterImplementation(typeof(IABTestConversionLogger), typeof(ABTestConversionLogger), Priority = RegistrationPriority.SystemDefault)]

namespace CMS.OnlineMarketing.Internal
{
    /// <summary>
    /// Defines set of methods for logging A/B test conversions.
    /// </summary>
    public interface IABTestConversionLogger
    {
        /// <summary>
        /// Log A/B test conversion with given parameters. Actual logging of conversions is different for content only sites and other sites based on given <paramref name="siteName"/>.
        /// </summary>
        /// <typeparam name="TIdentifier">Type of the A/B variant identifier in <see cref="ABUserStateManager{TIdentifier}"/>.</typeparam>
        /// <param name="requestDomain">Domain of current HTTP request.</param>
        /// <param name="siteName">Site name where conversion occurred.</param>
        /// <param name="culture">Culture of page where conversion occurred.</param>
        /// <param name="conversionName">Name of conversion registered in system.</param>
        /// <param name="itemIdentifier">Identifier of object connected to conversion.</param>
        /// <param name="objectId">ID of object connected with conversion.</param>
        /// <param name="count">Number of conversions that occurred.</param>
        /// <param name="value">Conversion value.</param>
        /// <seealso cref="ABTestConversionDefinition.ConversionName"/>
        /// <seealso cref="ABTestConversionDefinitionRegister.Register"/>
        void LogConversion<TIdentifier>(string requestDomain, string siteName, string culture, string conversionName, string itemIdentifier, int objectId, int count, double value);
    }
}
