using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DataEngine.CollectionExtensions;
using CMS.DocumentEngine.Web.UI;
using CMS.EventLog;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;

namespace CMS.UIControls
{
    /// <summary>
    /// UniSelector's selection dialog base class.
    /// </summary>
    public abstract class SelectionDialog : CMSUserControl, IObjectTypeDriven, ICallbackEventHandler
    {
        #region "Variables"

        private string txtClientId;
        private string hdnClientId;
        private string hdnDrpId;
        private string hashId;
        private string callbackValues;

        private string whereCondition;
        private string orderBy;
        private string siteWhereCondition;

        private string emptyReplacement = "&nbsp;";
        private string dialogGridName = "~/CMSAdminControls/UI/UniSelector/DialogItemList.xml";
        private string additionalSearchColumns = String.Empty;
        private string parentClientId;
        private string additionalColumns;
        private string callbackMethod;
        private string disabledItems;
        private string filterMode;
        private string displayNameFormat;
        private string filterControl;
        private string securityPurpose;

        private string mSearchColumns;
        private string mZeroRowsText;
        private string mFilteredZeroRowsText;
        private string mGlobalObjectSuffix;

        private bool allowEditTextBox;
        private bool fireOnChanged;
        private bool allRowsChecked = true;
        private bool useDefaultNameFilter = true;
        private bool mHasDependingFields;

        private int itemsPerPage = 10;

        private Hashtable parameters;
        private CMSAbstractBaseFilterControl searchControl;
        private GeneralizedInfo iObjectType;

        private SelectionModeEnum selectionMode = SelectionModeEnum.SingleButton;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether to remove multiple commas (can happen when DisplayNameFormat is like {%column1%}, {%column2%}, {column3} and column2 is empty.
        /// </summary>
        public bool RemoveMultipleCommas
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the selector uses the type condition to get the data
        /// </summary>
        public bool UseTypeCondition
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or set the suffix which is added to global objects if AddGlobalObjectSuffix is true. Default is "(global)".
        /// </summary>
        public string GlobalObjectSuffix
        {
            get
            {
                if (string.IsNullOrEmpty(mGlobalObjectSuffix))
                {
                    mGlobalObjectSuffix = GetString("general.global");
                }
                return mGlobalObjectSuffix;
            }
            set
            {
                mGlobalObjectSuffix = value;
            }
        }


        /// <summary>
        /// Indicates whether global objects have suffix "(global)" in the grid.
        /// </summary>
        public bool AddGlobalObjectSuffix
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether global object name should have prefix '.'
        /// </summary>
        public bool AddGlobalObjectNamePrefix
        {
            get;
            set;
        }


        /// <summary>
        /// Specifies whether the selection dialog should resolve localization macros.
        /// </summary>
        public bool LocalizeItems
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Item prefix.
        /// </summary>
        public string ItemPrefix
        {
            get
            {
                return ValidationHelper.GetString(ViewState["ItemPrefix"], string.Empty);
            }
            set
            {
                ViewState["ItemPrefix"] = value;
            }
        }


        /// <summary>
        /// Contains current where condition used for filtering.
        /// </summary>
        public string FilterWhere
        {
            get
            {
                return ValidationHelper.GetString(ViewState["FilterWhere"], string.Empty);
            }
            set
            {
                ViewState["FilterWhere"] = value;
            }
        }


        /// <summary>
        /// Gets or sets current object type.
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the trimmed search text.
        /// </summary>
        private string TrimmedSearchText
        {
            get
            {
                return DefaultFilterInput.Text.Trim();
            }
        }


        /// <summary>
        /// Indicates whether localized filtering is allowed.
        /// </summary>
        private bool AllowLocalizedFiltering
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether display name column was selected as search column.
        /// </summary>
        private bool DisplayNameSelectedAsSearchColumn
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["DisplayNameSelected"], false);
            }
            set
            {
                ViewState["DisplayNameSelected"] = value;
            }
        }


        /// <summary>
        /// Indicates whether localized filtering can be used for current selection dialog and is enabled.
        /// </summary>
        private bool UseLocalizedFiltering
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["UseLocalizedFiltering"], false)
                    && DisplayNameSelectedAsSearchColumn
                    && AllowLocalizedFiltering
                    && !String.IsNullOrEmpty(TrimmedSearchText);
            }
            set
            {
                ViewState["UseLocalizedFiltering"] = value;
            }
        }


        /// <summary>
        /// Indicates if the selection mode is one of the multiple modes.
        /// </summary>
        private bool IsMultipleMode
        {
            get
            {
                return selectionMode == SelectionModeEnum.Multiple || selectionMode == SelectionModeEnum.MultipleButton || selectionMode == SelectionModeEnum.MultipleTextBox;
            }
        }


        /// <summary>
        /// Returns character used for item separation in multiple modes.
        /// </summary>
        private char ValueSeparator
        {
            get;
            set;
        } = ';';


        /// <summary>
        /// Returns name of the column which value(s) is/are returned by the dialog.
        /// </summary>
        private string ReturnColumnName
        {
            get;
            set;
        }

        #endregion


        #region "Inner controls"

        /// <summary>
        /// Unigrid for item selection.
        /// </summary>
        public virtual UniGrid UniGrid => null;


        /// <summary>
        /// Hidden field for storing selected items.
        /// </summary>
        protected virtual HiddenField ItemsField => null;


        /// <summary>
        /// Panel containing default filter.
        /// </summary>
        protected virtual Panel DefaultFilterPanel => null;


        /// <summary>
        /// Default filter input control.
        /// </summary>
        protected virtual TextBox DefaultFilterInput => null;


        /// <summary>
        /// Default filter submit button.
        /// </summary>
        protected virtual Button DefaultFilterButton => null;


        /// <summary>
        /// Panel for custom filter placement.
        /// </summary>
        protected virtual Panel CustomFilterPanel => null;


        /// <summary>
        /// Panel for select/deselect all actions - visible only in "multiple" selection modes.
        /// </summary>
        protected virtual Panel ActionsPanel => null;

        #endregion


        #region "Control events"

        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Load parameters passed from uniselector
            LoadParameters();

            UniGrid.OnExternalDataBound += uniGrid_OnExternalDataBound;
            UniGrid.OnPageChanged += uniGrid_OnPageChanged;
            DefaultFilterButton.Click += btnSearch_Click;

            // Get control IDs from parent window
            txtClientId = QueryHelper.GetString("txtElem", string.Empty);
            hdnClientId = QueryHelper.GetString("hidElem", string.Empty);
            hdnDrpId = QueryHelper.GetString("selectElem", string.Empty);
            hashId = QueryHelper.GetString("hashElem", string.Empty);
            parentClientId = QueryHelper.GetControlClientId("clientId", string.Empty);

            // Load custom filter if defined
            LoadCustomFilter();

            // Load select/deselect all action buttons for multiple modes
            LoadSelectAllActions();
        }


        /// <summary>
        /// Load event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (RequestHelper.IsCallback())
            {
                return;
            }

            // Init control components
            InitControls();
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            if (!RequestHelper.IsPostBack())
            {
                ChangeSearchCondition();
            }

            // Load the grid data
            ReloadGrid();

            // Register client scripts
            RegisterBaseClientScripts();

            var headerRow = UniGrid.GridView.HeaderRow;
            if (headerRow != null)
            {
                if (IsMultipleMode)
                {
                    // Add checkbox for selection of all items on the current page
                    var chkAll = new CMSCheckBox
                    {
                        ID = "chkAll",
                        ToolTip = GetString("UniSelector.CheckAll"),
                        Checked = allRowsChecked
                    };
                    chkAll.Attributes.Add("onclick", "SelectAllItems(this)");

                    headerRow.Cells[0].Controls.Clear();
                    headerRow.Cells[0].Controls.Add(chkAll);
                    headerRow.Cells[1].Text = GetString("general.itemname");
                    UniGrid.GridView.Columns[0].ItemStyle.CssClass = "unigrid-selection";

                    // Register client scripts for multiple modes
                    RegisterMultipleModeClientScripts(chkAll.ClientID);
                }
                else
                {
                    // Single selection
                    UniGrid.GridView.Columns[0].Visible = false;
                    headerRow.Cells[1].Text = GetString("general.itemname");
                }
            }

            base.OnPreRender(e);
        }


        /// <summary>
        /// Unigrid external data bound handler.
        /// </summary>
        private object uniGrid_OnExternalDataBound(object sender, string sourceName, object parameter)
        {
            switch (sourceName.ToLowerInvariant())
            {
                case "yesno":
                    return UniGridFunctions.ColoredSpanYesNo(parameter);

                case "select":
                    {
                        // Add checkbox for multiple selection
                        if (IsMultipleMode)
                        {
                            var drv = (DataRowView)parameter;

                            // Get item ID
                            string itemId = drv[ReturnColumnName].ToString();

                            return GetItemCheckBoxHtml(itemId);
                        }
                    }
                    break;

                case "itemname":
                    {
                        DataRowView drv = (DataRowView)parameter;

                        // Get item ID
                        string itemId = drv[ReturnColumnName].ToString();

                        // Get item name
                        string itemName;

                        // Special formatted user name
                        if (displayNameFormat == UniSelector.USER_DISPLAY_FORMAT)
                        {
                            string userName = DataHelper.GetStringValue(drv.Row, "UserName");
                            string fullName = DataHelper.GetStringValue(drv.Row, "FullName");

                            itemName = UserInfoProvider.GetFormattedUserName(userName, fullName, isLiveSite: IsLiveSite);
                        }
                        else if (displayNameFormat == null)
                        {
                            itemName = drv[iObjectType.DisplayNameColumn].ToString();
                        }
                        else
                        {
                            MacroResolver resolver = MacroResolver.GetInstance();
                            foreach (DataColumn item in drv.Row.Table.Columns)
                            {
                                resolver.SetNamedSourceData(item.ColumnName, drv.Row[item.ColumnName]);
                            }
                            itemName = resolver.ResolveMacros(displayNameFormat);
                        }

                        if (RemoveMultipleCommas)
                        {
                            itemName = TextHelper.RemoveMultipleCommas(itemName);
                        }

                        itemName = AddPrefixIfNotPresent(ItemPrefix, itemName);

                        var ti = iObjectType.TypeInfo;

                        if (string.IsNullOrEmpty(itemName))
                        {
                            itemName = emptyReplacement;
                        }

                        if (AddGlobalObjectSuffix && HasSiteIdColumn(ti))
                        {
                            itemName += (DataHelper.GetIntValue(drv.Row, ti.SiteIDColumn) > 0 ? string.Empty : " " + GlobalObjectSuffix);
                        }

                        // Link action
                        string onclick = null;
                        bool disabled = disabledItems.Contains(";" + itemId + ";");
                        if (!disabled)
                        {
                            onclick = GetItemLinkClickScript(itemId, itemName);
                        }

                        if (LocalizeItems)
                        {
                            itemName = ResHelper.LocalizeString(itemName);
                        }

                        return "<div " + (!disabled ? "class=\"SelectableItem\" " : null) + (onclick != null ? "onclick=\"" + HTMLHelper.EncodeForHtmlAttribute(onclick) + "\"" : null) + ">" + HTMLHelper.HTMLEncode(TextHelper.LimitLength(itemName, 100)) + "</div>";
                    }
            }

            return null;
        }


        /// <summary>
        /// Unigrid pager change event handler.
        /// </summary>
        private void uniGrid_OnPageChanged(object sender, EventArgs eventArgs)
        {
            ReloadGrid();
        }


        /// <summary>
        /// Default filter event handler.
        /// </summary>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            ChangeSearchCondition();
        }


        /// <summary>
        /// Custom filter event handler.
        /// </summary>
        private void searchControl_OnFilterChanged()
        {
            ChangeSearchCondition();
        }


        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            UpdateHiddenFields(true);
        }


        private void btnDeselectAll_Click(object sender, EventArgs e)
        {
            UpdateHiddenFields(false);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Overridden to get the parameters.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        public override object GetValue(string propertyName)
        {
            if ((parameters != null) && parameters.Contains(propertyName))
            {
                return parameters[propertyName];
            }

            return base.GetValue(propertyName);
        }


        /// <summary>
        /// Overridden set value to collect parameters.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="value">Value</param>
        public override bool SetValue(string propertyName, object value)
        {
            // Handle special properties
            if (propertyName.Equals("itemprefix", StringComparison.OrdinalIgnoreCase))
            {
                ItemPrefix = ValidationHelper.GetString(value, string.Empty);
            }

            base.SetValue(propertyName, value);

            // Set parameters for dialog
            parameters[propertyName] = value;

            return true;
        }


        /// <summary>
        /// Loads dynamically custom filter if is defined.
        /// </summary>
        private void LoadCustomFilter()
        {
            // Use custom filter
            if (string.IsNullOrEmpty(filterControl))
            {
                return;
            }

            CustomFilterPanel.Controls.Clear();

            searchControl = (CMSAbstractBaseFilterControl)LoadUserControl(filterControl);
            if (searchControl != null)
            {
                searchControl.ID = "filterElem";
                searchControl.OnFilterChanged += searchControl_OnFilterChanged;
                searchControl.Parameters = parameters;
                searchControl.FilteredControl = this;
                searchControl.SelectedValue = ItemsField.Value.Replace(ValueSeparator.ToString(), string.Empty);
                searchControl.FilterMode = filterMode;

                CustomFilterPanel.Controls.Add(searchControl);
                CustomFilterPanel.Visible = true;

                // Get init filter where condition
                FilterWhere = SqlHelper.AddWhereCondition(string.Empty, searchControl.WhereCondition);

                // When both filters are rendered, mark the first as followed by another
                if (useDefaultNameFilter)
                {
                    CustomFilterPanel.CssClass += " header-panel-not-last";
                }
            }
        }


        /// <summary>
        /// Loads control parameters.
        /// </summary>
        private void LoadParameters()
        {
            string identifier = QueryHelper.GetString("params", null);

            parameters = (Hashtable)WindowHelper.GetItem(identifier);
            if (parameters == null)
            {
                return;
            }

            ResourcePrefix = ValidationHelper.GetString(parameters["ResourcePrefix"], null);
            selectionMode = (SelectionModeEnum)parameters["SelectionMode"];
            ObjectType = ValidationHelper.GetString(parameters["ObjectType"], null);
            ReturnColumnName = ValidationHelper.GetString(parameters["ReturnColumnName"], null);
            ValueSeparator = ValidationHelper.GetValue(parameters["ValuesSeparator"], ';');
            filterControl = ValidationHelper.GetString(parameters["FilterControl"], null);
            useDefaultNameFilter = ValidationHelper.GetBoolean(parameters["UseDefaultNameFilter"], true);
            whereCondition = ValidationHelper.GetString(parameters["WhereCondition"], null);
            orderBy = ValidationHelper.GetString(parameters["OrderBy"], null);
            itemsPerPage = ValidationHelper.GetInteger(parameters["ItemsPerPage"], 10);
            emptyReplacement = ValidationHelper.GetString(parameters["EmptyReplacement"], "&nbsp;");
            dialogGridName = ValidationHelper.GetString(parameters["DialogGridName"], dialogGridName);
            additionalColumns = ValidationHelper.GetString(parameters["AdditionalColumns"], null);
            callbackMethod = ValidationHelper.GetString(parameters["CallbackMethod"], null);
            allowEditTextBox = ValidationHelper.GetBoolean(parameters["AllowEditTextBox"], false);
            fireOnChanged = ValidationHelper.GetBoolean(parameters["FireOnChanged"], false);
            disabledItems = ";" + ValidationHelper.GetString(parameters["DisabledItems"], String.Empty) + ";";
            GlobalObjectSuffix = ValidationHelper.GetString(parameters["GlobalObjectSuffix"], string.Empty);
            AddGlobalObjectSuffix = ValidationHelper.GetBoolean(parameters["AddGlobalObjectSuffix"], false);
            AddGlobalObjectNamePrefix = ValidationHelper.GetBoolean(parameters["AddGlobalObjectNamePrefix"], false);
            RemoveMultipleCommas = ValidationHelper.GetBoolean(parameters["RemoveMultipleCommas"], false);
            filterMode = ValidationHelper.GetString(parameters["FilterMode"], null);
            displayNameFormat = ValidationHelper.GetString(parameters["DisplayNameFormat"], null);
            additionalSearchColumns = ValidationHelper.GetString(parameters["AdditionalSearchColumns"], String.Empty);
            siteWhereCondition = ValidationHelper.GetString(parameters["SiteWhereCondition"], null);
            UseTypeCondition = ValidationHelper.GetBoolean(parameters["UseTypeCondition"], true);
            AllowLocalizedFiltering = ValidationHelper.GetBoolean(parameters["AllowLocalizedFiltering"], true);
            mZeroRowsText = ValidationHelper.GetString(parameters["ZeroRowsText"], string.Empty);
            mFilteredZeroRowsText = ValidationHelper.GetString(parameters["FilteredZeroRowsText"], string.Empty);
            mHasDependingFields = ValidationHelper.GetBoolean(parameters["HasDependingFields"], false);
            securityPurpose = ValidationHelper.GetString(parameters["SecurityPurpose"], String.Empty);

            // Set item prefix if it was passed by UniSelector's AdditionalUrlParameters
            var itemPrefix = QueryHelper.GetString("ItemPrefix", null);
            if (!string.IsNullOrEmpty(itemPrefix))
            {
                ItemPrefix = itemPrefix;
            }

            // Init object
            if (!string.IsNullOrEmpty(ObjectType))
            {
                iObjectType = ModuleManager.GetReadOnlyObject(ObjectType);
                if (iObjectType == null)
                {
                    throw new Exception("[UniSelector.SelectionDialog]: Object type '" + ObjectType + "' not found.");
                }

                if (ReturnColumnName == null)
                {
                    ReturnColumnName = iObjectType.TypeInfo.IDColumn;
                }
            }

            // Pre-select unigrid values passed from parent window
            if (!RequestHelper.IsPostBack())
            {
                string values = (string)parameters["Values"];
                if (!string.IsNullOrEmpty(values))
                {
                    ItemsField.Value = values;
                    parameters["Values"] = null;
                }
            }
        }


        /// <summary>
        /// Loads variables and objects.
        /// </summary>
        private void InitControls()
        {
            mSearchColumns = GetSearchColumns();

            // Display default name filter only if search columns are specified
            if (useDefaultNameFilter && (!String.IsNullOrEmpty(mSearchColumns) || !String.IsNullOrEmpty(additionalSearchColumns) || (displayNameFormat == UniSelector.USER_DISPLAY_FORMAT)))
            {
                DefaultFilterPanel.Visible = true;

                if (!RequestHelper.IsPostBack())
                {
                    ScriptHelper.RegisterStartupScript(this, typeof(string), "Focus", ScriptHelper.GetScript("try{document.getElementById('" + DefaultFilterInput.ClientID + "').focus();}catch(err){}"));
                }
            }

            if (!RequestHelper.IsPostBack())
            {
                UniGrid.Pager.DefaultPageSize = itemsPerPage;
            }

            UniGrid.IsLiveSite = IsLiveSite;
            UniGrid.GridName = dialogGridName;
            UniGrid.GridView.EnableViewState = false;
        }


        /// <summary>
        /// Init select/deselect all actions in "multiple" selection modes.
        /// </summary>
        private void LoadSelectAllActions()
        {
            if (!IsMultipleMode)
            {
                return;
            }

            var btnSelectAll = new LocalizedButton
            {
                ID = "btnSelectAll",
                ButtonStyle = ButtonStyle.Default,
                ResourceString = "uniselector.selectall",
                EnableViewState = false
            };

            var btnDeselectAll = new LocalizedButton
            {
                ID = "btnDeselectAll",
                ButtonStyle = ButtonStyle.Default,
                ResourceString = "uniselector.deselectall",
                EnableViewState = false
            };

            btnSelectAll.Click += btnSelectAll_Click;
            btnDeselectAll.Click += btnDeselectAll_Click;

            ActionsPanel.Controls.Clear();
            ActionsPanel.Controls.Add(btnSelectAll);
            ActionsPanel.Controls.Add(btnDeselectAll);
            ActionsPanel.Visible = true;
        }


        /// <summary>
        /// Returns dataset for specified GeneralizedInfo.
        /// </summary>
        private DataSet GetData(bool applyFilter = true)
        {
            int totalRecords = 0;
            return GetData(0, 0, ref totalRecords, true, applyFilter);
        }


        /// <summary>
        /// Returns dataset for specified GeneralizedInfo.
        /// </summary>
        private DataSet GetData(int offset, int maxRecords, ref int totalRecords, bool selection, bool applyFilter = true)
        {
            // If object type is set
            if (iObjectType == null)
            {
                totalRecords = 0;
                return null;
            }

            // Init columns
            string columns = null;
            DataSet ds = null;

            if (!selection)
            {
                if (displayNameFormat == UniSelector.USER_DISPLAY_FORMAT)
                {
                    // Ensure columns which are needed for USER_DISPLAY_FORMAT
                    columns = "UserName, FullName";
                }
                else if (displayNameFormat != null)
                {
                    columns = DataHelper.GetNotEmpty(MacroProcessor.GetMacros(displayNameFormat, true), iObjectType.DisplayNameColumn).Replace(";", ", ");
                }
                else
                {
                    columns = iObjectType.DisplayNameColumn;
                }
            }

            // Add return column name
            columns = SqlHelper.MergeColumns(columns, ReturnColumnName);

            // Add additional columns
            columns = SqlHelper.MergeColumns(columns, additionalColumns);

            // Ensure display name column for query within localized filtering (SelectAll/DeselectAll calls the query with ID column only)
            if (UseLocalizedFiltering)
            {
                columns = SqlHelper.MergeColumns(columns, iObjectType.TypeInfo.DisplayNameColumn);
            }

            var ti = iObjectType.TypeInfo;

            // Get SiteID column if needed
            if (AddGlobalObjectSuffix && HasSiteIdColumn(ti))
            {
                columns = SqlHelper.MergeColumns(columns, ti.SiteIDColumn);
            }

            string where = whereCondition;
            if (applyFilter)
            {
                where = SqlHelper.AddWhereCondition(where, FilterWhere);
            }
            if (!string.IsNullOrEmpty(UniGrid.WhereClause))
            {
                where = SqlHelper.AddWhereCondition(where, UniGrid.WhereClause);
            }

            // Apply site restrictions
            if (!string.IsNullOrEmpty(siteWhereCondition))
            {
                where = SqlHelper.AddWhereCondition(where, siteWhereCondition);
            }

            // Order by
            string order = string.IsNullOrEmpty(orderBy) ? iObjectType.DisplayNameColumn : orderBy;

            try
            {
                // Get the data query
                var q = iObjectType.GetDataQuery(
                    UseTypeCondition,
                    s => s
                        .Where(where)
                        .OrderBy(order)
                        .Columns(columns),
                    true
                );

                q.IncludeBinaryData = false;

                if (UseLocalizedFiltering)
                {
                    ds = q.Result;

                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        var displayNameColumn = iObjectType.DisplayNameColumn;

                        LocalizeAndFilterDataSet(ds, displayNameColumn, TrimmedSearchText, applyFilter);
                        SortDataSetTable(ds, displayNameColumn);

                        totalRecords = ds.Tables[0].Rows.Count;
                    }
                    else
                    {
                        ds = null;
                        totalRecords = 0;
                    }
                }
                else
                {
                    q.Offset = offset;
                    q.MaxRecords = maxRecords;

                    ds = q.Result;
                    totalRecords = q.TotalRecords;
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("UniSelector", "GETDATA", ex);

                UniGrid.ShowError(ex.Message);
            }

            AddPrefixes(ds);

            return ds;
        }


        /// <summary>
        /// Updates the <paramref name="dataSet"/> and adds prefixes (item prefix and global object name prefix).
        /// </summary>
        private void AddPrefixes(DataSet dataSet)
        {
            var typeInfo = iObjectType.TypeInfo;
            bool shouldAddGlobalPrefix = AddGlobalObjectNamePrefix && HasSiteIdColumn(typeInfo);

            if (shouldAddGlobalPrefix || !String.IsNullOrEmpty(ItemPrefix))
            {
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    var prefixToAdd = ItemPrefix;

                    if (shouldAddGlobalPrefix && row[typeInfo.SiteIDColumn] == DBNull.Value)
                    {
                        prefixToAdd = "." + ItemPrefix;
                    }

                    row[ReturnColumnName] = AddPrefixIfNotPresent(prefixToAdd, row[ReturnColumnName]?.ToString());
                }
            }
        }


        private static string AddPrefixIfNotPresent(string prefix, string text)
        {
            text = text ?? String.Empty;

            if (String.IsNullOrEmpty(prefix) || text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return text;
            }

            return $"{prefix}{text}";
        }


        /// <summary>
        /// Returns true if given <paramref name="typeInfo"/> has SiteID column name set.
        /// </summary>
        private bool HasSiteIdColumn(ObjectTypeInfo typeInfo)
        {
            return !string.IsNullOrEmpty(typeInfo.SiteIDColumn) && !string.Equals(typeInfo.SiteIDColumn, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Sorts the dataset table values.
        /// </summary>
        private void SortDataSetTable(DataSet ds, string sortColumnName)
        {
            ds.Tables[0].DefaultView.Sort = sortColumnName;
            var sortedTable = ds.Tables[0].DefaultView.ToTable();
            ds.Tables.Clear();
            ds.Tables.Add(sortedTable);
        }


        /// <summary>
        /// Localizes the specified column in a dataset table and removes rows that does not contain the required filter value.
        /// </summary>
        private void LocalizeAndFilterDataSet(DataSet ds, string filterColum, string filterValue, bool applyFilter)
        {
            var rows = ds.Tables[0].Rows;
            for (int i = rows.Count - 1; i >= 0; i--)
            {
                var localizedColumnValue = ResHelper.LocalizeString(Convert.ToString(rows[i][filterColum]));
                if (applyFilter && localizedColumnValue.IndexOf(filterValue, StringComparison.InvariantCultureIgnoreCase) == -1)
                {
                    rows[i].Delete();
                }
                else
                {
                    rows[i][filterColum] = localizedColumnValue;
                }
            }

            ds.AcceptChanges();
        }


        /// <summary>
        /// Changes ViewState with search condition for UniGrid.
        /// </summary>
        private void ChangeSearchCondition()
        {
            if (iObjectType == null)
            {
                return;
            }

            string where = null;

            // Get default filter where
            if (useDefaultNameFilter && !string.IsNullOrEmpty(TrimmedSearchText))
            {
                // Avoid SQL injection
                string searchText = SqlHelper.EscapeQuotes(TrimmedSearchText);

                // Escape like patterns
                searchText = SqlHelper.EscapeLikeText(searchText);

                if (displayNameFormat == UniSelector.USER_DISPLAY_FORMAT)
                {
                    // Ensure search in columns which are needed for USER_DISPLAY_FORMAT
                    where = string.Format("UserName LIKE N'%{0}%' OR FullName LIKE N'%{0}%'", searchText);
                }

                // Try enabled localized search
                if (AllowLocalizedFiltering && DisplayNameSelectedAsSearchColumn)
                {
                    UseLocalizedFiltering = true;
                }
                else if (!string.IsNullOrEmpty(mSearchColumns))
                {
                    // Combine main search columns with additional
                    additionalSearchColumns = additionalSearchColumns.TrimEnd(';') + ";" + mSearchColumns;
                }

                // Append additional columns that should be used for search
                if (!string.IsNullOrEmpty(additionalSearchColumns))
                {
                    string[] columns = additionalSearchColumns.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string column in columns)
                    {
                        where = SqlHelper.AddWhereCondition(where, $"{column.Trim()} LIKE N'%{searchText}%'", "OR");
                    }
                }
            }

            // Add custom search filter where
            if (searchControl != null)
            {
                where = SqlHelper.AddWhereCondition(where, searchControl.WhereCondition);
            }

            // Save where condition to the view state
            FilterWhere = where;
        }


        /// <summary>
        /// Returns main search columns to filter.
        /// </summary>
        private string GetSearchColumns()
        {
            var ti = iObjectType.TypeInfo;

            if ((ti.DisplayNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                DisplayNameSelectedAsSearchColumn = true;

                // Get column for display name
                return ti.DisplayNameColumn;
            }

            if ((ti.CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                // Get column for code name
                return ti.CodeNameColumn;
            }

            if (!string.IsNullOrEmpty(displayNameFormat))
            {
                // Get columns from display name format if empty don't filter anything
                return MacroProcessor.GetMacros(displayNameFormat);
            }

            return string.Empty;
        }


        /// <summary>
        /// Reloads the grid with given page index.
        /// </summary>
        private void ReloadGrid()
        {
            int totalRecords = 0;
            int offset = UniGrid.Pager.CurrentPageSize * (UniGrid.Pager.CurrentPage - 1);

            // Reload data set with new page index
            if (UniGrid.DataSource == null)
            {
                UniGrid.DataSource = GetData(offset, UniGrid.Pager.CurrentPageSize, ref totalRecords, false);
                UniGrid.PagerForceNumberOfResults = totalRecords;
            }

            if (string.IsNullOrEmpty(FilterWhere))
            {
                UniGrid.ZeroRowsText = string.IsNullOrEmpty(mZeroRowsText) ? GetString("general.nodatafound") : mZeroRowsText;
            }
            else
            {
                UniGrid.ZeroRowsText = string.IsNullOrEmpty(mFilteredZeroRowsText) ? GetString("general.noitemsfound") : mFilteredZeroRowsText;
            }

            UniGrid.ReloadData();
        }


        /// <summary>
        /// Returns string safe for inserting to javascript as parameter.
        /// </summary>
        /// <param name="param">Parameter</param>
        private string GetSafe(string param)
        {
            if (string.IsNullOrEmpty(param))
            {
                return param;
            }

            // Replace + char for %20 to make it compatible with client side decodeURIComponent
            return ScriptHelper.GetString(Server.UrlEncode(param).Replace("+", "%20"));
        }


        /// <summary>
        /// Projects currently filtered data to the selection data in hidden fields.
        /// If <paramref name="isSelectAllAction"/> is <c>True</c> new data is added otherwise it is removed.
        /// </summary>
        private void UpdateHiddenFields(bool isSelectAllAction)
        {
            // Get all values
            DataSet ds = GetData();
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                var values = ItemsField.Value.Split(new[] { ValueSeparator }, StringSplitOptions.RemoveEmptyEntries).ToHashSetCollection();

                // Process all items
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string value = dr[ReturnColumnName].ToString();

                    if (isSelectAllAction)
                    {
                        values.Add(value);
                    }
                    else
                    {
                        values.Remove(value);
                    }
                }

                ItemsField.Value = string.Format("{0}{1}{0}", ValueSeparator, values.Join(ValueSeparator.ToString()));
            }

            OnMultiSelectionChanged(isSelectAllAction);
        }


        /// <summary>
        /// Called after select/deselect all actions. Should be overriden in child implementation to handle custom actions.
        /// </summary>
        /// <param name="isSelectAllAction">Indicates if the action is select all or deselect all</param>
        protected virtual void OnMultiSelectionChanged(bool isSelectAllAction)
        {

        }


        private void RegisterBaseClientScripts()
        {
            string stringValuesSeparator = ValueSeparator.ToString();
            string scriptValuesSeparator = ScriptHelper.GetString(stringValuesSeparator);
            string regexEscapedValuesSeparator = ScriptHelper.GetString(Regex.Escape(stringValuesSeparator), false);

            string script;

            switch (selectionMode)
            {
                // Button modes
                case SelectionModeEnum.SingleButton:
                case SelectionModeEnum.MultipleButton:
                    {
                        // Register javascript code
                        if (callbackMethod == null)
                        {
                            script = string.Format("function SelectItems(items,hash) {{ wopener.US_SelectItems_{0}(items,hash); CloseDialog(); }}", parentClientId);
                        }
                        else
                        {
                            script = string.Format("function SelectItems(items,hash) {{ wopener.{0}(items.replace(/^{1}+|{1}+$/g, ''),hash); CloseDialog(); }}", callbackMethod, regexEscapedValuesSeparator);
                        }
                    }
                    break;

                // Selector modes
                default:
                    {
                        // Register javascript code
                        script = @"
function SelectItems(items, names, hiddenFieldId, txtClientId, hashClientId, hash) {
    wopener.US_SetItems(items, names, hiddenFieldId, txtClientId, null, hashClientId, hash);" +
    (fireOnChanged ? "wopener.US_SelectionChanged_" + parentClientId + "();" : "")
    + @"
    return CloseDialog();
}

function SelectItemsReload(items, names, hiddenFieldId, txtClientId, hidValue, hashClientId, hash) {
    wopener.US_SetItems(items, names, hiddenFieldId, txtClientId, hidValue, hashClientId, hash);

    wopener.US_ReloadPage_" + parentClientId + @"();
    return CloseDialog();
}";
                    }
                    break;
            }

            script += string.Format(@"
function ItemsElem() {{
    return document.getElementById('{0}');
}}

function GetCheckboxValue(chkbox) {{
    return chkbox.id.substr(3);
}}

function escapeRegExp(string) {{
  return string.replace(/[.*+?^${{}}()|[\]\\]/g, '\\$&'); // $& means the whole matched string
}}

function ProcessItem(chkbox, changeChecked) {{
    var itemsElem = ItemsElem();
    var items = itemsElem.value;
    if (chkbox != null) {{
        var item = GetCheckboxValue(chkbox);
        if (changeChecked) {{
            chkbox.checked = !chkbox.checked;
        }}
        if (chkbox.checked) {{
            if (items == '') {{
                itemsElem.value = {1} + item + {1};
            }}
            else if (items.toLowerCase().indexOf({1} + item.toLowerCase() + {1}) < 0) {{
                itemsElem.value += item + {1};
            }}
        }}
        else
        {{
            var pattern = escapeRegExp({1} + item + {1});
            var re = new RegExp(pattern, 'i');
            itemsElem.value = items.replace(re, {1});
        }}
    }}
}}
", ItemsField.ClientID, scriptValuesSeparator);

            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "SelectionDialog_" + ClientID, script, true);
        }


        private void RegisterMultipleModeClientScripts(string checkboxAllClientId)
        {
            // Register scripts for multiple modes
            string stringValuesSeparator = ValueSeparator.ToString();
            string regexEscapedValuesSeparator = ScriptHelper.GetString(Regex.Escape(stringValuesSeparator), false);

            string script = "function ProcessResult(items, hash){";

            switch (selectionMode)
            {
                // Button modes
                case SelectionModeEnum.MultipleButton:
                    script += "SelectItems(EncodeValue(items),hash);";
                    break;

                // Textbox modes
                case SelectionModeEnum.MultipleTextBox:
                    if (allowEditTextBox && !mHasDependingFields)
                    {
                        script += "SelectItems(EncodeValue(items), EncodeValue(items.replace(/^" + regexEscapedValuesSeparator + "+|" + regexEscapedValuesSeparator + "+$/g, '')), " + ScriptHelper.GetString(hdnClientId) + ", " + ScriptHelper.GetString(txtClientId) + ", " + ScriptHelper.GetString(hashId) + ", hash);";
                    }
                    else if (mHasDependingFields)
                    {
                        script += "SelectItemsReload(EncodeValue(items), EncodeValue(items.replace(/^" + regexEscapedValuesSeparator + "+|" + regexEscapedValuesSeparator + "+$/g, '')), " + ScriptHelper.GetString(hdnClientId) + ", " + ScriptHelper.GetString(txtClientId) + ", " + ScriptHelper.GetString(hdnDrpId) + ", " + ScriptHelper.GetString(hashId) + ", hash);";
                    }
                    else
                    {
                        script += "SelectItemsReload(EncodeValue(items), '', " + ScriptHelper.GetString(hdnClientId) + ", " + ScriptHelper.GetString(txtClientId) + ", " + ScriptHelper.GetString(hdnDrpId) + ", " + ScriptHelper.GetString(hashId) + ", hash);";
                    }
                    break;

                // Other modes
                default:
                    script += "SelectItemsReload(EncodeValue(items), '', " + ScriptHelper.GetString(hdnClientId) + ", " + ScriptHelper.GetString(txtClientId) + ", " + ScriptHelper.GetString(hdnDrpId) + ", " + ScriptHelper.GetString(hashId) + ", hash);";
                    break;
            }

            script += @"}
function EncodeValue(value){
    return encodeURIComponent(value).replace(/'/g, '%27');
}

function GetCheckboxAllElement() {
    return document.getElementById('" + checkboxAllClientId + @"');
}

function UpdateCheckboxAllElement() {
    var checked = true;
    var checkboxes = document.getElementsByClassName('chckbox');

    for(var i = 0; i < checkboxes.length; i++) {
        var chkbox = checkboxes[i];

        if (!chkbox.checked) {
            checked = false;
            break;
        }
    }

    var chkAll = GetCheckboxAllElement();

    if (chkAll != null && chkAll.checked != checked) {
        chkAll.checked = checked;
    }
}

function SelectAllItems(checkbox) {
    var checkboxes = document.getElementsByClassName('chckbox');
    var checked = checkbox.checked;

    for(var i = 0; i < checkboxes.length; i++) {
        var chkbox = checkboxes[i];
        chkbox.checked = checked;

        ProcessItem(chkbox, false);
    }
}

function GetHash() {
    " + Page.ClientScript.GetCallbackEventReference(this, "ItemsElem().value", "UpdateSelection", null) + @";
}

function UpdateSelection(value) {
    var hashIndex = value.lastIndexOf('#');
    if(hashIndex >= 0){
        var values = value.slice(0, hashIndex);
        var hash = value.slice(hashIndex + 1);

        if (hash.length > 0) {
            ProcessResult(values, hash);
        }
    }
}

function US_Cancel() {
    CloseDialog();
    return false;
}

function US_Submit() {
    GetHash();
    return false;
}
";
            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "SelectionDialogMultipleMode_" + ClientID, script, true);
        }


        private string GetItemCheckBoxHtml(string itemId)
        {
            var itemWithSeparators = string.Format("{0}{1}{0}", ValueSeparator, itemId);

            string checkBox = $"<span class=\"checkbox\"><input id=\"{HTMLHelper.EncodeForHtmlAttribute("chk" + itemId)}\" type=\"checkbox\" onclick=\"ProcessItem(this, false); UpdateCheckboxAllElement();\" class=\"chckbox\" ";
            if (ItemsField.Value.IndexOf(itemWithSeparators, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                checkBox += "checked=\"checked\" ";
            }
            else
            {
                allRowsChecked = false;
            }

            if (disabledItems.Contains(itemWithSeparators))
            {
                checkBox += "disabled=\"disabled\" ";
            }

            checkBox += $"/><label for=\"{HTMLHelper.EncodeForHtmlAttribute("chk" + itemId)}\">&nbsp;</label></span>";

            return checkBox;
        }


        private string GetItemLinkClickScript(string itemId, string itemName)
        {
            string clickScript;

            string safeItemId = GetSafe(itemId);
            string itemHash = ValidationHelper.GetHashString(itemId, new HashSettings(securityPurpose));

            switch (selectionMode)
            {
                case SelectionModeEnum.Multiple:
                case SelectionModeEnum.MultipleTextBox:
                case SelectionModeEnum.MultipleButton:
                    clickScript = string.Format("ProcessItem(document.getElementById({0}), true); UpdateCheckboxAllElement(); return false;", ScriptHelper.GetString("chk" + itemId));
                    break;

                case SelectionModeEnum.SingleButton:
                    clickScript = string.Format("SelectItems({0},'{1}'); return false;", safeItemId, itemHash);
                    break;

                case SelectionModeEnum.SingleTextBox:
                    if (allowEditTextBox)
                    {
                        if (!mHasDependingFields)
                        {
                            clickScript = string.Format("SelectItems({0},{0},{1},{2},{3},'{4}'); return false;", safeItemId, ScriptHelper.GetString(hdnClientId), ScriptHelper.GetString(txtClientId), ScriptHelper.GetString(hashId), itemHash);
                        }
                        else
                        {
                            clickScript = string.Format("SelectItemsReload({0},{0},{1},{2},{3},{4},'{5}'); return false;", safeItemId, ScriptHelper.GetString(hdnClientId), ScriptHelper.GetString(txtClientId), ScriptHelper.GetString(hdnDrpId), ScriptHelper.GetString(hashId), itemHash);
                        }
                    }
                    else
                    {
                        if (!mHasDependingFields)
                        {
                            clickScript = string.Format("SelectItems({0},{1},{2},{3},{4},'{5}'); return false;", safeItemId, GetSafe(itemName), ScriptHelper.GetString(hdnClientId), ScriptHelper.GetString(txtClientId), ScriptHelper.GetString(hashId), itemHash);
                        }
                        else
                        {
                            clickScript = string.Format("SelectItemsReload({0},{1},{2},{3},{4},{5},'{6}'); return false;", safeItemId, GetSafe(itemName), ScriptHelper.GetString(hdnClientId), ScriptHelper.GetString(txtClientId), ScriptHelper.GetString(hdnDrpId), ScriptHelper.GetString(hashId), itemHash);
                        }
                    }
                    break;

                default:
                    clickScript = string.Format("SelectItemsReload({0},{1},{2},{3},{4},{5},'{6}'); return false;", safeItemId, GetSafe(itemName), ScriptHelper.GetString(hdnClientId), ScriptHelper.GetString(txtClientId), ScriptHelper.GetString(hdnDrpId), ScriptHelper.GetString(hashId), itemHash);
                    break;
            }

            return clickScript;
        }

        #endregion


        #region "ICallbackEventHandler Members"

        string ICallbackEventHandler.GetCallbackResult()
        {
            // Prepare the parameters for dialog
            string result = string.Empty;
            if (!string.IsNullOrEmpty(callbackValues))
            {
                var allowedValues = new List<string>();

                var values = callbackValues.Split(new[] { ValueSeparator }, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length > 0)
                {
                    // Get all (valid) data based on dialog configuration
                    var ds = GetData(false);
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        allowedValues = DataHelper.GetStringValues(ds.Tables[0], ReturnColumnName);
                    }
                }

                // Filter selected items by merging with valid data
                var filteredValues = values.Intersect(allowedValues, StringComparer.OrdinalIgnoreCase);

                // Update selected items and hash on client
                var valuesString = string.Format("{0}{1}{0}", ValueSeparator, filteredValues.Join(ValueSeparator.ToString()));
                result = $"{valuesString}#{ValidationHelper.GetHashString(valuesString, new HashSettings(securityPurpose))}";
            }

            return result;
        }


        void ICallbackEventHandler.RaiseCallbackEvent(string eventArgument)
        {
            callbackValues = eventArgument;
        }

        #endregion
    }
}
