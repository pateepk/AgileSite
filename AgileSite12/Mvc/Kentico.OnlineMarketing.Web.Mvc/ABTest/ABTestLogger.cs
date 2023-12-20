using System;
using System.Threading;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.OnlineMarketing;
using CMS.OnlineMarketing.Internal;

using Kentico.OnlineMarketing.Web.Mvc;

[assembly: RegisterImplementation(typeof(IABTestLogger), typeof(ABTestLogger), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Class for managing A/B test conversions logging.
    /// </summary>
    public class ABTestLogger : IABTestLogger
    {
        private IHttpContextAccessor HttpContextAccessor
        {
            get;
        }


        private IABTestConversionLogger ABTestConversionLogger
        {
            get;
        }


        /// <summary>
        /// Initializes a new instance of <see cref="ABTestLogger"/>.
        /// </summary>
        /// <param name="httpContextAccessor">Instance of accessor of HTTP context.</param>
        /// <param name="abTestConversionLogger">Instance of logger for A/B test conversions.</param>
        public ABTestLogger(IHttpContextAccessor httpContextAccessor, IABTestConversionLogger abTestConversionLogger)
        {
            HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            ABTestConversionLogger = abTestConversionLogger ?? throw new ArgumentNullException(nameof(abTestConversionLogger));
        }


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
        public void LogConversion(string conversionName, string siteName, string itemIdentifier = null, decimal defaultValue = 1m, int count = 1, string culture = null)
        {
            var requestDomain = HttpContextAccessor.HttpContext?.Request?.Url.Host ?? String.Empty;

            ABTestConversionLogger.LogConversion<Guid?>(requestDomain, siteName, culture ?? Thread.CurrentThread.CurrentCulture.Name, conversionName, itemIdentifier, 0, count, Decimal.ToDouble(defaultValue));
        }
    }
}
