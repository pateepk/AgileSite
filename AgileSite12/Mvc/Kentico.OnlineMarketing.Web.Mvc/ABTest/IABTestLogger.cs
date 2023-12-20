using CMS.OnlineMarketing;

namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Defines method for logging A/B test conversions.
    /// </summary>
    public interface IABTestLogger
    {
        /// <summary>
        /// Logs A/B test conversion.
        /// </summary>
        /// <param name="conversionName">Name of conversion registered in system.</param>
        /// <param name="siteName">Site name where conversion occurred.</param>
        /// <param name="itemIdentifier">Identifier of object connected to conversion.</param>
        /// <param name="defaultValue">Default value used for logging. This value has lower priority than the conversion default value defined in the A/B testing application.</param>
        /// <param name="count">Number of conversions that occurred - 1 by default.</param>
        /// <param name="culture">Culture name connected to conversion - current thread culture is used if not specified.</param>
        /// <seealso cref="ABTestConversionDefinition.ConversionName"/>
        /// <seealso cref="ABTestConversionDefinitionRegister.Register"/>
        void LogConversion(string conversionName, string siteName, string itemIdentifier = null, decimal defaultValue = 1m, int count = 1, string culture = null);
    }
}
