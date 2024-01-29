using System;
using System.Data;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;

namespace CMS.Globalization
{
    /// <summary>
    /// Static timezone accessing methods.
    /// </summary>
    public class TimeZoneHelper : CoreMethods
    {
        #region "Properties"

        /// <summary>
        /// Returns server timezone.
        /// </summary>
        public static TimeZoneInfo ServerTimeZone
        {
            get
            {
                return GetTimeZoneInfo(CoreServices.Settings["CMSServerTimeZone"]);
            }
        }


        /// <summary>
        /// Returns true if global CMSTimeZonesEnable setting key is true.
        /// </summary>
        public static bool TimeZonesEnabled
        {
            get
            {
                return ValidationHelper.GetBoolean(CoreServices.Settings["CMSTimeZonesEnable"], false);
            }
        }

        #endregion


        #region "Basic methods"

        /// <summary>
        /// Returns DateTime according timezone information.
        /// </summary>
        /// <param name="dateTime">Source datetime</param>
        /// <param name="srcTimeZoneInfo">Source timezone</param>
        /// <param name="destTimeZoneInfo">Destination timezone</param>
        /// <param name="timeZonesRequired">If true than the time is converted only if timezones are enabled in the system</param>
        public static DateTime ConvertTimeZoneDateTime(DateTime dateTime, TimeZoneInfo srcTimeZoneInfo, TimeZoneInfo destTimeZoneInfo, bool timeZonesRequired = true)
        {
            if ((timeZonesRequired && !TimeZonesEnabled) || (dateTime == DateTimeHelper.ZERO_TIME) || (srcTimeZoneInfo == null) || (destTimeZoneInfo == null))
            {
                return dateTime;
            }

            try
            {
                // Get UTC time from source time zone
                var utcTime = ConvertTimeToUTC(dateTime, srcTimeZoneInfo);

                // Get offset for destination time zone from UTC time
                double destGMT = GetGMToffset(utcTime, destTimeZoneInfo, true);

                // Calculate hours an minutes to shift
                int hour = Convert.ToInt32(destGMT);
                int minute = Convert.ToInt32((destGMT - hour) * 60);
                TimeSpan different = new TimeSpan(hour, minute, 0);

                return utcTime.Add(different);
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogException("Time zone", "Convert", ex);
            }
            return dateTime;
        }


        /// <summary>
        /// Compute gmt offset with dependence on daylight saving time.
        /// </summary>
        /// <param name="dt">DateTime to convert</param>
        /// <param name="tzi">Time zone info</param>
        /// <param name="isDestTimeZone">Indicates if offset is calculated from UTC to destination time zone</param>
        private static double GetGMToffset(DateTime dt, TimeZoneInfo tzi, bool isDestTimeZone)
        {
            if (tzi == null)
            {
                return 0;
            }

            if (!tzi.TimeZoneDaylight)
            {
                return tzi.TimeZoneGMT;
            }

            // Get shift values
            double destStartOffset = Convert.ToDouble(tzi.TimeZoneRuleStartRule.Split('|')[6]);
            double destEndOffset = Convert.ToDouble(tzi.TimeZoneRuleEndRule.Split('|')[6]);

            double dstOffset = Math.Max(destStartOffset, destEndOffset);

            if (IsDaylightSavingsTime(dt, tzi, isDestTimeZone, dstOffset))
            {
                return tzi.TimeZoneGMT + destStartOffset;
            }
            else
            {
                // Get end shift value
                return tzi.TimeZoneGMT + destEndOffset;
            }
        }


        /// <summary>
        /// Returns true if given DateTime is within daylight saving time in given time zone.
        /// </summary>
        /// <remarks>If <paramref name="isDestTimeZone"/> is true, input time has to be in UTC format</remarks>
        /// <param name="dt">DateTime to be checked</param>
        /// <param name="tzi">Time zone with DST configuration</param>
        /// <param name="isDestTimeZone">Indicates if offset is calculated from UTC to destination time zone</param>
        /// <param name="dstOffset">Offset between standard and DST time</param>
        private static bool IsDaylightSavingsTime(DateTime dt, TimeZoneInfo tzi, bool isDestTimeZone, double dstOffset = 0)
        {
            // If daylight saving is over start of new year add one year to end daylight time
            DateTime start = tzi.TimeZoneRuleStartIn;
            DateTime end = tzi.TimeZoneRuleEndIn;

            // Recalculate daylight start and end if converted DateTime is in different year
            if ((start.Year != dt.Year) || (end.Year != dt.Year))
            {
                start = TimeZoneInfoProvider.CreateRuleDateTime(tzi.TimeZoneRuleStartRule, dt.Year);
                end = TimeZoneInfoProvider.CreateRuleDateTime(tzi.TimeZoneRuleEndRule, dt.Year);
            }

            // Case of daylight across new year
            bool crossYearDayLight = (start > end) && (((dt > start) && (dt.Year == start.Year)) || ((dt < end) && (dt.Year == end.Year)));

            if (isDestTimeZone)
            {
                // For conversion from UTC to destination time zone we have to adjust start and end time of DST into UTC
                // to correct calculation
                end = end.AddHours(-tzi.TimeZoneGMT - dstOffset);
                start = start.AddHours(-tzi.TimeZoneGMT);
            }
            else if (dstOffset > 0)
            {
                // Adjust start and end of DST to correctly handle it because of potential overlapped times
                end = end.AddHours(-dstOffset);
                start = start.AddHours(dstOffset);
            }

            return ((DateTime.Compare(dt, start) >= 0) && (DateTime.Compare(dt, end) < 0)) || crossYearDayLight;
        }

        #endregion


        #region "TimeZoneInfo retrieval methods"


        /// <summary>
        /// Returns timezone info.
        /// </summary>
        /// <param name="timeZoneId">TimeZone ID</param>
        public static TimeZoneInfo GetTimeZoneInfo(int timeZoneId)
        {
            return TimeZoneInfoProvider.GetTimeZoneInfo(timeZoneId);
        }


        /// <summary>
        /// Returns timezone info.
        /// </summary>
        /// <param name="timeZoneId">TimeZone ID</param>
        public static TimeZoneInfo GetTimeZoneInfo(string timeZoneId)
        {
            return TimeZoneInfoProvider.GetTimeZoneInfo(timeZoneId);
        }


        /// <summary>
        /// Returns user timezone.
        /// </summary>
        /// <param name="userInfo">User info</param>
        public static TimeZoneInfo GetTimeZoneInfo(IUserInfo userInfo)
        {
            if (userInfo != null)
            {
                return GetTimeZoneInfo(userInfo.UserTimeZoneID);
            }

            return null;
        }


        /// <summary>
        /// Returns site timezone.
        /// </summary>
        /// <param name="siteInfo">Site info</param>
        public static TimeZoneInfo GetTimeZoneInfo(ISiteInfo siteInfo)
        {
            if (siteInfo != null)
            {
                return GetTimeZoneInfo(CoreServices.Settings[siteInfo.SiteName + ".CMSSiteTimeZone"]);
            }

            return null;
        }


        /// <summary>
        /// Returns timezone of given user or site timezone is the user one is not set. If also site timezone is not set then server timezone is returned.
        /// </summary>
        /// <param name="userInfo">User info</param>
        /// <param name="siteInfo">Site info</param>
        public static TimeZoneInfo GetTimeZoneInfo(IUserInfo userInfo, ISiteInfo siteInfo)
        {
            if ((userInfo != null) && (userInfo.UserTimeZoneID > 0))
            {
                // Get user timezone
                return GetTimeZoneInfo(userInfo);
            }
            else
            {
                TimeZoneInfo tzi = null;
                if (siteInfo != null)
                {
                    // Get site timezone
                    tzi = GetTimeZoneInfo(siteInfo);
                }

                if (tzi == null)
                {
                    // Get server timezone
                    tzi = ServerTimeZone;
                }

                return tzi;
            }
        }

        #endregion


        #region "User conversion methods"

        /// <summary>
        /// Returns user current date time in dependence on user time zone.
        /// </summary>
        /// <param name="userInfo">User info</param>
        public static DateTime GetUserDateTime(IUserInfo userInfo)
        {
            return ConvertToUserDateTime(DateTime.Now, userInfo);
        }


        /// <summary>
        /// Returns  user date time in dependence on user time zone.
        /// </summary>
        /// <param name="dateTime">DateTime</param>
        /// <param name="userInfo">User info</param>
        public static DateTime ConvertToUserDateTime(DateTime dateTime, IUserInfo userInfo)
        {
            TimeZoneInfo tzi = null;
            return ConvertToUserDateTime(dateTime, userInfo, out tzi);
        }


        /// <summary>
        /// Returns  user date time in dependence on user time zone.
        /// </summary>
        /// <param name="dateTime">DateTime</param>
        /// <param name="userInfo">User info</param>
        /// <param name="usedTimeZone">Destination time zone</param>
        public static DateTime ConvertToUserDateTime(DateTime dateTime, IUserInfo userInfo, out TimeZoneInfo usedTimeZone)
        {
            usedTimeZone = null;
            if (!TimeZonesEnabled)
            {
                return dateTime;
            }

            TimeZoneInfo userTimeZone = GetTimeZoneInfo(userInfo);
            TimeZoneInfo serverTimeZone = ServerTimeZone;

            if ((userTimeZone != null) && (serverTimeZone != null))
            {
                usedTimeZone = userTimeZone;
                return ConvertTimeZoneDateTime(dateTime, serverTimeZone, userTimeZone);
            }
            else
            {
                usedTimeZone = serverTimeZone;
                return dateTime;
            }
        }

        #endregion


        #region "Site conversion methods"

        /// <summary>
        /// Returns user current date time in dependence on user time zone.
        /// </summary>
        /// <param name="siteInfo">Site info</param>
        public static DateTime GetSiteDateTime(ISiteInfo siteInfo)
        {
            return ConvertToSiteDateTime(DateTime.Now, siteInfo);
        }


        /// <summary>
        /// Returns  site date time in dependence on site time zone.
        /// </summary>
        /// <param name="dateTime">DateTime</param>
        /// <param name="siteInfo">Site info</param>
        public static DateTime ConvertToSiteDateTime(DateTime dateTime, ISiteInfo siteInfo)
        {
            TimeZoneInfo tzi = null;
            return ConvertToSiteDateTime(dateTime, siteInfo, out tzi);
        }


        /// <summary>
        /// Returns  site date time in dependence on site time zone.
        /// </summary>
        /// <param name="dateTime">DateTime</param>
        /// <param name="siteInfo">Site info</param>
        /// <param name="usedTimeZone">Destination time zone</param>
        public static DateTime ConvertToSiteDateTime(DateTime dateTime, ISiteInfo siteInfo, out TimeZoneInfo usedTimeZone)
        {
            usedTimeZone = null;
            if (!TimeZonesEnabled)
            {
                return dateTime;
            }

            TimeZoneInfo siteTimeZone = GetTimeZoneInfo(siteInfo);
            TimeZoneInfo serverTimeZone = ServerTimeZone;

            if ((siteTimeZone != null) && (serverTimeZone != null))
            {
                usedTimeZone = siteTimeZone;
                return ConvertTimeZoneDateTime(dateTime, serverTimeZone, siteTimeZone);
            }
            else
            {
                usedTimeZone = serverTimeZone;
                return dateTime;
            }
        }

        #endregion


        #region "Server conversion methods"

        /// <summary>
        /// Returns  server date time in dependence on server time zone.
        /// </summary>
        /// <param name="dateTime">DateTime</param>
        /// <param name="userInfo">User info</param>
        public static DateTime ConvertToServerDateTime(DateTime dateTime, IUserInfo userInfo)
        {
            if (!TimeZonesEnabled)
            {
                return dateTime;
            }

            TimeZoneInfo userTimeZone = GetTimeZoneInfo(userInfo);
            TimeZoneInfo serverTimeZone = ServerTimeZone;

            if ((userTimeZone != null) && (serverTimeZone != null))
            {
                return ConvertTimeZoneDateTime(dateTime, userTimeZone, serverTimeZone);
            }
            else
            {
                return dateTime;
            }
        }


        /// <summary>
        /// Returns server date time in dependence on server time zone.
        /// </summary>
        /// <param name="dateTime">DateTime</param>
        /// <param name="customTimeZone">Custom time zone info</param>
        public static DateTime ConvertToServerDateTime(DateTime dateTime, TimeZoneInfo customTimeZone)
        {
            if (!TimeZonesEnabled)
            {
                return dateTime;
            }

            TimeZoneInfo serverTimeZone = ServerTimeZone;

            if ((customTimeZone != null) && (serverTimeZone != null))
            {
                return ConvertTimeZoneDateTime(dateTime, customTimeZone, serverTimeZone);
            }
            else
            {
                return dateTime;
            }
        }


        /// <summary>
        /// Returns server date time in dependence on server time zone.
        /// </summary>
        /// <param name="dateTime">DateTime</param>
        /// <param name="siteInfo">Site info</param>
        public static DateTime ConvertToServerDateTime(DateTime dateTime, ISiteInfo siteInfo)
        {
            if (!TimeZonesEnabled)
            {
                return dateTime;
            }

            TimeZoneInfo siteTimeZone = GetTimeZoneInfo(siteInfo);
            TimeZoneInfo serverTimeZone = ServerTimeZone;

            if ((siteTimeZone != null) && (serverTimeZone != null))
            {
                return ConvertTimeZoneDateTime(dateTime, siteTimeZone, serverTimeZone);
            }
            else
            {
                return dateTime;
            }
        }

        #endregion


        /// <summary>
        /// Returns UTC date time in dependence on given time zone.
        /// </summary>
        /// <param name="dateTime">DateTime to convert</param>
        /// <param name="timeZoneInfo">Time zone</param>
        public static DateTime ConvertTimeToUTC(DateTime dateTime, TimeZoneInfo timeZoneInfo)
        {
            if ((timeZoneInfo != null) && (dateTime != DateTimeHelper.ZERO_TIME) && TimeZonesEnabled)
            {
                double offset = GetGMToffset(dateTime, timeZoneInfo, false);
                dateTime = dateTime.AddHours(-offset);
            }

            return dateTime;
        }


        /// <summary>
        /// Returns user date time in dependence on user time zone from specified DateTime a TimeZone.
        /// </summary>
        /// <param name="dateTime">DateTime</param>
        /// <param name="timeZoneInfo">Time zone</param>
        /// <param name="userInfo">User info</param>
        public static DateTime ConvertTimeToUserTime(DateTime dateTime, TimeZoneInfo timeZoneInfo, IUserInfo userInfo)
        {
            if (!TimeZonesEnabled)
            {
                return dateTime;
            }

            TimeZoneInfo userTimeZone = GetTimeZoneInfo(userInfo);

            if (userTimeZone != null)
            {
                return ConvertTimeZoneDateTime(dateTime, timeZoneInfo, userTimeZone);
            }
            else
            {
                return dateTime;
            }
        }


        #region "DateTime to string conversion methods"

        /// <summary>
        /// Returns string representation of time zone shift in form '(UTC + 00:00)'.
        /// </summary>
        /// <param name="tzi">Time zone info</param>
        /// <param name="format">Format string, if null following format is used - '(UTC {0:zzz})'</param>
        public static string GetUTCStringOffset(TimeZoneInfo tzi, string format = null)
        {
            if (tzi != null)
            {
                if (String.IsNullOrEmpty(format))
                {
                    TimeSpan span = TimeSpan.FromHours(tzi.TimeZoneGMT);
                    if (span != null)
                    {
                        return String.Format("(UTC {0:zzz})", new DateTimeOffset(2007, 1, 1, 0, 0, 0, span)).Replace("+", "+ ").Replace("-", "- ");
                    }
                }
                else
                {
                    return String.Format(format, tzi.TimeZoneGMT);
                }
            }
            return null;
        }


        /// <summary>
        /// Returns "long" string representation of used time zone (user, site or server) in form '(UTC + 00:00) TimeZoneDisplayName'.
        /// Returns null if time zones are disabled.
        /// </summary>
        public static string GetUTCLongStringOffset(IUserInfo userInfo, ISiteInfo siteInfo)
        {
            TimeZoneInfo tzi = (TimeZoneHelper.TimeZonesEnabled) ? GetTimeZoneInfo(userInfo, siteInfo) : null;

            return GetUTCLongStringOffset(tzi);
        }


        /// <summary>
        /// Returns "long" string representation of time zone shift in form '(UTC + 00:00) TimeZoneDisplayName'.
        /// </summary>
        /// <param name="tzi">Time zone info</param>
        public static string GetUTCLongStringOffset(TimeZoneInfo tzi)
        {
            if (tzi != null)
            {
                return (GetUTCStringOffset(tzi) + " " + tzi.TimeZoneDisplayName).Trim();
            }
            return null;
        }


        /// <summary>
        /// Returns string representing of given time converted into user date time (site or server).
        /// </summary>
        /// <param name="time">DateTime to be converted</param>
        /// <param name="displayGMT">Indicates if GMT information should be appended to result string</param>
        /// <param name="ui">IUserInfo</param>
        /// <param name="si">ISiteInfo</param>
        /// <param name="format">Output format</param>        
        public static string ConvertToUserTimeZone(DateTime time, bool displayGMT, IUserInfo ui, ISiteInfo si, string format = null)
        {
            string result = null;

            // Get user datetime
            if (time != DateTimeHelper.ZERO_TIME)
            {
                TimeZoneInfo usedTimeZone = null;
                result = GetCurrentTimeZoneDateTimeStringInternal(time, ui, si, out usedTimeZone, format, displayGMT);
            }
            return result;
        }


        /// <summary>
        /// Returns string representation of the given date/time converted to user, site or server time zone.
        /// </summary>
        /// <param name="dt">Server DateTime value</param>
        /// <param name="ui">Current user info</param>
        /// <param name="si">Current site info</param>
        /// <param name="usedTimeZone">Used time zone</param>
        /// <param name="format">Output format</param>           
        public static string GetCurrentTimeZoneDateTimeString(DateTime dt, IUserInfo ui, ISiteInfo si, out TimeZoneInfo usedTimeZone, string format = null)
        {
            return GetCurrentTimeZoneDateTimeStringInternal(dt, ui, si, out usedTimeZone, format, true);
        }


        /// <summary>
        /// Returns string representation of the given date/time converted to user, site or server time zone.
        /// </summary>
        /// <param name="dt">Server DateTime value</param>
        /// <param name="ui">Current user info</param>
        /// <param name="si">Current site info</param>
        /// <param name="usedTimeZone">Used time zone</param>
        /// <param name="format">Output format</param>
        /// <param name="displayGMT">Indicates if GMT information should be appended to result string</param>          
        private static string GetCurrentTimeZoneDateTimeStringInternal(DateTime dt, IUserInfo ui, ISiteInfo si, out TimeZoneInfo usedTimeZone, string format, bool displayGMT)
        {
            usedTimeZone = null;
            if ((dt != DateTimeHelper.ZERO_TIME) && TimeZonesEnabled)
            {
                TimeZoneInfo userTimeZone = GetTimeZoneInfo(ui);
                if (userTimeZone != null)
                {
                    // User time zone is used
                    dt = ConvertToUserDateTime(dt, ui, out usedTimeZone);
                }
                else
                {
                    TimeZoneInfo siteTimeZone = GetTimeZoneInfo(si);
                    if (siteTimeZone != null)
                    {
                        // Site time zone is used
                        dt = ConvertToSiteDateTime(dt, si, out usedTimeZone);
                    }
                    else
                    {
                        // Server time zone is used
                        usedTimeZone = ServerTimeZone;
                    }
                }
            }

            string result = null;
            if (dt == DateTimeHelper.ZERO_TIME)
            {
                result = GetString("general.na");
            }
            else
            {
                result = (!String.IsNullOrEmpty(format)) ? dt.ToString(format) : dt.ToString();
            }

            return (displayGMT) ? String.Format("{0}  {1}", result, GetUTCStringOffset(usedTimeZone)).TrimEnd() : result;
        }

        #endregion


        #region "DataSet conversion methods"

        /// <summary>
        /// Converts the DateTime columns values of the DataSet to user datetime.
        /// </summary>
        /// <param name="dataSet">DataSet to be converted</param>
        /// <param name="userInfo">IUserInfo object</param>
        public static void ConvertDataSet(DataSet dataSet, IUserInfo userInfo)
        {
            if (!TimeZonesEnabled)
            {
                return;
            }

            if (!DataHelper.DataSourceIsEmpty(dataSet))
            {
                foreach (DataTable table in dataSet.Tables)
                {
                    ConvertDataTable(table, userInfo);
                }
            }
        }


        /// <summary>
        /// Converts the DateTime columns of the table values to user datetime.
        /// </summary>
        /// <param name="table">DataTable to be converted</param>
        /// <param name="userInfo">IUserInfo object</param>
        public static void ConvertDataTable(DataTable table, IUserInfo userInfo)
        {
            if (DataHelper.DataSourceIsEmpty(table) || !TimeZonesEnabled)
            {
                return;
            }

            // Get the DateTime columns indexes
            DataColumnCollection columns = table.Columns;
            int[] columnIndexes = new int[columns.Count];
            int index = 0;
            for (int i = 0; i < columns.Count; i++)
            {
                DataColumn col = columns[i];
                if (col.DataType == typeof(DateTime))
                {
                    columnIndexes[index] = i;
                    index++;
                }
            }

            // If there are some DateTime columns (index is nonzero) then adjust them
            if (index > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    switch (row.RowState)
                    {
                        case DataRowState.Unchanged:

                            AdjustDateTimeValues(row, columnIndexes, userInfo);

                            // This is to make sure that the row appears to be unchanged again.
                            row.AcceptChanges();

                            break;

                        case DataRowState.Deleted:

                            // This is to make sure that you obtain the right results if 
                            // the RejectChanges() method is called.

                            // Undo the delete
                            row.RejectChanges();

                            AdjustDateTimeValues(row, columnIndexes, userInfo);

                            // This is to mark the changes as permanent.
                            row.AcceptChanges();

                            // Set the same state at the beginning
                            row.Delete();

                            break;

                        default:
                            AdjustDateTimeValues(row, columnIndexes, userInfo);
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// Converts DateTime values in DataRow according to user TimeZone settings.
        /// </summary>
        /// <param name="dr">DataRow to be converted</param>
        /// <param name="dateTimeColumnsIndexes">Indexes of DateTime columns in datarow</param>
        /// <param name="userInfo">IUserInfo object</param>
        private static void AdjustDateTimeValues(DataRow dr, int[] dateTimeColumnsIndexes, IUserInfo userInfo)
        {
            for (int i = 0; i < dateTimeColumnsIndexes.Length; i++)
            {
                int columnIndex = dateTimeColumnsIndexes[i];
                DateTime value = ValidationHelper.GetDateTime(dr[columnIndex], DateTimeHelper.ZERO_TIME);
                if (value != DateTimeHelper.ZERO_TIME)
                {
                    dr[columnIndex] = ConvertToUserDateTime(value, userInfo);
                }
            }
        }

        #endregion
    }
}