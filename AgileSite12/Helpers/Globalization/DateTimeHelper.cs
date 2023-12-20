using System;
using System.Globalization;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Methods to work with the DateTime.
    /// </summary>
    public class DateTimeHelper : CoreMethods
    {
        #region "Variables"

        /// <summary>
        /// Zero time constant.
        /// </summary>
        public static readonly DateTime ZERO_TIME = DateTime.MinValue;

        /// <summary>
        /// Start date/time for the Unix time stamp
        /// </summary>
        public static readonly DateTime UNIX_TIME_START = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


        /// <summary>
        /// Macro representing current date and time.
        /// </summary>
        public const string MACRO_TIME_NOW = "##NOW##";


        /// <summary>
        /// Macro representing current date.
        /// </summary>
        public const string MACRO_DATE_TODAY = "##TODAY##";

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets current date without seconds.
        /// </summary>
        /// <param name="date">Date</param>
        public static DateTime GetMinuteStart(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
        }


        /// <summary>
        /// Gets current date without minutes and seconds.
        /// </summary>
        /// <param name="date">Date</param>
        public static DateTime GetHourStart(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
        }


        /// <summary>
        /// Gets start of the day of specific date.
        /// </summary>
        /// <param name="date">Date</param>
        public static DateTime GetDayStart(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day);
        }


        /// <summary>
        /// Gets the date from the beginning of week.
        /// </summary>
        /// <param name="date">Date of week</param>
        public static DateTime GetWeekStart(DateTime date)
        {
            return GetWeekStart(date, null);
        }


        /// <summary>
        /// Gets the date from the beginning of week.
        /// </summary>
        /// <param name="date">Date of week</param>
        /// <param name="culture">Culture</param>
        public static DateTime GetWeekStart(DateTime date, string culture)
        {
            DateTime result = new DateTime(date.Year, date.Month, date.Day);
            DayOfWeek firstDay = GetFirstDayOfWeek(culture);

            while (result.DayOfWeek != firstDay)
            {
                result = result.AddDays(-1);
            }

            return result;
        }


        /// <summary>
        /// Gets the first day of the month from specific date.
        /// </summary>
        /// <param name="date">Date</param>
        public static DateTime GetMonthStart(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }


        /// <summary>
        /// Gets the first day of the year from specific date.
        /// </summary>
        /// <param name="date">Date</param>
        public static DateTime GetYearStart(DateTime date)
        {
            return new DateTime(date.Year, 1, 1);
        }


        /// <summary>
        /// Gets the first day of the week.
        /// </summary>
        public static DayOfWeek GetFirstDayOfWeek()
        {
            return GetFirstDayOfWeek(null);
        }


        /// <summary>
        /// Gets the first day of the week.
        /// </summary>
        /// <param name="culture">Culture</param>
        public static DayOfWeek GetFirstDayOfWeek(string culture)
        {
            CultureInfo ci = CultureHelper.GetCultureInfo(culture);
            return ci.DateTimeFormat.FirstDayOfWeek;
        }


        /// <summary>
        /// Gets the week of year number.
        /// </summary>
        /// <param name="time">DateTime value</param>
        public static int GetWeekOfYear(DateTime time)
        {
            return GetWeekOfYear(time, null);
        }


        /// <summary>
        /// Gets the week of year number.
        /// </summary>
        /// <param name="time">DateTime value</param>
        /// <param name="culture">Culture</param>
        public static int GetWeekOfYear(DateTime time, string culture)
        {
            CultureInfo ci = CultureHelper.GetCultureInfo(culture);

            return ci.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, GetFirstDayOfWeek(culture));
        }


        /// <summary>
        /// Gets number of weeks in specified time interval.
        /// </summary>
        /// <param name="dateFrom">Time from</param>
        /// <param name="dateTo">Time to</param>
        public static int NumberOfWeeks(DateTime dateFrom, DateTime dateTo)
        {
            return NumberOfWeeks(dateFrom, dateTo, null);
        }


        /// <summary>
        /// Gets number of weeks in specified time interval.
        /// </summary>
        /// <param name="dateFrom">Time from</param>
        /// <param name="dateTo">Time to</param>
        /// <param name="culture">Culture</param>
        public static int NumberOfWeeks(DateTime dateFrom, DateTime dateTo, string culture)
        {
            dateFrom = GetWeekStart(dateFrom, culture);
            TimeSpan span = dateTo.Subtract(dateFrom);
            return 1 + span.Days / 7;
        }


        /// <summary>
        /// Returns validity enumeration.
        /// </summary>
        /// <param name="validity">Validity</param>
        public static ValidityEnum GetValidityEnum(string validity)
        {
            if (String.IsNullOrEmpty(validity))
            {
                return ValidityEnum.Until;
            }

            switch (validity.ToLowerInvariant())
            {
                case "days":
                    return ValidityEnum.Days;

                case "weeks":
                    return ValidityEnum.Weeks;

                case "months":
                    return ValidityEnum.Months;

                case "years":
                    return ValidityEnum.Years;

                case "until":
                    return ValidityEnum.Until;

                default:
                    return ValidityEnum.Until;
            }
        }


        /// <summary>
        /// Returns validity enumeration as string.
        /// </summary>
        /// <param name="validity">Validity</param>
        public static string GetValidityString(ValidityEnum validity)
        {
            switch (validity)
            {
                case ValidityEnum.Days:
                    return "DAYS";

                case ValidityEnum.Weeks:
                    return "WEEKS";

                case ValidityEnum.Months:
                    return "MONTHS";

                case ValidityEnum.Years:
                    return "YEARS";

                case ValidityEnum.Until:
                    return "UNTIL";

                default:
                    return "UNTIL";
            }
        }


        /// <summary>
        /// Returns validity as formatted string.
        /// </summary>
        /// <param name="validity">Validity type</param>
        /// <param name="validFor">Multiplier for time period set by validity type</param>
        /// <param name="validUntil">Valid until date and time</param>
        public static string GetFormattedValidity(ValidityEnum validity, int validFor, DateTime validUntil)
        {
            switch (validity)
            {
                case ValidityEnum.Days:
                    // Valid for n days
                    return String.Format(GetString("general.validity.days"), validFor);

                case ValidityEnum.Weeks:
                    // Valid for n weeks
                    return String.Format(GetString("general.validity.weeks"), validFor);

                case ValidityEnum.Months:
                    // Valid for n months
                    return String.Format(GetString("general.validity.months"), validFor);

                case ValidityEnum.Years:
                    // Valid for n years
                    return String.Format(GetString("general.validity.years"), validFor);

                case ValidityEnum.Until:
                    if (validUntil == ZERO_TIME)
                    {
                        // Unlimited validity
                        return GetString("general.validity.unlimited");
                    }
                    else
                    {
                        // Valid until specific date
                        return String.Format(GetString("general.validity.until"), validUntil);
                    }

                default:
                    return null;
            }
        }


        /// <summary>
        /// Calculate valid to date and time according to given parameters.
        /// </summary>
        /// <param name="orignalValidTo">Original valid to date and time</param>
        /// <param name="unitsType">Type of time units to add or subtract</param>
        /// <param name="units">Time units count</param>
        public static DateTime GetValidTo(DateTime orignalValidTo, ValidityEnum unitsType, int units)
        {
            DateTime newValidTo = ZERO_TIME;

            // Calculate new valid to
            switch (unitsType)
            {
                case ValidityEnum.Days:
                    newValidTo = orignalValidTo.AddDays(units);
                    break;

                case ValidityEnum.Weeks:
                    newValidTo = orignalValidTo.AddDays(7 * units);
                    break;

                case ValidityEnum.Months:
                    newValidTo = orignalValidTo.AddMonths(units);
                    break;

                case ValidityEnum.Years:
                    newValidTo = orignalValidTo.AddYears(units);
                    break;
            }

            return newValidTo;
        }


        /// <summary>
        /// Returns true if DateTime "from" is lower than or equal to DateTime "to" or one of them is equal to ZeroTime.
        /// </summary>
        /// <param name="from">DateTime From</param>
        /// <param name="to">DateTime To</param>
        public static bool IsValidFromTo(DateTime from, DateTime to)
        {
            return ((from <= to) || (to == ZERO_TIME));
        }


        /// <summary>
        /// Returns true, if the given value is ##NOW## or ##TODAY## macro
        /// </summary>
        /// <param name="value">Value to check</param>
        public static bool IsNowOrToday(string value)
        {
            return !string.IsNullOrEmpty(value) &&
                (value.Equals(MACRO_DATE_TODAY, StringComparison.OrdinalIgnoreCase) ||
                value.Equals(MACRO_TIME_NOW, StringComparison.OrdinalIgnoreCase));
        }

        #endregion
    }
}