using System;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Class encapsulating conversion rate interval data.
    /// </summary>
    public sealed class ABConversionRateInterval
    {
        #region "Properties"

        /// <summary>
        /// Returns lower bound of the conversion rate interval.
        /// </summary>
        public double ConversionRateLowerBound
        {
            get;
            private set;
        }


        /// <summary>
        /// Returns upper bound of the conversion rate interval.
        /// </summary>
        public double ConversionRateUpperBound
        {
            get;
            private set;
        }


        /// <summary>
        /// Returns conversions rate.
        /// </summary>
        public double Rate
        {
            get
            {
                return (ConversionRateLowerBound + ConversionRateUpperBound) / 2;
            }
        }


        /// <summary>
        /// Returns estimated conversion rate variance.
        /// </summary>
        public double EstimatedConversionRateVariance
        {
            get
            {
                return ConversionRateUpperBound - Rate;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="conversionRateLowerBound">Conversion rate lower bound. Argument must be larger than 0 and lower than 1.</param>
        /// <param name="conversionRateUpperBound">Conversion rate upper bound. Argument must be larger than 0 and lower than 1.</param>
        public ABConversionRateInterval(double conversionRateLowerBound, double conversionRateUpperBound)
        {
            if ((conversionRateLowerBound < 0) || (conversionRateLowerBound > 1))
            {
                throw new ArgumentException("[ABConversionRateInterval.Constructor]: Conversion rate lower bound is out of range. Argument must be larger than 0 and lower than 1.", "conversionRateLowerBound");
            }
            if ((conversionRateUpperBound < 0) || (conversionRateUpperBound > 1))
            {
                throw new ArgumentException("[ABConversionRateInterval.Constructor]: Conversion rate upper bound is out of range. Argument must be larger than 0 and lower than 1.", "conversionRateUpperBound");
            }
            if (conversionRateLowerBound > conversionRateUpperBound)
            {
                throw new ArgumentException("[ABConversionRateInterval.Constructor]: Conversion rate lower bound can not be larger than the upper bound.");
            }

            ConversionRateLowerBound = conversionRateLowerBound;
            ConversionRateUpperBound = conversionRateUpperBound;
        }


        /// <summary>
        /// Returns formatted interval as X % +- Y %.
        /// </summary>
        public override string ToString()
        {
            // Represent the plus-minus sign with &plusmn; character
            return String.Format("{0:P2} &plusmn; {1:P2}", Rate, EstimatedConversionRateVariance);
        }

        #endregion
    }
}