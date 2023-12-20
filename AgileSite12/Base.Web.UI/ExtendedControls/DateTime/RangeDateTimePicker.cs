using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Globalization;
using CMS.Helpers;
using CMS.Localization;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Range date time picker.
    /// </summary>
    [ToolboxData("<{0}:RangeDateTimePicker runat='server' />")]
    public class RangeDateTimePicker : AbstractDateTimePicker
    {
        #region "Variables"

        private bool mUseDynamicDefaultTime = true;
        private bool mPostbackOnOK = true;
        private HiddenField hdnFrom = new HiddenField();
        private HiddenField hdnTo = new HiddenField();
        private DateTime mSelectedDateTime = DateTimeHelper.ZERO_TIME;
        private DateTime mAlternateSelectedDateTime = DateTimeHelper.ZERO_TIME;

        /// <summary>
        /// DateTime alternate text box.
        /// </summary>
        protected CMSTextBox mAlternateDateTimeTextBox = new CMSTextBox();

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, postback is raised when OK button clicked.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool PostbackOnOK
        {
            get
            {
                return mPostbackOnOK;
            }
            set
            {
                mPostbackOnOK = value;
            }
        }


        /// <summary>
        /// If true month selector is disabled.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("If true, only year selection is enabled.")]
        public bool DisableMonthSelect
        {
            get;
            set;
        }


        /// <summary>
        /// If true month selector is disabled.
        /// </summary>
        [Category("Behavior"), Description("If true, day selector is disabled (user can select only year and month).")]
        public bool DisableDaySelect
        {
            get;
            set;
        }


        /// <summary>
        /// If true N/A button is displayed.
        /// </summary>
        [Category("Behavior"), Description("If true, N/A button is displayed.")]
        public bool DisplayNAButton
        {
            get;
            set;
        }


        /// <summary>
        /// If false empty textbox generates actual time. If true, begin (end) of the day is set.
        /// </summary>
        [Category("Behavior"), Description("If false 'now' button generates actual time, if true begin (end) of the day is set.")]
        public bool UseDynamicDefaultTime
        {
            get
            {
                return mUseDynamicDefaultTime;
            }
            set
            {
                mUseDynamicDefaultTime = value;
            }
        }


        /// <summary>
        /// Textbox displaying the selected date and time.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CMSTextBox AlternateDateTimeTextBox
        {
            get
            {
                return mAlternateDateTimeTextBox;
            }
            set
            {
                mAlternateDateTimeTextBox = value;
            }
        }


        /// <summary>
        /// Start date of range.
        /// </summary>
        [Category("Data"), Description("Selected date and time.")]
        public override DateTime SelectedDateTime
        {
            get
            {
                DateTime from = GetDateTime(DateTimeTextBox.Text, 1, null);
                DateTime to = AlternateSelectedDateTime;

                if (from > to)
                {
                    from = to;
                }

                return from;
            }
            set
            {
                EnsureChildControls();
                hdnFrom.Value = value.ToString();
                mSelectedDateTime = value;
            }
        }


        /// <summary>
        /// End date of range.
        /// </summary>
        [Category("Data"), Description("Selected date and time.")]
        public DateTime AlternateSelectedDateTime
        {
            get
            {
                return GetDateTime(AlternateDateTimeTextBox.Text, 12, null);
            }
            set
            {
                EnsureChildControls();
                hdnTo.Value = value.ToString();
                mAlternateSelectedDateTime = value;
            }
        }


        /// <summary>
        /// Full date time representation (from) (return precise DateTime, not start date of interval (end of year, month, day...).
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime SelectedFullDateTime
        {
            get
            {
                DateTime from = GetDateTime(DateTimeTextBox.Text, 1, hdnFrom.Value);
                DateTime to = AlternateSelectedFullDateTime;

                if (from > to)
                {
                    from = to;
                }

                return from;
            }
            set
            {
                EnsureChildControls();
                hdnFrom.Value = DateTimeToString(value);
            }
        }


        /// <summary>
        /// Full date time representation (to) - (return precise DateTime, not end date of interval (end of year,month,day...).
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime AlternateSelectedFullDateTime
        {
            get
            {
                return GetDateTime(AlternateDateTimeTextBox.Text, 12, hdnTo.Value);
            }
            set
            {
                EnsureChildControls();
                hdnTo.Value = DateTimeToString(value);
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Range DateTime picker constructor.
        /// </summary>
        public RangeDateTimePicker()
            : base()
        {
            CustomCalendarControlPath = "~/CMSAdminControls/ModalCalendar/RangeModalCalendar.ascx";
            DateTimeTextBox.TextChanged += new EventHandler(DateTimeTextBox_TextChanged);
            AlternateDateTimeTextBox.TextChanged += new EventHandler(AlternateDateTimeTextBox_TextChanged);
        }

        #endregion


        #region "Control events"

        /// <summary>
        /// Renders the control at run-time.
        /// </summary>
        protected override void CreateChildControls()
        {
            CssClass = "control-group-inline";
            Panel panel = new Panel { CssClass = "keep-white-space-fixed" };

            // DateTime textbox
            LocalizedLabel lblFrom = new LocalizedLabel();
            lblFrom.ResourceString = "general.from";
            lblFrom.DisplayColon = true;
            lblFrom.CssClass = "form-control-text input-label";
            DateTimeTextBox.ID = "tbFrom";
            lblFrom.AssociatedControlID = DateTimeTextBox.ID;
            panel.Controls.Add(lblFrom);

            DateTimeTextBox.CssClass = "CalendarTextBox RangeCalendarTextBox input-width-60 input-date";
            panel.Controls.Add(DateTimeTextBox);

            LocalizedLabel lblTo = new LocalizedLabel();
            lblTo.ResourceString = "general.to";
            lblTo.CssClass = "form-control-text input-label";
            lblTo.DisplayColon = true;
            AlternateDateTimeTextBox.ID = "tbTo";
            lblTo.AssociatedControlID = AlternateDateTimeTextBox.ID;
            panel.Controls.Add(lblTo);

            panel.Controls.Add(hdnFrom);
            panel.Controls.Add(hdnTo);

            AlternateDateTimeTextBox.CssClass = "CalendarTextBox RangeCalendarTextBox input-width-60 input-date";
            panel.Controls.Add(AlternateDateTimeTextBox);

            btnCalendarImage = new CMSAccessibleButton();
            CreateCalendarButton();

            panel.Controls.Add(btnCalendarImage);

            // Display time zone shift label
            if (ApplyTimeZones)
            {
                Label lblGMTShift = new Label();
                lblGMTShift.ID = "lblGMTShift";
                lblGMTShift.Text = "";
                panel.Controls.Add(lblGMTShift);
            }

            Controls.Add(panel);

            base.CreateChildControls();
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            if (!Enabled)
            {
                DateTimeTextBox.Enabled = false;
            }
            else
            {
                DateTimeTextBox.Enabled = true;

                // Display time zone shift label (including tooltip) if needed
                if (ApplyTimeZones)
                {
                    Control ctrl = FindControl("lblGMTShift");
                    if (ctrl != null)
                    {
                        Label lbl = (Label)ctrl;
                        lbl.Text = TimeZoneHelper.GetUTCStringOffset(ValidTimeZone);
                        ScriptHelper.AppendTooltip(lbl, TimeZoneHelper.GetUTCLongStringOffset(ValidTimeZone), "help");

                        // Get time zone offset relative to UTC
                        TimeZoneOffset = CalculateTimeZoneOffset(TimeZone, CustomTimeZone);
                    }
                }

                CMSCustomCalendarControl customCalendar = Page.LoadUserControl(CustomCalendarControlPath) as CMSCustomCalendarControl;
                if (customCalendar == null)
                {
                    Label lblError = new Label();
                    Controls.Add(lblError);

                    lblError.Text = ResHelper.GetString("calendar.nocontrol");
                    lblError.CssClass = "ErrorLabel";

                    Enabled = false;
                }
                else
                {
                    customCalendar.PickerControl = this;
                    Controls.Add(customCalendar);
                }
            }

            if (mSelectedDateTime != DateTimeHelper.ZERO_TIME)
            {
                DateTimeTextBox.Text = DateTimeToString(mSelectedDateTime);
            }

            if (mAlternateSelectedDateTime != DateTimeHelper.ZERO_TIME)
            {
                AlternateDateTimeTextBox.Text = DateTimeToString(mAlternateSelectedDateTime);
            }

            base.OnPreRender(e);
        }


        private void AlternateDateTimeTextBox_TextChanged(object sender, EventArgs e)
        {
            // Reset 1st textbox
            mSelectedDateTime = DateTimeHelper.ZERO_TIME;
        }


        private void DateTimeTextBox_TextChanged(object sender, EventArgs e)
        {
            // Reset 2nd textbox
            mAlternateSelectedDateTime = DateTimeHelper.ZERO_TIME;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Validates if time is in interval (set by UseCalendarLimit, MinDate and MaxDate properties).
        /// </summary>
        public override bool IsValidRange()
        {
            // Check min max date
            DateTime selection = SelectedDateTime;
            if (selection != DateTimeHelper.ZERO_TIME)
            {
                return IsValidRangeInternal(selection);
            }
            else if (!String.IsNullOrEmpty(DateTimeTextBox.Text.Trim()))
            {
                return false;
            }

            // Check min max date for alternate selected value
            selection = AlternateSelectedDateTime;
            if (selection != DateTimeHelper.ZERO_TIME)
            {
                return IsValidRangeInternal(selection);
            }
            else if (!String.IsNullOrEmpty(AlternateDateTimeTextBox.Text.Trim()))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Creates literal for image.
        /// </summary>
        private void CreateCalendarButton()
        {
            btnCalendarImage.EnableViewState = false;
            btnCalendarImage.UseSubmitBehavior = false;
            btnCalendarImage.CausesValidation = false;
            btnCalendarImage.IconOnly = true;
            btnCalendarImage.IconCssClass = "icon-calendar";
            btnCalendarImage.ScreenReaderDescription = ResHelper.GetString("DateTimePicker.Calendar");
            btnCalendarImage.ToolTip = ResHelper.GetString("DateTimePicker.Calendar");
            btnCalendarImage.OnClientClick = "return false;";
            btnCalendarImage.Enabled = Enabled;
        }


        /// <summary>
        /// Creates string representation of DateTime.
        /// </summary>
        /// <param name="value">DateTime object</param>
        protected override string DateTimeToString(DateTime value)
        {
            String result = String.Empty;
            if (DisableMonthSelect)
            {
                result = value.Year.ToString();
            }
            else if (DisableDaySelect)
            {
                result = String.Format("{0}/{1}", value.Year, value.Month);
            }
            else
            {
                result = base.DateTimeToString(value);
            }

            return result;
        }


        /// <summary>
        /// Returns DateTime for return value.
        /// </summary>
        /// <param name="value">Value with string representation</param>
        /// <param name="defaultMonth">1 (start) or 12 (end) of interval</param>
        /// <param name="defaultTimeString">Default time</param>
        private DateTime GetDateTime(String value, int defaultMonth, string defaultTimeString)
        {
            DateTime result = (defaultMonth == 12) ? mAlternateSelectedDateTime : mSelectedDateTime;
            DateTime defaultTime = DateTimeHelper.ZERO_TIME;

            try
            {
                if (!String.IsNullOrEmpty(defaultTimeString))
                {
                    defaultTime = DateTime.Parse(defaultTimeString);
                }

                // Disable month select - value should be year number (2000, 1995...)
                if (DisableMonthSelect)
                {
                    if (result != DateTimeHelper.ZERO_TIME)
                    {
                        value = result.Year.ToString();
                    }

                    if (ValidationHelper.IsInteger(value))
                    {
                        if (defaultTime.Year.ToString() == value)
                        {
                            result = defaultTime;
                        }
                        else
                        {
                            result = new DateTime(ValidationHelper.GetInteger(value, 0), defaultMonth, 1);
                            // If end of range add month minus second 
                            if (defaultMonth == 12)
                            {
                                result = result.AddMonths(1).AddSeconds(-1);
                            }
                        }
                    }
                }
                // Disable day select - value should be sting in format :year/month (2005/2)
                else if (DisableDaySelect)
                {
                    if (result != DateTimeHelper.ZERO_TIME)
                    {
                        value = String.Format("{0}/{1}", result.Year, result.Month);
                    }

                    string[] arr = value.Split('/');
                    if (arr.Length == 2)
                    {
                        // If year is integer
                        if (ValidationHelper.IsInteger(arr[0]))
                        {
                            // Get Year and months from string
                            int year = ValidationHelper.GetInteger(arr[0], 0);
                            int month = ValidationHelper.GetInteger(arr[1], defaultMonth);

                            if ((defaultTime.Year == year) && (defaultTime.Month == month))
                            {
                                result = defaultTime;
                            }
                            else
                            {
                                // If default month not 1 -> use day as end of month (it means this value is for alternative textbox --->"to")
                                result = new DateTime(year, month, 1);

                                if (defaultMonth == 12)
                                {
                                    result = result.AddMonths(1).AddSeconds(-1);
                                }
                            }
                        }
                    }
                    // Wrong format -- try set year at least
                    else if (ValidationHelper.IsInteger(value))
                    {
                        result = new DateTime(ValidationHelper.GetInteger(value, 0), 1, 1);
                    }
                }
                else
                {
                    if (result == DateTimeHelper.ZERO_TIME)
                    {
                        // Classic - parse value with given culture
                        if (!String.IsNullOrEmpty(CultureCode))
                        {
                            IFormatProvider culture = CultureHelper.GetCultureInfo(CultureCode);
                            result = DateTime.Parse(value, culture);
                        }
                        else
                        {
                            result = DateTime.Parse(value);
                        }
                    }

                    if (EditTime)
                    {
                        result = TimeZoneMethods.DateTimeServerConvert(result, TimeZone, CustomTimeZone);
                    }
                    else
                    {
                        if (defaultTime.Date == result.Date)
                        {
                            // Means return full date time (with hours) when clicked from days (week) interval
                            result = defaultTime;
                        }
                        // Default time is null (not returning fulldate) or day was changed - return trimmed hours
                        else
                        {
                            if (defaultMonth == 12)
                            {
                                // Returning TO - fix to 23:59:59
                                result = result.Date.AddDays(1).AddSeconds(-1);
                            }
                            else
                            {
                                // Return midnight - FROM
                                result = result.Date;
                            }
                        }
                    }
                }
            }
            catch
            {
                result = DateTimeHelper.ZERO_TIME;
            }

            return result;
        }

        #endregion
    }
}
