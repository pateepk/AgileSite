using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.DataEngine;
using CMS.Globalization;
using CMS.Globalization.Web.UI;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

using CultureInfo = System.Globalization.CultureInfo;
using TimeZoneInfo = CMS.Globalization.TimeZoneInfo;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Abstract class for date time picker server controls.
    /// </summary>
    public abstract class AbstractDateTimePicker : WebControl, INamingContainer
    {
        #region "Variables"

        /// <summary>
        /// DateTime text box
        /// </summary>
        private CMSTextBox mDateTimeTextBox = new CMSTextBox();
        
        /// <summary>
        /// Calendar image literal
        /// </summary>
        protected CMSAccessibleButton btnCalendarImage = new CMSAccessibleButton();

        private bool mUseCalendarLimit = true;
        private bool mEditTime = true;
        private bool? mApplyTimeZones;
        private const string mDefaultCustomCalendarSupportFolder = "~/CMSAdminControls/ModalCalendar/Themes/";
        private string mCustomCalendarSupportFolder = String.Empty;
        private TimeZoneTypeEnum mTimeZone = TimeZoneTypeEnum.Inherit;
        private TimeZoneInfo mCustomTimeZone;
        private TimeZoneInfo mValidTimeZone;
        private DateTime mMinDate = DateTimeHelper.ZERO_TIME;
        private DateTime mMaxDate = DateTimeHelper.ZERO_TIME;

        #endregion


        #region "Properties"

        /// <summary>
        /// TextBox displaying the selected date and time.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual CMSTextBox DateTimeTextBox
        {
            get
            {
                return mDateTimeTextBox;
            }
            set
            {
                mDateTimeTextBox = value;
            }
        }


        /// <summary>
        /// Client ID of calendar icon.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual string CalendarImageClientID
        {
            get
            {
                return btnCalendarImage.ClientID;
            }
        }


        /// <summary>
        /// CultureCode to correct DateTime convert.
        /// </summary>
        [Category("Data"), Description("Culture code of calendar.")]
        public virtual string CultureCode
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether is live site.
        /// </summary>
        [Category("Data"), Description("Indicates whether is live site.")]
        public bool IsLiveSite
        {
            get;
            set;
        }


        /// <summary>
        /// Custom calendar control path.
        /// </summary>
        [Category("Data"), Description("Path to custom calendar control.")]
        public virtual string CustomCalendarControlPath
        {
            get;
            set;
        }


        /// <summary>
        /// Path to theme files of custom calendar control.
        /// </summary>
        [Category("Data"), Description("Path to custom calendar support folder (folder with js a css files).")]
        public virtual string CustomCalendarSupportFolder
        {
            get
            {
                if (String.IsNullOrEmpty(mCustomCalendarSupportFolder))
                {
                    mCustomCalendarSupportFolder = mDefaultCustomCalendarSupportFolder + (IsLiveSite ? "LiveSite" : "Default");
                }

                return mCustomCalendarSupportFolder;
            }
            set
            {
                mCustomCalendarSupportFolder = value;
            }
        }
        

        /// <summary>
        /// Indicates if the user should be able to edit time.
        /// </summary>
        [Category("Behavior"), Description("Indicates if the user should be able to edit time.")]
        public bool EditTime
        {
            get
            {
                return mEditTime;
            }
            set
            {
                mEditTime = value;
                mApplyTimeZones = null;
            }
        }


        /// <summary>
        /// Specifies timezone type.
        /// </summary>
        [Category("Behavior"), DefaultValue("server"), Description("Specifies timezone type.")]
        public virtual TimeZoneTypeEnum TimeZone
        {
            get
            {
                if (mTimeZone == TimeZoneTypeEnum.Inherit)
                {
                    ITimeZoneManager man = TimeZoneUIMethods.GetTimeZoneManager(this);
                    if (man != null)
                    {
                        mTimeZone = man.TimeZoneType;

                        if (mTimeZone == TimeZoneTypeEnum.Custom)
                        {
                            CustomTimeZone = man.CustomTimeZone;
                        }
                        Inherited = (mTimeZone != TimeZoneTypeEnum.Inherit);
                    }
                }

                return mTimeZone;
            }
            set
            {
                mTimeZone = value;
                Inherited = false;
            }
        }


        /// <summary>
        /// Time zone offset (in minutes) relative to UTC.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual double TimeZoneOffset
        {
            get;
            set;
        }


        /// <summary>
        /// Custom TimeZoneInfo object.
        /// </summary>
        [Category("Behavior"), DefaultValue("null"), Description("Custom TimeZoneInfo object.")]
        public virtual TimeZoneInfo CustomTimeZone
        {
            get
            {
                return mCustomTimeZone;
            }
            set
            {
                if (!Inherited)
                {
                    mCustomTimeZone = value;
                }
            }
        }


        /// <summary>
        /// Indicates whether current time zone type is inherited or directly selected.
        /// </summary>
        [Category("Behavior"), DefaultValue("false"), Description("Display N/A option within the control.")]
        public bool Inherited
        {
            get;
            set;
        }


        /// <summary>
        /// If true calendar date are restricted by min and max date.
        /// </summary>
        [Category("Behavior"), Description("Use calendar date restriction")]
        public bool UseCalendarLimit
        {
            get
            {
                return mUseCalendarLimit;
            }
            set
            {
                mUseCalendarLimit = value;
            }
        }


        /// <summary>
        /// Maximum DateTime allowed for calendar.
        /// </summary>
        [Category("Behavior"), Description("Maximum date allowed for calendar")]
        public virtual DateTime MaxDate
        {
            get
            {
                if (mMaxDate == DateTimeHelper.ZERO_TIME)
                {
                    mMaxDate = DataTypeManager.MAX_DATETIME;

                    CultureInfo ci = CultureHelper.GetCultureInfo(CultureCode);
                    if (ci.Calendar.MaxSupportedDateTime < mMaxDate)
                    {
                        mMaxDate = DateTimeHelper.ZERO_TIME;
                    }
                }

                return mMaxDate;
            }
            set
            {
                mMaxDate = value;
            }
        }


        /// <summary>
        /// Minimum DateTime allowed for calendar.
        /// </summary>
        [Category("Behavior"), Description("Minimum date allowed for calendar")]
        public virtual DateTime MinDate
        {
            get
            {
                if (mMinDate == DateTimeHelper.ZERO_TIME)
                {
                    mMinDate = DataTypeManager.MIN_DATETIME;

                    CultureInfo ci = CultureHelper.GetCultureInfo(CultureCode);
                    if (ci.Calendar.MinSupportedDateTime > mMinDate)
                    {
                        mMinDate = DateTimeHelper.ZERO_TIME;
                    }
                }

                return mMinDate;
            }
            set
            {
                mMinDate = value;
            }
        }


        /// <summary>
        /// Selected date and time.
        /// </summary>
        public abstract DateTime SelectedDateTime
        {
            get;
            set;
        }


        /// <summary>
        /// Overrides the generation of the SPAN tag with custom tag.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                var setting = SettingsHelper.AppSettings["CMSControlElement"];
                if ((setting != null) && setting.Trim().EqualsCSafe("div", true))
                {
                    return HtmlTextWriterTag.Div;
                }
                else
                {
                    return HtmlTextWriterTag.Span;
                }
            }
        }


        /// <summary>
        /// Indicates if timezones can be applied based on timezones and edit time settings.
        /// </summary>
        internal bool ApplyTimeZones
        {
            get
            {
                if (mApplyTimeZones == null)
                {
                    mApplyTimeZones = EditTime && TimeZoneHelper.TimeZonesEnabled;
                }

                return mApplyTimeZones.Value;
            }
        }


        /// <summary>
        /// Valid time zone info object. It depends on <see cref="TimeZoneTypeEnum"/> (inherited, server, website).
        /// </summary>
        protected TimeZoneInfo ValidTimeZone
        {
            get
            {
                if (mValidTimeZone == null)
                {
                    mValidTimeZone = GetTimeZone();
                }

                return mValidTimeZone;
            }
        }

        #endregion


        #region "Control events"

        /// <summary>
        /// OnInit event.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            if (String.IsNullOrEmpty(CultureCode) && (CultureInfo.CurrentUICulture != null))
            {
                CultureCode = CultureInfo.CurrentUICulture.Name;
            }

            base.OnInit(e);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Validates if time is in interval (set by UseCalendarLimit, MinDate and MaxDate properties).
        /// </summary>
        public abstract bool IsValidRange();


        /// <summary>
        /// Basic validation if time is in interval (set by UseCalendarLimit, MinDate and MaxDate properties).
        /// </summary>
        /// <param name="value">DateTime value to be checked</param>
        protected virtual bool IsValidRangeInternal(DateTime value)
        {
            // Check min max date
            if (UseCalendarLimit)
            {
                if ((value > MaxDate) && (MaxDate != DateTimeHelper.ZERO_TIME))
                {
                    return false;
                }

                if ((value < MinDate) && (MinDate != DateTimeHelper.ZERO_TIME))
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Creates string representation of DateTime.
        /// </summary>
        /// <param name="value">DateTime object</param>
        protected virtual string DateTimeToString(DateTime value)
        {
            if (value == DateTimeHelper.ZERO_TIME)
            {
                return string.Empty;
            }

            if (EditTime)
            {
                value = TimeZoneMethods.DateTimeConvert(value, TimeZone, CustomTimeZone);
            }
            CultureInfo culture = String.IsNullOrEmpty(CultureCode) ? CultureInfo.CurrentUICulture : CultureHelper.GetCultureInfo(CultureCode);

            // If format contains only last two digits of years - make it full year format
            string format = culture.DateTimeFormat.ShortDatePattern;
            if (Regex.Matches(format, "y").Count == 2)
            {
                format = format.Replace("yy", "yyyy");
            }

            if (EditTime)
            {
                format += " " + culture.DateTimeFormat.LongTimePattern;
            }

            format = DateTimeToStringFormat(format, culture);

            return !String.IsNullOrEmpty(format) ? value.ToString(format, culture) : value.ToString(culture);
        }


        /// <summary>
        /// Allows to modify date and time format string.
        /// </summary>
        /// <param name="format">Base date and time format string</param>
        /// <param name="culture">Culture info that can be used to modify format string</param>
        protected virtual string DateTimeToStringFormat(string format, CultureInfo culture)
        {
            return format;
        }


        /// <summary>
        /// Returns TimeZoneInfo based on <see cref="TimeZoneTypeEnum"/>.
        /// </summary>
        private TimeZoneInfo GetTimeZone()
        {
            TimeZoneInfo tz = null;
            switch (TimeZone)
            {
                case TimeZoneTypeEnum.Inherit:
                    ITimeZoneManager man = TimeZoneUIMethods.GetTimeZoneManager(this);
                    if (man != null)
                    {
                        TimeZone = man.TimeZoneType;
                        tz = man.CustomTimeZone;
                        Inherited = (TimeZone != TimeZoneTypeEnum.Inherit);
                    }
                    break;

                case TimeZoneTypeEnum.Server:
                    tz = TimeZoneHelper.ServerTimeZone;
                    break;

                case TimeZoneTypeEnum.User:
                    tz = MembershipContext.AuthenticatedUser.TimeZoneInfo;
                    break;

                case TimeZoneTypeEnum.WebSite:
                    tz = TimeZoneHelper.GetTimeZoneInfo(SiteContext.CurrentSite);
                    break;

                case TimeZoneTypeEnum.Custom:
                    tz = CustomTimeZone;
                    break;
            }

            return tz;
        }


        /// <summary>
        /// Calculates offset of time zone specified by <paramref name="timeZoneType"/> relative to UTC.
        /// </summary>
        /// <param name="timeZoneType">Time zone type</param>
        /// <param name="customTimeZone">Custom time zone</param>
        protected double CalculateTimeZoneOffset(TimeZoneTypeEnum timeZoneType, TimeZoneInfo customTimeZone)
        {
            DateTime midNight = DateTime.Today;
            DateTime offset = TimeZoneMethods.DateTimeServerConvert(midNight, timeZoneType, customTimeZone).ToUniversalTime();

            return (midNight - offset).TotalMinutes;
        }

        #endregion
    }
}