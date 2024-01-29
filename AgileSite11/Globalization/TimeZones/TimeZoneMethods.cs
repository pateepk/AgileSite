using System;

using CMS.Helpers;
using CMS.Base;

namespace CMS.Globalization
{
    /// <summary>
    /// Time zone methods
    /// </summary>
    public class TimeZoneMethods
    {
        #region "DateTime methods"

        /// <summary>
        /// Convert datetime with dependence on time zone type to server date time.
        /// </summary>
        /// <param name="dateTime">User/Site date time</param>
        /// <param name="type">Time zone type</param>
        /// <param name="customTimeZone">Custom time zone info</param>
        public static DateTime DateTimeServerConvert(DateTime dateTime, TimeZoneTypeEnum type, TimeZoneInfo customTimeZone)
        {
            if (dateTime == DateTimeHelper.ZERO_TIME)
            {
                return dateTime;
            }

            DateTime newDate = dateTime;

            // Get DateTime according to type
            switch (type)
            {
                case TimeZoneTypeEnum.Custom:
                    newDate = TimeZoneHelper.ConvertToServerDateTime(dateTime, customTimeZone);
                    break;

                case TimeZoneTypeEnum.User:
                    {
                        var user = CMSActionContext.CurrentUser;
                        newDate = (user != null ? TimeZoneHelper.ConvertToServerDateTime(dateTime, user) : dateTime);
                    }
                    break;

                case TimeZoneTypeEnum.WebSite:
                    {
                        var site = CMSActionContext.CurrentSite;
                        newDate = (site != null ? TimeZoneHelper.ConvertToServerDateTime(dateTime, site) : dateTime);
                    }
                    break;
            }

            return newDate;
        }


        /// <summary>
        /// Convert date time with dependence on time zone type from server date time.
        /// </summary>
        /// <param name="dateTime">Server date time</param>
        /// <param name="type">Time zone type</param>
        /// <param name="customTimeZone">Custom time zone info</param>
        public static DateTime DateTimeConvert(DateTime dateTime, TimeZoneTypeEnum type, TimeZoneInfo customTimeZone)
        {
            TimeZoneInfo tzi;
            return DateTimeConvert(dateTime, type, customTimeZone, out tzi);
        }


        /// <summary>
        /// Convert date time with dependence on time zone type from server date time.
        /// </summary>
        /// <param name="dateTime">Server date time</param>
        /// <param name="type">Time zone type</param>
        /// <param name="customTimeZone">Custom time zone info</param>
        /// <param name="usedTimeZone">Time zone used for time conversion</param>
        public static DateTime DateTimeConvert(DateTime dateTime, TimeZoneTypeEnum type, TimeZoneInfo customTimeZone, out TimeZoneInfo usedTimeZone)
        {
            usedTimeZone = TimeZoneHelper.ServerTimeZone;

            if (dateTime == DateTimeHelper.ZERO_TIME)
            {
                return dateTime;
            }

            DateTime newDate = dateTime;
            
            // Get DateTime according to type
            switch (type)
            {
                case TimeZoneTypeEnum.Custom:
                    {
                        newDate = TimeZoneHelper.ConvertTimeZoneDateTime(dateTime, TimeZoneHelper.ServerTimeZone, customTimeZone);
                        usedTimeZone = customTimeZone;
                    }
                    break;

                case TimeZoneTypeEnum.User:
                    {
                        var user = CMSActionContext.CurrentUser;
                        newDate = (user != null ? TimeZoneHelper.ConvertToUserDateTime(dateTime, user) : dateTime);
                        if (user != null)
                        {
                            usedTimeZone = TimeZoneInfoProvider.GetTimeZoneInfo(user.UserTimeZoneID);
                        }
                    }
                    break;

                case TimeZoneTypeEnum.WebSite:
                    {
                        var site = CMSActionContext.CurrentSite;
                        newDate = (site != null ? TimeZoneHelper.ConvertToSiteDateTime(dateTime, site) : dateTime);
                        if (site != null)
                        {
                            usedTimeZone = TimeZoneHelper.GetTimeZoneInfo(site);
                        }
                    }
                    break;
            }

            return newDate;
        }

        #endregion
    }
}
