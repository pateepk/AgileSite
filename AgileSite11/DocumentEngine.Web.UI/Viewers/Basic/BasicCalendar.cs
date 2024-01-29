using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Globalization;
using CMS.Globalization.Web.UI;
using CMS.Helpers;

using TimeZoneInfo = CMS.Globalization.TimeZoneInfo;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Calendar control with databinding features.
    /// </summary>
    [ToolboxData("<{0}:BasicCalendar runat=server></{0}:BasicCalendar>"), Serializable()]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class BasicCalendar : UICalendar, INamingContainer, IRelatedData
    {
        #region "Variables"

        private object mDataSource;
        private string mDataMember;
        private string mDayField;
        private string mEventEndField;
        private ITemplate mItemTemplate = null;
        private ITemplate mNoEventsTemplate = null;
        private TableItemStyle mDayWithEventsStyle;
        private TableItemStyle mDayWithOutEventsStyle;
        private DataTable mDtSource;
        private bool mHideDefaultDayNumber = false;
        private bool mDisplayOnlySingleDayItem;
        private DataView dvDataSource = null;
        private TimeZoneTypeEnum mTimeZone = TimeZoneTypeEnum.Inherit;
        private TimeZoneInfo mCustomTimeZone = null;
        private ITimeZoneManager mTimeZoneManager = null;
        private bool mTimeZoneSets = false;
        private int mSelectedItemId;
        private string mSelectedItemIdColumn;

        /// <summary>
        /// Related data is loaded.
        /// </summary>
        protected bool mRelatedDataLoaded = false;

        /// <summary>
        /// Custom data connected to the object.
        /// </summary>
        protected object mRelatedData = null;

        /// <summary>
        /// Hashtable with events for current month (key - DateTime, value - DataTable with events).
        /// </summary>
        protected Hashtable mMonthEvents = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the value that indicates whether dynamic controls should be resolved
        /// </summary>
        public bool ResolveDynamicControls
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ResolveDynamicControls"], true);
            }
            set
            {
                ViewState["ResolveDynamicControls"] = value;
            }
        }


        /// <summary>
        /// Custom data connected to the object, if not set, returns the Related data of the nearest IDataControl.
        /// </summary>
        public virtual object RelatedData
        {
            get
            {
                if ((mRelatedData == null) && !mRelatedDataLoaded)
                {
                    // Load the related data to the object
                    mRelatedDataLoaded = true;
                    IRelatedData dataControl = (IRelatedData)ControlsHelper.GetParentControl(this, typeof(IRelatedData));
                    if (dataControl != null)
                    {
                        mRelatedData = dataControl.RelatedData;
                    }
                }

                return mRelatedData;
            }
            set
            {
                mRelatedData = value;
            }
        }


        /// <summary>
        /// Indicates whether the day number is displayed or cell is full filled by the transformation
        /// Current day is saved in the "__day" column
        /// </summary>
        [Category("Behavior"), Description("Indicates whether the day number is displayed or cell is full filled by the transformation.")]
        public bool HideDefaultDayNumber
        {
            get
            {
                return mHideDefaultDayNumber;
            }
            set
            {
                mHideDefaultDayNumber = value;
            }
        }


        /// <summary>
        /// Indicates whether the only one item is displayed in the day.
        /// </summary>
        [Category("Behavior"), Description("Indicates whether the only one item is displayed in the day.")]
        public bool DisplayOnlySingleDayItem
        {
            get
            {
                return mDisplayOnlySingleDayItem;
            }
            set
            {
                mDisplayOnlySingleDayItem = value;
            }
        }


        /// <summary>
        /// Data source with calendar events - either DataSet or DataTable object.
        /// </summary>
        [Category("Behavior"), Description("Data source with calendar events - either DataSet or DataTable object.")]
        public object DataSource
        {
            get
            {
                return mDataSource;
            }
            set
            {
                mDataSource = value;
            }
        }


        /// <summary>
        /// Name of the table when DataSet is used as a DataSource.
        /// </summary>
        [Category("Behavior"), Description("Name of the table when DataSet is used as a DataSource.")]
        public string DataMember
        {
            get
            {
                return mDataMember;
            }
            set
            {
                mDataMember = value;
            }
        }


        /// <summary>
        /// Name of the field in the DataSource that contains the datetime value.
        /// </summary>
        [Category("Behavior"), Description("Name of the field in the DataSource that contains the datetime value.")]
        public string DayField
        {
            get
            {
                return mDayField;
            }
            set
            {
                mDayField = value;
            }
        }


        /// <summary>
        /// Name of the field in the DataSource that contains the datetime value of event end date.
        /// </summary>
        [Category("Behavior"), Description("Name of the field in the DataSource that contains the datetime value of event end date.")]
        public string EventEndField
        {
            get
            {
                return mEventEndField;
            }
            set
            {
                mEventEndField = value;
            }
        }


        /// <summary>
        /// Style of the day with an event.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TableItemStyle DayWithEventsStyle
        {
            get
            {
                return mDayWithEventsStyle;
            }
            set
            {
                mDayWithEventsStyle = value;
            }
        }


        /// <summary>
        /// Style of the day without an event.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TableItemStyle DayWithOutEventsStyle
        {
            get
            {
                return mDayWithOutEventsStyle;
            }
            set
            {
                mDayWithOutEventsStyle = value;
            }
        }


        /// <summary>
        /// Template for displaying a day with event.
        /// </summary>
        [TemplateContainer(typeof(RepeaterItem)), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), RefreshProperties(RefreshProperties.Repaint), NotifyParentProperty(true)]
        public ITemplate ItemTemplate
        {
            get
            {
                return mItemTemplate;
            }
            set
            {
                mItemTemplate = value;
            }
        }


        /// <summary>
        /// Template for displaying a day without any event.
        /// </summary>
        [TemplateContainer(typeof(RepeaterItem)), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), RefreshProperties(RefreshProperties.Repaint), NotifyParentProperty(true)]
        public ITemplate NoEventsTemplate
        {
            get
            {
                return mNoEventsTemplate;
            }
            set
            {
                mNoEventsTemplate = value;
            }
        }


        /// <summary>
        /// Specifies timezone type.
        /// </summary>
        [Category("Behavior"), DefaultValue("server"), Description("Specifies timezone type.")]
        public TimeZoneTypeEnum TimeZone
        {
            get
            {
                if (mTimeZoneSets)
                {
                    return mTimeZone;
                }
                else
                {
                    if (mTimeZoneManager == null)
                    {
                        mTimeZoneManager = TimeZoneUIMethods.GetTimeZoneManager(this) as ITimeZoneManager;
                    }

                    if (mTimeZoneManager != null)
                    {
                        mTimeZone = mTimeZoneManager.TimeZoneType;
                    }

                    mTimeZoneSets = true;
                }
                return mTimeZone;
            }
            set
            {
                mTimeZone = value;
                mTimeZoneSets = true;
            }
        }


        /// <summary>
        /// Custom TimeZoneInfo object.
        /// </summary>
        [Category("Behavior"), DefaultValue("null"), Description("Custom TimeZoneInfo object.")]
        public TimeZoneInfo CustomTimeZone
        {
            get
            {
                if ((mTimeZoneSets) && (mCustomTimeZone != null))
                {
                    return mCustomTimeZone;
                }
                else
                {
                    if (mTimeZoneManager == null)
                    {
                        mTimeZoneManager = TimeZoneUIMethods.GetTimeZoneManager(this) as ITimeZoneManager;
                    }

                    if (mTimeZoneManager != null)
                    {
                        mCustomTimeZone = mTimeZoneManager.CustomTimeZone;
                    }

                    mTimeZoneSets = true;
                }
                return mCustomTimeZone;
            }
            set
            {
                mCustomTimeZone = value;
                mTimeZoneSets = true;
            }
        }


        /// <summary>
        /// ID column name of selected item displayed in the calendar.
        /// </summary>
        public string SelectedItemIDColumn
        {
            get
            {
                return mSelectedItemIdColumn;
            }
            set
            {
                mSelectedItemIdColumn = value;
            }
        }


        /// <summary>
        /// ID of selected item displayed in the calendar.
        /// </summary>
        public int SelectedItemID
        {
            get
            {
                return mSelectedItemId;
            }
            set
            {
                mSelectedItemId = value;
            }
        }

        #endregion


        #region "Private properties"

        /// <summary>
        /// Returns DataView for the DataSource.
        /// </summary>
        private DataView DataSourceView
        {
            get
            {
                if ((dvDataSource == null) && (mDtSource != null))
                {
                    dvDataSource = mDtSource.DefaultView;
                }

                return dvDataSource;
            }
        }


        /// <summary>
        /// Returns hashtable with events for current month.
        /// Key - DateTime, Value - DataTable with events
        /// </summary>
        private Hashtable MonthEvents
        {
            get
            {
                if (mMonthEvents == null)
                {
                    mMonthEvents = new Hashtable();
                }

                return mMonthEvents;
            }
        }

        #endregion


        #region "Events"

        /// <summary>
        /// True if the on before init was fired.
        /// </summary>
        protected bool mOnBeforeInitFired;


        /// <summary>
        /// On before init handler.
        /// </summary>
        public event EventHandler OnBeforeInit;


        /// <summary>
        /// Raises the OnBeforeInit event.
        /// </summary>
        protected void RaiseOnBeforeInit()
        {
            if ((OnBeforeInit != null) && !mOnBeforeInitFired)
            {
                mOnBeforeInitFired = true;
                OnBeforeInit(this, null);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public BasicCalendar()
        {
            SelectionMode = CalendarSelectionMode.None;
            ShowGridLines = true;
        }


        /// <summary>
        /// Initializes the control.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            RaiseOnBeforeInit();
            base.OnInit(e);
        }


        /// <summary>
        /// Binds a data source to calendar control and its child controls.
        /// </summary>
        public override void DataBind()
        {
            mDtSource = null;
            dvDataSource = null;

            // Get table with source data
            if ((!DataHelper.DataSourceIsEmpty(DataSource)) && (!string.IsNullOrEmpty(DayField)))
            {
                // Source is DataTable
                if (DataSource is DataTable)
                {
                    mDtSource = ((DataTable)DataSource);
                }
                // Source is DataSet
                if (DataSource is DataSet)
                {
                    DataSet ds = ((DataSet)DataSource);
                    // Get first or specific table from DataSet
                    if (string.IsNullOrEmpty(DataMember))
                    {
                        mDtSource = ds.Tables[0];
                    }
                    else
                    {
                        mDtSource = ds.Tables[DataMember];
                    }
                }
            }

            // Load data for current date or date that specifies the month to display (may be set from outside)
            if (SelectedDate.Equals(VisibleDate) || (VisibleDate.Equals(DateTimeHelper.ZERO_TIME)))
            {
                LoadData(SelectedDate);
            }
            else
            {
                LoadData(VisibleDate);
            }

            base.DataBind();
        }


        /// <summary>
        /// Handles calendar month change.
        /// </summary>
        /// <param name="newDate">First day of selected month</param>
        /// <param name="previousDate">First day of previous month</param>
        protected override void OnVisibleMonthChanged(DateTime newDate, DateTime previousDate)
        {
            LoadData(newDate);

            base.OnVisibleMonthChanged(newDate, previousDate);
        }


        /// <summary>
        /// Prepares event data to hashtable to speed up rendering of calendar cells (days).
        /// </summary>
        /// <param name="currentMonthDay">Date of month displayed in the calendar</param>
        protected void LoadData(DateTime currentMonthDay)
        {
            // Clear hashtable
            MonthEvents.Clear();

            // Check if data table is not empty
            if (!DataHelper.DataSourceIsEmpty(mDtSource))
            {
                // Make a copy of the original data table including its data because the data table may have come from the cache.
                // That way, once the data table is extended by the new columns bellow, it does not modify the cached data table.
                mDtSource = mDtSource.Copy();

                // Add columns for additional information (day, month, year, order, selected, first, last)
                if (!mDtSource.Columns.Contains("__day"))
                {
                    mDtSource.Columns.Add("__day", typeof(string));
                }
                if (!mDtSource.Columns.Contains("__month"))
                {
                    mDtSource.Columns.Add("__month", typeof(string));
                }
                if (!mDtSource.Columns.Contains("__year"))
                {
                    mDtSource.Columns.Add("__year", typeof(string));
                }
                if (!mDtSource.Columns.Contains("__order"))
                {
                    mDtSource.Columns.Add("__order", typeof(int));
                }
                if (!mDtSource.Columns.Contains("__selected"))
                {
                    mDtSource.Columns.Add("__selected", typeof(bool));
                }
                if (!mDtSource.Columns.Contains("__first"))
                {
                    mDtSource.Columns.Add("__first", typeof(bool));
                }
                if (!mDtSource.Columns.Contains("__last"))
                {
                    mDtSource.Columns.Add("__last", typeof(bool));
                }

                // Get range of days that could be displayed in calendar (current month with 7days from previous and 14days from next month)
                DateTime calStart = DateTimeHelper.GetMonthStart(currentMonthDay);
                DateTime calEnd = calStart.AddMonths(1).AddDays(14);
                calStart = calStart.AddDays(-7);

                if (string.IsNullOrEmpty(EventEndField))
                {
                    // Set column name with event end date
                    EventEndField = DayField;
                }

                // Filter events from current month (including few days from previous and next months)
                DataSourceView.RowFilter = string.Format("({3} IS NULL AND #{0}# <= {1} AND {1} <= #{2}#) OR " +
                                                         "(NOT {3} IS NULL AND ((#{0}# <= {1} AND {1} <= #{2}#) OR (#{0}# <= {3} AND {3} <= #{2}#) OR (#{0}# > {1} AND #{2}# < {3})))",
                                                         calStart.ToString(CultureInfo.InvariantCulture.DateTimeFormat),
                                                         DayField,
                                                         calEnd.ToString(CultureInfo.InvariantCulture.DateTimeFormat),
                                                         EventEndField);

                // Get table with filtered events
                DataTable eventTable = DataSourceView.ToTable();

                int eventId = 0;

                // Prepare data to hashtable; event dates are used as keys so its easy to get data in OnDayRender method
                foreach (DataRow dr in eventTable.Rows)
                {
                    // Get dates of event start and end
                    var eventStart = DataHelper.GetDateTimeValue(dr, DayField, DateTimeHelper.ZERO_TIME);
                    eventStart = TimeZoneUIMethods.ConvertDateTime(eventStart, this);
                    eventStart = DateTimeHelper.GetDayStart(eventStart);

                    var eventEnd = DataHelper.GetDateTimeValue(dr, EventEndField, DateTimeHelper.ZERO_TIME);
                    eventEnd = TimeZoneUIMethods.ConvertDateTime(eventEnd, this);
                    eventEnd = DateTimeHelper.GetDayStart(eventEnd);

                    if (eventStart != DateTimeHelper.ZERO_TIME)
                    {
                        // Preserve original event start
                        var realStart = eventStart;

                        if (eventStart < calStart)
                        {
                            // Event may start earlier than calendar can display
                            eventStart = calStart;
                        }
                        if ((eventEnd == DateTimeHelper.ZERO_TIME) || (eventEnd < eventStart))
                        {
                            // Event end may not be specified
                            eventEnd = eventStart;
                        }

                        // Preserve original event end (if it is not specified then event start is used)
                        var realEnd = eventEnd;

                        if (eventEnd > calEnd)
                        {
                            // Event may end later than calendar can display
                            eventEnd = calEnd;
                        }

                        while (eventStart <= eventEnd)
                        {
                            // Try to get data from hashtable
                            var dayTable = (DataTable)MonthEvents[eventStart];
                            // Don't insert another events if DisplayOnlySingleDayItem is TRUE
                            if (dayTable == null || !DisplayOnlySingleDayItem)
                            {
                                if (dayTable == null)
                                {
                                    // Prepare table with specific structure
                                    dayTable = dr.Table.Clone();
                                }

                                // Import event data
                                dayTable.ImportRow(dr);

                                var rowIndex = dayTable.Rows.Count - 1;
                                if (SelectedItemID > 0)
                                {
                                    // Get event ID
                                    eventId = DataHelper.GetIntValue(dr, SelectedItemIDColumn);
                                }

                                // Add additional info to event record
                                dayTable.Rows[rowIndex]["__day"] = eventStart.Date.Day.ToString();
                                dayTable.Rows[rowIndex]["__month"] = eventStart.Date.Month.ToString();
                                dayTable.Rows[rowIndex]["__year"] = eventStart.Date.Year.ToString();
                                dayTable.Rows[rowIndex]["__order"] = rowIndex;
                                dayTable.Rows[rowIndex]["__selected"] = ((SelectedItemID > 0) && (SelectedItemID == eventId));
                                dayTable.Rows[rowIndex]["__first"] = (realStart == eventStart);
                                dayTable.Rows[rowIndex]["__last"] = (realEnd == eventStart);

                                // Set data with events to hashtable
                                MonthEvents[eventStart] = dayTable;
                            }

                            eventStart = eventStart.AddDays(1);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Renders particular day.
        /// </summary>
        protected override void OnDayRender(TableCell cell, CalendarDay day)
        {
            // Context must exists
            if (Context == null)
            {
                return;
            }

            // Try to get event data for particular calendar day
            DataTable dt = (DataTable)MonthEvents[day.Date];

            if ((dt != null) && (dt.Rows.Count > 0))
            {
                if (ItemTemplate != null)
                {
                    // Add event day style
                    if (DayWithEventsStyle != null)
                    {
                        cell.ApplyStyle(DayWithEventsStyle);
                    }

                    // Create repeater instance
                    Repeater rep = new Repeater();
                    // Add item template to the repeater
                    rep.ItemTemplate = ItemTemplate;
                    // Add datasource 
                    rep.DataSource = dt;
                    rep.Page = Page;
                    rep.DataBind();


                    // OnDayRender is too late to resolve dynamics control
                    if (ResolveDynamicControls)
                    {
                        ControlsHelper.ResolveDynamicControls(rep);
                    }

                    ManageDayNumber(cell);

                    // Add repeater to the cell
                    cell.Controls.Add(rep);
                }
            }
            else
            {
                // Render day with no events
                cell = NoEventRendering(cell, day);
            }

            base.OnDayRender(cell, day);
        }


        /// <summary>
        /// Renders NoEvent transformation in calendar cell.
        /// </summary>
        /// <param name="cell">Table cell</param>
        /// <param name="day">Rendered day</param>
        private TableCell NoEventRendering(TableCell cell, CalendarDay day)
        {
            if (NoEventsTemplate != null)
            {
                // Generate empty and not useable column
                string mColumnName = "Nothing";

                if (!string.IsNullOrEmpty(DayField))
                {
                    mColumnName = DayField;
                }

                // Create table to use transformation for noEvent
                DataTable mTable = new DataTable();

                // Add column to the table
                mTable.Columns.Add(new DataColumn(mColumnName));
                mTable.Columns.Add(new DataColumn("__day"));
                mTable.Columns.Add(new DataColumn("__month"));
                mTable.Columns.Add(new DataColumn("__year"));
                mTable.Rows.Add("", day.DayNumberText, day.Date.Month.ToString(), day.Date.Year.ToString());

                // Add style to day without event
                if (DayWithOutEventsStyle != null)
                {
                    cell.ApplyStyle(DayWithOutEventsStyle);
                }

                Repeater rep = new Repeater();
                rep.ItemTemplate = NoEventsTemplate;
                rep.DataSource = mTable.DefaultView;
                rep.Page = Page;
                rep.DataBind();

                // OnDayRender is too late to resolve dynamics control
                if (ResolveDynamicControls)
                {
                    ControlsHelper.ResolveDynamicControls(rep);
                }

                ManageDayNumber(cell);

                cell.Controls.Add(rep);
            }

            return cell;
        }


        /// <summary>
        /// Hides day number in calendar cell if required.
        /// </summary>
        /// <param name="cell">Table cell</param>
        protected void ManageDayNumber(TableCell cell)
        {
            if (HideDefaultDayNumber)
            {
                // Remove default day
                if ((cell.Controls.Count > 0) && (cell.Controls[0] is LiteralControl))
                {
                    cell.Controls.RemoveAt(0);
                }
            }
            else
            {
                // Render new line if Default day number is displayed
                cell.Controls.Add(new LiteralControl("<br />"));
            }
        }

        #endregion
    }
}