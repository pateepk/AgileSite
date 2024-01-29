using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.DocumentEngine.Web.UI;
using CMS.DataEngine;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// UniMatrix base class.
    /// </summary>
    public abstract class UniMatrix : CMSUserControl, ICallbackEventHandler, IUniPageable
    {
        #region "Events"

        /// <summary>
        /// Item changed event.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="rowItemId">ID of the row item</param>
        /// <param name="colItemId">ID of the column item</param>
        /// <param name="newState">New item state (true if checked)</param>
        public delegate void ItemChangedEventHandler(object sender, int rowItemId, int colItemId, bool newState);


        /// <summary>
        /// Fires when the item changed.
        /// </summary>
        public event ItemChangedEventHandler OnItemChanged;


        /// <summary>
        /// Check if additional CSS  class should be rendered for current row event.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="rowData">Data to be checked</param>
        public delegate string GetRowItemCssClassEventHandler(object sender, DataRow rowData);


        /// <summary>
        /// Fires when data are checked for additional CSS class to be rendered.
        /// </summary>
        public event GetRowItemCssClassEventHandler OnGetRowItemCssClass;


        /// <summary>
        /// Occurs if the matrix wants to check the permission to edit particular item.
        /// </summary>
        /// <param name="value">Column value</param>
        public delegate bool OnCheckPermissions(object value);


        /// <summary>
        /// Fires when column permissions are checked
        /// </summary>
        public event OnCheckPermissions CheckColumnPermissions;


        /// <summary>
        /// Fires when row permissions are checked
        /// </summary>
        public event OnCheckPermissions CheckRowPermissions;


        /// <summary>
        /// Occurs when the control bind data.
        /// </summary>
        public event EventHandler<EventArgs> OnPageBinding;


        /// <summary>
        /// Occurs when the pager change the page and current mode is postback => reload data
        /// </summary>
        public event EventHandler<EventArgs> OnPageChanged;


        /// <summary>
        /// Occurs when data has been loaded. Allows manipulation with data.
        /// </summary>
        /// <param name="ds">Loaded dataset</param>
        public delegate void OnMatrixDataLoaded(DataSet ds);


        /// <summary>
        /// Fires when the data is loaded
        /// </summary>
        public event OnMatrixDataLoaded DataLoaded;

        #endregion


        #region "Variables"

        private bool mEnabled = true;
        private string mResourcePrefix = "general";

        private bool mShowHeaderRow = true;
        private bool mShowFilterRow = true;

        private int? mFilterLimit;

        /// <summary>
        /// List of columns which are disabled
        /// </summary>
        protected ArrayList disabledColumns = new ArrayList();


        private int mDefaultPageSize = 20;
        private string mCornerText = string.Empty;
        private readonly Hashtable mColumnPermissions = new Hashtable();
        private readonly Hashtable mRowPermissions = new Hashtable();
        private string mCallbackResult;

        /// <summary>
        /// Indicates if the content was already loaded or not
        /// </summary>
        protected bool mLoaded;

        /// <summary>
        /// DataSet with the content
        /// </summary>
        protected DataSet ds;

        /// <summary>
        /// Total number of rows
        /// </summary>
        protected int mTotalRows;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the site id column name
        /// </summary>
        public string SiteIDColumnName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value that indicates whether '(global)' suffix should be added to the global objects
        /// </summary>
        public bool AddGlobalObjectSuffix
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the enabled state of the control.
        /// </summary>
        public virtual bool Enabled
        {
            get
            {
                return mEnabled;
            }
            set
            {
                mEnabled = value;
            }
        }


        /// <summary>
        /// Prefix for the resource strings which will be used for the strings of the selector.
        /// Default is "General".
        /// </summary>
        public override string ResourcePrefix
        {
            get
            {
                return mResourcePrefix;
            }
            set
            {
                mResourcePrefix = value;
            }
        }


        /// <summary>
        /// Base where condition for the query.
        /// </summary>
        public virtual string WhereCondition
        {
            get;
            set;
        }


        /// <summary>
        /// Base order of the items for the query.
        /// </summary>
        public virtual string OrderBy
        {
            get;
            set;
        }


        /// <summary>
        /// Default number of items per page.
        /// </summary>
        public virtual int ItemsPerPage
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["ItemsPerPage"], 20);
            }
            set
            {
                ViewState["ItemsPerPage"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the codename column name.
        /// </summary>
        public virtual string RowItemCodeNameColumn
        {
            get;
            set;
        }


        /// <summary>
        /// Display name column of the row.
        /// </summary>
        public virtual string RowItemDisplayNameColumn
        {
            get;
            set;
        }


        /// <summary>
        /// Display name column of the column.
        /// </summary>
        public virtual string ColumnItemDisplayNameColumn
        {
            get;
            set;
        }


        /// <summary>
        /// ID column of the row.
        /// </summary>
        public virtual string RowItemIDColumn
        {
            get;
            set;
        }


        /// <summary>
        /// ID column of the column.
        /// </summary>
        public virtual string ColumnItemIDColumn
        {
            get;
            set;
        }


        /// <summary>
        /// Tooltip column of the row.
        /// </summary>
        public virtual string RowItemTooltipColumn
        {
            get;
            set;
        }


        /// <summary>
        /// Tooltip column of the column.
        /// </summary>
        public virtual string ColumnItemTooltipColumn
        {
            get;
            set;
        }


        /// <summary>
        /// Tooltip column of the item.
        /// </summary>
        public virtual string ItemTooltipColumn
        {
            get;
            set;
        }


        /// <summary>
        /// Query name to get the data.
        /// </summary>
        public virtual string QueryName
        {
            get;
            set;
        }


        /// <summary>
        /// Query parameters to get the data.
        /// </summary>
        public virtual QueryDataParameters QueryParameters
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the header row is shown.
        /// </summary>
        public virtual bool ShowHeaderRow
        {
            get
            {
                return mShowHeaderRow;
            }
            set
            {
                mShowHeaderRow = value;
            }
        }


        /// <summary>
        /// If true, the filter row is shown.
        /// </summary>
        public virtual bool ShowFilterRow
        {
            get
            {
                return mShowFilterRow;
            }
            set
            {
                mShowFilterRow = value;
            }
        }


        /// <summary>
        /// Gets the value that indicates whether current matrix contains some data.
        /// </summary>
        public virtual bool HasData
        {
            get
            {
                return false;
            }
            protected set
            {
            }
        }


        /// <summary>
        /// Minimal count of entries for display filter.
        /// </summary>
        public virtual int FilterLimit
        {
            get
            {
                if (mFilterLimit == null)
                {
                    mFilterLimit = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSDefaultListingFilterLimit"], 25);
                }
                return mFilterLimit.Value;
            }
            set
            {
                mFilterLimit = value;
            }
        }


        /// <summary>
        /// If true, the filling column is added to the table to move columns to the left
        /// </summary>
        public bool AddFillingColumn
        {
            get;
            set;
        }


        /// <summary>
        /// Number of expected matrix columns.
        /// </summary>
        public int ColumnsCount
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["ColumnsCount"], 10);
            }
            set
            {
                ViewState["ColumnsCount"] = value;
            }
        }


        /// <summary>
        /// Default page size at first load.
        /// </summary>
        public virtual int DefaultPageSize
        {
            get
            {
                if ((mDefaultPageSize <= 0) && (mDefaultPageSize != -1))
                {
                    mDefaultPageSize = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSDefaultListingPageSize"], 20);
                }
                return mDefaultPageSize;
            }
            set
            {
                mDefaultPageSize = value;
            }
        }


        /// <summary>
        /// Filter where condition.
        /// </summary>
        protected string FilterWhere
        {
            get
            {
                return ValidationHelper.GetString(ViewState["FilterWhere"], "");
            }
            set
            {
                ViewState["FilterWhere"] = value;
            }
        }


        /// <summary>
        /// Number of expected matrix columns.
        /// </summary>
        public string ColumnsPreferedOrder
        {
            get
            {
                return ValidationHelper.GetString(ViewState["ColumnsPreferedOrder"], "");
            }
            set
            {
                ViewState["ColumnsPreferedOrder"] = value;
            }
        }


        /// <summary>
        /// Sets or gets fixed width of first column.
        /// </summary>
        public string FirstColumnClass
        {
            get
            {
                return ValidationHelper.GetString(ViewState["FirstColumnClass"], string.Empty);
            }
            set
            {
                ViewState["FirstColumnClass"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the message which is displayed if there are no records.
        /// </summary>
        public string NoRecordsMessage
        {
            get;
            set;
        }


        /// <summary>
        /// Text displayed in the upper left corner of UniMatrix, if filter is not shown.
        /// </summary>
        public string CornerText
        {
            get
            {
                return mCornerText;
            }
            set
            {
                mCornerText = value;
            }
        }


        /// <summary>
        /// Indicates if content before rows should be displayed.
        /// </summary>
        public bool ShowContentBeforeRow
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets CSS class for content before rows.
        /// </summary>
        public string ContentBeforeRowCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Gets order in which data will be rendered.
        /// </summary>
        public int[] ColumnOrderIndex
        {
            get
            {
                return ViewState["ColumnOrderIndex"] as int[];
            }
            set
            {
                ViewState["ColumnOrderIndex"] = value;
            }
        }


        /// <summary>
        /// Mark HTML code for the disabled column in header.
        /// </summary>
        public string DisabledColumnMark
        {
            get;
            set;
        }


        /// <summary>
        /// Mark HTML code for the disabled row in header.
        /// </summary>
        public string DisabledRowMark
        {
            get;
            set;
        }


        /// <summary>
        /// CSS class of the matrix control
        /// </summary>
        public string CssClass
        {
            get;
            set;
        }


        /// <summary>
        /// UniPager control of UniMatrix.
        /// </summary>
        public abstract UniPager Pager
        {
            get;
        }


        /// <summary>
        /// Gets or sets HTML content to be rendered as additional content on the top of the matrix.
        /// </summary>
        public abstract TableRow ContentBeforeRow
        {
            get;
        }


        /// <summary>
        /// Pager data item object.
        /// </summary>
        public object PagerDataItem
        {
            get
            {
                return ds;
            }
            set
            {
                ds = (DataSet)value;
            }
        }


        /// <summary>
        /// Pager control.
        /// </summary>
        public UniPager UniPagerControl
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the number of result. Enables proceed "fake" datasets, where number 
        /// of results in the dataset is not correspondent to the real number of results
        /// This property must be equal -1 if should be disabled
        /// </summary>
        public int PagerForceNumberOfResults
        {
            get
            {
                return mTotalRows;
            }
            set
            {
            }
        }


        /// <summary>
        /// Page size options for pager.
        /// Numeric values or macro ##ALL## separated with comma.
        /// </summary>
        public abstract string PageSizeOptions
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Raises the OnItemChanged event.
        /// </summary>
        protected void RaiseOnItemChanged(int rowItemId, int colItemId, bool newState)
        {
            if (OnItemChanged != null)
            {
                OnItemChanged(this, rowItemId, colItemId, newState);
            }
        }


        /// <summary>
        /// Raises the OnGetRowItemCssClass event.
        /// </summary>
        protected string RaiseGetRowItemCssClass(DataRow dr)
        {
            if (OnGetRowItemCssClass != null)
            {
                return OnGetRowItemCssClass(this, dr);
            }
            return "";
        }


        /// <summary>
        /// Reloads the control data.
        /// </summary>
        /// <param name="forceReload">Force the reload of the control</param>
        public virtual void ReloadData(bool forceReload)
        {
            // Do nothing in base class
        }


        /// <summary>
        /// Disables column with specified index.
        /// </summary>
        /// <param name="index">Index of the column to disable</param>
        public virtual void DisableColumn(int index)
        {
            if (!disabledColumns.Contains(index))
            {
                disabledColumns.Add(index);
            }
        }


        /// <summary>
        /// Enables column with specified index.
        /// </summary>
        /// <param name="index">Index of the column to enable</param>
        public virtual void EnableColumn(int index)
        {
            if (disabledColumns.Contains(index))
            {
                disabledColumns.Remove(index);
            }
        }


        /// <summary>
        /// Gets matrix order by clause
        /// </summary>
        protected string GetOrderByClause()
        {
            // Prepare the order by
            string orderBy = OrderBy;
            if (orderBy == null)
            {
                orderBy = RowItemDisplayNameColumn + SqlHelper.ORDERBY_ASC;

                // Add additional sorting by codename for equal display names
                if (!String.IsNullOrEmpty(RowItemCodeNameColumn))
                {
                    orderBy += ", " + RowItemCodeNameColumn;
                }

                if (ColumnsCount > 1)
                {
                    orderBy += ", " + ColumnItemDisplayNameColumn + SqlHelper.ORDERBY_ASC;
                }
            }

            return orderBy;
        }


        /// <summary>
        /// Sets the filter where condition
        /// </summary>
        /// <param name="expr">Filter expression</param>
        protected void SetFilterWhere(string expr)
        {
            if (!String.IsNullOrEmpty(expr))
            {
                // Build the where condition for display name
                FilterWhere = RowItemDisplayNameColumn + " LIKE '%" + SqlHelper.EscapeLikeText(SqlHelper.EscapeQuotes(expr)) + "%'";
            }
            else
            {
                FilterWhere = null;
            }
        }


        /// <summary>
        /// Returns true if the given row is editable.
        /// </summary>
        /// <param name="rowValue">Row value</param>
        protected bool IsRowEditable(object rowValue)
        {
            if (CheckRowPermissions == null)
            {
                return true;
            }

            // Try to get cached value
            object editableObj = mRowPermissions[rowValue];
            if (editableObj == null)
            {
                // Get by external function
                editableObj = CheckRowPermissions(rowValue);
                mRowPermissions[rowValue] = editableObj;
            }

            return ValidationHelper.GetBoolean(editableObj, true);
        }


        /// <summary>
        /// Returns true if the given column is editable.
        /// </summary>
        /// <param name="columnValue">Column value</param>
        protected bool IsColumnEditable(object columnValue)
        {
            if (CheckColumnPermissions == null)
            {
                return true;
            }

            // Try to get cached value
            object editableObj = mColumnPermissions[columnValue];
            if (editableObj == null)
            {
                // Get by external function
                editableObj = CheckColumnPermissions(columnValue);
                mColumnPermissions[columnValue] = editableObj;
            }

            return ValidationHelper.GetBoolean(editableObj, true);
        }


        #region "ICallbackHandler methods"

        /// <summary>
        /// Gets the callback result.
        /// </summary>
        public string GetCallbackResult()
        {
            return mCallbackResult;
        }


        /// <summary>
        /// Processes the callback event.
        /// </summary>
        public void RaiseCallbackEvent(string eventArgument)
        {
            string[] parameters = eventArgument.Split(new [] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (parameters.Length == 4)
            {
                int rowItemId = ValidationHelper.GetInteger(parameters[1], 0);
                int colItemId = ValidationHelper.GetInteger(parameters[2], 0);
                bool newState = ValidationHelper.GetBoolean(parameters[3], false);

                // Raise the change
                RaiseOnItemChanged(rowItemId, colItemId, newState);

                // If row before content should be shown and displayed data was changed, render new HTML
                if (ShowContentBeforeRow || (ContentBeforeRow.Visible && (ContentBeforeRow.Cells.Count > 0)))
                {
                    mCallbackResult = ContentBeforeRow.GetRenderedHTML();
                }
            }
        }

        #endregion


        /// <summary>
        /// Evokes control databind.
        /// </summary>
        public void ReBind()
        {
            if (OnPageChanged != null)
            {
                OnPageChanged(this, null);
            }

            DataBind();
        }


        /// <summary>
        /// Gets an array of indexes which are sorted according to ColumnsPreferedOrder.
        /// i.e: Permission are "A","B","C" .. desired permission order is "C","A","B", then columnOrderIndex will be [2,0,1]
        /// </summary>
        /// <param name="columns">The columns</param>
        /// <param name="columnOrder">The column order</param>
        protected int[] GetColumnIndexes(IList<DataRow> columns, string columnOrder)
        {
            List<string> order = new List<string>(columnOrder.Split(','));
            List<bool> indexIsSet = new List<bool>();
            List<int> columnOrderIndex = new List<int>();

            // Initialize array
            for (int i = 0; i < columns.Count; i++)
            {
                indexIsSet.Add(false);
            }

            // Sort according to defined order
            foreach (DataRow dr in columns)
            {
                int index = order.IndexOf(DataHelper.GetStringValue(dr, "PermissionName"));
                if (index > -1)
                {
                    columnOrderIndex.Add(index);
                    indexIsSet[index] = true;
                }
            }

            // Insert original colums which were not defined in columnOrder
            for (int i = 0; i < indexIsSet.Count; i++)
            {
                if (indexIsSet[i] == false)
                {
                    columnOrderIndex.Add(i);
                    indexIsSet[i] = true;
                }
            }

            return columnOrderIndex.ToArray();
        }


        /// <summary>
        /// Returns additional CSS class formatted in a way to be concatenated with other row CSS classes.
        /// </summary>
        /// <param name="dr">DataRow with matrix row data</param>
        private string GetAdditionalCssClass(DataRow dr)
        {
            string cssClass = RaiseGetRowItemCssClass(dr);
            if (!String.IsNullOrEmpty(cssClass))
            {
                cssClass = " " + cssClass;
            }
            return cssClass;
        }


        /// <summary>
        /// Returns safe and localized tooltip from the given source column.
        /// </summary>
        /// <param name="dr">Data row with the tooltip column</param>
        /// <param name="columnName">Name of the tooltip source column</param>
        private string GetTooltip(DataRow dr, string columnName)
        {
            // Get tooltip string
            string tooltip = DataHelper.GetStringValue(dr, columnName);

            // Get safe an localized tooltip
            if (!string.IsNullOrEmpty(tooltip))
            {
                return HTMLHelper.HTMLEncode(MacroResolver.Resolve(tooltip));
            }

            return "";
        }


        /// <summary>
        /// Generate header of the matrix.
        /// </summary>
        /// <param name="matrixData">Data of the matrix to be generated</param>
        /// <param name="headerRow">Header row</param>
        protected void GenerateMatrixHeader(List<DataRow> matrixData, TableHeaderRow headerRow)
        {
            // Prepare matrix header
            foreach (int index in ColumnOrderIndex)
            {
                DataRow dr = matrixData[index];

                if (ShowHeaderRow)
                {
                    // Create header cell
                    var thc = new TableHeaderCell
                    {
                        Scope = TableHeaderScope.Column,
                        Text = HTMLHelper.HTMLEncode(MacroResolver.Resolve(Convert.ToString(dr[ColumnItemDisplayNameColumn]))),
                        ToolTip = (ColumnItemTooltipColumn != null) ? GetTooltip(dr, ItemTooltipColumn) : null,
                        EnableViewState = false
                    };
                    headerRow.Cells.Add(thc);

                    // Add disabled mark if needed
                    if (!IsColumnEditable(dr[ColumnItemIDColumn]))
                    {
                        thc.Text += DisabledColumnMark;
                    }
                }
                else
                {
                    // Create header cell
                    var thc = new TableHeaderCell
                    {
                        Scope = TableHeaderScope.Column,
                        Text = "&nbsp;",
                        EnableViewState = false
                    };
                    headerRow.Cells.Add(thc);
                }
            }

            if (AddFillingColumn)
            {
                AddFillingCell<TableHeaderCell>(headerRow);
            }
        }


        /// <summary>
        /// Generate body of the matrix.
        /// </summary>
        /// <param name="matrixData">Data of the matrix to be generated</param>
        /// <param name="tblMatrix">Matrix table</param>
        protected void GenerateMatrixBody(List<DataRow> matrixData, Table tblMatrix)
        {
            string lastRowId = "";
            int colIndex = 0;
            int rowIndex = 0;

            TableRow tr = null;

            // Render matrix rows
            int step = matrixData.Count;
            for (int i = 0; i < ds.Tables[0].Rows.Count; i = i + step)
            {
                foreach (int index in ColumnOrderIndex)
                {
                    DataRow rowData = ds.Tables[0].Rows[i + index];
                    string rowId = ValidationHelper.GetString(rowData[RowItemIDColumn], "");

                    // Detect new matrix row
                    TableCell tc;

                    if (rowId != lastRowId)
                    {
                        if ((ItemsPerPage > 0) && (rowIndex++ >= ItemsPerPage))
                        {
                            break;
                        }

                        if (AddFillingColumn)
                        {
                            AddFillingCell<TableCell>(tr);
                        }

                        // New Table row
                        tr = new TableRow
                        {
                            CssClass = GetAdditionalCssClass(rowData),
                            EnableViewState = false
                        };
                        tblMatrix.Rows.Add(tr);

                        // Header table cell
                        tc = new TableCell
                        {
                            CssClass = "matrix-header",
                            Text = HTMLHelper.HTMLEncode(MacroResolver.Resolve(ValidationHelper.GetString(rowData[RowItemDisplayNameColumn], null))),
                            ToolTip = (RowItemTooltipColumn != null) ? GetTooltip(rowData, RowItemTooltipColumn) : null,
                            EnableViewState = false
                        };
                        tr.Cells.Add(tc);

                        // Add disabled mark if needed
                        if (!IsRowEditable(rowId))
                        {
                            tc.Text += DisabledRowMark;
                        }

                        // Add global suffix if is required
                        if ((index == 0) && (AddGlobalObjectSuffix) && (ValidationHelper.GetInteger(rowData[SiteIDColumnName], 0) == 0))
                        {
                            tc.Text += " " + GetString("general.global");
                        }

                        // Update 
                        lastRowId = rowId;
                        colIndex = 0;
                    }

                    object columnValue = rowData[ColumnItemIDColumn];
                    var cellId = string.Format("chk:{0}:{1}", rowId, columnValue);

                    // New table cell
                    tc = new TableCell
                    {
                        EnableViewState = false
                    };
                    tr.Cells.Add(tc);

                    // Checkbox for data
                    var chk = new CMSCheckBox
                    {
                        ID = cellId,
                        ClientIDMode = ClientIDMode.Static,
                        ToolTip = GetTooltip(rowData, ItemTooltipColumn),
                        Checked = ValidationHelper.GetBoolean(rowData["Allowed"], false),
                        Enabled = Enabled &&
                                  !disabledColumns.Contains(colIndex) &&
                                  IsColumnEditable(columnValue) &&
                                  IsRowEditable(rowId),
                        EnableViewState = false
                    };
                    tc.Controls.Add(chk);

                    // Add click event to enabled checkbox
                    if (chk.Enabled)
                    {
                        chk.Attributes.Add("onclick", "UM_ItemChanged_" + ClientID + "(this);");
                    }

                    colIndex++;
                }
            }

            if (AddFillingColumn)
            {
                AddFillingCell<TableCell>(tr);
            }
        }


        /// <summary>
        /// Adds the filling cell to the table
        /// </summary>
        /// <typeparam name="TCell">Cell type</typeparam>
        /// <param name="tr">Table row to which add the cell</param>
        public static void AddFillingCell<TCell>(TableRow tr)
            where TCell : TableCell, new()
        {
            if (tr == null)
            {
                return;
            }

            // New table cell
            var tc = new TCell
            {
                EnableViewState = false,
                CssClass = "filling-column"
            };

            tc.Controls.Add(new LiteralControl("&nbsp;"));

            tr.Cells.Add(tc);
        }


        /// <summary>
        /// Raises the page binding event
        /// </summary>
        protected void RaiseOnPageBinding()
        {
            // Call page binding event
            if (OnPageBinding != null)
            {
                OnPageBinding(this, null);
            }
        }


        /// <summary>
        /// Load matrix data
        /// </summary>
        /// <param name="whereCondition">Where condition to filter data</param>
        /// <param name="orderBy">Order by clause to sort data</param>
        /// <param name="currentPage">Current data page to be displayed</param>
        /// <param name="displayOnlyHeader">Indicates if only matrix header should be displayed to user</param>
        protected List<DataRow> LoadData(string whereCondition, string orderBy, int currentPage, ref bool displayOnlyHeader)
        {
            List<DataRow> matrixData = null;
            bool load = true;

            // Load the data
            while (load)
            {
                // Get specific page
                int pageItems = ColumnsCount * Pager.PageSize;
                ds = ConnectionHelper.ExecuteQuery(QueryName, QueryParameters, whereCondition, orderBy, 0, null, (currentPage - 1) * pageItems, pageItems, ref mTotalRows);

                HasData = !DataHelper.DataSourceIsEmpty(ds);

                // If no records found, get the records for the original dataset
                if (!HasData && !String.IsNullOrEmpty(FilterWhere))
                {
                    // Get only first line
                    ds = ConnectionHelper.ExecuteQuery(QueryName, QueryParameters, WhereCondition, orderBy, ColumnsCount);
                    HasData = !DataHelper.DataSourceIsEmpty(ds);
                    displayOnlyHeader = true;
                }

                // Load the list of columns
                if (HasData)
                {
                    if (DataLoaded != null)
                    {
                        DataLoaded(ds);
                    }

                    matrixData = DataHelper.GetUniqueRows(ds.Tables[0], ColumnItemIDColumn);
                    ColumnOrderIndex = GetColumnIndexes(matrixData, ColumnsPreferedOrder);

                    // If more than current columns count found, and there is more data, get the correct data again
                    if ((matrixData.Count <= ColumnsCount) || (mTotalRows < pageItems))
                    {
                        load = false;
                    }
                    else
                    {
                        ColumnsCount = matrixData.Count;
                    }
                }
                else
                {
                    load = false;
                }
            }

            return matrixData;
        }


        /// <summary>
        /// Sets the page size
        /// </summary>
        /// <param name="forceReload">If true, the reload is forced</param>
        /// <param name="pageSizeDropdown">Page size dropdown control</param>
        protected void SetPageSize(bool forceReload, DropDownList pageSizeDropdown)
        {
            if ((pageSizeDropdown.Items.Count == 0) || forceReload)
            {
                pageSizeDropdown.Items.Clear();

                string[] sizes = PageSizeOptions.Split(',');
                if (sizes.Length > 0)
                {
                    List<int> sizesInt = new List<int>();
                    // Indicates if contains 'Select ALL' macro
                    bool containsAll = false;
                    foreach (string size in sizes)
                    {
                        if (size.ToUpperCSafe() == "##ALL##")
                        {
                            containsAll = true;
                        }
                        else
                        {
                            sizesInt.Add(ValidationHelper.GetInteger(size.Trim(), 0));
                        }
                    }
                    // Add default page size if not present
                    if ((DefaultPageSize > 0) && !sizesInt.Contains(DefaultPageSize))
                    {
                        sizesInt.Add(DefaultPageSize);
                    }
                    // Sort list of page sizes
                    sizesInt.Sort();

                    ListItem item;

                    foreach (int size in sizesInt)
                    {
                        // Skip zero values
                        if (size != 0)
                        {
                            item = new ListItem(size.ToString());
                            if (item.Value == DefaultPageSize.ToString())
                            {
                                item.Selected = true;
                            }
                            pageSizeDropdown.Items.Add(item);
                        }
                    }
                    // Add 'Select ALL' macro at the end of list
                    if (containsAll)
                    {
                        item = new ListItem(GetString("general.selectall"), "-1");
                        if (DefaultPageSize == -1)
                        {
                            item.Selected = true;
                        }
                        pageSizeDropdown.Items.Add(item);
                    }
                }
            }
        }

        #endregion
    }
}