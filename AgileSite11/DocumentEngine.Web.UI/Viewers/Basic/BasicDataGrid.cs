using System;
using System.ComponentModel;
using System.Data;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;

using CMS.DataEngine;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.Base;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Extended version of DataGrid class that automates databinding, sorting and paging.
    /// </summary>
    [ToolboxData("<{0}:BasicDataGrid runat=server></{0}:BasicDataGrid>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class BasicDataGrid : UIDataGrid, IRelatedData
    {
        #region "Variables"

        /// <summary>
        /// Related data is loaded.
        /// </summary>
        protected bool mRelatedDataLoaded = false;

        /// <summary>
        /// Custom data connected to the object.
        /// </summary>
        protected object mRelatedData = null;

        // Indicators used as sorting arrows
        private const string cDOWN = " ▼";
        private const string cUP = " ▲";

        #endregion


        #region "Properties"

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
        /// Indicates if page index is set to the first page after sort change.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates if page index is set to the first page after sort change..")]
        public virtual bool SetFirstPageAfterSortChange
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["SetFirstPageAfterSortChange"], true);
            }
            set
            {
                ViewState["SetFirstPageAfterSortChange"] = value;
            }
        }


        /// <summary>
        /// Indicates whether data binding should be performed by default.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates whether data binding should be performed by default.")]
        public virtual bool DataBindByDefault
        {
            get
            {
                if (string.IsNullOrEmpty(Convert.ToString(ViewState["DataBindByDefault"])))
                {
                    return true;
                }
                else
                {
                    return Convert.ToBoolean(ViewState["DataBindByDefault"]);
                }
            }
            set
            {
                ViewState["DataBindByDefault"] = value;
            }
        }


        /// <summary>
        /// Indicates if sorting should be processed in DataView instead of sorting on the SQL level.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates if sorting should be processed in DataView instead of sorting on the SQL level.")]
        public virtual bool ProcessSorting
        {
            get
            {
                if (string.IsNullOrEmpty(Convert.ToString(ViewState["ProcessSorting"])))
                {
                    return true;
                }
                else
                {
                    return Convert.ToBoolean(ViewState["ProcessSorting"]);
                }
            }
            set
            {
                ViewState["ProcessSorting"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the sort field. It can be used for setting the default sort field.
        /// </summary>        
        [Category("Behavior"), DefaultValue(""), Description("Gets or sets the sort field. It can be used for setting the default sort field.")]
        public virtual string SortField
        {
            get
            {
                return Convert.ToString(ViewState["SortField"]) + "";
            }
            set
            {
                if (!String.IsNullOrEmpty(value) && value.EqualsCSafe(SortField, true))
                {
                    // Change sort direction
                    SortAscending = !SortAscending;
                }

                ViewState["SortField"] = value;
            }
        }


        /// <summary>
        /// Direction of sorting. Default value is True.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Direction of sorting. Default value is True.")]
        public virtual bool SortAscending
        {
            get
            {
                if (string.IsNullOrEmpty(Convert.ToString(ViewState["SortAscending"])))
                {
                    return true;
                }
                else
                {
                    return Convert.ToBoolean(ViewState["SortAscending"]);
                }
            }
            set
            {
                ViewState["SortAscending"] = value;
            }
        }


        /// <summary>
        /// Hides the control when no data is loaded. Default value is False.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Hides the control when no data loaded. Default value is False.")]
        public virtual bool HideControlForZeroRows
        {
            get
            {
                if (!string.IsNullOrEmpty(Convert.ToString(ViewState["HideControlForZeroRows"])))
                {
                    return Convert.ToBoolean(ViewState["HideControlForZeroRows"]);
                }
                else
                {
                    return false;
                }
            }
            set
            {
                ViewState["HideControlForZeroRows"] = value;
            }
        }


        /// <summary>
        /// Text to be shown when control is hidden by HideControlForZeroRows.
        /// </summary>        
        [Category("Behavior"), DefaultValue(""), Description("Text to be shown when control hidden by HideControlForZeroRows.")]
        public virtual string ZeroRowsText
        {
            get
            {
                return ResHelper.LocalizeString(ValidationHelper.GetString(ViewState["ZeroRowsText"], ""));
            }
            set
            {
                ViewState["ZeroRowsText"] = value;
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
        public BasicDataGrid()
            : base()
        {
            ProcessSorting = true;
            PreRender += Control_PreRender;
        }


        /// <summary>
        /// Returns the pager position based on the given string.
        /// </summary>
        /// <param name="mode">String mode representation</param>
        public PagerMode GetPagerMode(string mode)
        {
            if (mode == null)
            {
                return PagerMode.NextPrev;
            }
            else
            {
                switch (mode.ToLowerCSafe())
                {
                    case "numericpages":
                        return PagerMode.NumericPages;
                    default:
                        return PagerMode.NextPrev;
                }
            }
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
        /// OnLoad event handler. It automatically binds data unless DataBindByDefault is false.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            if (Context == null)
            {
                base.OnLoad(e);
                return;
            }

            if (!DataHelper.DataSourceIsEmpty(DataSource))
            {
                if (DataBindByDefault)
                {
                    DataBind();
                }
                base.OnLoad(e);
            }
        }


        /// <summary>
        /// OnSortCommand event handler. It sorts data by selected column.
        /// </summary>
        protected override void OnSortCommand(DataGridSortCommandEventArgs e)
        {
            // Set sort field. Remove "▼ ▲" characters from columnNames
            SortField = RemoveSortingArrows(e.SortExpression);

            // Go to the first page only if it is enabled
            if (SetFirstPageAfterSortChange)
            {
                CurrentPageIndex = 0;
            }

            DataBind();

            base.OnSortCommand(e);
        }


        /// <summary>
        /// OnPageIndexChanged event handler. It ensures displaying of the given page.
        /// </summary>
        protected override void OnPageIndexChanged(DataGridPageChangedEventArgs e)
        {
            //if (!DataHelper.DataSourceIsEmpty(this.DataSource))
            {
                // Set CurrentPageIndex to the given page
                if (e.NewPageIndex < PageCount)
                {
                    CurrentPageIndex = e.NewPageIndex;
                }
                else
                {
                    CurrentPageIndex = PageCount - 1;
                }

                // Rebind data
                DataBind();

                base.OnPageIndexChanged(e);
            }
        }


        /// <summary>
        /// Render event handler, to maintain the proper control rendering.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (Context == null)
            {
                base.Render(writer);
            }
            // Hide the control when HideControlForZeroRows is set and there is no data to display
            if (HideControlForZeroRows)
            {
                if (DataHelper.DataSourceIsEmpty(DataSource))
                {
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(ZeroRowsText))
            {
                // If the ZeroRowsText is set, display it instead if the control itself
                if (DataHelper.DataSourceIsEmpty(DataSource))
                {
                    writer.Write(ZeroRowsText);
                    return;
                }
            }
            // Else regular rendering
            base.Render(writer);
        }


        /// <summary>
        /// Control prerender code.
        /// </summary>
        protected void Control_PreRender(object sender, EventArgs e)
        {
        }


        /// <summary>
        /// Binds data to the grid and displays the sorting symbols.
        /// </summary>
        /// <remarks>Sorting: if ProcessSorting is true, rows are sorted in the DataView by default.</remarks>    
        public override void DataBind()
        {
            // Process data binding only if the control is NOT in design mode
            if (Context == null)
            {
                return;
            }

            if (DataSource is DataSet)
            {
                if (((DataSet)(DataSource)).Tables.Count > 0)
                {
                    DataSource = ((DataSet)(DataSource)).Tables[0].DefaultView;
                }
            }
            if (DataSource is DataTable)
            {
                DataSource = ((DataTable)(DataSource)).DefaultView;
            }
            if (DataSource is DataView)
            {
                if (AllowSorting)
                {
                    // Add sorting and sorting arrows
                    AddSorting();
                }
            }

            if (DataHelper.DataSourceIsEmpty(DataSource))
            {
                DataSource = new DataSet();
                ((DataSet)DataSource).Tables.Add();
                AutoGenerateColumns = false;
            }

            // If no datasource given, exit
            if (DataSource != null)
            {
                base.DataBind();
            }
        }


        /// <summary>
        /// Adds the sorting of the DataSource. Inserts sorting arrows into the header columns.
        /// </summary>
        private void AddSorting()
        {
            //  set sorting
            if ((SortField == null) || string.IsNullOrEmpty(SortField.Trim()))
            {
                if (AutoGenerateColumns)
                {
                    // Sort by first column of DataSource
                    DataView dv = (DataView)DataSource;
                    if (dv.Table.Columns.Count > 0)
                    {
                        SortField = RemoveSortingArrows(dv.Table.Columns[0].ColumnName);
                    }
                }
                else
                {
                    // Sort by first column of DataGrid
                    if (Columns.Count > 0)
                    {
                        SortField = RemoveSortingArrows(Columns[0].SortExpression);
                    }
                }
            }

            string sort = null;
            if (ProcessSorting && (!string.IsNullOrEmpty(SortField)) && (!DataHelper.DataSourceIsEmpty(DataSource)))
            {
                sort = SortAscending ? SortField : SqlHelper.ReverseOrderBy(SortField);
            }

            // Sort the grid
            if (sort != null)
            {
                foreach (DataColumn dataCol in ((DataView)(DataSource)).Table.Columns)
                {
                    dataCol.ColumnName = RemoveSortingArrows(dataCol.ColumnName);
                }

                ((DataView)(DataSource)).Sort = RemoveSortingArrows(sort);
            }

            // Add sorting symbols either to automatically generated columns or by specified data bound columns
            if (AutoGenerateColumns)
            {
                // Automatically generated columns
                foreach (DataColumn dataCol in ((DataView)(DataSource)).Table.Columns)
                {
                    var col = dataCol;

                    // Remove previous sorting symbol
                    col.ColumnName = RemoveSortingArrows(col.ColumnName);

                    // Add new sorting symbol
                    if (SqlHelper.OrderByContains(sort, col.ColumnName, true))
                    {
                        col.ColumnName = col.ColumnName + cUP;
                    }
                    else if (SqlHelper.OrderByContains(sort, col.ColumnName, false))
                    {
                        col.ColumnName = col.ColumnName + cDOWN;
                    }
                }
            }
            else
            {
                foreach (DataGridColumn dataCol in Columns)
                {
                    var col = dataCol;

                    // Remove previous sorting symbol
                    col.HeaderText = RemoveSortingArrows(col.HeaderText);

                    // Add new sorting symbol
                    if (sort != null)
                    {
                        if (SqlHelper.OrderByContains(sort, col.SortExpression, true))
                        {
                            col.HeaderText = col.HeaderText + cUP;
                        }
                        else if (SqlHelper.OrderByContains(sort, col.SortExpression, false))
                        {
                            col.HeaderText = col.HeaderText + cDOWN;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Remove "▼ ▲" characters from columnNames.
        /// </summary>
        /// <param name="str">The string to modify</param>
        private string RemoveSortingArrows(string str)
        {
            return str.Replace(cUP, "").Replace(cDOWN, "");
        }

        #endregion
    }
}