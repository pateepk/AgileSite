using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DocumentEngine.Web.UI;
using CMS.EventLog;
using CMS.FormEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.IO;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;
using CMS.UIControls.UniGridConfig;

using Action = CMS.UIControls.UniGridConfig.Action;
using CultureInfo = System.Globalization.CultureInfo;

namespace CMS.UIControls
{
    /// <summary>
    /// UniGrid base class.
    /// </summary>
    [ParseChildren(true)]
    public abstract class UniGrid : CMSUserControl, IUniGrid, IExtensibleUniGrid, ICallbackEventHandler, IPostBackEventHandler, IUniPageable
    {
        #region "Constants"

        /// <summary>
        /// Constant representing all data source
        /// </summary>
        public const string ALL = "##ALL##";


        /// <summary>
        /// Selection external source name. Used for external DataBound of selection column.
        /// </summary>
        public const string SELECTION_EXTERNAL_DATABOUND = "UG_Selection_DataBound";


        /// <summary>
        /// Select all external source name. Used for header external DataBound of SelectAll check box.
        /// </summary>
        public const string SELECTALL_EXTERNAL_DATABOUND = "UG_SelectAll_DataBound";


        /// <summary>
        /// Name of the custom filter control set via GridOptions.FilterPath. Custom filter can be accessed by FilterFields[CUSTOM_FILTER_SOURCE_NAME].
        /// </summary>
        public const string CUSTOM_FILTER_SOURCE_NAME = "CustomFilter";

        private const string FORM_FILTER_SOURCE_NAME = "FormFilter";

        private const string DEFAULT_ACTIONS_MENU = "~/CMSAdminControls/UI/UniGrid/Controls/UniGridMenu.ascx";

        private const int HALF_PAGE_COUNT_LIMIT = 1000;

        private const string FILTER_SOURCE_FORMNAME = "FilterFormName";
        private const string FILTER_SOURCE_CUSTOM = "Custom";
        private const string FILTER_SOURCE_CONFIGURED = "Configured";

        #endregion


        #region "Events"

        /// <summary>
        /// Event raised on action post back.
        /// </summary>
        public event OnActionEventHandler OnAction;


        /// <summary>
        /// Event raised when external source data required.
        /// </summary>
        public event OnExternalDataBoundEventHandler OnExternalDataBound;


        /// <summary>
        /// Event raised when data are reloaded from external data source.
        /// </summary>
        public event OnDataReloadEventHandler OnDataReload;


        /// <summary>
        /// Event raised before data are reloaded from external data source.
        /// </summary>
        public event OnBeforeDataReload OnBeforeDataReload;


        /// <summary>
        /// Event raised after data are reloaded from external data source.
        /// </summary>
        public event OnAfterDataReload OnAfterDataReload;


        /// <summary>
        /// Raised before sorting is called.
        /// </summary>
        public event OnBeforeSorting OnBeforeSorting;


        /// <summary>
        /// Raised before filtering from UniGrid filter is called.
        /// </summary>
        public event OnBeforeFiltering OnBeforeFiltering;


        /// <summary>
        /// Raised on page size changed.
        /// </summary>
        public event OnPageSizeChanged OnPageSizeChanged;


        /// <summary>
        /// Raised when columns are loaded dynamically and not from XML.
        /// </summary>
        public event OnLoadColumns OnLoadColumns;


        /// <summary>
        /// Raised when after data are retrieved.
        /// </summary>
        public event OnAfterRetrieveData OnAfterRetrieveData;


        /// <summary>
        /// Raised when filter field is created.
        /// </summary>
        public event OnFilterFieldCreated OnFilterFieldCreated;


        /// <summary>
        /// OnShowButtonClick event handler.
        /// </summary>
        public event EventHandler OnShowButtonClick;

        #endregion


        #region "Variables"

        private string mFilterDirectoryPath = "~/CMSAdminControls/UI/UniGrid/Filters/";
        private string mImageDirectoryPath;
        private string mDefaultImageDirectoryPath;
        private string mOrderBy;
        private string mZeroRowsText = ResHelper.GetString("general.nodatafound");
        private string mFilteredZeroRowsText = ResHelper.GetString("unigrid.filteredzerorowstext");


        /// <summary>
        /// Indicates whether a filter should be displayed above the UniGrid. If the amount of displayed rows is lower than the value of the FilterLimit key, the filter will be hidden despite this setting.
        /// Loaded from UniGrid options.
        /// </summary>
        protected bool displayFilter;

        private string filterSource;


        /// <summary>
        /// Dictionary of where conditions by filters for filtering by query parameters.
        /// Filter name = Where condition.
        /// </summary>
        protected Dictionary<string, string> filterQueryParameters = new Dictionary<string, string>();


        /// <summary>
        /// Indicates whether a column allowing the selection of rows should be displayed on the left of the UniGrid.
        /// Loaded from UniGrid options.
        /// </summary>
        protected bool showSelection;


        /// <summary>
        /// Determines if an arrow showing the sorting direction should be displayed next to the header of the column used for sorting.
        /// Loaded from UniGrid options.
        /// </summary>
        protected bool showSortDirection = true;


        /// <summary>
        /// Indicates whether external data source is used.
        /// </summary>
        protected bool dataSourceIsUsed;


        /// <summary>
        /// Indicates whether UniGrid action was raised. Used for data reloading.
        /// </summary>
        protected bool onActionUsed;


        /// <summary>
        /// Total number of available records returned from data reload.
        /// </summary>
        protected int pagerForceNumberOfResults = -1;


        private bool mApplyPageSize = true;

        private bool? mShowActionsMenu;
        private bool? mShowExportMenu;

        private bool mShowObjectMenu = true;
        private string mObjectType;
        private bool mGroupObject;
        private GeneralizedInfo mInfoObject;
        private Regex mOrderByRegex;
        private int? mFilterLimit;

        private List<string> mTextColumns = new List<string>();

        private ISet<string> mActionsID = NewItemsSet();

        private List<string> mSelectedItems;

        private Dictionary<string, DataControlField> mNamedColumns;

        private readonly UIPager mPager = null;

        // Stores external data bound callbacks of the columns added dynamically via the AddAdditionalColumn method
        private Dictionary<string, OnExternalDataBoundEventHandler> mAdditionalExternalDataBounds;

        private string callbackArguments;
        private bool mShowActionsLabel = true;

        // Control GUID
        private string mControlGUID;

        #endregion


        #region "Controls properties"

        /// <summary>
        /// Returns the grid header panel
        /// </summary>
        protected abstract Panel HeaderPanel
        {
            get;
        }


        /// <summary>
        /// Grid information label
        /// </summary>
        protected abstract Label InfoLabel
        {
            get;
        }


        /// <summary>
        /// Gets the menu placeholder
        /// </summary>
        protected abstract PlaceHolder MenuPlaceHolder
        {
            get;
        }


        /// <summary>
        /// Grid view used by UniGrid.
        /// </summary>
        public abstract UIGridView GridView
        {
            get;
        }


        /// <summary>
        /// Control containing mass actions.
        /// </summary>
        public abstract MassActions MassActions
        {
            get;
        }


        /// <summary>
        /// Hidden field containing selected items.
        /// </summary>
        public abstract HiddenField SelectionHiddenField
        {
            get;
        }


        /// <summary>
        /// Hidden field containing selected items hash.
        /// </summary>
        protected abstract HiddenField SelectionHashHiddenField
        {
            get;
        }


        /// <summary>
        /// Hidden field for the command name
        /// </summary>
        protected abstract HiddenField CmdNameHiddenField
        {
            get;
        }


        /// <summary>
        /// Hidden field for the command argument
        /// </summary>
        protected abstract HiddenField CmdArgHiddenField
        {
            get;
        }


        /// <summary>
        /// UniGrid pager used by UniGrid.
        /// </summary>
        public virtual UIPager Pager
        {
            get
            {
                return mPager;
            }
        }


        /// <summary>
        /// Hidden field with action ids.
        /// </summary>
        protected abstract HiddenField ActionsHidden
        {
            get;
        }


        /// <summary>
        /// Hidden field with hashed action ids.
        /// </summary>
        protected abstract HiddenField ActionsHashHidden
        {
            get;
        }


        /// <summary>
        /// Gets page size dropdown from UniGrid.
        /// </summary>
        public abstract DropDownList PageSizeDropdown
        {
            get;
        }


        /// <summary>
        /// Gets filter placeHolder from UniGrid.
        /// </summary>
        public abstract PlaceHolder FilterPlaceHolder
        {
            get;
        }


        /// <summary>
        /// Returns the filter form placeholder
        /// </summary>
        protected abstract PlaceHolder FilterFormPlaceHolder
        {
            get;
        }


        /// <summary>
        /// Gets filter form.
        /// </summary>
        public abstract FilterForm FilterForm
        {
            get;
        }


        /// <summary>
        /// Returns the advanced export control of the current grid
        /// </summary>
        protected abstract AdvancedExport AdvancedExportControl
        {
            get;
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Actions definition.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public UniGridActions GridActions
        {
            get;
            set;
        }


        /// <summary>
        /// Mass actions definition.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public UniGridMassActions GridMassActions
        {
            get;
            set;
        }


        /// <summary>
        /// Columns definition.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public UniGridColumns GridColumns
        {
            get;
            set;
        }


        /// <summary>
        /// Pager configuration.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public UniGridPagerConfig PagerConfig
        {
            get;
            set;
        }


        /// <summary>
        /// Options.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public UniGridOptions GridOptions
        {
            get;
            set;
        }


        /// <summary>
        /// Inline control parameter - mirrors the GridName property.
        /// </summary>
        public override string Parameter
        {
            get
            {
                return GridName;
            }
            set
            {
                GridName = value;
            }
        }


        /// <summary>
        /// External grid DataSource, when set, overrides the default data source settings.
        /// </summary>
        public object DataSource
        {
            get;
            set;
        }


        /// <summary>
        /// Xml file with source information.
        /// </summary>
        public string GridName
        {
            get;
            set;
        }


        /// <summary>
        /// Volume of returned records.
        /// </summary>
        public int PagerForceNumberOfResults
        {
            get
            {
                return pagerForceNumberOfResults;
            }
            set
            {
                pagerForceNumberOfResults = value;
            }
        }


        /// <summary>
        /// Defines whether to show object menu (menu containing relationships, export/backup, destroy object, ... functionality)
        /// </summary>
        public bool ShowObjectMenu
        {
            get
            {
                return mShowObjectMenu;
            }
            set
            {
                mShowObjectMenu = value;
            }
        }


        /// <summary>
        /// Where condition that can be set from other classes.
        /// </summary>
        public String WhereCondition
        {
            get;
            set;
        }


        /// <summary>
        /// Order columns.
        /// </summary>
        public string OrderBy
        {
            get
            {
                return mOrderBy;
            }
            set
            {
                mOrderBy = value;
                if (!RequestHelper.IsPostBack())
                {
                    SortDirect = value;
                }
            }
        }


        /// <summary>
        /// Gets current order by.
        /// </summary>
        protected string CurrentOrder
        {
            get
            {
                // Set order by
                string currentOrder = OrderBy;
                if (!String.IsNullOrEmpty(SortDirect))
                {
                    currentOrder = SortDirect;
                }
                return currentOrder;
            }
        }


        /// <summary>
        /// Gets current page size from pager.
        /// </summary>
        protected int CurrentPageSize
        {
            get
            {
                return ApplyPageSize ? Pager.CurrentPageSize : 0;
            }
        }


        /// <summary>
        /// Determines whether to apply CurrentPageSize when retrieving data.
        /// </summary>
        public bool ApplyPageSize
        {
            get
            {
                return mApplyPageSize;
            }
            set
            {
                mApplyPageSize = value;
            }
        }


        /// <summary>
        /// Gets current offset.
        /// </summary>
        public int CurrentOffset
        {
            get
            {
                return CurrentPageSize * (Pager.CurrentPage - 1);
            }
        }


        /// <summary>
        /// Description.
        /// </summary>
        protected Regex OrderByRegex
        {
            get
            {
                return mOrderByRegex ?? (mOrderByRegex = RegexHelper.GetRegex("([\\w\\.]*)(?:\\]??.*?)(asc|desc)+?", true));
            }
        }


        /// <summary>
        /// Allows to explicitly specify all columns that can be retrieved from UniGrid
        /// Comma-separated value e.g. "ItemID, ItemName"
        /// </summary>
        public string AllColumns
        {
            get;
            set;
        }


        /// <summary>
        /// Columns to get from the database.
        /// </summary>
        public string Columns
        {
            get;
            set;
        }


        /// <summary>
        /// Number of records to display (load).
        /// </summary>
        public int TopN
        {
            get;
            set;
        }


        /// <summary>
        /// Query parameters for the query.
        /// </summary>
        public QueryDataParameters QueryParameters
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets directory path for images.
        /// </summary>
        public string ImageDirectoryPath
        {
            get
            {
                return mImageDirectoryPath;
            }
            set
            {
                if (!value.EndsWith("/", StringComparison.Ordinal))
                {
                    mImageDirectoryPath = value + "/";
                }
                else
                {
                    mImageDirectoryPath = value;
                }
            }
        }


        /// <summary>
        /// Gets or sets directory path for custom filter controls.
        /// </summary>
        public string FilterDirectoryPath
        {
            get
            {
                return mFilterDirectoryPath;
            }
            set
            {
                mFilterDirectoryPath = value.TrimEnd('/') + "/";
            }
        }


        /// <summary>
        /// Gets default image directory path for images.
        /// </summary>
        public string DefaultImageDirectoryPath
        {
            get
            {
                if (string.IsNullOrEmpty(mDefaultImageDirectoryPath))
                {
                    // Set default image directory path
                    mDefaultImageDirectoryPath = UIHelper.GetImagePath(Page, "Design/Controls/UniGrid/Actions/", IsLiveSite, true);
                }

                return mDefaultImageDirectoryPath;
            }
        }


        /// <summary>
        /// Gets named columns from grid view.
        /// </summary>
        public Dictionary<string, DataControlField> NamedColumns
        {
            get
            {
                return mNamedColumns ?? (mNamedColumns = new Dictionary<string, DataControlField>());
            }
            protected set
            {
                if (mNamedColumns == null)
                {
                    mNamedColumns = new Dictionary<string, DataControlField>();
                }
                mNamedColumns = value;
            }
        }


        /// <summary>
        /// Gets sort columns from grid view.
        /// </summary>
        protected List<string> SortColumns
        {
            get
            {
                List<string> list = ViewState["SortColumns"] as List<string>;
                if (list == null)
                {
                    list = new List<string>();
                    ViewState["SortColumns"] = list;
                }
                return list;
            }
            set
            {
                ViewState["SortColumns"] = value;
            }
        }


        /// <summary>
        /// Text displayed when UniGrid data source is empty.
        /// </summary>
        public string ZeroRowsText
        {
            get
            {
                return mZeroRowsText;
            }

            set
            {
                mZeroRowsText = value;
            }
        }


        /// <summary>
        /// Indicates if the data source is empty.
        /// </summary>
        public bool IsEmpty
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates whether hide UniGrid or not when its data source is empty.
        /// </summary>
        public bool HideControlForZeroRows
        {
            get;
            set;
        }


        /// <summary>
        /// Query used in grid view.
        /// </summary>
        public string Query
        {
            get
            {
                object o = ViewState["Query"];
                return (o == null) ? String.Empty : (string)o;
            }
            set
            {
                ViewState["Query"] = value;
            }
        }


        /// <summary>
        /// Where clause used in grid view.
        /// </summary>
        public string WhereClause
        {
            get
            {
                object o = ViewState["WhereClause"];
                return (o == null) ? string.Empty : (string)o;
            }
            set
            {
                ViewState["WhereClause"] = value;
            }
        }


        /// <summary>
        /// Sort directive used in grid view.
        /// </summary>
        public string SortDirect
        {
            get
            {
                object o = ViewState["SortDirect"];
                return (o == null) ? String.Empty : (string)o;
            }
            set
            {
                ViewState["SortDirect"] = value;
            }
        }


        /// <summary>
        /// Array list of selection ids of UniGrid.
        /// </summary>
        public ISet<string> SelectionsID
        {
            get
            {
                var list = ViewState["SelectionsID"] as ISet<string>;
                if (list == null)
                {
                    list = NewItemsSet();
                    ViewState["SelectionsID"] = list;
                }

                return list;
            }
            set
            {
                ViewState["SelectionsID"] = value;
            }
        }


        /// <summary>
        /// Array list of text columns used in sorting when query is used.
        /// </summary>
        public List<string> TextColumns
        {
            get
            {
                return mTextColumns;
            }
            set
            {
                mTextColumns = value;
            }
        }


        /// <summary>
        /// Array list of actions on current page of UniGrid.
        /// </summary>
        public ISet<string> ActionsID
        {
            get
            {
                return mActionsID;
            }
            set
            {
                mActionsID = value;
            }
        }


        /// <summary>
        /// Gets or sets selected items of UniGrid.
        /// Returns empty collection if hash validation fails.
        /// </summary>
        public List<string> SelectedItems
        {
            get
            {
                return mSelectedItems ?? (mSelectedItems = GetHiddenValues(SelectionHiddenField, SelectionHashHiddenField, ClientID));
            }
            set
            {
                SetHiddenValues(value, SelectionHiddenField, SelectionHashHiddenField, ClientID);

                mSelectedItems = value;
                OriginallySelected = value;
            }
        }


        /// <summary>
        /// Gets array list of originally selected items.
        /// </summary>
        protected List<string> OriginallySelected
        {
            get
            {
                var selected = ViewState["OriginallySelected"] as List<string>;
                if (selected == null)
                {
                    selected = new List<string>();
                    ViewState["OriginallySelected"] = selected;
                }

                return selected;
            }
            set
            {
                ViewState["OriginallySelected"] = value;
            }
        }


        /// <summary>
        /// Gets deselected items from UniGrid.
        /// </summary>
        public List<string> DeselectedItems
        {
            get
            {
                return ValidateHiddenValues(SelectionHiddenField, SelectionHashHiddenField, ClientID) ? OriginallySelected.Except(SelectedItems).ToList() : new List<string>();
            }
        }


        /// <summary>
        /// Gets newly selected items from UniGrid.
        /// </summary>
        public List<string> NewlySelectedItems
        {
            get
            {
                return ValidateHiddenValues(SelectionHiddenField, SelectionHashHiddenField, ClientID) ? SelectedItems.Except(OriginallySelected).ToList() : new List<string>();
            }
        }


        /// <summary>
        /// Returns the complete where condition.
        /// </summary>
        public string CompleteWhereCondition
        {
            get
            {
                return SqlHelper.AddWhereCondition(WhereClause, CurrentResolver.ResolveMacros(WhereCondition));
            }
        }


        /// <summary>
        /// UniGrid page size values separated with comma.
        /// Default: 5,10,25,50,100
        /// </summary>
        public string PageSize
        {
            get
            {
                return Pager.PageSizeOptions;
            }
            set
            {
                Pager.PageSizeOptions = value;
            }
        }


        /// <summary>
        /// Minimal count of entries for display filter. 0 means that filter is displayed always and -1 means that filter is never displayed.
        /// </summary>
        public int FilterLimit
        {
            get
            {
                if (mFilterLimit == null)
                {
                    mFilterLimit = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSDefaultListingFilterLimit"], 25);
                }
                return (int)mFilterLimit;
            }
            set
            {
                mFilterLimit = value;
            }
        }


        /// <summary>
        /// Indicates if delayed reload should be used.
        /// If True reload data must be called externally.
        /// </summary>
        public bool DelayedReload
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if data source is sorted outside of UniGrid.
        /// </summary>
        public bool DataSourceIsSorted
        {
            get;
            set;
        }


        /// <summary>
        /// Name of javascript function called after selection checkbox is clicked.
        /// First argument will be value of selection column or first column in data source.
        /// Second argument will be true/false according to checkbox.
        /// </summary>
        public string SelectionJavascript
        {
            get;
            set;
        }


        /// <summary>
        /// Gets rows count from DataSet.
        /// Sets after ReloadData().
        /// </summary>
        public int RowsCount
        {
            get;
            private set;
        }


        /// <summary>
        /// Object type which is loaded to the grid
        /// </summary>
        public string ObjectType
        {
            get
            {
                return mObjectType;
            }
            set
            {
                mObjectType = value;
                mInfoObject = null;
            }
        }


        /// <summary>
        /// If defined, uses this macro to retrieve the grid data source. The macro must return IDataQuery object. Macro source has higher priority than object type.
        /// </summary>
        public string Macro
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the object type is community group related.
        /// </summary>
        public bool GroupObject
        {
            get
            {
                return mGroupObject;
            }
            set
            {
                mGroupObject = value;
                mInfoObject = null;
            }
        }


        /// <summary>
        /// Gets instance of info object based on ObjectType property.
        /// </summary>
        public GeneralizedInfo InfoObject
        {
            get
            {
                return mInfoObject ?? (mInfoObject = ModuleManager.GetObject(ObjectType));
            }
        }


        /// <summary>
        /// Number of records to display (load). It's being set before ReloadData event
        /// is fired in order to optimize the data loading.
        /// </summary>
        public int CurrentTopN
        {
            get;
            set;
        }


        /// <summary>
        /// URL for the edit action handling.
        /// </summary>
        public string EditActionUrl
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the editing action is done through a dialog
        /// </summary>
        public bool EditInDialog
        {
            get;
            set;
        }


        /// <summary>
        /// Width of edited dialog
        /// </summary>
        public String DialogWidth
        {
            get;
            set;
        }


        /// <summary>
        /// Height of
        /// </summary>
        public String DialogHeight
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the actions label is displayed
        /// </summary>
        public bool ShowActionsLabel
        {
            get
            {
                return mShowActionsLabel;
            }
            set
            {
                mShowActionsLabel = value;
            }
        }


        /// <summary>
        /// Defines whether to show export menu (menu in the header row of actions column).
        /// </summary>
        public bool ShowExportMenu
        {
            get
            {
                // Explicitly set value
                if (mShowExportMenu != null)
                {
                    return mShowExportMenu.Value;
                }

                // Automatic value based on the object
                var obj = InfoObject;
                if (obj != null)
                {
                    return !IsLiveSite && obj.TypeInfo.AllowDataExport;
                }

                return false;
            }
            set
            {
                mShowExportMenu = value;
            }
        }


        /// <summary>
        /// Defines whether to show actions menu (menu in the header row of actions column).
        /// </summary>
        public bool ShowActionsMenu
        {
            get
            {
                // Explicitly set value
                if (mShowActionsMenu != null)
                {
                    return mShowActionsMenu.Value;
                }

                return ShowExportMenu || RememberState || FilterIsAvailable;
            }
            set
            {
                mShowActionsMenu = value;
                ShowExportMenu = true;
            }
        }


        /// <summary>
        /// Name used for exporting files (without extension).
        /// </summary>
        public string ExportFileName
        {
            get;
            set;
        }


        /// <summary>
        /// Path to JavaScript module that will be loaded for this grid.
        /// </summary>
        public string JavaScriptModule
        {
            get;
            set;
        }


        /// <summary>
        /// Data that will be sent to the grid's JavaScript module.
        /// </summary>
        public Object JavaScriptModuleData
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if control is used on live site
        /// </summary>
        public override bool IsLiveSite
        {
            get
            {
                return base.IsLiveSite;
            }
            set
            {
                base.IsLiveSite = value;
                MessagesPlaceHolder.IsLiveSite = value;
            }
        }


        /// <summary>
        /// Unique ID of the control within the given request.
        /// </summary>
        public string ControlGUID
        {
            get
            {
                if (mControlGUID == null)
                {
                    int index = 0;

                    // Get first available ID
                    do
                    {
                        mControlGUID = "cg" + index;
                        index++;
                    } while (RequestStockHelper.Contains(mControlGUID, true));

                    RequestStockHelper.Add(mControlGUID, this, true);
                }

                return mControlGUID;
            }
            set
            {
                mControlGUID = value;
            }
        }

        #endregion


        #region "Paging properties"

        /// <summary>
        /// Text displayed in pagers first page link.
        /// </summary>
        public string PagerFirstPageText
        {
            get
            {
                return Pager.FirstPageText;
            }
            set
            {
                Pager.FirstPageText = value;
            }
        }


        /// <summary>
        /// Text displayed in pagers previous page link.
        /// </summary>
        public string PagerPreviousPageText
        {
            get
            {
                return Pager.PreviousPageText;
            }
            set
            {
                Pager.PreviousPageText = value;
            }
        }


        /// <summary>
        /// Text displayed in pagers previous group link.
        /// </summary>
        public string PagerPreviousGroupText
        {
            get
            {
                return Pager.PreviousGroupText;
            }
            set
            {
                Pager.PreviousGroupText = value;
            }
        }


        /// <summary>
        /// Text displayed in pagers next group link.
        /// </summary>
        public string PagerNextGroupText
        {
            get
            {
                return Pager.NextGroupText;
            }
            set
            {
                Pager.NextGroupText = value;
            }
        }


        /// <summary>
        /// Text displayed in pagers next page link.
        /// </summary>
        public string PagerNextPageText
        {
            get
            {
                return Pager.NextPageText;
            }
            set
            {
                Pager.NextPageText = value;
            }
        }


        /// <summary>
        /// Text displayed in pagers last page link.
        /// </summary>
        public string PagerLastPageText
        {
            get
            {
                return Pager.LastPageText;
            }
            set
            {
                Pager.LastPageText = value;
            }
        }

        #endregion


        #region "Filter properties"

        /// <summary>
        /// True if the filtering should be set from the query string parameters.
        /// </summary>
        public bool FilterByQueryString
        {
            get;
            set;
        }


        /// <summary>
        /// Text displayed when UniGrid data source is empty after filtering.
        /// </summary>
        public string FilteredZeroRowsText
        {
            get
            {
                return mFilteredZeroRowsText;
            }

            set
            {
                mFilteredZeroRowsText = value;
            }
        }


        /// <summary>
        /// Indicates whether hide filter "Show" button or not.
        /// </summary>
        public bool HideFilterButton
        {
            get;
            set;
        }


        /// <summary>
        /// Custom filter control
        /// </summary>
        public IFilterControl CustomFilter
        {
            get;
            private set;
        }


        /// <summary>
        /// Filter mode passed to custom filters.
        /// </summary>
        public string FilterMode
        {
            get
            {
                return ValidationHelper.GetString(GetValue("FilterMode"), "");
            }
            set
            {
                SetValue("FilterMode", value);
            }
        }


        /// <summary>
        /// Indicates if filter is set. It's being used when filter by query string is used.
        /// </summary>
        public bool FilterIsSet
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["FilterIsSet"], false);
            }

            set
            {
                ViewState["FilterIsSet"] = value;
            }
        }


        /// <summary>
        /// Filter form information
        /// </summary>
        private FormInfo FilterFormInfo
        {
            get;
            set;
        }


        /// <summary>
        /// This name is used for alternative form.
        /// </summary>
        public string FilterFormName
        {
            get;
            set;
        }


        /// <summary>
        /// Data container for filter form.
        /// </summary>
        public IDataContainer FilterFormData
        {
            get;
            set;
        }


        /// <summary>
        /// If count of rows is greater or equal to filter limit, show filter.
        /// </summary>
        public bool ShowFilter
        {
            get
            {
                // Check explicitly set value
                var show = ViewState["ShowFilter"];
                if (show != null)
                {
                    return ValidationHelper.GetBoolean(show, false);
                }

                // Display based on filter limit
                if ((Pager != null) && (Pager.UniPager != null))
                {
                    return (Pager.UniPager.DataSourceItemsCount >= FilterLimit);
                }

                return false;
            }
            set
            {
                ViewState["ShowFilter"] = value;
            }
        }


        /// <summary>
        /// Returns true if the filter is available
        /// </summary>
        private bool FilterIsAvailable
        {
            get
            {
                return (FilterFormInfo != null) || (CustomFilter != null);
            }
        }


        /// <summary>
        /// Returns true if the grid is using the filter form
        /// </summary>
        private bool IsUsingFilterForm
        {
            get
            {
                return (FilterFormInfo != null) || !String.IsNullOrEmpty(FilterFormName);
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets description for error message when dependencies check fails.
        /// </summary>
        /// <param name="infoObj">Info object to be deleted</param>
        public string GetCheckDependenciesDescription(BaseInfo infoObj)
        {
            // Get names of dependencies and display message
            var names = infoObj.Generalized.GetDependenciesNames();
            if ((names == null) || (names.Count <= 0))
            {
                return null;
            }

            // Encode and localize names
            return String.Format("{0}<br />{1}", GetString(infoObj.TypeInfo.ObjectType.Replace(".", "_") + ".objectlist|unigrid.objectlist"),
                names.Select(item => ResHelper.LocalizeString(item, null, true)).Join("<br />"));
        }


        /// <summary>
        /// Handles the display of the grid based on whether the data is loaded or not
        /// </summary>
        protected void HandleVisibility()
        {
            if (FilterIsSet)
            {
                // Check for FilteredZeroRowsText
                if ((GridView.Rows.Count == 0) && !String.IsNullOrEmpty(FilteredZeroRowsText))
                {
                    // Display filter zero rows text
                    InfoLabel.Text = FilteredZeroRowsText;
                    InfoLabel.Visible = true;
                    Pager.Visible = false;
                }
                else
                {
                    InfoLabel.Visible = false;
                    Pager.Visible = true;
                }
            }
            else
            {
                // Check for ZeroRowsText
                if (GridView.Rows.Count == 0)
                {
                    if (!HideControlForZeroRows && !String.IsNullOrEmpty(ZeroRowsText))
                    {
                        // Display zero rows text
                        InfoLabel.Text = ZeroRowsText;
                        InfoLabel.Visible = true;
                        Pager.Visible = false;

                        // Check additional filter visibility
                        UpdateFilterVisibility();
                    }
                    else
                    {
                        InfoLabel.Visible = false;
                        Pager.Visible = false;
                        FilterPlaceHolder.Visible = false;
                    }
                }
                else
                {
                    InfoLabel.Visible = false;
                    Pager.Visible = true;

                    // Check additional filter visibility
                    UpdateFilterVisibility();
                }
            }

            if (Pager.CurrentPage > HALF_PAGE_COUNT_LIMIT)
            {
                // Enlarge direct page TextBox
                TextBox txtPage = ControlsHelper.GetChildControl(Pager, typeof(TextBox), "txtPage") as TextBox;
                if (txtPage != null)
                {
                    txtPage.Style.Add(HtmlTextWriterStyle.Width, "50px");
                }
            }

            // Hide mass action drop-down when grid view does not have any item
            MassActions.Visible &= GridView.Rows.Count != 0;

            // Hide info label when error message is displayed
            InfoLabel.Visible &= String.IsNullOrEmpty(MessagesPlaceHolder.ErrorText);
        }


        /// <summary>
        /// Reloads the grid data.
        /// </summary>
        public void ReloadData()
        {
            try
            {
                // Ensure grid definition before reload data
                LoadGridDefinition();

                if (!RequestHelper.IsPostBack())
                {
                    RestoreState();
                }

                RaiseOnBeforeDataReload();

                // Get Current TOP N
                if (CurrentPageSize > 0)
                {
                    int currentPageIndex = Pager.CurrentPage;
                    int pageSize = (CurrentPageSize > 0) ? CurrentPageSize : GridView.PageSize;

                    CurrentTopN = pageSize * (currentPageIndex + Pager.GroupSize);
                }

                if (CurrentTopN < TopN)
                {
                    CurrentTopN = TopN;
                }

                // If first/last & previous/next buttons and direct page control in pager is hidden use current topN for better performance
                if (!Pager.ShowDirectPageControl && !Pager.ShowFirstLastButtons && !Pager.ShowPreviousNextButtons)
                {
                    TopN = CurrentTopN;
                }

                // Retrieve data
                GridView.DataSource = RetrieveData();

                RaiseOnAfterDataReload();

                SetUnigridControls();

                // Check if DataSource is loaded
                if (DataHelper.DataSourceIsEmpty(GridView.DataSource) && (Pager.CurrentPage > 1))
                {
                    Pager.UniPager.CurrentPage = 1;
                    ReloadData();
                }

                // Resolve the edit action URL
                if (!String.IsNullOrEmpty(EditActionUrl))
                {
                    EditActionUrl = MacroResolver.Resolve(EditActionUrl);
                }

                SortColumns.Clear();

                RowsCount = DataHelper.GetItemsCount(GridView.DataSource);

                GridView.DataBind();
            }
            catch (ThreadAbortException)
            {
                // Do not log any exception to event log for ThreadAbortException
            }
            catch (Exception ex)
            {
                // Display tooltip only development mode is enabled
                string desc = null;
                if (SystemContext.DevelopmentMode)
                {
                    desc = ex.Message;
                }

                ShowError(GetString("unigrid.error.reload"), desc);

                // Log exception
                EventLogProvider.LogException("UniGrid", "RELOADDATA", ex);

                // Reset grid state in case the exception is caused by invalid filter setting
                ResetState();
            }
        }


        private void UpdateActionSettings(bool filterDisplayed)
        {
            var actions = GridActions;
            if (actions != null)
            {
                actions.AllowExport = ShowExportMenu;
                actions.AllowReset = RememberState;
                actions.AllowShowFilter = FilterIsAvailable && !filterDisplayed;
            }
        }

        #endregion


        #region "Additional columns methods"

        /// <summary>
        /// Adds column to the UniGrid's columns with optional callback which specifies what will be displayed in a column cells.
        /// </summary>
        /// <remarks>
        /// If <paramref name="externalDataBoundCallback"/> is specified, property ExternalSourceName of the <paramref name="column"/> parameter has to be specified as well.
        /// Callback <paramref name="externalDataBoundCallback"/> will be called only for the cells in the added column.
        /// This method has to be called before the <see cref="OnLoadColumns"/> event is fired, what usually happens on the Page_Load.
        /// </remarks>
        /// <param name="column">Column</param>
        /// <param name="externalDataBoundCallback">Optional external data bound callback. Function which returns content of the column's cells</param>
        /// <exception cref="ArgumentNullException"><paramref name="column"/> is null</exception>
        public void AddAdditionalColumn(Column column, OnExternalDataBoundEventHandler externalDataBoundCallback = null)
        {
            if (column == null)
            {
                throw new ArgumentNullException("column");
            }

            if (externalDataBoundCallback != null)
            {
                if (string.IsNullOrEmpty(column.ExternalSourceName))
                {
                    throw new ArgumentException("[UniGrid.AddAdditionalColumn]: Cannot add column with external data bound callback specified but without name of the external data bound");
                }

                AddAdditionalExternalDataBoundCallback(column.ExternalSourceName, externalDataBoundCallback);
            }

            OnLoadColumns += () => GridColumns.Columns.Add(column);
        }


        /// <summary>
        /// Adds external data bound callback to the dictionary.
        /// </summary>
        /// <param name="externalSourceName">Name of the external source</param>
        /// <param name="externalDataBoundCallback">Callback which provides cells content</param>
        private void AddAdditionalExternalDataBoundCallback(string externalSourceName, OnExternalDataBoundEventHandler externalDataBoundCallback)
        {
            if (mAdditionalExternalDataBounds == null)
            {
                mAdditionalExternalDataBounds = new Dictionary<string, OnExternalDataBoundEventHandler>();
            }
            if (mAdditionalExternalDataBounds.ContainsKey(externalSourceName))
            {
                throw new InvalidOperationException(String.Format("Additional column with external source name '{0}' already exists.", externalSourceName));
            }
            mAdditionalExternalDataBounds.Add(externalSourceName, externalDataBoundCallback);
        }


        /// <summary>
        /// Gets the callback function associated with the specified external source name.
        /// </summary>
        /// <param name="externalSourceName">External source name. This is the ExternalSourceName property of the column parameter passed to the AddAdditionalColumn method</param>
        /// <param name="externalDataBoundCallback">Found callback</param>
        /// <returns>True if the callback was found; otherwise, false</returns>
        private bool TryGetAdditionalExternalDataBoundCallback(string externalSourceName, out OnExternalDataBoundEventHandler externalDataBoundCallback)
        {
            if (mAdditionalExternalDataBounds == null)
            {
                externalDataBoundCallback = null;
                return false;
            }

            return mAdditionalExternalDataBounds.TryGetValue(externalSourceName, out externalDataBoundCallback);
        }

        #endregion


        #region "Event methods"

        /// <summary>
        /// Raises the action event.
        /// </summary>
        /// <param name="cmdName">Command name</param>
        /// <param name="cmdValue">Command value</param>
        public void RaiseAction(string cmdName, string cmdValue)
        {
            if (OnAction != null)
            {
                if ((ActionsHidden != null) && (ActionsHashHidden != null))
                {
                    // Validate the hash
                    string actions = ActionsHidden.Value;
                    if (actions.Contains(String.Format("|{0}|", cmdValue)) && ValidationHelper.ValidateHash(actions, ActionsHashHidden.Value, new HashSettings(ClientID)))
                    {
                        onActionUsed = true;
                        OnAction(cmdName, cmdValue);
                    }
                }
            }
        }


        /// <summary>
        /// Raises the external data bound event.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="sourceName">External source name</param>
        /// <param name="parameter">Source parameter</param>
        public object RaiseExternalDataBound(object sender, string sourceName, object parameter)
        {
            // Special handling for built-in grid transformations
            if ((sourceName != null) && (sourceName[0] == '#'))
            {
                // Object transformation
                if (sourceName.StartsWith(UniGridTransformations.TRANSFORM_PREFIX, StringComparison.OrdinalIgnoreCase))
                {
                    int prefixLength = UniGridTransformations.TRANSFORM_PREFIX.Length;

                    int trIndex = sourceName.IndexOf(':', prefixLength);
                    if (trIndex <= 0)
                    {
                        // Search for the last part (column name)
                        trIndex = sourceName.LastIndexOf(".", StringComparison.Ordinal);
                    }

                    if (trIndex > 0)
                    {
                        int objectId;

                        string objectType = sourceName.Substring(prefixLength, trIndex - prefixLength).Trim();
                        string columnName = sourceName.Substring(trIndex + 1).Trim();

                        if (parameter is DataRowView)
                        {
                            // ##ALL##
                            int lIndex = objectType.IndexOf("=>", StringComparison.Ordinal);
                            if (lIndex <= 0)
                            {
                                return null;
                            }

                            string idValue = objectType.Substring(lIndex + 2).Trim();
                            objectType = objectType.Substring(0, lIndex).Trim();

                            // Resolve macros in object type and ID value
                            CurrentResolver.SetAnonymousSourceData(parameter);

                            idValue = CurrentResolver.ResolveMacros(idValue);
                            objectType = CurrentResolver.ResolveMacros(objectType);

                            objectId = ValidationHelper.GetInteger(idValue, 0);
                        }
                        else
                        {
                            // Specific column (ID of the object)
                            objectId = ValidationHelper.GetInteger(parameter, 0);
                        }

                        if (objectId <= 0)
                        {
                            return null;
                        }

                        // Add the field transformation control that handles the translation
                        var tr = new ObjectTransformation(objectType, objectId);
                        tr.Transformation = columnName;

                        return tr;
                    }
                }

                // Try to find the transformation
                if (UniGridTransformations.Global.ExecuteTransformation(sender, sourceName, ref parameter))
                {
                    return parameter;
                }
            }

            // If column was added using AddAdditionalColumn method and its external data bound callback was specified, fire it
            OnExternalDataBoundEventHandler additionalExternalDataBound;
            if (TryGetAdditionalExternalDataBoundCallback(sourceName, out additionalExternalDataBound))
            {
                return additionalExternalDataBound(sender, sourceName, parameter);
            }

            return OnExternalDataBound?.Invoke(sender, sourceName, parameter);
        }


        /// <summary>
        /// Raises the reload data event and returns DataSet.
        /// </summary>
        public DataSet RaiseDataReload()
        {
            if (OnDataReload != null)
            {
                return OnDataReload(CompleteWhereCondition, CurrentOrder, TopN, Columns, CurrentOffset, CurrentPageSize, ref pagerForceNumberOfResults);
            }

            if (DataSource != null)
            {
                dataSourceIsUsed = true;
                return (DataSet)(DataSource);
            }

            return null;
        }


        /// <summary>
        /// Raises the before data reload event.
        /// </summary>
        public void RaiseOnBeforeDataReload()
        {
            OnBeforeDataReload?.Invoke();
        }


        /// <summary>
        /// Raises the after data reload event.
        /// </summary>
        public void RaiseOnAfterDataReload()
        {
            OnAfterDataReload?.Invoke();
        }


        /// <summary>
        /// Raised when filter field is created.
        /// </summary>
        /// <param name="columnName">Filter column name</param>
        /// <param name="filterDefinition">Filter definition</param>
        public void RaiseOnFilterFieldCreated(string columnName, UniGridFilterField filterDefinition)
        {
            OnFilterFieldCreated?.Invoke(columnName, filterDefinition);
        }


        /// <summary>
        /// Raises the before page index changing event.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Grid view page event arguments</param>
        public void RaiseBeforeSorting(object sender, EventArgs e)
        {
            OnBeforeSorting?.Invoke(sender, e);
        }


        /// <summary>
        /// Raises the before filtering event.
        /// </summary>
        /// <param name="where">Where condition</param>
        public string RaiseBeforeFiltering(string where)
        {
            if (OnBeforeFiltering != null)
            {
                return OnBeforeFiltering(where);
            }

            return where;
        }


        /// <summary>
        /// Raises the before page index changing event.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Grid view page event arguments</param>
        public void RaiseShowButtonClick(object sender, EventArgs e)
        {
            OnShowButtonClick?.Invoke(sender, e);
        }


        /// <summary>
        /// Raises the after page size dropdown changed.
        /// </summary>
        public void RaisePageSizeChanged()
        {
            OnPageSizeChanged?.Invoke();
        }


        /// <summary>
        /// Raises when columns are loaded dynamically and not from XML.
        /// </summary>
        public void RaiseLoadColumns()
        {
            OnLoadColumns?.Invoke();
        }


        /// <summary>
        /// Raises after data are retrieved.
        /// </summary>
        /// <param name="ds">Data</param>
        public DataSet RaiseAfterRetrieveData(DataSet ds)
        {
            if (OnAfterRetrieveData != null)
            {
                return OnAfterRetrieveData(ds);
            }

            return ds;
        }


        /// <summary>
        /// Occurs when the pager change the page and current mode is postback => reload data
        /// </summary>
        public event EventHandler<EventArgs> OnPageChanged;


        /// <summary>
        /// Evokes control DataBind.
        /// </summary>
        public virtual void ReBind()
        {
            OnPageChanged?.Invoke(this, null);

            ReloadData();
        }

        #endregion


        #region "State management"

        #region "State management - Variables"

        /// <summary>
        /// Indicates whether UniGrid remembers its state, i.e., filter, page number, page size and sorting order.
        /// </summary>
        /// <remarks>
        /// Null value indicates that UniGrid uses the system settings.
        /// </remarks>
        private bool? mRememberState;


        /// <summary>
        /// A comma-separated list of names of query parameters whose values constitute a part of UniGrid's identity.
        /// </summary>
        /// <remarks>
        /// UniGrid's identity consists of many attributes such as page's virtual path, control's unique identifier or current site identifier.
        /// However, in some situations the UniGrid's identity might not be unique, e.g., when the page with UniGrid control is shared between CMS Desk and Site Manager.
        /// In this case it is necessary to make the identity stronger and include additional information from the query string.
        /// You can specify more parameters separated by comma, or you could use the asterisk (*) to include the complete query string into UniGrid's identity.
        /// </remarks>
        private string mRememberStateByParam = "*";


        /// <summary>
        /// A UniGrid's unique identifier.
        /// </summary>
        private string mStateId;


        private Button mShowButton;

        private Button mResetButton;

        private bool mResetSelection;

        #endregion


        #region "State management - Properties"

        /// <summary>
        /// Gets or sets the value indicating whether UniGrid remembers its state, i.e., filter, page number, page size and sorting order.
        /// </summary>
        public bool RememberState
        {
            get
            {
                if (mRememberState.HasValue)
                {
                    return mRememberState.Value;
                }

                if (!String.IsNullOrEmpty(ObjectType))
                {
                    var objectType = ObjectTypeManager.GetTypeInfo(ObjectType);
                    if (objectType?.RememberUniGridState != null)
                    {
                        return objectType.RememberUniGridState.Value;
                    }
                }

                return SettingsKeyInfoProvider.GetBoolValue("CMSRememberUniGridState", SiteContext.CurrentSiteName);
            }
            set
            {
                mRememberState = value;
            }
        }


        /// <summary>
        /// Gets or sets a comma-separated list of names of query parameters whose value constitute a part of UniGrid's identity.
        /// </summary>
        /// <remarks>
        /// UniGrid's identity consists of many attributes such as page's virtual path, control's unique identifier or current site identifier.
        /// However, in some situations the UniGrid's identity might not be unique, e.g., when the page with UniGrid control is shared between CMS Desk and Site Manager.
        /// In this case it is necessary to make the identity stronger and include additional information from the query string.
        /// You can specify more parameters separated by comma, or you could use the asterisk (*) to include the complete query string into UniGrid's identity.
        /// </remarks>
        public string RememberStateByParam
        {
            get
            {
                return mRememberStateByParam;
            }
            set
            {
                mRememberStateByParam = value;
            }
        }


        /// <summary>
        /// Indicates whether UniGrid remembers its default state.
        /// </summary>
        /// <remarks>
        /// To reduce memory usage UniGrid by default does not store its default state.
        /// However, there might be a filter control whose non-default state results in an empty filtering condition.
        /// In that case it's necessary to force UniGrid to store its default state.
        /// </remarks>
        public bool RememberDefaultState
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the value indicating whether UniGrid is in a default state, i.e., without filtering condition, on the first page and with the default page size and sorting order.
        /// </summary>
        protected bool HasDefaultState
        {
            get
            {
                return
                    String.IsNullOrEmpty(WhereClause) &&
                    (String.IsNullOrEmpty(SortDirect) || (SortDirect == OrderBy)) &&
                    (Pager.CurrentPage == 1) &&
                    (Pager.CurrentPageSize == Pager.DefaultPageSize);
            }
        }


        /// <summary>
        /// Gets or sets the value indicating whether UniGrid has restored its state.
        /// </summary>
        protected bool StateRestored
        {
            get;
            set;
        }


        /// <summary>
        /// If true, relative ancestor div is checked in context menu
        /// </summary>
        public bool CheckRelative
        {
            get;
            set;
        }

        #endregion


        #region "State management - Methods"

        /// <summary>
        /// Resets the UniGrid state, i.e., restores the filter to the initial state, navigates to the first page and sets the default page size and sorting order, and finally reloads data.
        /// </summary>
        /// <remarks>
        /// When some of the custom filter controls doesn't implement the ResetFilter method, UniGrid displays an error message and logs a warning to the event log.
        /// </remarks>
        public void Reset()
        {
            try
            {
                ResetState();

                SetFilter(true);
            }
            catch (NotImplementedException exception)
            {
                if (SystemContext.DiagnosticLogging)
                {
                    string message = GetString("unigrid.error.resetfilter");
                    ShowError(message);
                    EventLogProvider.LogWarning("UniGrid", "Reset", exception, SiteContext.CurrentSiteID, "A UniGrid's custom filter does not implement a method to reset its state.");
                }
                else
                {
                    // As one of the custom filters does not support resetting its state, the page will be reloaded.
                    ScriptHelper.RegisterStartupScript(this, GetType(), "Reset", "window.location = window.location;", true);
                }
            }
        }


        /// <summary>
        /// Resets the state of all Unigrids displaying objects of given type.
        /// </summary>
        /// <param name="objectType">Unigrids showing objects of this type will be reset.</param>
        public static void ResetStates(string objectType)
        {
            var stateIdPrefix = GetStateIdPrefix(objectType).ToLowerInvariant();

            SessionHelper.Clear(stateIdPrefix);
        }


        /// <summary>
        /// Resets the UniGrid state, i.e., restores the filter to the initial state, navigates to the first page and sets the default page size and sorting order.
        /// </summary>
        private void ResetState()
        {
            string stateId = GetStateId();
            Session.Remove(stateId);

            WhereClause = null;
            SortDirect = OrderBy;

            // Reset paging
            ResetPaging();

            if (CustomFilter != null)
            {
                CustomFilter.ResetFilter();
            }

            // Reset filter form
            if (IsUsingFilterForm)
            {
                FilterForm.ResetFilter();
            }
        }


        private void ResetPaging()
        {
            Pager.UniPager.PageSize = Pager.DefaultPageSize;
            PageSizeDropdown.SelectedValue = Pager.DefaultPageSize.ToString("D", CultureInfo.InvariantCulture);
            Pager.UniPager.CurrentPage = 1;
        }


        /// <summary>
        /// Stores the UniGrid's state, if applicable.
        /// </summary>
        protected void StoreState()
        {
            if (RememberState)
            {
                string stateId = GetStateId();
                if (!RememberDefaultState && HasDefaultState)
                {
                    SessionHelper.Remove(stateId);
                }
                else
                {
                    UniGridState state = new UniGridState();
                    StoreState(state);
                    SessionHelper.SetValue(stateId, state);
                }
            }
        }


        /// <summary>
        /// Restores the UniGrid's state, if applicable.
        /// </summary>
        protected void RestoreState()
        {
            if (RememberState && !StateRestored)
            {
                StateRestored = true;
                string stateId = GetStateId();

                var state = SessionHelper.GetValue(stateId) as UniGridState;
                if (state != null)
                {
                    RestoreState(state);

                    // State was restored from saved state, we want to keep remembering it even if it is default
                    RememberDefaultState = true;
                }
            }
        }


        /// <summary>
        /// Stores the UniGrid's state.
        /// </summary>
        /// <param name="state">The object to store the UniGrid's state into.</param>
        private void StoreState(UniGridState state)
        {
            state.WhereClause = WhereClause;
            state.SortDirect = SortDirect;
            state.PageSize = Pager.UniPager.PageSize;
            state.CurrentPage = Pager.UniPager.CurrentPage;
            state.FilterIsSet = FilterIsSet;

            if (CustomFilter != null)
            {
                StoreFilterState(state, CustomFilter, CUSTOM_FILTER_SOURCE_NAME);
            }

            // Save filter form state
            if (IsUsingFilterForm)
            {
                StoreFilterState(state, FilterForm, GetFormFilterName());
            }
        }


        private string GetFormFilterName()
        {
            return FORM_FILTER_SOURCE_NAME + "_" + FilterFormName;
        }


        private static void StoreFilterState(UniGridState state, IFilterControl filter, string key)
        {
            var filterState = new UniGridFilterFieldState
            {
                Name = key
            };

            filter.StoreFilterState(filterState.State);

            state.FilterFieldStates.Add(filterState);
        }


        /// <summary>
        /// Restores the UniGrid's state.
        /// </summary>
        /// <param name="state">The object to restore the UniGrid's state from.</param>
        private void RestoreState(UniGridState state)
        {
            FilterIsSet = state.FilterIsSet;

            bool restored = true;

            if (CustomFilter != null)
            {
                restored = RestoreFilterState(state, CustomFilter, CUSTOM_FILTER_SOURCE_NAME);
            }

            // Save filter form state
            if (IsUsingFilterForm)
            {
                restored = RestoreFilterState(state, FilterForm, GetFormFilterName());
            }

            if (restored)
            {
                WhereClause = state.WhereClause;
                SortDirect = state.SortDirect;

                Pager.UniPager.PageSize = state.PageSize;
                PageSizeDropdown.SelectedValue = state.PageSize.ToString("D", CultureInfo.InvariantCulture);
                Pager.UniPager.CurrentPage = state.CurrentPage;
            }
        }


        private static bool RestoreFilterState(UniGridState state, IFilterControl filter, string key)
        {
            var formState = state.FilterFieldStates.FirstOrDefault(s => s.Name == key);
            if (formState != null)
            {
                filter.RestoreFilterState(formState.State);
                return true;
            }

            return false;
        }


        private static string GetStateIdPrefix(string objectType)
        {
            return string.Format("UniGridState:{0}", objectType);
        }


        /// <summary>
        /// Creates a UniGrid's unique identifier, and returns it.
        /// </summary>
        /// <returns>A UniGrid's unique identifier.</returns>
        private string GetStateId()
        {
            if (mStateId == null)
            {
                var builder = new StringBuilder();

                builder.AppendFormat(
                    "{0}:{1}:{2}:{3:D}:{4:D}:{5}:{6}",
                    GetStateIdPrefix(ObjectType),
                    Page.AppRelativeVirtualPath,
                    UniqueID,
                    SiteContext.CurrentSiteID,
                    CurrentUser.UserID,
                    Query,
                    filterSource
                );

                if (!String.IsNullOrEmpty(RememberStateByParam))
                {
                    string[] parameterNames = RememberStateByParam.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
                    string currentURL = RequestContext.CurrentURL;
                    if ((parameterNames.Length == 1) && parameterNames[0].Equals("*", StringComparison.Ordinal))
                    {
                        var orderedQuery = URLHelper.GetQuery(currentURL).TrimStart('?').Split('&').ToList();
                        orderedQuery.Sort();

                        builder.Append(":?").Append(String.Join("&", orderedQuery));
                    }
                    else
                    {
                        foreach (string parameterName in parameterNames)
                        {
                            string value = URLHelper.GetQueryValue(currentURL, parameterName);
                            builder.Append(":").Append(value);
                        }
                    }
                }

                mStateId = builder.ToString().ToLowerInvariant();
            }

            return mStateId;
        }


        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            AdvancedExportControl.UniGrid = this;

            PageSizeDropdown.SelectedIndexChanged += PageSizeDropdown_SelectedIndexChanged;
            GridView.Sorting += GridView_Sorting;
            GridView.DataBinding += GridView_DataBinding;
            GridView.DataBound += GridView_DataBound;
            GridView.RowCreated += GridView_RowCreated;
        }


        /// <summary>
        /// Load event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                EnsureChildControls();
                MassActions.SelectedItemsClientID = GetSelectionFieldClientID();

                base.OnLoad(e);

                if (StopProcessing)
                {
                    Visible = false;
                    FilterForm.StopProcessing = true;
                }
                else
                {
                    LoadGrid();
                }

                // Clear hidden action on load event. If UniGrid is invisible, page pre render is not fired
                ClearActions();
            }
            catch (ThreadAbortException)
            {
                // Do not log any exception to event log for ThreadAbortException
            }
            catch (Exception ex)
            {
                ShowError(GetString("unigrid.error.reload"), ex.Message);
                EventLogProvider.LogException("UniGrid", "LOADDATA", ex);
            }
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Start JavaScript module if specified
            if (!String.IsNullOrEmpty(JavaScriptModule))
            {
                ScriptHelper.RegisterModule(this, JavaScriptModule, JavaScriptModuleData);
            }

            StoreState();

            HandleVisibility();

            if (Visible && !StopProcessing)
            {
                RegisterCmdScripts();
            }

            AdvancedExportControl.Visible = ShowActionsMenu;
        }

        #endregion

        #endregion


        #region "Data loading methods"

        /// <summary>
        /// Loads the grid
        /// </summary>
        protected void LoadGrid()
        {
            SetPager();

            if (LoadGridDefinition())
            {
                if (RequestHelper.IsCallback())
                {
                    // Set filter form
                    if (IsUsingFilterForm)
                    {
                        SetBasicFormFilter();
                    }
                }
                else
                {
                    HandlePostback();

                    // Set order by clause
                    ProcessSorting();

                    // Set filter form
                    if (displayFilter && IsUsingFilterForm)
                    {
                        SetBasicFormFilter();
                    }

                    if (displayFilter && (!RequestHelper.IsPostBack() || FilterByQueryString))
                    {
                        SetFilter(true);
                    }
                    else if (!IsInternalPostBack() && !DelayedReload)
                    {
                        ReloadData();
                    }
                }
            }
        }


        /// <summary>
        /// Gets a DataSet with data based on UniGrid's settings.
        /// </summary>
        /// <returns>DataSet with data</returns>
        public DataSet RetrieveData()
        {
            DataSet ds;

            // Query source
            if (!string.IsNullOrEmpty(Query))
            {
                ds = LoadDataFromQuery();
            }
            // Macro source
            else if (!string.IsNullOrEmpty(Macro))
            {
                ds = LoadDataFromMacro();
            }
            // Object type source
            else if (InfoObject != null)
            {
                ds = LoadDataFromObjectType();
            }
            // External DataSet is used
            else
            {
                ds = LoadExternalData();
            }

            // Add empty DataSet
            if (ds == null)
            {
                ds = new DataSet();
                ds.Tables.Add();
            }

            // Raise event 'OnAfterRetrieveData'
            ds = RaiseAfterRetrieveData(ds);

            return ds;
        }


        private DataSet LoadExternalData()
        {
            DataSet ds = RaiseDataReload();

            SortUniGridDataSource(ds);
            ds = DataHelper.TrimDataSetPage(ds, CurrentOffset, CurrentPageSize, ref pagerForceNumberOfResults);

            return ds;
        }


        private DataSet LoadDataFromQuery()
        {
            return ConnectionHelper.ExecuteQuery(Query, QueryParameters, CompleteWhereCondition, CurrentOrder, TopN, Columns, CurrentOffset, CurrentPageSize, ref pagerForceNumberOfResults);
        }


        private DataSet LoadDataFromMacro()
        {
            // Execute the macro
            var result = UIContext.ContextResolver.ResolveMacroExpression(Macro, true);
            if (result != null)
            {
                // Load the result query
                IDataQuery q = result.Result as IDataQuery;
                if (q != null)
                {
                    return LoadDataFromDataQuery(q);
                }
            }

            // No results
            pagerForceNumberOfResults = 0;
            return null;
        }


        private DataSet LoadDataFromObjectType()
        {
            // Get the result set
            var q = InfoObject.GetDataQuery(
                true,
                s =>
                {
                    s.Where(CompleteWhereCondition).OrderBy(CurrentOrder).TopN(TopN).Columns(Columns);
                    s.Parameters = QueryParameters;
                },
                true
                );

            return LoadDataFromDataQuery(q, false);
        }


        private DataSet LoadDataFromDataQuery(IDataQuery q, bool applySettings = true)
        {
            if (applySettings)
            {
                q.ApplySettings(s =>
                {
                    s.Where(CompleteWhereCondition).OrderBy(CurrentOrder).TopN(TopN).Columns(Columns);
                    s.Parameters = QueryParameters;
                });
            }

            q.IncludeBinaryData = false;
            q.Offset = CurrentOffset;
            q.MaxRecords = CurrentPageSize;

            // Get the data
            DataSet ds = q.Result;
            pagerForceNumberOfResults = q.TotalRecords;

            return ds;
        }

        #endregion


        #region "Configuration methods"

        /// <summary>
        /// Loads the grid definition.
        /// </summary>
        public bool LoadGridDefinition()
        {
            if (GridView.Columns.Count == 0)
            {
                using (Panel filterPanel = new Panel())
                {
                    filterPanel.CssClass = "form-horizontal form-filter";
                    FilterPlaceHolder.Controls.Clear();

                    // Clear all columns from the grid view
                    GridView.Columns.Clear();
                    if (!LoadXmlConfiguration())
                    {
                        return false;
                    }

                    EnsureSelectionColumn();

                    // Load options
                    if (GridOptions != null)
                    {
                        LoadOptionsDefinition(GridOptions, filterPanel);
                    }

                    if ((GridActions == null) && ShowActionsMenu)
                    {
                        EmptyAction emptyAction = new EmptyAction();

                        GridActions = new UniGridActions();
                        GridActions.Actions.Add(emptyAction);
                    }

                    // Actions
                    if (GridActions != null)
                    {
                        LoadActionsDefinition(GridActions);
                    }

                    // Mass actions
                    if (GridMassActions != null)
                    {
                        LoadMassActionsDefinition(GridMassActions);
                    }

                    // Load pager configuration
                    if (PagerConfig != null)
                    {
                        LoadPagerDefinition(PagerConfig);
                    }

                    LoadColumns();

                    if (displayFilter)
                    {
                        // Finish filter form with "Show" button
                        CreateFilterButtons(filterPanel);
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Ensures whether column with checkboxes in <see cref="GridView"/> is shown.
        /// </summary>
        /// <remarks>Method is marked as internal for testing purposes.</remarks>
        internal void EnsureSelectionColumn()
        {
            showSelection = ShouldShowSelection();
            if (!showSelection)
            {
                return;
            }

            TemplateField chkColumn = new TemplateField();

            using (CMSCheckBox headerBox = new CMSCheckBox { ID = "headerBox" })
            using (CMSCheckBox itemBox = new CMSCheckBox { ID = "itemBox" })
            {
                // Set selection argument
                itemBox.Attributes["selectioncolumn"] = GridOptions != null ? GridOptions.SelectionColumn : null;
                chkColumn.HeaderTemplate = new GridViewTemplate(ListItemType.Header, this, headerBox);
                chkColumn.ItemTemplate = new GridViewTemplate(ListItemType.Item, this, itemBox);
            }

            GridView.Columns.Add(chkColumn);
        }


        /// <summary>
        /// Returns <c>true</c> if either <see cref="UniGridOptions.ShowSelection"/> is set
        /// or there is at least one <see cref="UniGridMassActions.MassActions"/>, otherwise <c>false</c>.
        /// </summary>
        private bool ShouldShowSelection()
        {
            return (GridOptions != null && GridOptions.ShowSelection)
                || (GridMassActions != null && GridMassActions.MassActions.Any());
        }


        private void LoadColumns()
        {
            // Raise load columns event
            RaiseLoadColumns();

            // Get the initial filter form info
            if (CustomFilter == null)
            {
                var fi = FormHelper.GetFormInfo(FilterFormName, true, false, true);
                if (fi != null)
                {
                    filterSource = FILTER_SOURCE_FORMNAME;
                    FilterFormInfo = fi;
                }
            }

            // Load columns
            if (GridColumns != null)
            {
                foreach (Column col in GridColumns.Columns)
                {
                    col.DataBind();

                    // Load column definition
                    LoadColumnDefinition(col);
                }
            }
        }


        /// <summary>
        /// Returns the physical file path that corresponds to the specified virtual path.
        /// </summary>
        /// <param name="virtualPath">The virtual path in the Web application.</param>
        protected virtual string MapPathInternal(string virtualPath)
        {
            return Server.MapPath(virtualPath);
        }


        /// <summary>
        /// Loads the XML configuration of the grid.
        /// </summary>
        public bool LoadXmlConfiguration()
        {
            // If no configuration is given, do not process
            if (string.IsNullOrEmpty(GridName))
            {
                return true;
            }
            string filePath = MapPathInternal(GridName);

            XElement configuration = GetConfigurationXml(filePath);
            if (configuration == null)
            {
                ShowError(String.Format(GetString("unigrid.noxmlfile"), filePath));
                return false;
            }

            // Load options definition
            var options = configuration.GetElement("options");
            if (options != null)
            {
                GridOptions = new UniGridOptions(options);
            }

            // Load actions definition
            var actions = configuration.GetElement("actions");
            if (actions != null)
            {
                GridActions = new UniGridActions(actions);
            }

            // Load mass actions definition
            var massActions = configuration.GetElement("massactions");
            if (massActions != null)
            {
                GridMassActions = new UniGridMassActions(massActions);
            }

            // Load pager definition
            var pager = configuration.GetElement("pager");
            if (pager != null)
            {
                PagerConfig = new UniGridPagerConfig(pager);
            }

            // Select list of "column" nodes
            var columns = configuration.GetElement("columns");
            if (columns != null)
            {
                GridColumns = new UniGridColumns(columns);
            }

            // Try to get Macro
            var macro = configuration.GetElement("macro");
            if (macro != null)
            {
                // Get object type information
                LoadMacroDefinition(macro);
            }
            else
            {
                // Try to get ObjectType from definition
                var objectType = configuration.GetElement("objecttype");
                if (objectType != null)
                {
                    // Get object type information
                    LoadObjectTypeDefinition(objectType);
                }
                else
                {
                    // Get query information
                    var query = configuration.GetElement("query");
                    LoadQueryDefinition(query);
                }
            }

            return true;
        }


        /// <summary>
        /// Returns XML document from given path.
        /// </summary>
        /// <param name="filePath">File path to the XML document.</param>
        protected virtual XElement GetConfigurationXml(string filePath)
        {
            // Check the configuration file
            if (!File.Exists(filePath))
            {
                return null;
            }

            // Load the XML configuration
            XElement element;
            using (var fileStream = File.OpenRead(filePath))
            {
                element = XElement.Load(fileStream);
            }

            return element;
        }


        /// <summary>
        /// Load options definition.
        /// </summary>
        /// <param name="options">Options configuration</param>
        /// <param name="filterWrapperControl">Wrapper control for filter</param>
        protected void LoadOptionsDefinition(UniGridOptions options, Control filterWrapperControl)
        {
            // Add custom filter or filter wrapper panel according to the key value "DisplayFilter"
            displayFilter = options.DisplayFilter;

            if (displayFilter)
            {
                // Add custom filter
                if ((CustomFilter == null) && !string.IsNullOrEmpty(options.FilterPath))
                {
                    var filter = LoadFilterControl(options.FilterPath);

                    filter.OnFilterChanged += ReloadData;

                    CustomFilter = filter;
                    filterSource = FILTER_SOURCE_CUSTOM;

                    var ctrl = (Control)filter;

                    FilterPlaceHolder.Controls.Add(ctrl);

                    // Raise the filter created event
                    RaiseOnFilterFieldCreated(CUSTOM_FILTER_SOURCE_NAME, new UniGridFilterField
                    {
                        ValueControl = ctrl
                    });
                }
                // Add wrapper panel for default filter
                else
                {
                    FilterPlaceHolder.Controls.Add(filterWrapperControl);
                }
            }

            // Filter limit
            if (options.FilterLimit > -1)
            {
                FilterLimit = options.FilterLimit;
            }

            // Display sort direction images
            showSortDirection = options.ShowSortDirection;
        }


        /// <summary>
        /// Loads actions definition.
        /// </summary>
        /// <param name="actions">Configuration of the actions</param>
        protected void LoadActionsDefinition(UniGridActions actions)
        {
            // Custom template field of the grid view
            TemplateField actionsColumn = new TemplateField();

            // Ensure width of the column
            if (!String.IsNullOrEmpty(actions.Width))
            {
                actionsColumn.ItemStyle.Width = new Unit(actions.Width);
            }

            // Get empty object for security checks
            var emptyObject = UniGridFunctions.GetEmptyObjectWithSiteID(ObjectType, ValidationHelper.GetInteger(UIContext["SiteID"], 0));

            // Add object menu if possible
            if (actions.Actions.Count > 0 && !(actions.Actions.FirstOrDefault() is EmptyAction) && ShowObjectMenu && UniGridFunctions.ShowUniGridObjectContextMenu(emptyObject, SiteContext.CurrentSiteName))
            {
                actions.Actions.RemoveAll(a => a is EmptyAction);

                // Check if object menu already contained
                var objectMenuActionExists = actions.Actions
                    .Any(action => action.Name.Equals("#objectmenu", StringComparison.OrdinalIgnoreCase) || !String.IsNullOrEmpty(action.ContextMenu));

                // Add object menu of necessary
                if (!objectMenuActionExists && !IsLiveSite)
                {
                    Action action = new Action("#objectmenu");
                    action.ExternalSourceName = "#objectmenu";
                    actions.Actions.Add(action);
                }
            }

            if (actions.ShowHeader)
            {
                if (ShowActionsMenu && string.IsNullOrEmpty(actions.ContextMenu))
                {
                    actions.ContextMenu = DEFAULT_ACTIONS_MENU;
                    actions.Caption = "General.OtherActions";
                }

                // Fill in the custom template field
                string label = (ShowActionsLabel ? GetString("unigrid.actions") : String.Empty);

                GridViewTemplate headerTemplate = new GridViewTemplate(ListItemType.Header, this, actions, label, ImageDirectoryPath, DefaultImageDirectoryPath, Page);

                headerTemplate.ContextMenuParent = MenuPlaceHolder;
                headerTemplate.CheckRelative = CheckRelative;

                actionsColumn.HeaderTemplate = headerTemplate;

                if (ShowActionsMenu)
                {
                    if (actions.Actions.FirstOrDefault() is EmptyAction)
                    {
                        actionsColumn.HeaderStyle.CssClass = "unigrid-actions-header-empty";
                    }
                    else
                    {
                        actionsColumn.HeaderStyle.CssClass = "unigrid-actions-header";
                    }
                }
            }

            var actionsTemplate = new GridViewTemplate(ListItemType.Item, this, actions, null, ImageDirectoryPath, DefaultImageDirectoryPath, Page);

            actionsTemplate.OnExternalDataBound += RaiseExternalDataBound;
            actionsTemplate.ContextMenuParent = MenuPlaceHolder;
            actionsTemplate.CheckRelative = CheckRelative;

            actionsColumn.ItemTemplate = actionsTemplate;

            if (IsLiveSite)
            {
                actionsColumn.ItemStyle.Wrap = false;
            }

            if (!String.IsNullOrEmpty(actions.CssClass))
            {
                actionsColumn.HeaderStyle.CssClass = CssHelper.JoinClasses(actionsColumn.HeaderStyle.CssClass, actions.CssClass);
                actionsColumn.ItemStyle.CssClass = CssHelper.JoinClasses(actionsColumn.ItemStyle.CssClass, actions.CssClass);
                actionsColumn.FooterStyle.CssClass = CssHelper.JoinClasses(actionsColumn.FooterStyle.CssClass, actions.CssClass);
            }

            // Add custom column to grid view
            GridView.Columns.Add(actionsColumn);
        }


        /// <summary>
        /// Loads mass actions definition.
        /// </summary>
        /// <param name="actions">Configuration of the actions</param>
        protected void LoadMassActionsDefinition(UniGridMassActions actions)
        {
            MassActions.AdditionalParameters = new Lazy<object>(() =>
            {
                var elementUrl = new Uri(UIContextHelper.GetElementUrl(UIContext.UIElement), UriKind.Relative);
                return new MassActionParameters(ObjectType, elementUrl, GetReloadScript(), CompleteWhereCondition);
            });

            if (!actions.MassActions.Any())
            {
                return;
            }

            MassActions.Visible = true;

            MassActions.AddMassActions(actions.MassActions.ToArray());
        }


        /// <summary>
        /// Load UniGrid pager configuration.
        /// </summary>
        /// <param name="config">Pager configuration</param>
        protected void LoadPagerDefinition(UniGridPagerConfig config)
        {
            if (config.DisplayPager != null)
            {
                Pager.DisplayPager = config.DisplayPager.Value;
            }

            // Load definition only if pager is displayed
            if (Pager.DisplayPager)
            {
                if (config.PageSizeOptions != null)
                {
                    Pager.PageSizeOptions = config.PageSizeOptions;
                }
                if (config.ShowDirectPageControl != null)
                {
                    Pager.ShowDirectPageControl = config.ShowDirectPageControl.Value;
                }
                if (config.ShowFirstLastButtons != null)
                {
                    Pager.ShowFirstLastButtons = config.ShowFirstLastButtons.Value;
                }
                if (config.ShowPageSize != null)
                {
                    Pager.ShowPageSize = config.ShowPageSize.Value;
                }
                if (config.ShowPreviousNextButtons != null)
                {
                    Pager.ShowPreviousNextButtons = config.ShowPreviousNextButtons.Value;
                }
                if (config.ShowPreviousNextPageGroup != null)
                {
                    Pager.ShowPreviousNextPageGroup = config.ShowPreviousNextPageGroup.Value;
                }
                if (config.GroupSize > 0)
                {
                    Pager.GroupSize = config.GroupSize;
                }
                if (config.DefaultPageSize > 0)
                {
                    Pager.DefaultPageSize = config.DefaultPageSize;
                }

                // Try to get page size from request
                string selectedPageSize = Request.Form[Pager.PageSizeDropdown.UniqueID];
                int pageSize = 0;

                if (selectedPageSize != null)
                {
                    pageSize = ValidationHelper.GetInteger(selectedPageSize, 0);
                }
                else if (config.DefaultPageSize > 0)
                {
                    pageSize = config.DefaultPageSize;
                }

                if ((pageSize > 0) || (pageSize == -1))
                {
                    Pager.CurrentPageSize = pageSize;
                }
            }
            else
            {
                // Reset page size
                Pager.CurrentPageSize = -1;
            }
        }


        /// <summary>
        /// Load single column definition.
        /// </summary>
        /// <param name="column">Column to use</param>
        protected void LoadColumnDefinition(Column column)
        {
            DataControlField field;
            string cssClass = column.CssClass;
            string columnCaption = null;

            // Process the column type Hyperlink or BoundColumn based on the parameters
            if ((column.Href != null) ||
                (column.ExternalSourceName != null) ||
                (column.Localize) ||
                (column.Icon != null) ||
                (column.Tooltip != null) ||
                (column.Action != null) ||
                (column.Style != null) ||
                (column.MaxLength > 0))
            {
                ExtendedBoundField linkColumn = new ExtendedBoundField();
                field = linkColumn;

                // Attribute "source"
                if (column.Source != null)
                {
                    linkColumn.DataField = column.Source;

                    // Allow sorting
                    if ((column.AllowSorting) && ((GridOptions == null) || GridOptions.AllowSorting))
                    {
                        if (!String.IsNullOrEmpty(column.Sort))
                        {
                            linkColumn.SortExpression = column.Sort;
                        }
                        else if (!column.Source.Equals(ExtendedBoundField.ALL_DATA, StringComparison.OrdinalIgnoreCase))
                        {
                            linkColumn.SortExpression = column.Source;
                        }
                    }
                }

                // Action parameters
                if (column.Action != null)
                {
                    linkColumn.Action = column.Action;

                    // Action parameters
                    if (column.CommandArgument != null)
                    {
                        linkColumn.CommandArgument = column.CommandArgument;
                    }
                }

                // Action parameters
                if (column.Parameters != null)
                {
                    linkColumn.ActionParameters = column.Parameters;
                }

                // Navigate URL
                if (column.Href != null)
                {
                    linkColumn.NavigateUrl = column.Href;
                }

                // External source
                if (column.ExternalSourceName != null)
                {
                    linkColumn.ExternalSourceName = column.ExternalSourceName;
                    linkColumn.OnExternalDataBound += RaiseExternalDataBound;
                }

                // Localize strings?
                linkColumn.LocalizeStrings = column.Localize;

                // Style
                if (column.Style != null)
                {
                    linkColumn.Style = column.Style;
                }

                // Icon
                if (column.Icon != null)
                {
                    if (String.IsNullOrEmpty(linkColumn.DataField))
                    {
                        linkColumn.DataField = ExtendedBoundField.ALL_DATA;
                    }
                    linkColumn.Icon = GetActionImage(column.Icon);
                }

                // Max length
                if (column.MaxLength > 0)
                {
                    linkColumn.MaxLength = column.MaxLength;
                }

                // Process "tooltip" node
                ColumnTooltip tooltip = column.Tooltip;
                if (tooltip != null)
                {
                    // If there is some tooltip register TooltipScript
                    if ((tooltip.Source != null) || (tooltip.ExternalSourceName != null))
                    {
                        ScriptHelper.RegisterTooltip(Page);
                    }

                    // Tooltip source
                    if (tooltip.Source != null)
                    {
                        linkColumn.TooltipSourceName = tooltip.Source;
                    }

                    // Tooltip external source
                    if (tooltip.ExternalSourceName != null)
                    {
                        linkColumn.TooltipExternalSourceName = tooltip.ExternalSourceName;

                        // Ensure external data bound event handler
                        if (string.IsNullOrEmpty(column.ExternalSourceName))
                        {
                            linkColumn.OnExternalDataBound += RaiseExternalDataBound;
                        }
                    }

                    // Tooltip width
                    if (tooltip.Width != null)
                    {
                        linkColumn.TooltipWidth = tooltip.Width;
                    }

                    // Encode tooltip
                    linkColumn.TooltipEncode = tooltip.Encode;
                }
            }
            else
            {
                BoundField userColumn = new BoundField(); // Custom column of the grid view
                field = userColumn;

                // Attribute "source"
                if (column.Source != null)
                {
                    userColumn.DataField = column.Source;

                    // Allow sorting
                    if ((column.AllowSorting) && ((GridOptions == null) || GridOptions.AllowSorting))
                    {
                        if (!column.Source.Equals(ExtendedBoundField.ALL_DATA, StringComparison.OrdinalIgnoreCase))
                        {
                            userColumn.SortExpression = column.Source;
                        }
                        else if (column.Sort != null)
                        {
                            userColumn.SortExpression = column.Sort;
                        }
                    }
                }
            }

            if (IsLiveSite)
            {
                field.HeaderStyle.Wrap = false;
            }

            // Column name
            if (column.Name != null)
            {
                NamedColumns[column.Name] = field;
            }

            // Caption
            if (column.Caption != null)
            {
                columnCaption = GetString(LocalizationHelper.GetResourceName(column.Caption));
                field.HeaderText = columnCaption;
            }

            // Width
            if (column.Width != null)
            {
                if (GridView.ShowHeader)
                {
                    field.HeaderStyle.Width = new Unit(column.Width);
                }
                else
                {
                    field.ItemStyle.Width = new Unit(column.Width);
                }
            }

            // Visible
            field.Visible = column.Visible;

            // Is text?
            if (column.IsText && (column.Source != null))
            {
                TextColumns.Add(column.Source);
            }

            // Wrap?
            if (column.Wrap)
            {
                field.ItemStyle.CssClass = "wrap-normal";
            }
            else if (IsLiveSite)
            {
                field.ItemStyle.Wrap = false;
            }

            // Class name
            if (cssClass != null)
            {
                field.HeaderStyle.CssClass = CssHelper.JoinClasses(field.HeaderStyle.CssClass, cssClass);
                field.ItemStyle.CssClass = CssHelper.JoinClasses(field.ItemStyle.CssClass, cssClass);
                field.FooterStyle.CssClass = CssHelper.JoinClasses(field.FooterStyle.CssClass, cssClass);
            }

            // Process "filter" node
            if (displayFilter && (CustomFilter == null))
            {
                // Filter
                var filter = column.Filter;
                if (filter != null)
                {
                    string value = null;

                    // Filter via query string
                    if (FilterByQueryString)
                    {
                        if (String.IsNullOrEmpty(filter.Path))
                        {
                            string values = QueryHelper.GetString(column.Source, null);
                            if (!string.IsNullOrEmpty(values))
                            {
                                string[] pair = values.Split(';');
                                value = pair[1];
                            }
                        }
                        else
                        {
                            value = QueryHelper.GetString(column.Source, null);
                        }
                    }

                    AddFilterField(filter, column.Source, columnCaption, value);
                }
            }

            // Add custom column to GridView
            GridView.Columns.Add(field);

            // Store corresponding field
            column.Field = field;
        }


        /// <summary>
        /// Load macro definition from XML.
        /// </summary>
        /// <param name="element">Macro XML element</param>
        private void LoadMacroDefinition(XElement element)
        {
            if (element != null)
            {
                ResetSources();

                Macro = element.GetAttributeStringValue("expression");

                // Set the columns property if columns are defined
                LoadColumns(element);
            }
        }


        private void ResetSources()
        {
            // These three settings cannot coexist
            Macro = null;
            ObjectType = null;
            Query = null;
        }


        /// <summary>
        /// Load object type definition from XML.
        /// </summary>
        /// <param name="element">XML element with object type name</param>
        private void LoadObjectTypeDefinition(XElement element)
        {
            if (element == null)
            {
                return;
            }

            ResetSources();

            ObjectType = element.GetAttributeStringValue("name");

            // Set the columns property if columns are defined
            LoadColumns(element);
        }


        /// <summary>
        /// Load query definition from XML.
        /// </summary>
        /// <param name="element">XML query definition element</param>
        private void LoadQueryDefinition(XElement element)
        {
            if (element == null)
            {
                return;
            }

            ResetSources();

            Query = element.GetAttributeStringValue("name");

            // Set the columns property if columns are defined
            LoadColumns(element);
            LoadAllColumns(element);

            // Load the query parameters
            var childElements = element.GetElements("parameter").ToList();
            if (childElements.Count > 0)
            {
                QueryDataParameters newParams = new QueryDataParameters();

                foreach (var xElement in childElements)
                {
                    string name = xElement.GetAttributeStringValue("name");
                    object value = xElement.GetAttributeStringValue("value");
                    string type = xElement.GetAttributeStringValue("type");

                    switch (type.ToLowerInvariant())
                    {
                        case "int":
                            value = ValidationHelper.GetInteger(value, 0);
                            break;

                        case "double":
                            value = Convert.ToDouble(value);
                            break;

                        case "decimal":
                            value = Convert.ToDecimal(value);
                            break;

                        case "bool":
                            value = Convert.ToBoolean(value);
                            break;
                    }

                    newParams.Add(name, value);
                }

                QueryParameters = newParams;
            }
        }


        /// <summary>
        /// Sets the columns property if columns are defined.
        /// </summary>
        /// <param name="element">Node from which to load columns</param>
        private void LoadAllColumns(XElement element)
        {
            var allColumns = element.GetAttributeStringValue("allcolumns");
            if (allColumns != null)
            {
                AllColumns = DataHelper.GetNotEmpty(allColumns, AllColumns);
            }
        }


        /// <summary>
        /// Sets the columns property if columns are defined.
        /// </summary>
        /// <param name="element">Node from which to load columns</param>
        private void LoadColumns(XElement element)
        {
            var columns = element.GetAttributeStringValue("columns");
            if (columns != null)
            {
                Columns = DataHelper.GetNotEmpty(columns, Columns);
            }
        }

        #endregion


        #region "Filter methods"

        /// <summary>
        /// Determines whether to show external filter.
        /// </summary>
        /// <param name="filterIsSet">Is external filter set?</param>
        /// <returns>TRUE if external filter should be displayed</returns>
        public bool DisplayExternalFilter(bool filterIsSet)
        {
            if (filterIsSet)
            {
                return true;
            }

            if (!DataHelper.DataSourceIsEmpty(GridView.DataSource))
            {
                return (PagerForceNumberOfResults > FilterLimit) || (FilterLimit <= 0);
            }

            return false;
        }


        /// <summary>
        /// Applies current filter where condition.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">An System.EventArgs that contains no event data</param>
        public void ApplyFilter(object sender, EventArgs e)
        {
            RaiseShowButtonClick(sender, e);

            string where = GetFilter();
            where = RaiseBeforeFiltering(where);

            // If where condition is empty string, filter is in not-set state
            FilterIsSet = !string.IsNullOrEmpty(where);

            Pager.UniPager.CurrentPage = 1;
            SetFilter(!DelayedReload, where);
        }


        /// <summary>
        /// Loads the filter control
        /// </summary>
        /// <param name="path">Control path</param>
        protected IFilterControl LoadFilterControl(string path)
        {
            string fullPath = (path.StartsWith("~/", StringComparison.Ordinal) ? path : FilterDirectoryPath + path.TrimStart('/'));

            // Load the filter control
            var ctrl = LoadUserControl(fullPath);
            if (ctrl != null)
            {
                // Setup the filter control
                ctrl.ID = "cf";

                var filterControl = ctrl as IFilterControl;
                if (filterControl != null)
                {
                    filterControl.FilteredControl = this;
                }

                return filterControl;
            }

            return null;
        }


        /// <summary>
        /// Add filter field to the filter table.
        /// </summary>
        /// <param name="filter">Filter definition</param>
        /// <param name="columnSourceName">Column source field name</param>
        /// <param name="fieldDisplayName">Field display name</param>
        /// <param name="filterValue">Filter value</param>
        private void AddFilterField(ColumnFilter filter, string columnSourceName, string fieldDisplayName, string filterValue)
        {
            var fieldSourceName = filter.Source ?? columnSourceName;

            if (String.IsNullOrEmpty(fieldSourceName))
            {
                return;
            }

            // Ensure fieldSourceName is JavaScript valid
            fieldSourceName = fieldSourceName.Replace(ALL, "__ALL__");

            // Ensure filter form info
            if (FilterFormInfo == null)
            {
                FilterFormInfo = new FormInfo();
            }

            var fi = FilterFormInfo;
            var fieldName = filter.FieldName ?? fieldSourceName;

            var ffi = new FormFieldInfo
            {
                Name = fi.GetUniqueFieldName(fieldName),
                FieldType = FormFieldControlTypeEnum.CustomUserControl,
                Caption = fieldDisplayName
            };

            string filterFormat = ConvertFilterFormat(filter.Format);

            // If the field name is not the original one, adjust the filter format to use the original source
            if (ffi.Name != fieldSourceName)
            {
                if (String.IsNullOrEmpty(filterFormat))
                {
                    filterFormat = "[" + fieldSourceName + "] {2} {1}";
                }
                else
                {
                    filterFormat = filterFormat.Replace("{0}", fieldSourceName);
                }
            }

            // Set the where condition format based on the filter format
            if (!String.IsNullOrEmpty(filterFormat))
            {
                ffi.Settings["WhereConditionFormat"] = filterFormat;
            }

            // Apply custom filter parameters
            if (filter.CustomFilterParameters != null)
            {
                filter.CustomFilterParameters.Parameters.ForEach(parameter => ffi.Settings[parameter.Name] = parameter.Value);
            }

            string fieldPath = filter.Path;

            switch (filter.Type)
            {
                // Text filter
                case UniGridFilterTypeEnum.Text:
                    {
                        int filterSize = filter.Size;

                        ffi.DataType = (filterSize > 0) ? FieldDataType.Text : FieldDataType.LongText;
                        ffi.Settings["controlname"] = "TextFilter";
                        ffi.Size = filterSize;
                    }
                    break;

                // Boolean filter
                case UniGridFilterTypeEnum.Bool:
                    {
                        ffi.DataType = FieldDataType.Boolean;
                        ffi.Settings["controlname"] = "BooleanFilter";
                    }
                    break;

                // Integer filter
                case UniGridFilterTypeEnum.Integer:
                case UniGridFilterTypeEnum.Double:
                case UniGridFilterTypeEnum.Decimal:
                    {
                        ffi.DataType = GetNumericFilterDataType(filter.Type);
                        ffi.Settings["controlname"] = "NumberFilter";
                    }
                    break;

                case UniGridFilterTypeEnum.Site:
                    {
                        // Site selector
                        fieldPath = "~/CMSFormControls/Filters/SiteFilter.ascx";
                    }
                    break;

                case UniGridFilterTypeEnum.Custom:
                    // Custom control
                    {
                        var controlName = filter.ControlName;

                        if (!String.IsNullOrEmpty(controlName))
                        {
                            ffi.DataType = FieldDataType.LongText;
                            ffi.Settings["controlname"] = controlName;
                        }
                        else if (String.IsNullOrEmpty(fieldPath))
                        {
                            throw new Exception("[UniGrid.AddFilterField]: Custom filter must have the field path or control name set.");
                        }
                    }
                    break;

                default:
                    // Not supported filter type
                    throw new Exception("[UniGrid.AddFilterField]: Filter type '" + filter.Type + "' is not supported. Supported filter types: integer, double, decimal, bool, text, site, custom.");
            }

            // Else if filter path is defined use custom filter
            if (fieldPath != null)
            {
                ffi.DataType = FieldDataType.LongText;
                ffi.Settings["controlpath"] = fieldPath;
            }

            // Set default value
            ffi.SetPropertyValue(FormFieldPropertyEnum.DefaultValue, filterValue ?? filter.DefaultValue);

            // Add the field to the form
            fi.AddFormItem(ffi);

            filterSource = FILTER_SOURCE_CONFIGURED;
        }


        private static string GetNumericFilterDataType(UniGridFilterTypeEnum filterType)
        {
            switch (filterType)
            {
                case UniGridFilterTypeEnum.Integer:
                    return FieldDataType.Integer;

                case UniGridFilterTypeEnum.Decimal:
                    return FieldDataType.Decimal;

                default:
                    return FieldDataType.Double;
            }
        }


        /// <summary>
        /// Converts the filter format from the unigrid format to form control format
        /// </summary>
        /// <param name="filterFormat">Filter format</param>
        private string ConvertFilterFormat(string filterFormat)
        {
            if (String.IsNullOrEmpty(filterFormat))
            {
                return filterFormat;
            }

            // {3} means full default where condition, but is not supported by underlying control, we need to convert it
            filterFormat = filterFormat.Replace("{3}", "[{0}] {1} {2}");

            // Unigrid format is 0=column 1=operator 2=value, while form control format is 0=column, 1=value, 2=operator
            return filterFormat.Replace("{1}", "##OP##").Replace("{2}", "{1}").Replace("##OP##", "{2}");
        }


        /// <summary>
        /// Returns where condition from UniGrid filters.
        /// </summary>
        public string GetFilter()
        {
            var where = new WhereCondition();

            IFilterControl filter = CustomFilter ?? FilterForm;
            if (filter != null)
            {
                where.Where(filter.WhereCondition);

                // Prepare query string
                if (FilterByQueryString && RequestHelper.IsPostBack())
                {
                    filterQueryParameters[CUSTOM_FILTER_SOURCE_NAME] = Convert.ToString(filter.Value);
                }
            }

            return where.ToString(true);
        }


        /// <summary>
        /// Sets basic form filter.
        /// </summary>
        protected void SetBasicFormFilter()
        {
            // Get form info
            var fi = FilterFormInfo;
            if (fi != null)
            {
                var filter = FilterForm;

                filter.FilteredControl = this;

                filter.OnValidationFailed += BasicForm_OnValidationFailed;
                filter.OnAfterSave += BasicForm_OnAfterSave;
                filter.OnAfterDataLoad += filter_OnAfterDataLoad;
                filter.OnFilterChanged += ReloadData;

                // Set form info
                filter.FormInformation = fi;

                FilterFormData = fi.CreateDataContainer();

                // Set filter button
                var showButton = filter.SubmitButton;

                showButton.ID = "btnShow";
                showButton.ResourceString = "general.search";
                showButton.ButtonStyle = ButtonStyle.Primary;
                showButton.RegisterHeaderAction = false;

                // Set custom layout
                if (!String.IsNullOrEmpty(FilterFormName))
                {
                    var afi = AlternativeFormInfoProvider.GetAlternativeFormInfo(FilterFormName);
                    filter.AltFormInformation = afi;
                }

                filter.CheckFieldEmptiness = false;
                filter.EnsureMessagesPlaceholder(MessagesPlaceHolder);

                filter.LoadData(FilterFormData);
            }
            else
            {
                // Hide filter
                FilterForm.StopProcessing = true;
                FilterForm.Visible = false;
            }
        }


        /// <summary>
        /// Resets filter as a reaction to failure of validation of filter basic form.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event argument</param>
        private void BasicForm_OnValidationFailed(object sender, EventArgs e)
        {
            SetFilter(false);
        }


        private void filter_OnAfterDataLoad(object sender, EventArgs e)
        {
            if (RememberState && (FilterForm.FormButtonPanel != null))
            {
                mResetButton = CreateResetButton();

                // Add reset button to the form
                FilterForm.FormButtonPanel.Controls.AddAt(0, mResetButton);
            }

            // Raise field created event
            foreach (DictionaryEntry item in FilterForm.FieldControls)
            {
                var columnName = item.Key.ToString();

                var ctrl = item.Value as Control;
                if (ctrl != null)
                {
                    // Get a proper control for the wrapped filter control
                    var filterWrapper = ctrl as FilterFormControlWrapper;
                    if (filterWrapper != null)
                    {
                        ctrl = (Control)filterWrapper.FilterControl;
                    }

                    RaiseOnFilterFieldCreated(columnName, new UniGridFilterField
                    {
                        ValueControl = ctrl
                    });
                }
            }
        }


        /// <summary>
        /// Handles OnAfterSave event of basic form.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event argument</param>
        private void BasicForm_OnAfterSave(object sender, EventArgs e)
        {
            ApplyFormFilter();

            // We want to explicitly remember the default state to make sure it gets properly saved
            RememberDefaultState = true;
        }


        /// <summary>
        /// Sets the filter based on the status of the filter form
        /// </summary>
        private void ApplyFormFilter()
        {
            // Set where clause
            WhereClause = FilterForm.GetWhereCondition();
            FilterForm.StopProcessing = true;

            WhereClause = RaiseBeforeFiltering(WhereClause);

            // When the filter button is clicked, the filter is always set
            FilterIsSet = true;

            Pager.UniPager.CurrentPage = 1;
            ReloadData();
        }


        /// <summary>
        /// Sets filter to the grid view and save it to the view state.
        /// </summary>
        /// <param name="reloadData">Reload data</param>
        /// <param name="where">Filter where condition</param>
        protected void SetFilter(bool reloadData, string where = null)
        {
            // Where can be empty string - it means that filter condition was added to WhereCondition property
            if (where == null)
            {
                where = GetFilter();
            }

            // Filter by query string
            if (FilterByQueryString && !reloadData)
            {
                string url = RequestContext.CurrentURL;
                foreach (var queryParameter in filterQueryParameters)
                {
                    if (queryParameter.Value != null)
                    {
                        string value = HttpContext.Current.Server.UrlEncode(queryParameter.Value);
                        url = URLHelper.AddParameterToUrl(url, queryParameter.Key, value);
                    }
                }
                URLHelper.Redirect(url);
            }
            else
            {
                WhereClause = where;
                if (!String.IsNullOrEmpty(where))
                {
                    FilterIsSet = true;
                }
                if ((!DelayedReload) && (reloadData))
                {
                    // Get data from database and set them to the grid view
                    ReloadData();
                }
            }
        }


        /// <summary>
        /// Process data from filter.
        /// </summary>
        private void ShowButton_Click(object sender, EventArgs e)
        {
            if (!IsUsingFilterForm)
            {
                ApplyFilter(sender, e);
            }
            else
            {
                ApplyFormFilter();
            }

            // We want to explicitly remember the default state to make sure it gets properly saved
            RememberDefaultState = true;
        }


        private void ResetButton_Click(object sender, EventArgs e)
        {
            Reset();
        }


        /// <summary>
        /// Creates filter show button.
        /// </summary>
        private void CreateFilterButtons(Control filterControl)
        {
            if (!HideFilterButton && !IsUsingFilterForm)
            {
                Panel filterWrapperPanel = new Panel
                {
                    CssClass = "form-group form-group-buttons"
                };

                Panel panelButtons = new Panel
                {
                    CssClass = "filter-form-buttons-cell-wide",
                    EnableViewState = false
                };

                // If we remember the grid state we need to add the reset button to allow the user to restore the initial state
                if (RememberState)
                {
                    mResetButton = CreateResetButton();

                    panelButtons.Controls.Add(mResetButton);
                }

                mShowButton = CreateShowButton();

                panelButtons.Controls.Add(mShowButton);

                filterWrapperPanel.Controls.Add(panelButtons);

                if (CustomFilter != null)
                {
                    FilterPlaceHolder.Controls.Add(filterWrapperPanel);
                }
                else
                {
                    filterControl.Controls.Add(filterWrapperPanel);
                }
            }
        }


        private CMSButton CreateShowButton()
        {
            var showButton = new CMSButton
            {
                ID = "btnShow",
                Text = GetString("general.filter"),
                ButtonStyle = ButtonStyle.Primary,
                EnableViewState = false
            };

            showButton.Click += ShowButton_Click;
            return showButton;
        }


        private CMSButton CreateResetButton()
        {
            var resetButton = new CMSButton
            {
                ID = "btnReset",
                Text = GetString("general.reset"),
                ButtonStyle = ButtonStyle.Default,
                EnableViewState = false
            };

            resetButton.Click += ResetButton_Click;

            return resetButton;
        }


        /// <summary>
        /// Sets filter visibility depending on the UniGrid's configuration and number of objects. Returns true if the filter is displayed.
        /// </summary>
        private bool UpdateFilterVisibility()
        {
            FilterPlaceHolder.Visible = false;
            FilterFormPlaceHolder.Visible = false;

            if (displayFilter)
            {
                if (FilterLimit > -1)
                {
                    bool filterVisible = FilterIsSet || ShowFilter;

                    if (IsUsingFilterForm)
                    {
                        FilterFormPlaceHolder.Visible = filterVisible;
                    }
                    else
                    {
                        FilterPlaceHolder.Visible = filterVisible;
                    }

                    return filterVisible;
                }
            }

            return false;
        }

        #endregion


        #region "IPostbackEventHandler, ICallBackEventHandler Members"

        /// <summary>
        /// Raises the postback event
        /// </summary>
        /// <param name="eventArgument"></param>
        public void RaisePostBackEvent(string eventArgument)
        {
            switch (eventArgument.ToLowerInvariant())
            {
                case "reset":
                    Reset();
                    break;

                case "showfilter":
                    ShowFilter = true;
                    UpdateFilterVisibility();
                    break;
            }
        }


        /// <summary>
        /// Sets callback event arguments.
        /// </summary>
        /// <param name="eventArgument">Arguments of raised event.</param>
        public void RaiseCallbackEvent(string eventArgument)
        {
            callbackArguments = eventArgument;
        }


        /// <summary>
        /// Returns callback result.
        /// </summary>
        public string GetCallbackResult()
        {
            if (!string.IsNullOrEmpty(callbackArguments))
            {
                // Get argument fractions
                string[] fractions = callbackArguments.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

                if (fractions.Length >= 2)
                {
                    // Validate all selected/deselected items
                    var items = new
                    {
                        Values = new List<string>(fractions.Length - 1),
                        Hashes = new List<string>(fractions.Length - 1)
                    };

                    fractions
                        .Skip(1)
                        .Select(f => f.Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries))
                        .Aggregate(items, (i, p) =>
                        {
                            i.Values.Add(p[0]);
                            i.Hashes.Add(p[1]);

                            return i;
                        });

                    // Validate the hashes
                    bool isValid = ValidationHelper.ValidateHashes(items.Values, items.Hashes, new HashSettings(ClientID) { Redirect = false });
                    if (isValid)
                    {
                        // If hashes valid, return the hash of the first item as success
                        return ValidationHelper.GetHashString(fractions[0], new HashSettings(ClientID));
                    }
                }
            }

            return string.Empty;
        }

        #endregion


        #region "Action methods"

        /// <summary>
        /// Clears UniGrid's information on recently performed action. Under normal circumstances there is no need to perform this action.
        /// However sometimes forcing grid to clear the actions is required.
        /// </summary>
        public void ClearActions()
        {
            // Clear hidden fields
            CmdNameHiddenField.Value = null;
            CmdArgHiddenField.Value = null;
        }


        /// <summary>
        /// Sets hidden field with actions hashes.
        /// </summary>
        protected void SetActionsHash()
        {
            if (ActionsID.Count > 0)
            {
                SetHiddenValues(ActionsID, ActionsHidden, ActionsHashHidden, ClientID);
            }
        }


        /// <summary>
        /// Returns icon file for current theme or from default if current doesn't exist.
        /// </summary>
        /// <param name="iconfile">Icon file name</param>
        protected string GetActionImage(string iconfile)
        {
            if (File.Exists(MapPath(ImageDirectoryPath + iconfile)))
            {
                return (ImageDirectoryPath + iconfile);
            }

            // Short path to the icon
            if (ControlsExtensions.RenderShortIDs)
            {
                return UIHelper.GetShortImageUrl(UIHelper.UNIGRID_ICONS, iconfile);
            }

            return GetImageUrl("Design/Controls/UniGrid/Actions/" + iconfile);
        }


        /// <summary>
        /// Handles the action event.
        /// </summary>
        /// <param name="cmdName">Command name</param>
        /// <param name="cmdValue">Command value</param>
        public void HandleAction(string cmdName, string cmdValue)
        {
            string action = cmdName.ToLowerInvariant();

            // Check action security and redirect if user not authorized
            CheckActionAndRedirect(action);

            switch (action)
            {
                case "#delete":
                case "#destroyobject":
                case "#moveup":
                case "#movedown":
                case "#move":
                    ProcessInternalAction(cmdName, cmdValue, action);
                    break;

                default:
                    RaiseAction(cmdName, cmdValue);
                    break;
            }
        }


        private void ProcessInternalAction(string cmdName, string cmdValue, string action)
        {
            // Command value for #move action is in form 'objectId:previousIndex:newIndex'
            string value = cmdName.Equals("#move", StringComparison.OrdinalIgnoreCase) ? cmdValue.Remove(cmdValue.IndexOf(':')) : cmdValue;
            int objectId = ValidationHelper.GetInteger(value, 0);

            if (objectId > 0)
            {
                var infoObj = ModuleManager.GetReadOnlyObject(ObjectType);
                var objectType = infoObj.TypeInfo.IsListingObjectTypeInfo ? infoObj.TypeInfo.OriginalObjectType : infoObj.TypeInfo.ObjectType;
                infoObj = ProviderHelper.GetInfoById(objectType, objectId);

                if (infoObj != null)
                {
                    switch (action)
                    {
                        case "#delete":
                            ProcessDeleteAction(infoObj, cmdName, cmdValue);

                            break;

                        case "#destroyobject":
                            ProcessDestroyAction(infoObj, cmdValue);

                            break;

                        case "#moveup":
                        case "#movedown":
                        case "#move":
                            ProcessMoveAction(infoObj, action, cmdValue);

                            break;
                    }
                }
            }
        }


        /// <summary>
        /// Checks permissions and tries to destroy given info object.
        /// </summary>
        /// <param name="infoObj">Info object to be destroyed.</param>
        /// <param name="cmdValue">Command value</param>
        private void ProcessDestroyAction(BaseInfo infoObj, string cmdValue)
        {
            try
            {
                if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerObject(PermissionsEnum.Destroy, infoObj, SiteContext.CurrentSiteName, true))
                {
                    return;
                }

                using (var context = new CMSActionContext())
                {
                    context.CreateVersion = false;

                    var action = GridActions.GetActionByName("#delete");
                    if (action != null)
                    {
                        HandleAction("#delete", cmdValue);
                    }
                    else
                    {
                        action = GridActions.GetActionByName("delete");
                        if (action != null)
                        {
                            RaiseAction("delete", cmdValue);
                        }
                        else
                        {
                            ShowError(GetString("objectversioning.destroyobject.nodeleteaction"));
                        }
                    }
                }
            }
            catch (PermissionCheckException ex)
            {
                ShowNotAuthorizedMessage(ex.ModuleName, ex.PermissionFailed);
            }
        }


        /// <summary>
        /// Checks permissions and tries to delete given info object.
        /// Displays error message if other objects are dependent or user is not granted with corresponding permission.
        /// </summary>
        /// <param name="infoObj">Info object to be deleted.</param>
        /// <param name="cmdName">Command name</param>
        /// <param name="cmdValue">Command value</param>
        /// <returns>True if object was successfully deleted.</returns>
        private void ProcessDeleteAction(BaseInfo infoObj, string cmdName, string cmdValue)
        {
            try
            {
                if (infoObj.CheckPermissions(PermissionsEnum.Delete, SiteContext.CurrentSiteName, CurrentUser, true))
                {
                    // Delete the object
                    infoObj.Delete();

                    // Raise for additional actions
                    RaiseAction(cmdName, cmdValue);
                }
            }
            catch (CheckDependenciesException)
            {
                var description = GetCheckDependenciesDescription(infoObj);
                ShowError(GetString("unigrid.deletedisabledwithoutenable"), description);
            }
            catch (PermissionCheckException ex)
            {
                ShowNotAuthorizedMessage(ex.ModuleName, ex.PermissionFailed);
            }
        }


        /// <summary>
        /// Checks permissions and moves given info object up or down.
        /// Displays error message if user is not granted with corresponding permission.
        /// </summary>
        /// <param name="infoObj">Info object to be moved.</param>
        /// <param name="action">Action specifying movement direction. Use "#moveup" or "#movedown".</param>
        /// <param name="cmdValue">Contains command value if "#move" is in the <paramref name="action"/>.</param>
        private void ProcessMoveAction(BaseInfo infoObj, string action, string cmdValue)
        {
            try
            {
                // Check permissions and move object according to action
                if (infoObj.CheckPermissions(PermissionsEnum.Modify, SiteContext.CurrentSiteName, CurrentUser, true))
                {
                    switch (action)
                    {
                        case "#moveup":
                            {
                                infoObj.Generalized.MoveObjectUp();
                            }
                            break;

                        case "#movedown":
                            {
                                infoObj.Generalized.MoveObjectDown();
                            }
                            break;

                        case "#move":
                            {
                                if (!String.IsNullOrEmpty(cmdValue))
                                {
                                    // Command value for #move action is in form 'objectId:previousIndex:newIndex'
                                    string[] data = cmdValue.Split(':');

                                    if (data.Length == 3)
                                    {
                                        int previousIndex = Int32.Parse(data[1]);
                                        int newIndex = Int32.Parse(data[2]);

                                        infoObj.Generalized.SetObjectOrder(newIndex - previousIndex, true);
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            catch (PermissionCheckException ex)
            {
                ShowNotAuthorizedMessage(ex.ModuleName, ex.PermissionFailed);
            }
        }


        /// <summary>
        /// Shows error message stating that user has insufficient privileges.
        /// </summary>
        /// <param name="resourceCodeName">Codename of the module.</param>
        /// <param name="permissionCodeName">Codename of the permission.</param>
        private void ShowNotAuthorizedMessage(string resourceCodeName, string permissionCodeName)
        {
            var resource = ResourceInfoProvider.GetResourceInfo(resourceCodeName);
            if (resource != null)
            {
                var permission = PermissionNameInfoProvider.GetPermissionNameInfo(permissionCodeName, resourceCodeName, null);

                if (permission != null)
                {
                    var title = string.Format(GetString("general.permissionresource"), permission.PermissionDisplayName, resource.ResourceDisplayName);
                    ShowError(title);
                }
            }
        }


        /// <summary>
        /// Checks whether user is authorized for specified action.
        /// </summary>
        /// <param name="actionName">Action name</param>
        private void CheckActionAndRedirect(string actionName)
        {
            if (GridActions == null)
            {
                return;
            }

            // Get the action
            var action = GridActions.GetActionByName(actionName);
            if (action != null)
            {
                // Check module permissions
                if (!action.CheckPermissionsAuthorization())
                {
                    RedirectToAccessDenied(action.ModuleName, action.Permissions);
                }

                // Check module UI elements
                if (!action.CheckUIElementsAuthorization())
                {
                    RedirectToUIElementAccessDenied(action.ModuleName, action.UIElements);
                }
            }
        }

        #endregion


        #region "Sort methods"

        /// <summary>
        /// Sorts UniGrid data source according to sort directive saved in ViewState.
        /// </summary>
        protected void SortUniGridDataSource(object ds)
        {
            if (!String.IsNullOrEmpty(SortDirect))
            {
                // If source isn't empty
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    try
                    {
                        // Set sort directive from ViewState
                        if (ds is DataTable)
                        {
                            ((DataTable)(ds)).DefaultView.Sort = SortDirect;
                        }
                        else if (ds is DataSet)
                        {
                            ((DataSet)(ds)).Tables[0].DefaultView.Sort = SortDirect;
                        }
                        else if (ds is DataView)
                        {
                            ((DataView)(ds)).Sort = SortDirect;
                        }
                    }
                    catch
                    {
                        // Suppress error
                    }
                }
            }
        }


        /// <summary>
        /// Changes sorting direction by specified column.
        /// </summary>
        /// <param name="orderByColumn">Column name to order by</param>
        /// <param name="orderByString">Old order by string</param>
        private void ChangeSortDirection(string orderByColumn, string orderByString)
        {
            orderByColumn = orderByColumn.Trim().TrimStart('[').TrimEnd(']').Trim();
            orderByString = orderByString.Trim().TrimStart('[');

            // If order by column is long text use CAST in ORDER BY part of query
            if (TextColumns.Contains(orderByColumn))
            {
                SortDirect = String.Format("CAST([{0}] AS nvarchar(32)) {1}", orderByColumn, orderByString.EndsWith("DESC", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC");
            }
            else
            {
                // Sort by the same column -> the other direction
                if (orderByString.StartsWith(orderByColumn, StringComparison.OrdinalIgnoreCase))
                {
                    string orderByDirection = "ASC";

                    Match orderByMatch = OrderByRegex.Match(orderByString);
                    if (orderByMatch.Success)
                    {
                        if (orderByMatch.Groups[2].Success)
                        {
                            orderByDirection = orderByMatch.Groups[2].Value;
                        }
                    }

                    SortDirect = String.Format("[{0}] {1}", orderByColumn, orderByDirection.Equals("DESC", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC");
                }
                // Sort by a new column -> implicitly direction is ASC
                else
                {
                    SortDirect = String.Format("[{0}] ASC", orderByColumn);
                }
            }
        }


        /// <summary>
        /// Sets the sort direction if current request is sorting.
        /// </summary>
        protected void ProcessSorting()
        {
            // Get current event target
            string uniqieId = ValidationHelper.GetString(Request.Params[Page.postEventSourceID], String.Empty);

            // Get current argument
            string eventargument = ValidationHelper.GetString(Request.Params[Page.postEventArgumentID], String.Empty);

            if ((uniqieId == GridView.UniqueID) && eventargument.StartsWith("Sort", StringComparison.OrdinalIgnoreCase))
            {
                string orderByColumn = Convert.ToString(eventargument.Split('$')[1]);

                if (SortColumns.Contains(orderByColumn.ToLowerInvariant()))
                {
                    // If sorting is called for the first time and default sorting (OrderBy property) is set
                    if ((SortDirect == "") && !string.IsNullOrEmpty(OrderBy))
                    {
                        ChangeSortDirection(orderByColumn, OrderBy);
                    }
                    else
                    {
                        ChangeSortDirection(orderByColumn, SortDirect);
                    }
                }
            }
        }

        #endregion


        #region "Hidden values methods"

        /// <summary>
        /// Returns array list from hidden field.
        /// </summary>
        /// <param name="field">Hidden field with values separated with |</param>
        /// <param name="hashField">Hidden field containing hash of <paramref name="field"/></param>
        /// <param name="securityPurpose">Prevents the control values from being maliciously used in a different control. A suitable value can be the control's <see cref="Control.ClientID"/>.</param>
        private static List<string> GetHiddenValues(HiddenField field, HiddenField hashField, string securityPurpose)
        {
            var result = new List<string>();

            if (ValidateHiddenValues(field, hashField, securityPurpose))
            {
                // Add values to the result
                result = field.Value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            return result;
        }


        /// <summary>
        /// Validates values in hidden fields with corresponding hash.
        /// </summary>
        /// <param name="field">Hidden field with values separated with |</param>
        /// <param name="hashField">Hidden field containing hash of <paramref name="field"/></param>
        /// <param name="securityPurpose">Prevents the control values from being maliciously used in a different control. A suitable value can be the control's <see cref="Control.ClientID"/>.</param>
        private static bool ValidateHiddenValues(HiddenField field, HiddenField hashField, string securityPurpose)
        {
            return ValidationHelper.ValidateHash(field.Value, hashField.Value, new HashSettings(securityPurpose) { Redirect = false });
        }


        /// <summary>
        /// Sets values into hidden field.
        /// </summary>
        /// <param name="values">Values to set</param>
        /// <param name="actionsField">Hidden field</param>
        /// <param name="hashField">Hash field</param>
        /// <param name="securityPurpose">Prevents the control values from being maliciously used in a different control. A suitable value can be the control's <see cref="Control.ClientID"/>.</param>
        private static void SetHiddenValues(IEnumerable<string> values, HiddenField actionsField, HiddenField hashField, string securityPurpose)
        {
            if (values != null)
            {
                if (actionsField != null)
                {
                    // Action IDs
                    string actions = String.Format("|{0}|", values.Join("|"));
                    actionsField.Value = actions;

                    // Actions hash
                    if (hashField != null)
                    {
                        hashField.Value = ValidationHelper.GetHashString(actions, new HashSettings(securityPurpose));
                    }
                }
            }
        }


        /// <summary>
        /// Clears all selected items from hidden values.
        /// </summary>
        /// <param name="field">Hidden field</param>
        private static void ClearHiddenValues(HiddenField field)
        {
            if (field != null)
            {
                field.Value = String.Empty;
            }
        }

        #endregion


        #region "IUniPageable Members"

        /// <summary>
        /// Pager data item.
        /// </summary>
        public object PagerDataItem
        {
            get
            {
                return GridView.DataSource;
            }
            set
            {
                GridView.DataSource = value;
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
        /// Occurs when the control bind data.
        /// </summary>
        public event EventHandler<EventArgs> OnPageBinding;


        /// <summary>
        /// Raises the OnPageBinding event
        /// </summary>
        protected void RaiseOnPageBinding()
        {
            // Call page binding event
            OnPageBinding?.Invoke(this, null);
        }

        #endregion


        #region "Inner controls methods"

        /// <summary>
        /// Add new basic column into grid view.
        /// </summary>
        /// <param name="header">Header caption</param>
        /// <param name="columnName">Data column name</param>
        /// <param name="width">Width of column in GridView. If width is zero no width will be set</param>
        /// <param name="allowSorting">Indicates if the sorting is allowed</param>
        public void AddColumn(string header, string columnName, int width = 0, bool allowSorting = false)
        {
            BoundField column = new BoundField { HeaderText = header, DataField = columnName };

            column.ItemStyle.Wrap = false;
            column.HeaderStyle.Wrap = false;

            if (width > 0)
            {
                if (GridView.ShowHeader)
                {
                    column.HeaderStyle.Width = Unit.Percentage(width);
                }
                else
                {
                    column.ItemStyle.Width = Unit.Percentage(width);
                }
            }

            if (allowSorting)
            {
                column.SortExpression = columnName;
            }

            GridView.Columns.Add(column);
        }


        private void GridView_RowCreated(object sender, GridViewRowEventArgs e)
        {
            SetupGridRow(e.Row);
        }


        private void GridView_DataBinding(object sender, EventArgs e)
        {
            SetPager();

            RaiseOnPageBinding();

            bool filterDisplayed = UpdateFilterVisibility();

            UpdateActionSettings(filterDisplayed);
        }


        private void GridView_DataBound(object sender, EventArgs e)
        {
            // Set actions hash into hidden field
            SetActionsHash();
        }


        private void GridView_Sorting(object sender, GridViewSortEventArgs e)
        {
            RaiseBeforeSorting(sender, e);
        }


        private void PageSizeDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            RaisePageSizeChanged();
        }


        /// <summary>
        /// Sets up an individual grid row
        /// </summary>
        /// <param name="row">Row to set up</param>
        protected void SetupGridRow(GridViewRow row)
        {
            if (row.RowType == DataControlRowType.Header)
            {
                var sort = SortDirect.ToLowerInvariant();

                // Add sorting definition to list of sort columns
                SortColumns.Add(sort);

                // Parse the sort expression
                sort = sort.Replace("[", "").Replace("]", "").Trim();
                if (sort.StartsWith("cast(", StringComparison.Ordinal))
                {
                    sort = sort.Substring(5);
                }

                Match sortMatch = OrderByRegex.Match(sort);
                string sortColumn = null;
                string sortDirection;
                if (sortMatch.Success)
                {
                    // Get column name
                    if (sortMatch.Groups[1].Success)
                    {
                        sortColumn = sortMatch.Groups[1].Value;
                    }
                    // Get sort direction
                    sortDirection = sortMatch.Groups[2].Success ? sortMatch.Groups[2].Value : "asc";
                }
                else
                {
                    // Get column name from sort expression
                    int space = sort.IndexOfAny(new[]
                    {
                        ' ',
                        ','
                    });
                    sortColumn = space > -1 ? sort.Substring(0, space) : sort;
                    sortDirection = "asc";
                }

                // Check if displaying arrow indicating sorting is requested
                if (showSortDirection)
                {
                    // Prepare the columns
                    foreach (TableCell Cell in row.Cells)
                    {
                        // If there is some sorting expression
                        DataControlFieldCell dataField = (DataControlFieldCell)Cell;
                        string fieldSortExpression = dataField.ContainingField.SortExpression;
                        if (!DataHelper.IsEmpty(fieldSortExpression))
                        {
                            SortColumns.Add(fieldSortExpression.ToLowerInvariant());

                            // If actual sorting expressions is this cell
                            if (CMSString.Equals(sortColumn, fieldSortExpression.Replace("[", "").Replace("]", "").Trim(), true))
                            {
                                // Initialize sort arrow
                                Literal sortArrow = new Literal
                                {
                                    Text = String.Format("<i class=\"{0}\" aria-hidden=\"true\"></i>", ((sortDirection == "desc") ? "icon-caret-down" : "icon-caret-up"))
                                };

                                if (DataHelper.IsEmpty(Cell.Text))
                                {
                                    if (Cell.Controls.Count != 0)
                                    {
                                        // Add original text
                                        Cell.Controls[0].Controls.Add(new LiteralControl(String.Format("<span class=\"unigrid-sort-label\">{0}</span>", ((LinkButton)(Cell.Controls[0])).Text)));
                                        Cell.Controls[0].Controls.Add(sortArrow);
                                    }
                                    else
                                    {
                                        Cell.Controls.Add(sortArrow);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (row.RowType == DataControlRowType.Footer)
            {
                row.CssClass = "unigrid-footer";
            }
            else if (row.RowType == DataControlRowType.Pager)
            {
                row.CssClass = "UniGridPager";
            }
        }


        /// <summary>
        /// Sets UniGrid controls.
        /// </summary>
        protected void SetUnigridControls()
        {
            FilterPlaceHolder.Visible = displayFilter;

            // Indicates whether UniGrid DataSource is empty or not
            IsEmpty = DataHelper.DataSourceIsEmpty(GridView.DataSource);

            if (IsEmpty)
            {
                // Try to reload data for previous page if action was used and no data loaded (mostly delete)
                if (onActionUsed && Pager.CurrentPage > 1)
                {
                    Pager.UniPager.CurrentPage = Pager.CurrentPage - 1;
                    ReloadData();
                }
                else if (HideControlForZeroRows && String.IsNullOrEmpty(WhereClause))
                {
                    // Hide filter
                    FilterPlaceHolder.Visible = false;
                }
            }
            else
            {
                // Disable GridView paging because UniGridPager will provide paging
                GridView.AllowPaging = false;
            }
        }


        /// <summary>
        /// Sets pager control.
        /// </summary>
        protected void SetPager()
        {
            Pager.PagedControl = this;
        }

        #endregion


        #region "Postback methods"

        /// <summary>
        /// Returns true if current request was fired by page change or filter show button.
        /// </summary>
        protected bool IsInternalPostBack()
        {
            if (RequestHelper.IsPostBack())
            {
                // Get current event target
                string uniqueId = ValidationHelper.GetString(Request.Params[Page.postEventSourceID], String.Empty);

                // Get current argument
                string eventargument = ValidationHelper.GetString(Request.Params[Page.postEventArgumentID], String.Empty);

                // Check whether current request is paging
                if (!String.IsNullOrEmpty(uniqueId) && (uniqueId == GridView.UniqueID) && eventargument.StartsWith("page", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // Check whether show button is defined and it caused request
                if ((mShowButton != null) && !string.IsNullOrEmpty(Request.Params[mShowButton.UniqueID]))
                {
                    return true;
                }

                if ((mResetButton != null) && !string.IsNullOrEmpty(Request.Params[mResetButton.UniqueID]))
                {
                    return true;
                }

                // Check whether show button in basic form is defined
                if ((FilterForm.SubmitButton != null) && (!string.IsNullOrEmpty(Request.Params[FilterForm.SubmitButton.UniqueID])))
                {
                    return true;
                }

                // Check whether pager caused request
                if (ControlsHelper.CausedPostBack(Pager.UniPager))
                {
                    return true;
                }

                // Check whether current event was caused by a part of dynamic filter
                if (ControlsHelper.CausedPostBack(true, FilterPlaceHolder))
                {
                    return true;
                }
            }

            // No post-back at all
            return false;
        }


        private void HandlePostback()
        {
            // Handle post-backs
            if (RequestHelper.IsPostBack() && Request.Form[Page.postEventSourceID] == UniqueID &&
                Request.Form[Page.postEventArgumentID] == "UniGridAction" && !string.IsNullOrEmpty(CmdNameHiddenField.Value))
            {
                // Raise row action command
                HandleAction(CmdNameHiddenField.Value, CmdArgHiddenField.Value);
            }
        }

        #endregion


        #region "Selection methods"

        /// <summary>
        /// Creates a new set to track item values
        /// </summary>
        private static HashSet<string> NewItemsSet()
        {
            return new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Uncheck all checkboxes in selection column.
        /// </summary>
        /// <param name="reset">Indicates if reset selection javascript should be registered</param>
        public void ResetSelection(bool reset = true)
        {
            ClearHiddenValues(SelectionHiddenField);
            ClearHiddenValues(SelectionHashHiddenField);

            mResetSelection = reset;
        }

        #endregion


        #region "Script methods"

        /// <summary>
        /// Gets JavaScript for clearing selection of the grid.
        /// </summary>
        /// <returns>Script that clears selection of the grid</returns>
        public string GetClearSelectionScript()
        {
            return String.Format(GetJSModule() + ".clearSelection();");
        }


        /// <summary>
        /// Gets the JS module representing the UniGrid
        /// </summary>
        public string GetJSModule()
        {
            return "window.CMS.UG_" + ClientID;
        }


        /// <summary>
        /// Gets JavaScript for reload of the grid.
        /// </summary>
        /// <returns>Script that reloads the grid</returns>
        public string GetReloadScript()
        {
            return String.Format(GetJSModule() + ".reload();");
        }


        /// <summary>
        /// Gets JavaScript for checking selection of the grid.
        /// </summary>
        /// <param name="includeSemicolon">Indicates if semicolon is at the end</param>
        /// <returns>Script that checks selection of the grid</returns>
        public string GetCheckSelectionScript(bool includeSemicolon = true)
        {
            return String.Format(GetJSModule() + ".checkSelection()" + (includeSemicolon ? ";" : string.Empty));
        }


        /// <summary>
        /// Gets identifier of field containing selected items.
        /// </summary>
        /// <returns>Identifier of selection field</returns>
        public string GetSelectionFieldClientID()
        {
            return SelectionHiddenField.ClientID;
        }


        /// <summary>
        /// Register UniGrid commands scripts.
        /// </summary>
        protected void RegisterCmdScripts()
        {
            // Check if "#move" action is present
            var allowRowMove = (GridActions?.GetActionByName("#move") != null);

            // Setup the module
            object config = new
            {
                id = ClientID,
                uniqueId = UniqueID,
                hdnCmdNameId = CmdNameHiddenField.ClientID,
                hdnCmdArgId = CmdArgHiddenField.ClientID,
                hdnSelHashId = SelectionHashHiddenField.ClientID,
                hdnSelId = SelectionHiddenField.ClientID,
                gridId = GridView.ClientID,
                resetSelection = mResetSelection,
                allowSorting = allowRowMove
            };

            // This must be called in order to make the WebFormCaller module methods available
            ScriptHelper.EnsurePostbackMethods(this);

            ScriptHelper.RegisterModule(this, "CMS/UniGrid", config);

            if (allowRowMove)
            {
                ScriptHelper.RegisterJQueryUI(Page);
            }
        }

        #endregion
    }
}