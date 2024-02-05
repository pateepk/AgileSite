using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Globalization;
using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Date and time picker.
    /// </summary>
    [ToolboxData("<{0}:DateTimePicker runat=server />")]
    public class DateTimePicker : AbstractDateTimePicker, ICallbackEventHandler
    {
        #region "Variables"

        private static bool? mUseCustomCalendar;
        private bool mAllowEmptyValue = true;
        private string mCultureCode;
        private string mSupportFolder;
        private string mTextBoxCultureCode;

        private const string DEFAULT_FOLDER = "~/CMSAdminControls/Calendar";

        #endregion


        #region "Controls variables"

        /// <summary>
        /// Enabled image
        /// </summary>
        protected Image imgEnabled = null;


        /// <summary>
        /// Disabled image
        /// </summary>
        protected Image imgDisabled = null;


        /// <summary>
        /// Now button.
        /// </summary>
        protected LinkButton btnNow = new LinkButton();


        /// <summary>
        /// N/A button
        /// </summary>
        protected LinkButton btnNA = new LinkButton();


        /// <summary>
        /// GMT shift label
        /// </summary>
        protected Label lblGMTShift = new Label();

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether use custom calendar. Default value is true. Can be overwritten by 'CMSUseCustomCalendarPicker' application setting.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool UseCustomCalendar
        {
            get
            {
                if (mUseCustomCalendar == null)
                {
                    mUseCustomCalendar = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUseCustomCalendarPicker"], true);
                }

                return mUseCustomCalendar.Value;
            }
        }
        

        /// <summary>
        /// Indicates if empty value is allowed.
        /// </summary>
        [Category("Behavior"), DefaultValue("true"), Description("Indicates if empty value is allowed.")]
        public bool AllowEmptyValue
        {
            get
            {
                return mAllowEmptyValue;
            }
            set
            {
                mAllowEmptyValue = value;
            }
        }

        
        /// <summary>
        /// Path to the folder with supporting files.
        /// </summary>
        [Category("Behavior"), DefaultValue("Calendar"), Description("Path to the folder with supporting files.")]
        public string SupportFolder
        {
            get
            {
                if (String.IsNullOrEmpty(mSupportFolder))
                {
                    mSupportFolder = DEFAULT_FOLDER;
                }

                return mSupportFolder.TrimEnd('/');
            }
            set
            {
                mSupportFolder = value;
            }
        }


        /// <summary>
        /// Display now option within the control.
        /// </summary>
        [Category("Behavior"), DefaultValue("true"), Description("Display now option within the control.")]
        public bool DisplayNow
        {
            get
            {
                return btnNow.Visible;
            }
            set
            {
                btnNow.Visible = value;
            }
        }


        /// <summary>
        /// Display N/A option within the control.
        /// </summary>
        [Category("Behavior"), DefaultValue("false"), Description("Display N/A option within the control.")]
        public bool DisplayNA
        {
            get
            {
                return btnNA.Visible;
            }
            set
            {
                btnNA.Visible = value;
            }
        }


        /// <summary>
        /// Indicates whether to render also client scripts for enabling controls.
        /// </summary>
        [Category("Behavior"), DefaultValue("false"), Description("Indicates whether to render also client scripts for enabling controls")]
        public bool RenderDisableScript
        {
            get;
            set;
        }


        /// <summary>
        /// Culture for string representation of TextBox.
        /// </summary>
        private string TextBoxCultureCode
        {
            get
            {
                if (String.IsNullOrEmpty(mTextBoxCultureCode))
                {
                    mTextBoxCultureCode = CultureCode;
                }

                return mTextBoxCultureCode;
            }
            set
            {
                mTextBoxCultureCode = value;
            }
        }


        /// <summary>
        /// CultureCode to correct DateTime convert. Returns current culture name if <see cref="UseCustomCalendar"/> is false.
        /// </summary>
        [Category("Data"), Description("Culture code of calendar.")]
        public override string CultureCode
        {
            get
            {
                return UseCustomCalendar ? mCultureCode : CultureInfo.CurrentUICulture.Name;
            }
            set
            {
                mCultureCode = value;
            }
        }


        /// <summary>
        /// Selected date and time.
        /// </summary>
        [Category("Data"), Description("Selected date and time.")]
        public override DateTime SelectedDateTime
        {
            get
            {
                return GetDateTime(true);
            }
            set
            {
                EnsureChildControls();
                DateTimeTextBox.Text = DateTimeToString(value);
                TextBoxCultureCode = CultureCode;
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor.
        /// </summary>
        public DateTimePicker()
        {
            DateTimeTextBox.ID = "txtDateTime";
            btnNow.ID = "btnNow";
            btnNA.ID = "btnNA";
            btnNA.Visible = false;
            CustomCalendarControlPath = "~/CMSAdminControls/ModalCalendar/ModalCalendar.ascx";
            Load += DateTimePicker_Load;
        }

        #endregion


        #region "Control events"

        private void DateTimePicker_Load(object sender, EventArgs e)
        {
            EnsureChildControls();
        }


        /// <summary>
        /// Renders the control at run-time.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Create div to hold date time picker
            Panel panel = new Panel { CssClass = "control-group-inline" };

            // Datetime textbox
            panel.Controls.Add(DateTimeTextBox);

            // Calendar image literal
            panel.Controls.Add(btnCalendarImage);

            // Calendar image literal in INIT because of accessible button rendering cycle
            CreateCalendarButton();

            // Now button
            panel.Controls.Add(btnNow);

            // N/A button
            panel.Controls.Add(btnNA);

            // Display time zone shift label
            if (ApplyTimeZones)
            {
                panel.Controls.Add(lblGMTShift);
            }

            Controls.Add(panel);
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            SetupControls();

            // Ensure that disabled Date-TimePicker disables button
            btnCalendarImage.Enabled = Enabled;

            DateTime dt = SelectedDateTime;

            // Re-save the value to textbox so it holds correct format (timezone...)
            if (dt != DateTimeHelper.ZERO_TIME)
            {
                SelectedDateTime = dt;
            }

            // Display time zone shift label (including tooltip) if needed
            if (ApplyTimeZones)
            {
                lblGMTShift.Text = TimeZoneHelper.GetUTCStringOffset(ValidTimeZone);
                ScriptHelper.AppendTooltip(lblGMTShift, TimeZoneHelper.GetUTCLongStringOffset(ValidTimeZone), "help");

                // Remove titles from form wrappers - Publish from and Publish to form group
                var script = string.Format(@"
var labelGMT{0} = $cmsj('#{1}');
var closest{0} = $cmsj(labelGMT{0}).closest('.form-group');
var tempTitle{0} = $cmsj(closest{0}).map(function () {{
    return this.title;
}}).get();
$cmsj(labelGMT{0}).bind('mouseover', function () {{
    $cmsj(closest{0}).removeAttr('title');
}});
$cmsj(labelGMT{0}).bind('mouseout', function () {{
    $cmsj(closest{0}).attr('title', tempTitle{0});
}});", ClientID, lblGMTShift.ClientID);

                ScriptHelper.RegisterStartupScript(this, typeof(string), "GMT_Shift_Title" + ClientID, ScriptHelper.GetScript(script));

                ScriptHelper.RegisterTooltip(Page);
            }

            if (!Enabled)
            {
                DateTimeTextBox.Enabled = false;
                btnNow.OnClientClick = string.Empty;
            }
            else
            {
                DateTimeTextBox.Enabled = true;

                if (ApplyTimeZones)
                {
                    // Get time zone offset relative to UTC
                    TimeZoneOffset = CalculateTimeZoneOffset(TimeZone, CustomTimeZone);
                }

                if (UseCustomCalendar)
                {
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
                else
                {
                    // Register the dialog script
                    ScriptHelper.RegisterDialogScript(Page);
                }
            }

            if (Visible)
            {
                ScriptHelper.RegisterJQuery(Page);

                // Register control specific scripts
                StringBuilder sb = new StringBuilder();

                sb.Append(@"
function SelectDate_", DateTimeTextBox.ClientID, @"(param, pickerId) { 
    if (window.Changed != null) {
        window.Changed();
    }
    ", Page.ClientScript.GetCallbackEventReference(this, "param", "SetDate", "pickerId"), @" 
}");

                if (RenderDisableScript)
                {
                    // Client script for enabling and disabling controls
                    sb.Append(@"
function SetEnabled_", ClientID, @"(enabled) {
    var textBoxElem = document.getElementById('", DateTimeTextBox.ClientID, @"');
    var nowElem = document.getElementById('", btnNow.ClientID, @"');
    var naElem = document.getElementById('", btnNA.ClientID, @"');
    var image_obj = $cmsj('#", CalendarImageClientID, @"')
    if (textBoxElem != null) {
        textBoxElem.disabled = enabled ? '' : 'disabled';
    }
    if (nowElem != null) {
        nowElem.disabled = enabled ? '' : 'disabled';
    }
    if (naElem != null)  {
        naElem.disabled = enabled ? '' : 'disabled';
    }

    if (enabled) {
        image_obj.removeAttr('disabled');
        image_obj.removeClass('icon-disabled');
        image_obj.addClass('CalendarIcon');
        image_obj.on('click', function() {
            $cmsj('#", DateTimeTextBox.ClientID, @"').cmsdatepicker('show');
        });    
    }   
    else {
        image_obj.prop('disabled', true);
        image_obj.addClass('icon-disabled');
        image_obj.removeClass('CalendarIcon');
        image_obj.off('click');
    }
}");
                }

                ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "DTP_" + DateTimeTextBox.ClientID, ScriptHelper.GetScript(sb.ToString()));

                // Register global scripts
                sb.Length = 0;

                sb.Append(@"
function SetDate(result, context) { 
    var elem = document.getElementById(context); 
    elem.value = result;
    $cmsj(elem).trigger('change');
}");

                ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "DTP_Global", ScriptHelper.GetScript(sb.ToString()));
            }
        }


        /// <summary>
        /// Renders the control at design-time.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            // Hide 'Now' and 'N/A' links if disabled
            if (!Enabled)
            {
                DisplayNow = DisplayNA = false;
            }
            output.Write("<div class=\"date-time-picker\">");
            base.Render(output);
            output.Write("</div>");
        }

        #endregion


        #region "Public methods"

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

            if (!String.IsNullOrWhiteSpace(DateTimeTextBox.Text))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Generate JavaScript onClick event for non custom calendar.
        /// </summary>
        public string GenerateNonCustomCalendarImageEvent()
        {
            string timeZoneId = String.Empty;
            if (ApplyTimeZones && (ValidTimeZone != null))
            {
                timeZoneId = "&timezoneid=" + ValidTimeZone.TimeZoneID;
            }

            return String.Format(" modalDialog('{0}/calendar.aspx?seldate=' + document.getElementById('{1}').value + '&controlid={1}&allowempty={2}&editTime={3}&UILang={4}{5}', 'calendar', 370, 300); return false;", UrlResolver.ResolveUrl(SupportFolder), DateTimeTextBox.ClientID, AllowEmptyValue, EditTime, CultureInfo.CurrentUICulture, timeZoneId);
        }

        #endregion


        #region "Private methods"

        private void SetupControls()
        {
            // DateTime TextBox
            DateTimeTextBox.AddCssClass("form-control");
            DateTimeTextBox.AddCssClass("CalendarTextBox");
            DateTimeTextBox.AddCssClass("input-width-58");

            // Now button
            btnNow.EnableViewState = false;
            btnNow.AddCssClass("calendar-action");
            btnNow.AddCssClass("form-control-text");
            btnNow.Text = EditTime ? ResHelper.GetString("DateTimePicker.Now") : ResHelper.GetString("calendar.today");
            btnNow.OnClientClick = String.Format("SelectDate_{0}('{1}', '{0}'); return false;", DateTimeTextBox.ClientID, EditTime ? "now" : "today");

            // N/A button
            btnNA.EnableViewState = false;
            btnNA.AddCssClass("calendar-action");
            btnNA.AddCssClass("form-control-text");
            btnNA.Text = ResHelper.GetString("general.na");
            btnNA.OnClientClick = String.Format("document.getElementById('{0}').value = '';return false;", DateTimeTextBox.ClientID);

            // Display time zone shift label
            if (ApplyTimeZones)
            {
                lblGMTShift.ID = "lblGMTShift";
                lblGMTShift.Text = String.Empty;
                lblGMTShift.EnableViewState = false;
                lblGMTShift.AddCssClass("calendar-gmt-info");
                lblGMTShift.AddCssClass("form-control-text");
            }
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

            if (!UseCustomCalendar)
            {
                btnCalendarImage.OnClientClick = GenerateNonCustomCalendarImageEvent();
            }
        }


        /// <summary>
        /// Returns DateTime from TextBox.
        /// </summary>
        private DateTime GetDateTime(bool applyTimeZone)
        {
            DateTime result;

            if (String.IsNullOrWhiteSpace(DateTimeTextBox.Text))
            {
                result = DateTimeHelper.ZERO_TIME;
            }
            else
            {
                try
                {
                    // Trim date time from TextBox
                    if (ValidationHelper.GetString(TextBoxCultureCode, String.Empty).Trim() != String.Empty)
                    {
                        IFormatProvider culture = CultureHelper.GetCultureInfo(TextBoxCultureCode, true);
                        result = DateTime.Parse(DateTimeTextBox.Text, culture);
                    }
                    else
                    {
                        result = DateTime.Parse(DateTimeTextBox.Text);
                    }

                    // Apply time zone
                    if ((result != DateTimeHelper.ZERO_TIME) && applyTimeZone && EditTime)
                    {
                        result = TimeZoneMethods.DateTimeServerConvert(result, TimeZone, CustomTimeZone);
                    }
                }
                catch
                {
                    result = DateTimeHelper.ZERO_TIME;
                }
            }

            return result;
        }


        /// <summary>
        /// Modifies date and time format string based on <see cref="UseCustomCalendar"/> property.
        /// </summary>
        /// <param name="format">Base date and time format string</param>
        /// <param name="culture">Culture info that can be used to modify format string</param>
        protected override string DateTimeToStringFormat(string format, CultureInfo culture)
        {
            if (!UseCustomCalendar)
            {
                format = string.Empty;

                if (EditTime)
                {
                    // Old calendar uses 'HH:mm:ss' time format
                    format = culture.DateTimeFormat.ShortDatePattern + " HH:mm:ss";
                }
            }

            return format;
        }

        #endregion


        #region "CallBack handling"

        private string callbackResult = String.Empty;


        /// <summary>
        /// Callback processing.
        /// </summary>
        /// <param name="eventArgument">Event argument</param>
        public void RaiseCallbackEvent(string eventArgument)
        {
            EditTime = String.Equals(eventArgument, "now", StringComparison.OrdinalIgnoreCase);

            // Get current date time string
            callbackResult = DateTimeToString(DateTime.Now);
        }


        /// <summary>
        /// Returns the result of the callback.
        /// </summary>
        public string GetCallbackResult()
        {
            return callbackResult;
        }

        #endregion
    }
}
