using System;
using System.Web.UI.DataVisualization.Charting;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Class calculating conversion rate interval and challengers' chance to beat original using the Normal Distribution Function at 90% significance.
    /// </summary>
    public class ABVariantPerformanceCalculator : IABVariantPerformanceCalculator
    {
        #region "Variables"

        /// <summary>
        /// Represents 80% significance.
        /// </summary>
        private const double SIGNIFICANCE = 0.841621234d;

        private int mOriginalConversions;
        private int mOriginalVisits;
        private double mOriginalConversionRate;
        private double mOriginalStandardError;
        private bool mIsInitialized;

        #endregion


        #region "Public methods"

        /// <summary>
        /// Initializes ABVariantPerformanceCalculator.
        /// Needs to be called first, as this class needs the original's conversions and visits to work properly.
        /// </summary>
        /// <param name="originalConversions">Number of original conversions</param>
        /// <param name="originalVisits">Number of original visits</param>
        /// <exception cref="ArgumentException"><paramref name="originalConversions"/> is negative or <paramref name="originalVisits"/> is zero or negative</exception>
        public virtual void Initialize(int originalConversions, int originalVisits)
        {
            if (originalConversions < 0)
            {
                throw new ArgumentException("[ABVariantPerformanceCalculator.Constructor]: Conversion count can not be negative.", "originalConversions");
            }
            if (originalVisits <= 0)
            {
                throw new ArgumentException("[ABVariantPerformanceCalculator.Constructor]: Visits count can not be zero or negative.", "originalVisits");
            }

            mOriginalConversions = originalConversions;
            mOriginalVisits = originalVisits;

            mOriginalConversionRate = GetConversionRate(mOriginalConversions, mOriginalVisits);
            mOriginalStandardError = GetStandardError(mOriginalConversionRate, mOriginalVisits);

            mIsInitialized = true;
        }


        /// <summary>
        /// Returns challenging variant's chance to beat original.
        /// </summary>
        /// <param name="challengerConversions">Number of challenger conversions</param>
        /// <param name="challengerVisits">Number of challenger visits</param>
        /// <exception cref="InvalidOperationException">ABVariantPerformanceCalculator hasn't been initialized. Call Initialize method first.</exception>
        /// <exception cref="ArgumentException"><paramref name="challengerConversions"/> is negative or <paramref name="challengerVisits"/> is zero or negative</exception>
        public virtual double GetChanceToBeatOriginal(int challengerConversions, int challengerVisits)
        {
            if (!mIsInitialized)
            {
                throw new InvalidOperationException("[ABVariantPerformanceCalculator.GetChanceToBeatOriginal]: This object is not initialized. Use Initialize method to initialize it.");
            }
            if (challengerConversions < 0)
            {
                throw new ArgumentException("[ABVariantPerformanceCalculator.GetChanceToBeatOriginal]: Conversion count can not be negative.", "challengerConversions");
            }
            if (challengerVisits <= 0)
            {
                throw new ArgumentException("[ABVariantPerformanceCalculator.GetChanceToBeatOriginal]: Visits count can not be zero or negative.", "challengerVisits");
            }

            double challengerConversionRate = GetConversionRate(challengerConversions, challengerVisits);
            double challengerStandardError = GetStandardError(challengerConversionRate, challengerVisits);

            double challengerZScore = GetVariantZScore(challengerConversionRate, challengerStandardError);

            // There is not NormalDistribution function in System.Math, so we must use chart function
            return 1 - new Chart().DataManipulator.Statistics.NormalDistribution(challengerZScore);
        }


        /// <summary>
        /// Returns AB conversion rate interval for specified variant.
        /// </summary>
        /// <param name="variantConversions">Number of variant conversions</param>
        /// <param name="variantVisits">Number of variant visits</param>
        /// <exception cref="InvalidOperationException">ABVariantPerformanceCalculator hasn't been initialized. Call Initialize method first.</exception>
        /// <exception cref="ArgumentException"><paramref name="variantConversions"/> is negative or <paramref name="variantVisits"/> is zero or negative</exception>
        public virtual ABConversionRateInterval GetConversionRateInterval(int variantConversions, int variantVisits)
        {
            if (!mIsInitialized)
            {
                throw new InvalidOperationException("[ABVariantPerformanceCalculator.GetConversionRateInterval]: This object is not initialized. Use Initialize method to initialize it.");
            }
            if (variantConversions < 0)
            {
                throw new ArgumentException("[ABVariantPerformanceCalculator.GetConversionRateInterval]: Conversion count can not be negative.", "variantConversions");
            }
            if (variantVisits <= 0)
            {
                throw new ArgumentException("[ABVariantPerformanceCalculator.GetConversionRateInterval]: Visits count can not be zero or negative.", "variantVisits");
            }

            double conversionRate = GetConversionRate(variantConversions, variantVisits);
            double standardError = GetStandardError(conversionRate, variantVisits);

            double conversionRateLowerBound = Math.Min(Math.Max(0, conversionRate - (SIGNIFICANCE * standardError)), 1);
            double conversionRateUpperBound = Math.Min(conversionRate + (SIGNIFICANCE * standardError), 1);

            return new ABConversionRateInterval(conversionRateLowerBound, conversionRateUpperBound);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns Z-score for specified challenger's data compared to the original.
        /// </summary>
        /// <param name="variantConversionRate">Variant conversion rate</param>
        /// <param name="variantStandardError">Variant standard error</param>
        private double GetVariantZScore(double variantConversionRate, double variantStandardError)
        {
            return (mOriginalConversionRate - variantConversionRate) / Math.Sqrt(Math.Pow(mOriginalStandardError, 2) + Math.Pow(variantStandardError, 2));
        }


        /// <summary>
        /// Returns conversion rate for specified variant's data.
        /// </summary>
        /// <param name="conversions">Number of conversions</param>
        /// <param name="visits">Number of visits</param>
        private double GetConversionRate(int conversions, int visits)
        {
            return (double)conversions / visits;
        }


        /// <summary>
        /// Returns standard error for specified variant's data.
        /// </summary>
        /// <param name="conversionRate">Conversion rate</param>
        /// <param name="visits">Number of visits</param>
        private double GetStandardError(double conversionRate, int visits)
        {
            return Math.Sqrt(conversionRate * (1 - conversionRate) / visits);
        }

        #endregion
    }
}