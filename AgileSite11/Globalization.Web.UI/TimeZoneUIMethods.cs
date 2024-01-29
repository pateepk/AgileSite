using System;
using System.Web.UI;

using CMS.Helpers;

namespace CMS.Globalization.Web.UI
{
    /// <summary>
    /// Time zone UI methods
    /// </summary>
    public class TimeZoneUIMethods
    {
        /// <summary>
        /// Gets the time zone manager for the given control.
        /// </summary>
        /// <param name="control">Control for which to get the time zone manager</param>
        public static ITimeZoneManager GetTimeZoneManager(Control control)
        {
            while (control != null)
            {
                // If control is time zone manager return it
                if (control is ITimeZoneManager)
                {
                    if (((ITimeZoneManager)control).TimeZoneType != TimeZoneTypeEnum.Inherit)
                    {
                        return (ITimeZoneManager)control;
                    }
                }

                control = control.Parent;
            }

            // If no manager found in control collection, try to load one from request stock helper
            ITimeZoneManager itzm = RequestStockHelper.GetItem("RequestTimeZoneManager") as ITimeZoneManager;
            return itzm;
        }


        /// <summary>
        /// Gets the date time value for the given control.
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="dateTime">Date time</param>
        public static DateTime GetDateTimeForControl(Control control, DateTime dateTime)
        {
            TimeZoneInfo tzi = null;
            return GetDateTimeForControl(control, dateTime, out tzi);
        }


        /// <summary>
        /// Gets the date time value for the given control.
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="dateTime">Date time</param>
        /// <param name="usedTimeZone">Time zone used for time conversion</param>
        public static DateTime GetDateTimeForControl(Control control, DateTime dateTime, out TimeZoneInfo usedTimeZone)
        {
            usedTimeZone = null;

            // Get the time zone manager
            if (TimeZoneHelper.TimeZonesEnabled)
            {
                ITimeZoneManager tzm = GetTimeZoneManager(control);
                if (tzm != null)
                {
                    // Convert date time
                    return TimeZoneMethods.DateTimeConvert(dateTime, tzm.TimeZoneType, tzm.CustomTimeZone, out usedTimeZone);
                }
            }

            return dateTime;
        }


        /// <summary>
        /// Convert date time with dependences on current time zone manager.
        /// </summary>
        /// <param name="dateTime">Date time to convert</param>
        /// <param name="sender">Sender</param>
        public static DateTime ConvertDateTime(DateTime dateTime, Control sender)
        {
            ITimeZoneManager tzm = GetTimeZoneManager(sender);
            if (tzm != null)
            {
                return TimeZoneMethods.DateTimeConvert(ValidationHelper.GetDateTime(dateTime, DateTimeHelper.ZERO_TIME), tzm.TimeZoneType, tzm.CustomTimeZone);
            }

            return ValidationHelper.GetDateTime(dateTime, DateTimeHelper.ZERO_TIME);
        }
    }
}
