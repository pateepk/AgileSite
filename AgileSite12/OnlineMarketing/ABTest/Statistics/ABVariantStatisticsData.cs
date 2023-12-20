using System;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Class that holds AB variant's statistics data.
    /// </summary>
    public sealed class ABVariantStatisticsData
    {
        #region "Properties"

        /// <summary>
        /// Gets conversions count.
        /// </summary>
        public int ConversionsCount
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets visits count.
        /// </summary>
        public int Visits
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets conversions value.
        /// </summary>
        public double ConversionsValue
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets conversion rate.
        /// </summary>
        public double ConversionRate
        {
            get
            {
                if (Visits == 0)
                {
                    return 0;
                }

                return (double)ConversionsCount / Visits;
            }
        }


        /// <summary>
        /// Gets average conversion value.
        /// </summary>
        public double AverageConversionValue
        {
            get
            {
                if (ConversionsCount == 0)
                {
                    return 0;
                }

                return ConversionsValue / ConversionsCount;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="conversionsCount">Conversions count</param>
        /// <param name="conversionsValue">Conversions value</param>
        /// <param name="visits">Visits count</param>
        public ABVariantStatisticsData(int conversionsCount, double conversionsValue, int visits)
        {
            if (conversionsCount < 0)
            {
                throw new ArgumentException("[ABVariantStatisticsData.Constructor]: Conversions count can not be negative.", "conversionsCount");
            }
            if (visits < 0)
            {
                throw new ArgumentException("[ABVariantStatisticsData.Constructor]: Visits count can not be negative.", "visits");
            }

            ConversionsCount = conversionsCount;
            ConversionsValue = conversionsValue;
            Visits = visits;
        }

        #endregion
    }
}