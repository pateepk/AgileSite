using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// HitsInfo data container class.
    /// </summary>
    public class HitsInfo : AbstractInfo<HitsInfo>
    {
        /// <summary>
        /// Hits ID.
        /// </summary>
        public virtual int HitsID
        {
            get
            {
                return GetIntegerValue("HitsID", 0);
            }
            set
            {
                SetValue("HitsID", value);
            }
        }


        /// <summary>
        /// Hits statistics ID.
        /// </summary>
        public virtual int HitsStatisticsID
        {
            get
            {
                return GetIntegerValue("HitsStatisticsID", 0);
            }
            set
            {
                SetValue("HitsStatisticsID", value);
            }
        }


        /// <summary>
        /// Hits start time.
        /// </summary>
        public virtual DateTime HitsStartTime
        {
            get
            {
                return GetDateTimeValue("HitsStartTime", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("HitsStartTime", value);
            }
        }


        /// <summary>
        /// Hits end time.
        /// </summary>
        public virtual DateTime HitsEndTime
        {
            get
            {
                return GetDateTimeValue("HitsEndTime", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("HitsEndTime", value);
            }
        }


        /// <summary>
        /// Hits count.
        /// </summary>
        public virtual int HitsCount
        {
            get
            {
                return GetIntegerValue("HitsCount", 0);
            }
            set
            {
                SetValue("HitsCount", value);
            }
        }


        /// <summary>
        /// Converts enum to database name.
        /// </summary>
        public static string HitsIntervalEnumString(HitsIntervalEnum interval)
        {
            switch (interval)
            {
                case HitsIntervalEnum.Hour:
                    return "analytics.hitshour";

                case HitsIntervalEnum.Day:
                    return "analytics.hitsday";

                case HitsIntervalEnum.Week:
                    return "analytics.hitsweek";

                case HitsIntervalEnum.Month:
                    return "analytics.hitsmonth";

                case HitsIntervalEnum.Year:
                    return "analytics.hitsyear";

                default:
                    return "";
            }
        }


        /// <summary>
        /// Converts enum to table name.
        /// </summary>
        public static string HitsIntervalEnumTableName(HitsIntervalEnum interval)
        {
            switch (interval)
            {
                case HitsIntervalEnum.Hour:
                    return "analytics_hourhits";

                case HitsIntervalEnum.Day:
                    return "analytics_dayhits";

                case HitsIntervalEnum.Week:
                    return "analytics_weekhits";

                case HitsIntervalEnum.Month:
                    return "analytics_monthhits";

                case HitsIntervalEnum.Year:
                    return "analytics_yearhits";

                default:
                    return "";
            }
        }


        /// <summary>
        /// Constructor - Creates new object of HitsInfo with the year granularity
        /// </summary>
        public HitsInfo()
            : this(HitsIntervalEnum.Year)
        {
        }


        /// <summary>
        /// Constructor - Creates new object of HitsInfo based on the enum.
        /// </summary>
        public HitsInfo(HitsIntervalEnum interval)
        {
            DataClass = DataClassFactory.NewDataClass(HitsIntervalEnumString(interval));
        }


        /// <summary>
        /// Constructor - Creates a new HitsInfo object from the given DataRow.
        /// </summary>
        public HitsInfo(DataRow dr, HitsIntervalEnum interval)
        {
            DataClass = DataClassFactory.NewDataClass(HitsIntervalEnumString(interval), dr);
        }


        /// <summary>
        /// Sets the Hits start time and end time to the interval matching the given time.
        /// If interval changed, clears the ID of the object.
        /// </summary>
        /// <param name="time">Time</param>
        public void LoadTime(DateTime time)
        {
            if (HitsEndTime <= time)
            {
                HitsID = 0;
                HitsCount = 0;
            }

            switch (ClassName.ToLowerCSafe())
            {
                case "analytics.hitshour":
                    // Get beginnig of the current hour
                    HitsStartTime = DateTimeHelper.GetHourStart(time);
                    // Add 1 hour
                    HitsEndTime = HitsStartTime.AddHours(1);
                    break;

                case "analytics.hitsday":
                    // Get the beginning of the current day
                    HitsStartTime = DateTimeHelper.GetDayStart(time);
                    // Add 1 day
                    HitsEndTime = HitsStartTime.AddDays(1);
                    break;

                case "analytics.hitsweek":
                    // Get the beginning of the current week
                    HitsStartTime = DateTimeHelper.GetWeekStart(time);
                    // Add 1 week
                    HitsEndTime = HitsStartTime.AddDays(7);
                    break;

                case "analytics.hitsmonth":
                    // Get the first day of the current month
                    HitsStartTime = DateTimeHelper.GetMonthStart(time);
                    // Add 1 month
                    HitsEndTime = HitsStartTime.AddMonths(1);
                    break;

                case "analytics.hitsyear":
                    // Get the first day of the current year
                    HitsStartTime = DateTimeHelper.GetYearStart(time);
                    // Add 1 year
                    HitsEndTime = HitsStartTime.AddYears(1);
                    break;

                case "":
                    break;
            }
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            HitsInfoProvider.SetHitsInfo(this);
        }
    }
}