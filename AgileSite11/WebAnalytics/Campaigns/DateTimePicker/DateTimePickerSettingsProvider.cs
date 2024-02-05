using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using CMS;
using CMS.Core;
using CMS.Globalization;
using CMS.SiteProvider;
using CMS.WebAnalytics;

[assembly: RegisterImplementation(typeof(IDateTimePickerSettingsProvider), typeof(DateTimePickerSettingsProvider), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides dictionary containing all culture dependent settings needed for initialization of cmsdatepicker javascript component.
    /// </summary>
    public class DateTimePickerSettingsProvider : IDateTimePickerSettingsProvider
    {
        private readonly ILocalizationService mLocalizationService;
        
        /// <summary>
        /// Constructor, creates instance of <see cref="DateTimePickerSettingsProvider"/>.
        /// </summary>
        /// <param name="localizationService">Service for strings localization</param>
        /// <example>
        /// This example shows how to use the settings when calling angular cms-date-time-picker component.
        /// <code>
        /// 
        /// service.cs
        /// 
        /// var settingsProvider = new DateTimePickerSettingsProvider(localizationService);
        /// var settings = JsonConvert.SerializeObject(settingsProvider.GetDateTimePickerSettings(culture, currentDateTime));
        /// 
        /// ... pass settings to the client
        /// 
        /// 
        /// controller.js
        /// $scope.settings = getSettingsFromServer();
        ///  
        /// 
        /// markup.aspx
        /// <cms-date-time-picker settings="{{settings}}"></cms-date-time-picker>
        /// 
        /// </code>
        /// </example>
        /// <exception cref="ArgumentNullException"><paramref name="localizationService"/> is null</exception>
        public DateTimePickerSettingsProvider(ILocalizationService localizationService)
        {
            if (localizationService == null)
            {
                throw new ArgumentNullException("localizationService");
            }
            
            mLocalizationService = localizationService;
        }


        /// <summary>
        /// Get dictionary containing all culture dependent settings needed by cmsdatepicker component, as well as timezone dependent labels.
        /// </summary>
        /// <param name="culture">Culture for which the settings will be evaluated</param>
        /// <param name="currentSite">Reference to the executing site. This information is needed, because current time for the datepicker is retrieved based on the site time zone</param>
        /// <param name="currentDateTime">Current date time for the site. Should be already shifted by timezone, if necessary</param>
        /// <remarks>
        /// Settings are suitable for serialization using Newtonsoft.Json. For example, please refer to <see cref="DateTimePickerSettingsProvider"/>.
        /// This service is directly dependent on the <see cref="TimeZoneHelper"/> class.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="culture"/> or <paramref name="currentSite"/> is null</exception>
        /// <returns>Settings object suitable for serialization</returns>
        public object GetDateTimePickerSettings(CultureInfo culture, SiteInfo currentSite, DateTime currentDateTime)
        {
            if (culture == null)
            {
                throw new ArgumentNullException("culture");
            }

            if (currentSite == null)
            {
                throw new ArgumentNullException("currentSite");
            }

            var datePickerSettings = new Dictionary<string, object>();
            var dateTimeFormatInfo = culture.DateTimeFormat;
            string cultureCode = culture.Name;
            string dateFormat = string.Format("{0} {1}", culture.DateTimeFormat.ShortDatePattern.Replace("'", ""), culture.DateTimeFormat.LongTimePattern);
            
            AddMonthNames(datePickerSettings, dateTimeFormatInfo);
            AddDayNames(datePickerSettings, dateTimeFormatInfo);
            AddHourDesignators(datePickerSettings, culture);
            AddButtonsText(datePickerSettings, cultureCode);
            AddRTLFlag(datePickerSettings, culture);
            AddDateFormattingAndCurrentDate(datePickerSettings, dateFormat, culture, currentDateTime);
            
            // This settings tell the date-picker to throw exception when parsing error occurs.
            // By default, new Date object is returned in this scenario
            datePickerSettings.Add("throwOnParseError", true);

            var currentTimeZone = TimeZoneHelper.GetTimeZoneInfo(currentSite);
              
            return new
            {
                datePickerSettings,
                dateFormat,
                labelTooltip = TimeZoneHelper.GetUTCLongStringOffset(currentTimeZone),
                label = TimeZoneHelper.GetUTCStringOffset(currentTimeZone)
            };
        }


        /// <summary>
        /// Add values representing the month names.
        /// </summary>
        /// <remarks>
        /// Default monthNames for en-US culture are <c>January, February, ...</c>.
        /// Default monthNamesShort for en-US culture are <c>Jan, Feb, ...</c>.
        /// </remarks>
        /// <param name="dictionary">Settings dictionary</param>
        /// <param name="dateTimeFormatInfo">Provides culture-specific information about the format of date and time values</param>
        private void AddMonthNames(Dictionary<string, object> dictionary, DateTimeFormatInfo dateTimeFormatInfo)
        {
            // Both month names and their abbreviations has 13 items, the last item is empty
            dictionary.Add("monthNames", dateTimeFormatInfo.MonthNames.Take(12).ToArray());
            dictionary.Add("monthNamesShort", dateTimeFormatInfo.AbbreviatedMonthNames.Take(12).ToArray());
        }


        /// <summary>
        /// Adds values representing the day names and flag determining which day starts the week.
        /// </summary>
        /// <remarks>
        /// Default dayNames for en-US culture are <c>Sunday, Monday, ...</c>.
        /// Default dayNamesMin for en-US culture are <c>Su, Mo, ...</c>.
        /// Default firstDay for en-US culture is <c>0</c>.
        /// </remarks>
        /// <param name="dictionary">Settings dictionary</param>
        /// <param name="dateTimeFormatInfo">Provides culture-specific information about the format of date and time values</param>
        private void AddDayNames(Dictionary<string, object> dictionary, DateTimeFormatInfo dateTimeFormatInfo)
        {
            dictionary.Add("dayNames", dateTimeFormatInfo.DayNames);
            dictionary.Add("dayNamesMin", dateTimeFormatInfo.AbbreviatedDayNames);
            dictionary.Add("firstDay", dateTimeFormatInfo.FirstDayOfWeek);
        }


        /// <summary>
        /// Adds text representing the ante and poste meridiem suffixes.
        /// </summary>
        /// <remarks>
        /// Default AMDesignator for en-US culture is <c>AM</c>.
        /// Default PMDesignator for en-US culture is <c>PM</c>.
        /// </remarks>
        /// <param name="dictionary">Settings dictionary</param>
        /// <param name="culture">Provides information about culture context</param>
        private void AddHourDesignators(Dictionary<string, object> dictionary, CultureInfo culture)
        {
            dictionary.Add("AMDesignator", mLocalizationService.GetString(culture.DateTimeFormat.AMDesignator, culture.Name));
            dictionary.Add("PMDesignator", mLocalizationService.GetString(culture.DateTimeFormat.PMDesignator, culture.Name));
        }


        /// <summary>
        /// Adds texts that will be rendered on various buttons within the calendar control.
        /// </summary>
        /// <remarks>
        /// Default closeText for en-US culture is <c>Select</c>.
        /// Default prevText for en-US culture is <c>Prev</c>.
        /// Default nextText for en-US culture is <c>Next</c>.
        /// Default NAText for en-US culture is <c>N/A</c>.
        /// Default currentText for en-US culture is <c>Now</c>.
        /// </remarks>
        /// <param name="dictionary">Settings dictionary</param>
        /// <param name="cultureCode">The culture name in the format languagecode2-country/regioncode2</param>
        private void AddButtonsText(Dictionary<string, object> dictionary, string cultureCode)
        {
            dictionary.Add("closeText", mLocalizationService.GetString("general.select", cultureCode));
            dictionary.Add("prevText", mLocalizationService.GetString("calendar.previous", cultureCode));
            dictionary.Add("nextText", mLocalizationService.GetString("calendar.next", cultureCode));
            dictionary.Add("NAText", mLocalizationService.GetString("general.notavailable", cultureCode));
            dictionary.Add("currentText", mLocalizationService.GetString("calendar.now", cultureCode));
        }


        /// <summary>
        /// Adds flag determining whether right-to-left rendering should be used.
        /// </summary>
        /// <remarks>
        /// Default isRTL for en-US culture is <c>false</c>.
        /// </remarks>
        /// <param name="dictionary">Settings dictionary</param>
        /// <param name="culture">Provides information about culture context</param>
        private void AddRTLFlag(Dictionary<string, object> dictionary, CultureInfo culture)
        {
            dictionary.Add("isRTL", culture.TextInfo.IsRightToLeft);
        }


        /// <summary>
        /// Adds date formatting and flag determining whether 24 hour format is used for the time.
        /// </summary>
        /// <remarks>
        /// Default dateFormat for en-US culture is <c>M/d/yy h:mm:ss tt</c>.
        /// Default use24HourMode for en-US culture is <c>false</c>.
        /// </remarks>
        /// <param name="dictionary">Settings dictionary</param>
        /// <param name="dateFormat">Specifies how the date will be formatted</param>
        /// <param name="culture">Provides information about culture context</param>
        /// <param name="currentDateTime">Current date time for the site. Should be already shifted by timezone, if necessary</param>
        private void AddDateFormattingAndCurrentDate(Dictionary<string, object> dictionary, string dateFormat, CultureInfo culture, DateTime currentDateTime)
        {
            // jQuery UI datepicker does not accept 'yyyy' literal, for four-digit year 'yy' should be used
            dateFormat = dateFormat.Replace("'", "").Replace("yyyy", "yy");

            dictionary.Add("dateFormat", dateFormat);
            dictionary.Add("use24HourMode", !dateFormat.Contains("tt"));
            dictionary.Add("defaultDate", currentDateTime.ToString(dateFormat, culture));
        }
    }
}
