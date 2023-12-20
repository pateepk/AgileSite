using System;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;
using CMS.WebAnalytics;

namespace CMS.OnlineMarketing.Internal
{
    /// <summary>
    /// Class providing method for logging A/B test conversions.
    /// </summary>
    internal class ABTestConversionLogger : IABTestConversionLogger
    {
        private IABUserStateManagerFactory ABUserStateManagerFactory
        {
            get;
        }


        private IAnalyticsConsentProvider AnalyticsConsentProvider
        {
            get;
        }


        /// <summary>
        /// Initializes a new instance of <see cref="ABTestConversionLogger"/> with given <paramref name="abUserStateManagerFactory"/> and <paramref name="analyticsConsentProvider"/>.
        /// </summary>
        public ABTestConversionLogger(IABUserStateManagerFactory abUserStateManagerFactory, IAnalyticsConsentProvider analyticsConsentProvider)
        {
            ABUserStateManagerFactory = abUserStateManagerFactory ?? throw new ArgumentNullException(nameof(abUserStateManagerFactory));
            AnalyticsConsentProvider = analyticsConsentProvider ?? throw new ArgumentNullException(nameof(analyticsConsentProvider));
        }


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
        public void LogConversion<TIdentifier>(string requestDomain, string siteName, string culture, string conversionName, string itemIdentifier, int objectId, int count, double value)
        {
            var site = SiteInfoProvider.GetSiteInfo(siteName);
            if (site == null || !LicenseHelper.CheckFeature(requestDomain, FeatureEnum.ABTesting) || String.IsNullOrEmpty(conversionName) || !ABTestInfoProvider.ABTestingEnabled(siteName) || (!site.SiteIsContentOnly && !AnalyticsConsentProvider.HasConsentForLogging()))
            {
                return;
            }

            conversionName = ABTestConversionHelper.GetConversionFullName(conversionName, itemIdentifier);

            // Loop through A/B tests in user's cookies (must be materialized as the collection may be modified within the loop)
            foreach (var abTestName in ABTestHelper.GetUsersTests().ToList())
            {
                var abTestInfo = ABTestInfoProvider.GetABTestInfo(abTestName, siteName);
                if (abTestInfo == null || !ABTestStatusEvaluator.ABTestIsRunning(abTestInfo))
                {
                    continue;
                }

                var manager = ABUserStateManagerFactory.Create<TIdentifier>(abTestInfo.ABTestName);
                if (manager.IsExcluded)
                {
                    continue;
                }

                var variantIdentifier = ValidationHelper.GetString(manager.GetVariantIdentifier(), String.Empty);
                if (String.IsNullOrEmpty(variantIdentifier) || !IsConversionInABTest(abTestInfo, site, conversionName))
                {
                    continue;
                }

                var conversionValue = GetConversionValue(abTestInfo, site, conversionName, value);

                var codeNameSuffix = $"{abTestName};{variantIdentifier}";

                // Log first conversion if the conversion isn't saved as permanent
                if (!manager.GetPermanentConversions().Contains(conversionName))
                {
                    HitLogProvider.LogHit($"{ABTestConstants.ABSESSIONCONVERSION_FIRST};{codeNameSuffix}", siteName, culture, conversionName, objectId, count, conversionValue);
                }
                // Log recurring conversion if the conversion isn't saved as session
                else if (!manager.GetSessionConversions().Contains(conversionName) && manager.IsABVisit())
                {
                    HitLogProvider.LogHit($"{ABTestConstants.ABSESSIONCONVERSION_RECURRING};{codeNameSuffix}", siteName, culture, conversionName, objectId, count, conversionValue);
                }

                manager.AddConversion(conversionName);

                // Always log transaction conversion
                HitLogProvider.LogHit($"{ABTestConstants.ABCONVERSION};{codeNameSuffix}", siteName, culture, conversionName, objectId, count, conversionValue);
            }
        }


        private static bool IsConversionInABTest(ABTestInfo abTest, SiteInfo site, string conversionName)
        {
            // For portal engine sites, conversion should be always logged
            if (!site.SiteIsContentOnly)
            {
                return true;
            }

            return abTest.ABTestConversionConfiguration.TryGetConversion(conversionName, out _);
        }


        /// <summary>
        /// Returns value that should be used as value of given <paramref name="conversionName"/>.
        /// </summary>
        /// <param name="abTest">A/B test for which conversion is logged.</param>
        /// <param name="site">Related site info.</param>
        /// <param name="conversionName">Name of conversion for which value is returned.</param>
        /// <param name="value">Conversion value used for portal engine A/B tests, or fallback conversion value for content only A/B tests.</param>
        /// <returns>Conversion value.</returns>
        private static double GetConversionValue(ABTestInfo abTest, SiteInfo site, string conversionName, double value)
        {
            if (site.SiteIsContentOnly
                && abTest.ABTestConversionConfiguration.TryGetConversion(conversionName, out var testConversion)
                && !testConversion.Value.Equals(0.0m))
            {
                return ValidationHelper.GetDouble(testConversion.Value, 0.0);
            }

            return value;
        }
    }
}
