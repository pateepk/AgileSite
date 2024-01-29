using CMS;
using CMS.Core;
using CMS.OnlineMarketing.Web.UI;

[assembly: RegisterImplementation(typeof(IABVariantPerformanceCalculator), typeof(ABVariantPerformanceCalculator), Priority = CMS.Core.RegistrationPriority.Fallback, Lifestyle = Lifestyle.Transient)]

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Interface for calculating performance of AB variants.
    /// </summary>
    public interface IABVariantPerformanceCalculator
    {
        /// <summary>
        /// Initializes AB variant performance calculator.
        /// Needs to be called first, as this class needs the original's conversions and visits to work properly.
        /// </summary>
        /// <param name="originalConversions">Number of original conversions</param>
        /// <param name="originalVisits">Number of original visits</param>
        void Initialize(int originalConversions, int originalVisits);


        /// <summary>
        /// Returns chance to beat original for the specified challenger variant.
        /// </summary>
        /// <param name="challengerConversions">Number of challenger conversions</param>
        /// <param name="challengerVisits">Number of challenger visits</param>
        double GetChanceToBeatOriginal(int challengerConversions, int challengerVisits);


        /// <summary>
        /// Returns conversion rate interval for the specified variant.
        /// </summary>
        /// <param name="variantConversions">Number of conversions</param>
        /// <param name="variantVisits">Number of visits</param>
        ABConversionRateInterval GetConversionRateInterval(int variantConversions, int variantVisits);
    }
}
