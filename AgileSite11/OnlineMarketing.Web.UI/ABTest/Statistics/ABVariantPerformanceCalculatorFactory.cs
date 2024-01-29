using CMS.Core;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Class that provides IABVariantPerformanceCalculator implementation.
    /// </summary>
    public static class ABVariantPerformanceCalculatorFactory
    {
        #region "Static methods"

        /// <summary>
        /// Returns new instance of IABVariantPerformanceCalculator implementation.
        /// </summary>
        /// <param name="controlConversions">Number of control conversions</param>
        /// <param name="controlVisits">Number of control visits</param>
        public static IABVariantPerformanceCalculator GetImplementation(int controlConversions, int controlVisits)
        {
            var implementation = Service.Resolve<IABVariantPerformanceCalculator>();
            implementation.Initialize(controlConversions, controlVisits);
            return implementation;
        }

        #endregion
    }
}