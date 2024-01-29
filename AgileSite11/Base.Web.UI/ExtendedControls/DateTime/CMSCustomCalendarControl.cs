using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Globalization;
using CMS.Helpers;
using CMS.IO;

using CultureInfo = System.Globalization.CultureInfo;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Basic class for custom date picker.
    /// </summary>
    public abstract class CMSCustomCalendarControl : AbstractUserControl
    {
        private object mPickerControl;

        /// <summary>
        /// Instance of picker control.
        /// </summary>
        public object PickerControl
        {
            get
            {
                return mPickerControl;
            }
            set
            {
                mPickerControl = value;
            }
        }


        /// <summary>
        /// Registers jQuery and default JS files for date time picker.
        /// </summary>
        /// <param name="datePickerObject">Calendar control with settings</param>
        protected void RegisterDefaultScripts(AbstractDateTimePicker datePickerObject)
        {
            // Register default JS files
            ScriptHelper.RegisterJQuery(Page);
            ScriptHelper.RegisterScriptFile(Page, "JQuery/jquery-ui-datetimepicker.js");
            ScriptHelper.RegisterScriptFile(Page, "Controls/modalCalendar.js");

            // Load picker resources from its support folder
            RegisterResources(datePickerObject);
        }


        /// <summary>
        /// Returns string with initial parameters for the date time picker. The result is joined from custom parameters defined by <paramref name="customParams"/> and default parameters.
        /// </summary>
        /// <param name="datePickerObject">Calendar control with settings</param>
        /// <param name="customParams">Custom initial parameters that will be added into the result parameters</param>
        protected string GetScriptParameters(AbstractDateTimePicker datePickerObject, HashSet<string> customParams)
        {
            customParams = customParams ?? new HashSet<string>();

            // Default settings
            customParams.Add("numberOfRows:6");
            customParams.Add("hideOnDateSelection:false");
            customParams.Add("displaySeconds:true");
            customParams.Add("showOn:'button'");
            customParams.Add("selectOtherMonths:true");
            customParams.Add("showOtherMonths:true");
            customParams.Add("changeYear:true");

            if (datePickerObject.UseCalendarLimit)
            {
                if (datePickerObject.MinDate != DateTimeHelper.ZERO_TIME)
                {
                    customParams.Add("minDate:" + (datePickerObject.MinDate.Date - DateTime.Now.Date).Days.ToString());
                }

                if (datePickerObject.MaxDate != DateTimeHelper.ZERO_TIME)
                {
                    customParams.Add("maxDate: " + (datePickerObject.MaxDate.Date - DateTime.Now.Date).Days.ToString());
                }
            }

            CultureInfo culture = CultureHelper.GetCultureInfo(datePickerObject.CultureCode, true);
            DateTimeFormatInfo info = culture.DateTimeFormat;

            string datePattern = info.ShortDatePattern.Replace("yyyy", "yy").Replace("'", "");
            if (datePickerObject.EditTime)
            {
                datePattern += " " + info.LongTimePattern;
            }

            bool use24HourMode = !datePattern.Contains("tt");

            string format = info.ShortDatePattern;
            if (Regex.Matches(format, "y").Count == 2)
            {
                format = format.Replace("yy", "yyyy");
            }

            string now = TimeZoneMethods.DateTimeConvert(DateTime.Now, datePickerObject.TimeZone, datePickerObject.CustomTimeZone).ToString(format, culture);
            customParams.Add(String.Format("defaultDate:'{0}'", now));

            // Localized settings
            string localize = String.Format("monthNames:{0},monthNamesShort:{1},dayNames:{2},dayNamesMin:{3},firstDay:{4},", ArrayToString(info.MonthNames), ArrayToString(info.AbbreviatedMonthNames), ArrayToString(info.DayNames), ArrayToString(info.ShortestDayNames), ConvertFirstDayToNumber(info.FirstDayOfWeek));
            localize += String.Format("AMDesignator:{0},PMDesignator:{1},closeText:{2},isRTL:{3},prevText:{4},nextText:{5}", ScriptHelper.GetLocalizedString(info.AMDesignator), ScriptHelper.GetLocalizedString(info.PMDesignator), ScriptHelper.GetLocalizedString("general.select"), culture.TextInfo.IsRightToLeft.ToString().ToLowerCSafe(), ScriptHelper.GetLocalizedString("calendar.previous"), ScriptHelper.GetLocalizedString("calendar.next"));
            customParams.Add(localize);

            // Other settings
            string initParameters = String.Format("IconID:'{0}',dateFormat:'{1}',timeZoneOffset:{2},use24HourMode:{3},applyTimeZones:{4}", datePickerObject.CalendarImageClientID, datePattern, datePickerObject.TimeZoneOffset, use24HourMode.ToString().ToLowerCSafe(), datePickerObject.ApplyTimeZones.ToString().ToLowerCSafe());
            customParams.Add(initParameters);

            return String.Join(",", customParams);
        }


        /// <summary>
        /// Loads resources for calendar control from its support folder.
        /// </summary>
        /// <param name="datePickerObject">Calendar control with settings</param>
        private void RegisterResources(AbstractDateTimePicker datePickerObject)
        {
            string supportFolderPath = Server.MapPath(datePickerObject.CustomCalendarSupportFolder);
            if (Directory.Exists(supportFolderPath))
            {
                // Register JS files
                string[] files = Directory.GetFiles(supportFolderPath, "*.js");
                string path = Server.MapPath("~/");
                foreach (string file in files)
                {
                    string relative = "~/" + file.Substring(path.Length).Replace(@"\", "/");
                    ScriptHelper.RegisterScriptFile(Page, relative);
                }

                // Register CSS files
                files = Directory.GetFiles(supportFolderPath, "*.css");
                foreach (string file in files)
                {
                    string relative = "~/" + file.Substring(path.Length).Replace(@"\", "/");
                    CssRegistration.RegisterCssLink(Page, relative);
                }
            }
        }


        /// <summary>
        /// Converts array to string in special format: [item1,item2...]
        /// </summary>
        /// <param name="arr">Input array</param>
        private string ArrayToString(IEnumerable<string> arr)
        {
            StringBuilder ret = new StringBuilder();
            foreach (string str in arr)
            {
                ret.Append(ScriptHelper.GetLocalizedString(str) + ",");
            }
            return String.Format("[{0}]", ret.ToString().TrimEnd(','));
        }


        /// <summary>
        /// Converts starting day of week from enum to number - passed to calendar jQuery control.
        /// </summary>
        /// <param name="name">Day name</param>
        private int ConvertFirstDayToNumber(DayOfWeek name)
        {
            switch (name)
            {
                case DayOfWeek.Monday:
                    return 1;

                case DayOfWeek.Tuesday:
                    return 2;

                case DayOfWeek.Wednesday:
                    return 3;

                case DayOfWeek.Thursday:
                    return 4;

                case DayOfWeek.Friday:
                    return 5;

                case DayOfWeek.Saturday:
                    return 6;

                default:
                    return 0;
            }
        }
    }
}
