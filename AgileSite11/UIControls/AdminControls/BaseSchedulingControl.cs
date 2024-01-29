using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.Scheduler;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for time scheduling controls
    /// </summary>
    public abstract class BaseSchedulingControl : CMSUserControl
    {
        #region "Constants"

        /// <summary>
        /// Minimum quantity of period units between repetitions
        /// </summary>
        protected const int QUANTITY_MINIMUM = 1;

        /// <summary>
        /// Maximum quantity of period units between repetitions
        /// </summary>
        protected const int QUANTITY_MAXIMUM = 10000;

        /// <summary>
        /// Minimum hour that can be used
        /// </summary>
        protected const int HOURS_MINIMUM = 0;

        /// <summary>
        /// Maximum hour that can be used
        /// </summary>
        protected const int HOURS_MAXIMUM = 23;

        /// <summary>
        /// Minimum minute that can be used
        /// </summary>
        protected const int MINUTES_MINIMUM = 0;

        /// <summary>
        /// Maximum hour that can be used
        /// </summary>
        protected const int MINUTES_MAXIMUM = 59;

        #endregion


        #region "Variables"

        private string mDefaultPeriod = SchedulingHelper.PERIOD_MINUTE;

        private string mScheduleInterval;
        private bool mDisplaySecond = true;
        private bool mDisplayOnce = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Type of time scheduling.
        /// </summary>
        public virtual string Mode
        {
            get
            {
                if (Period != null)
                {
                    return Period.SelectedValue;
                }
                return DefaultPeriod;
            }
        }


        /// <summary>
        /// Control providing amount of time units
        /// </summary>
        public virtual CMSTextBox Quantity
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Control providing window of opportunity start hours
        /// </summary>
        public virtual CMSTextBox FromHours
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Control providing window of opportunity start minutes
        /// </summary>
        public virtual CMSTextBox FromMinutes
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Control providing window of opportunity end hours
        /// </summary>
        public virtual CMSTextBox ToHours
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Control providing window of opportunity end minutes
        /// </summary>
        public virtual CMSTextBox ToMinutes
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Control providing start time
        /// </summary>
        public virtual DateTimePicker StartTime
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Control providing time unit used for scheduling
        /// </summary>
        public virtual CMSDropDownList Period
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Control providing allowed weekdays
        /// </summary>
        public virtual CMSCheckBoxList WeekDays
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Control providing allowed weekend days
        /// </summary>
        public virtual CMSCheckBoxList WeekEnd
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Control to select date mode for month
        /// </summary>
        public virtual CMSRadioButton MonthModeDate
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Control providing day in the month
        /// </summary>
        public virtual CMSDropDownList MonthDate
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Control to select specification mode for month
        /// </summary>
        public virtual CMSRadioButton MonthModeSpecification
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Control providing order of the desired day in the month
        /// </summary>
        public virtual CMSDropDownList MonthOrder
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Control providing desired day of week in the month
        /// </summary>
        public virtual CMSDropDownList MonthDay
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Default period. Allowed values: second, timesecond, minute, hour, day, week, month, once.
        /// </summary>
        public virtual string DefaultPeriod
        {
            get
            {
                return mDefaultPeriod;
            }
            set
            {
                switch (value.ToLowerCSafe())
                {
                    case SchedulingHelper.PERIOD_DAY:
                        mDefaultPeriod = SchedulingHelper.PERIOD_DAY;
                        break;

                    case SchedulingHelper.PERIOD_HOUR:
                        mDefaultPeriod = SchedulingHelper.PERIOD_HOUR;
                        break;

                    case SchedulingHelper.PERIOD_MINUTE:
                        mDefaultPeriod = SchedulingHelper.PERIOD_MINUTE;
                        break;

                    case SchedulingHelper.PERIOD_MONTH:
                        mDefaultPeriod = SchedulingHelper.PERIOD_MONTH;
                        break;

                    case SchedulingHelper.PERIOD_ONCE:
                        mDefaultPeriod = SchedulingHelper.PERIOD_ONCE;
                        break;

                    case "second":
                    case SchedulingHelper.PERIOD_SECOND:
                        mDefaultPeriod = SchedulingHelper.PERIOD_SECOND;
                        break;

                    case SchedulingHelper.PERIOD_WEEK:
                        mDefaultPeriod = SchedulingHelper.PERIOD_WEEK;
                        break;

                    default:
                        mDefaultPeriod = SchedulingHelper.PERIOD_MINUTE;
                        break;
                }
            }
        }


        /// <summary>
        /// Scheduled interval encoded into string.
        /// </summary>
        public string ScheduleInterval
        {
            get
            {
                mScheduleInterval = EncodeInterval();
                return mScheduleInterval;
            }
            set
            {
                mScheduleInterval = value;
            }
        }


        /// <summary>
        /// Object storing all the collected data
        /// </summary>
        public TaskInterval TaskInterval
        {
            get
            {
                if (!String.IsNullOrEmpty(mScheduleInterval))
                {
                    return SchedulingHelper.DecodeInterval(mScheduleInterval);
                }
                return null;
            }
        }


        /// <summary>
        /// Indicated whether week-list is checked.
        /// </summary>
        protected bool WeekListChecked
        {
            get;
            set;
        }


        /// <summary>
        /// If true, drop-down list with mode contains 'once' 
        /// </summary>
        public bool DisplayOnce
        {
            get
            {
                return mDisplayOnce;
            }
            set
            {
                mDisplayOnce = value;
            }
        }


        /// <summary>
        /// If true, drop-down list with mode contains 'seconds'
        /// </summary>
        public bool DisplaySecond
        {
            get
            {
                return mDisplaySecond;
            }
            set
            {
                mDisplaySecond = value;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Checks if interval start is before interval end as it should.
        /// </summary>
        public bool CheckIntervalPreceedings()
        {
            switch (Period.SelectedIndex)
            {
                case 0: // Second
                case 1: // Minute
                case 2: // Hour
                    TimeSpan start = new TimeSpan(Convert.ToInt32(FromHours.Text), Convert.ToInt32(FromMinutes.Text), 0);
                    TimeSpan end = new TimeSpan(Convert.ToInt32(ToHours.Text), Convert.ToInt32(ToMinutes.Text), 0);
                    return (start.CompareTo(end) < 0);

                default:
                    return true;
            }
        }


        /// <summary>
        /// Checks if at least one day of week is allowed.
        /// </summary>
        public bool CheckOneDayMinimum()
        {
            switch (Period.SelectedIndex)
            {
                case 0: // Second
                case 1: // Minute
                case 2: // Hour
                case 3: // Day
                    return ((WeekDays.SelectedItem != null) || (WeekEnd.SelectedItem != null));
                default:
                    return true;
            }
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Creates schedule interval string.
        /// </summary>
        protected virtual string EncodeInterval()
        {
            //string intervalCode = null;
            TaskInterval ti = new TaskInterval();

            string error = GetString("ScheduleInterval.WrongFormat");
            string empty = GetString("ScheduleInterval.ErrorEmpty");
            string result = string.Empty;

            try
            {
                ti.Period = Mode;
                if (StartTime != null)
                {
                    ti.StartTime = StartTime.SelectedDateTime;
                }

                switch (Mode)
                {
                    case SchedulingHelper.PERIOD_SECOND:
                    case SchedulingHelper.PERIOD_MINUTE:
                    case SchedulingHelper.PERIOD_HOUR:
                        result = new Validator().NotEmpty(Quantity.Text, empty).IsInteger(Quantity.Text, error).Result;
                        if (String.IsNullOrEmpty(result))
                        {
                            if (IsValidQuantity(Quantity.Text))
                            {
                                ti.Every = Convert.ToInt32(Quantity.Text);

                                // Handle additional fields
                                if ((FromHours != null) && (FromMinutes != null) && (ToHours != null) && (ToMinutes != null))
                                {
                                    result = new Validator()
                                        .NotEmpty(FromHours.Text, empty)
                                        .IsInteger(FromHours.Text, error)
                                        .NotEmpty(FromMinutes.Text, empty)
                                        .IsInteger(FromMinutes.Text, error)
                                        .NotEmpty(ToHours.Text, empty)
                                        .IsInteger(ToHours.Text, error)
                                        .NotEmpty(ToMinutes.Text, empty)
                                        .IsInteger(ToMinutes.Text, error)
                                        .Result;

                                    if (string.IsNullOrEmpty(result))
                                    {
                                        if (IsValidHour(FromHours.Text) && IsValidMinute(FromMinutes.Text))
                                        {
                                            if (IsValidHour(ToHours.Text) && IsValidMinute(ToMinutes.Text))
                                            {
                                                DateTime time = new DateTime(1, 1, 1, Convert.ToInt32(FromHours.Text), Convert.ToInt32(FromMinutes.Text), 0);
                                                ti.BetweenStart = time;
                                                time = new DateTime(1, 1, 1, Convert.ToInt32(ToHours.Text), Convert.ToInt32(ToMinutes.Text), 0);
                                                ti.BetweenEnd = time;
                                            }
                                            else
                                            {
                                                ShowError(error);
                                                ToHours.ForeColor = Color.Red;
                                                ToMinutes.ForeColor = Color.Red;
                                            }
                                        }
                                        else
                                        {
                                            ShowError(error);
                                            FromHours.ForeColor = Color.Red;
                                            FromMinutes.ForeColor = Color.Red;
                                        }
                                    }
                                }
                                else
                                {
                                    ti.BetweenStart = DateTime.MinValue;
                                    ti.BetweenEnd = DateTime.MaxValue;
                                }

                                // Handle days selection
                                AddWeekDays(ti);
                                AddWeekEnd(ti);
                            }
                            else
                            {
                                ShowError(error);
                                Quantity.ForeColor = Color.Red;
                            }
                        }
                        break;

                    case SchedulingHelper.PERIOD_DAY:
                        result = new Validator()
                            .NotEmpty(Quantity.Text, empty)
                            .IsInteger(Quantity.Text, error)
                            .Result;
                        if (string.IsNullOrEmpty(result))
                        {
                            if (IsValidQuantity(Quantity.Text))
                            {
                                ti.Every = Convert.ToInt32(Quantity.Text);

                                // Days
                                AddWeekDays(ti);
                                AddWeekEnd(ti);
                            }
                            else
                            {
                                ShowError(error);
                                Quantity.ForeColor = Color.Red;
                            }
                        }
                        break;

                    case SchedulingHelper.PERIOD_WEEK:
                    case SchedulingHelper.PERIOD_YEAR:
                        result = new Validator()
                            .NotEmpty(Quantity.Text, empty)
                            .IsInteger(Quantity.Text, error)
                            .Result;
                        if (string.IsNullOrEmpty(result))
                        {
                            if (IsValidQuantity(Quantity.Text))
                            {
                                ti.Every = Convert.ToInt32(Quantity.Text);
                            }
                            else
                            {
                                ShowError(error);
                                Quantity.ForeColor = Color.Red;
                            }
                        }
                        break;

                    case SchedulingHelper.PERIOD_MONTH:
                        if (MonthModeDate.Checked)
                        {
                            ti.Order = MonthDate.SelectedValue;
                        }
                        else
                        {
                            ti.Order = MonthOrder.SelectedValue;
                            ti.Day = MonthDay.SelectedValue;
                        }
                        ti.Every = 1;
                        break;

                    case SchedulingHelper.PERIOD_ONCE:
                        break;
                }
            }
            catch
            {
                ShowError(error);
                if (StartTime != null)
                {
                    StartTime.DateTimeTextBox.ForeColor = Color.Red;
                }
            }

            if (!string.IsNullOrEmpty(result))
            {
                ShowError(result);
                ti = new TaskInterval();
            }

            WeekListChecked = true;

            return SchedulingHelper.EncodeInterval(ti);
        }


        /// <summary>
        /// Initializes dialog according to selected period.
        /// </summary>
        protected abstract void OnPeriodChangeInit();


        /// <summary>
        /// Initialization of controls.
        /// </summary>
        protected virtual void ControlInit()
        {
            // Panel Period initialization
            SetPeriods();

            // Select default period
            Period.SelectedValue = DefaultPeriod;

            // Calendar initialization
            StartTime.DateTimeTextBox.AddCssClass("EditingFormCalendarTextBox");

            // Weekday and weekend lists' initialization
            InitializeWeekDays();
            InitializeWeekEnd();

            // Month list initialization
            InitializeMonthDays(31, true);

            // Repeat list initialization
            if (MonthOrder != null)
            {
                MonthOrder.Items.Clear();
                MonthOrder.Items.Add(new ListItem(GetString("ScheduleInterval.Months.First"), SchedulingHelper.MONTHS_FIRST));
                MonthOrder.Items.Add(new ListItem(GetString("ScheduleInterval.Months.Second"), SchedulingHelper.MONTHS_SECOND));
                MonthOrder.Items.Add(new ListItem(GetString("ScheduleInterval.Months.Third"), SchedulingHelper.MONTHS_THIRD));
                MonthOrder.Items.Add(new ListItem(GetString("ScheduleInterval.Months.Fourth"), SchedulingHelper.MONTHS_FOURTH));
                MonthOrder.Items.Add(new ListItem(GetString("ScheduleInterval.Months.Last"), SchedulingHelper.MONTHS_LAST));
            }

            MonthOrder.Enabled = false;
            MonthDay.Enabled = false;

            // Initialize dialog according to default period
            OnPeriodChangeInit();
        }


        /// <summary>
        /// Month mode changed
        /// </summary>
        /// <param name="dateMode">Indicates if in date mode</param>
        protected void MonthModeSelectionChanged(bool dateMode)
        {
            if (dateMode)
            {
                MonthModeSpecification.Checked = false;
            }
            else
            {
                MonthModeDate.Checked = false;
            }

            MonthDate.Enabled = dateMode;
            MonthOrder.Enabled = !dateMode;
            MonthDay.Enabled = !dateMode;
        }


        /// <summary>
        /// Sets periods
        /// </summary>
        protected virtual void SetPeriods()
        {
            if (Period != null)
            {
                List<ListItem> listItems = new List<ListItem>()
                {
                    new ListItem(GetString("ScheduleInterval.Period.Minute"), SchedulingHelper.PERIOD_MINUTE),
                    new ListItem(GetString("ScheduleInterval.Period.Hour"), SchedulingHelper.PERIOD_HOUR),
                    new ListItem(GetString("ScheduleInterval.Period.Day"), SchedulingHelper.PERIOD_DAY),
                    new ListItem(GetString("ScheduleInterval.Period.Week"), SchedulingHelper.PERIOD_WEEK),
                    new ListItem(GetString("ScheduleInterval.Period.Month"), SchedulingHelper.PERIOD_MONTH),
                    new ListItem(GetString("ScheduleInterval.Period.Year"), SchedulingHelper.PERIOD_YEAR)
                };

                if (DisplaySecond)
                {
                    listItems.Insert(0, new ListItem(GetString("ScheduleInterval.Period.Second"), SchedulingHelper.PERIOD_SECOND));
                }

                if (DisplayOnce)
                {
                    listItems.Add(new ListItem(GetString("ScheduleInterval.Period.Once"), SchedulingHelper.PERIOD_ONCE));
                }

                Period.Items.Clear();
                Period.Items.AddRange(listItems.ToArray());
            }
        }


        /// <summary>
        /// Sets days
        /// </summary>
        /// <param name="control">Check-box list control</param>
        /// <param name="days">List of days</param>
        protected void SetDays(CheckBoxList control, List<DayOfWeek> days)
        {
            if (control != null)
            {
                foreach (ListItem li in control.Items)
                {
                    li.Selected = days.Any(day => day.ToString().ToLowerCSafe() == li.Value.ToLowerCSafe());
                }
            }
        }


        /// <summary>
        /// Adds selected days from the list to the task interval (or uses defaultDays if no list provided)
        /// </summary>
        /// <param name="taskInterval">Task interval</param>
        /// <param name="list">List with day items</param>
        /// <param name="defaultDays">Default days to add when no list provided</param>
        private void AddDaysFromList(TaskInterval taskInterval, ListControl list, params DayOfWeek[] defaultDays)
        {
            if (list == null)
            {
                taskInterval.Days.AddRange(defaultDays);
            }
            else
            {
                foreach (ListItem item in list.Items.Cast<ListItem>().Where(item => item.Selected))
                {
                    DayOfWeek dayOfWeek;
                    if (Enum.TryParse(item.Value, true, out dayOfWeek))
                    {
                        taskInterval.Days.Add(dayOfWeek);
                    }
                }
            }
        }


        /// <summary>
        /// Adds selected week days to the task interval
        /// </summary>
        /// <param name="taskInterval">Task interval</param>
        protected void AddWeekDays(TaskInterval taskInterval)
        {
            AddDaysFromList(
                taskInterval,
                WeekDays,
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday);

        }


        /// <summary>
        /// Adds selected weekend days to the task interval
        /// </summary>
        /// <param name="taskInterval">Task interval</param>
        protected void AddWeekEnd(TaskInterval taskInterval)
        {
            AddDaysFromList(
                taskInterval,
                WeekEnd,
                DayOfWeek.Saturday,
                DayOfWeek.Sunday);
        }


        /// <summary>
        /// Sets days to MonthDate drop-down list.
        /// </summary>
        /// <param name="days">Amount of days to add</param>
        /// <param name="addLast">Add "last" to the end</param>
        protected void InitializeMonthDays(int days, bool addLast)
        {
            if (MonthDate != null)
            {
                MonthDate.Items.Clear();
                for (int i = 1; i <= days; i++)
                {
                    MonthDate.Items.Add(new ListItem(i.ToString()));
                }

                if (addLast)
                {
                    MonthDate.Items.Add(new ListItem(GetString("dateintervalsel.last"), "-1"));
                }
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns true if value is in between given boundaries
        /// </summary>
        /// <param name="value">Value to check (number as string)</param>
        /// <param name="minimum">Lowest acceptable value</param>
        /// <param name="maximum">Highest acceptable value</param>
        /// <returns></returns>
        private static bool IsWithinBoundaries(string value, int minimum, int maximum)
        {
            int convertedValue;            
            return Int32.TryParse(value, out convertedValue) && ((convertedValue >= minimum) && (convertedValue <= maximum));
        }


        /// <summary>
        /// Returns true if quantity is in between given boundaries (0..10000)
        /// </summary>
        /// <param name="quantity">Quantity to check (number as string)</param>
        private static bool IsValidQuantity(string quantity)
        {
            return IsWithinBoundaries(quantity, QUANTITY_MINIMUM, QUANTITY_MAXIMUM);
        }


        /// <summary>
        /// Returns true if hour is in between given boundaries (0..23)
        /// </summary>
        /// <param name="hours">Hour to check (number as string)</param>
        private static bool IsValidHour(string hours)
        {
            return IsWithinBoundaries(hours, HOURS_MINIMUM, HOURS_MAXIMUM);
        }


        /// <summary>
        /// Returns true if minute is in between given boundaries (0..59)
        /// </summary>
        /// <param name="minutes">Minute to check (number as string)</param>
        private static bool IsValidMinute(string minutes)
        {
            return IsWithinBoundaries(minutes, MINUTES_MINIMUM, MINUTES_MAXIMUM);
        }

        /// <summary>
        /// Creates array with week end days
        /// </summary>
        private ListItem[] CreateWeekEndArray()
        {
            ListItem[] li = new ListItem[2];
            li[0] = new ListItem(GetString("ScheduleInterval.Days.Saturday"), DayOfWeek.Saturday.ToString());
            li[1] = new ListItem(GetString("ScheduleInterval.Days.Sunday"), DayOfWeek.Sunday.ToString());
            return li;
        }


        /// <summary>
        /// Creates array with week days
        /// </summary>
        private ListItem[] CreateWeekDaysArray()
        {
            ListItem[] li = new ListItem[5];
            li[0] = new ListItem(GetString("ScheduleInterval.Days.Monday"), DayOfWeek.Monday.ToString());
            li[1] = new ListItem(GetString("ScheduleInterval.Days.Tuesday"), DayOfWeek.Tuesday.ToString());
            li[2] = new ListItem(GetString("ScheduleInterval.Days.Wednesday"), DayOfWeek.Wednesday.ToString());
            li[3] = new ListItem(GetString("ScheduleInterval.Days.Thursday"), DayOfWeek.Thursday.ToString());
            li[4] = new ListItem(GetString("ScheduleInterval.Days.Friday"), DayOfWeek.Friday.ToString());

            return li;
        }


        /// <summary>
        /// Fills days check-box list and month day DropDownList with week days
        /// </summary>
        private void InitializeWeekDays()
        {
            // Create unique array for every control to prevent error 'Cannot have multiple items selected in a DropDownList'
            if (WeekDays != null)
            {
                WeekDays.Items.Clear();
                WeekDays.Items.AddRange(CreateWeekDaysArray());
            }
            if (MonthDay != null)
            {
                MonthDay.Items.Clear();
                MonthDay.Items.AddRange(CreateWeekDaysArray());
            }
        }


        /// <summary>
        /// Fills days check-box list and month day DropDownList with week-end days
        /// </summary>
        private void InitializeWeekEnd()
        {
            // Create unique array for every control to prevent error 'Cannot have multiple items selected in a DropDownList'
            if (WeekEnd != null)
            {
                WeekEnd.Items.Clear();
                WeekEnd.Items.AddRange(CreateWeekEndArray());
            }
            if (MonthDay != null)
            {
                MonthDay.Items.AddRange(CreateWeekEndArray());
            }
        }

        #endregion
    }
}
