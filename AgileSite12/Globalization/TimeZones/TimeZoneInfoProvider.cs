using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Globalization
{
    /// <summary>
    /// Class providing TimeZoneInfo management.
    /// </summary>
    public class TimeZoneInfoProvider : AbstractInfoProvider<TimeZoneInfo, TimeZoneInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public TimeZoneInfoProvider()
            : base(TimeZoneInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true,
                    Name = true
                })
        {
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns the TimeZoneInfo structure for the specified Id.
        /// </summary>
        /// <param name="timeZoneId">TimeZone id</param>
        public static TimeZoneInfo GetTimeZoneInfo(int timeZoneId)
        {
            return ProviderObject.GetInfoById(timeZoneId);
        }


        /// <summary>
        /// Returns the TimeZoneInfo structure for the specified name.
        /// </summary>
        /// <param name="timeZoneName">TimeZone name</param>
        public static TimeZoneInfo GetTimeZoneInfo(string timeZoneName)
        {
            return ProviderObject.GetInfoByCodeName(timeZoneName);
        }


        /// <summary>
        /// Returns DataSet with all time zones.
        /// </summary>
        public static ObjectQuery<TimeZoneInfo> GetTimeZones()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Sets (updates or inserts) specified timeZone.
        /// </summary>
        /// <param name="timeZone">TimeZone to set</param>
        public static void SetTimeZoneInfo(TimeZoneInfo timeZone)
        {
            ProviderObject.SetInfo(timeZone);
        }


        /// <summary>
        /// Deletes specified timeZone.
        /// </summary>
        /// <param name="infoObj">TimeZone object</param>
        public static void DeleteTimeZoneInfo(TimeZoneInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified timeZone.
        /// </summary>
        /// <param name="timeZoneId">TimeZone id</param>
        public static void DeleteTimeZoneInfo(int timeZoneId)
        {
            TimeZoneInfo infoObj = GetTimeZoneInfo(timeZoneId);
            DeleteTimeZoneInfo(infoObj);
        }


        /// <summary>
        /// Returns DateTime for specified rule.
        /// </summary>
        /// <param name="rule">Rule</param>
        public static DateTime CreateRuleDateTime(string rule)
        {
            return CreateRuleDateTime(rule, DateTime.Now.Year);
        }


        /// <summary>
        /// Returns DateTime for specified rule and year.
        /// </summary>
        /// <param name="rule">Rule</param>
        /// <param name="year">Year</param>
        public static DateTime CreateRuleDateTime(string rule, int year)
        {
            if (rule != null)
            {
                string[] val = rule.Split('|');
                if (val.Length == 7)
                {
                    // 0 = Month
                    // 1 = Day
                    // 2 = DayValue
                    // 3 = Condition
                    // 4 = Hour
                    // 5 = Minute
                    // 6 = Value

                    // Get Month
                    int month = 0;
                    switch (val[0])
                    {
                        case "JAN":
                            month = 1;
                            break;

                        case "FEB":
                            month = 2;
                            break;

                        case "MAR":
                            month = 3;
                            break;

                        case "APR":
                            month = 4;
                            break;

                        case "MAY":
                            month = 5;
                            break;

                        case "JUN":
                            month = 6;
                            break;

                        case "JUL":
                            month = 7;
                            break;

                        case "AUG":
                            month = 8;
                            break;

                        case "SEP":
                            month = 9;
                            break;

                        case "OCT":
                            month = 10;
                            break;

                        case "NOV":
                            month = 11;
                            break;

                        case "DEC":
                            month = 12;
                            break;
                    }

                    string weekDay = val[1];
                    int dayValue;
                    int day = 0;
                    DateTime finalDate;

                    // Select right condition
                    switch (val[3])
                    {
                        case "FIRST":
                            dayValue = 1;
                            finalDate = GetDay(weekDay, dayValue, month, year, 1);
                            day = finalDate.Day;
                            month = finalDate.Month;
                            break;

                        case "LAST":
                            dayValue = DateTime.DaysInMonth(year, month);
                            finalDate = GetDay(weekDay, dayValue, month, year, -1);
                            day = finalDate.Day;
                            month = finalDate.Month;
                            break;

                        case ">=":
                            dayValue = Convert.ToInt32(val[2]);
                            finalDate = GetDay(weekDay, dayValue, month, year, 1);
                            day = finalDate.Day;
                            month = finalDate.Month;
                            break;

                        case "<=":
                            dayValue = Convert.ToInt32(val[2]);
                            finalDate = GetDay(weekDay, dayValue, month, year, -1);
                            day = finalDate.Day;
                            month = finalDate.Month;
                            break;

                        case "=":
                            day = Convert.ToInt32(val[2]);
                            break;
                    }
                    if ((month != 0) || (day != 0))
                    {
                        return new DateTime(year, month, day, ValidationHelper.GetInteger(val[4], 0), ValidationHelper.GetInteger(val[5], 0), 0);
                    }
                }
            }
            return new DateTime(2000, 1, 1);
        }


        /// <summary>
        /// Regenerate all time zone rules.
        /// </summary>
        public static void GenerateTimeZoneRules()
        {
            DataSet ds = GetTimeZones().OrderBy("TimeZoneName");
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // All time zones
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    SetTimeZoneInfo(new TimeZoneInfo(row));
                }
            }
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(TimeZoneInfo info)
        {
            // Recreate time zone rules
            info.TimeZoneRuleStartIn = CreateRuleDateTime(info.TimeZoneRuleStartRule);
            info.TimeZoneRuleEndIn = CreateRuleDateTime(info.TimeZoneRuleEndRule);

            // Set time zone info
            base.SetInfo(info);
        }


        /// <summary>
        /// Returns day of specified condition.
        /// </summary>
        /// <param name="weekDay">3 digit code of week day</param>
        /// <param name="dayValue">Starting day</param>
        /// <param name="month">Month</param>
        /// <param name="year">Year</param>
        /// <param name="add">Step (1,-1)</param>
        private static DateTime GetDay(string weekDay, int dayValue, int month, int year, double add)
        {
            // Done ?
            bool done = false;

            // Get starting date
            DateTime date = new DateTime(year, month, dayValue);

            // Get DayOfWeek according to day value
            DayOfWeek dayOfWeek = DayOfWeek.Monday;
            switch (weekDay)
            {
                case "MON":
                    dayOfWeek = DayOfWeek.Monday;
                    break;

                case "TUE":
                    dayOfWeek = DayOfWeek.Tuesday;
                    break;

                case "WED":
                    dayOfWeek = DayOfWeek.Wednesday;
                    break;

                case "THU":
                    dayOfWeek = DayOfWeek.Thursday;
                    break;

                case "FRI":
                    dayOfWeek = DayOfWeek.Friday;
                    break;

                case "SAT":
                    dayOfWeek = DayOfWeek.Saturday;
                    break;

                case "SUN":
                    dayOfWeek = DayOfWeek.Sunday;
                    break;
            }

            // Circle until done
            while (!done)
            {
                if (date.DayOfWeek == dayOfWeek)
                {
                    done = true;
                }
                else
                {
                    date = date.AddDays(add);
                }
            }

            return date;
        }

        #endregion
    }
}