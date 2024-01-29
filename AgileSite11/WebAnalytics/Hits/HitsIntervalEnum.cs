using System;

using CMS.Base;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Hits interval type enumeration.
    /// </summary>
    public enum HitsIntervalEnum
    {
        /// <summary>
        /// Year interval.
        /// </summary>
        Year = 0,

        /// <summary>
        /// Month interval.
        /// </summary>
        Month = 1,

        /// <summary>
        /// Week interval.
        /// </summary>
        Week = 2,

        /// <summary>
        /// Day interval.
        /// </summary>
        Day = 3,

        /// <summary>
        /// Hour interval.
        /// </summary>
        Hour = 4,

        /// <summary>
        /// No interval
        /// </summary>
        None = 5
    }


    /// <summary>
    /// Class to provider safe conversion.
    /// </summary>
    public static class HitsIntervalEnumFunctions
    {
        /// <summary>
        /// Converts string to HitsIntervalEnum.
        /// </summary>
        /// <param name="value">Value to convert</param>
        public static HitsIntervalEnum StringToHitsConversion(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return HitsIntervalEnum.None;
            }

            switch (value.ToLowerCSafe())
            {
                case "hour":
                    return HitsIntervalEnum.Hour;

                case "day":
                    return HitsIntervalEnum.Day;

                case "week":
                    return HitsIntervalEnum.Week;

                case "month":
                    return HitsIntervalEnum.Month;

                case "year":
                    return HitsIntervalEnum.Year;

                case "none":
                    return HitsIntervalEnum.None;

                default:
                    return HitsIntervalEnum.None;
            }
        }


        /// <summary>
        /// Converts HitsIntervalEnum to string.
        /// </summary>
        /// <param name="value">Value to convert</param>
        public static string HitsConversionToString(HitsIntervalEnum value)
        {
            switch (value)
            {
                case HitsIntervalEnum.Hour:
                    return "hour";

                case HitsIntervalEnum.Day:
                    return "day";

                case HitsIntervalEnum.Week:
                    return "week";

                case HitsIntervalEnum.Month:
                    return "month";

                case HitsIntervalEnum.Year:
                    return "year";

                case HitsIntervalEnum.None:
                    return "none";

                default:
                    return "none";
            }
        }
    }
}