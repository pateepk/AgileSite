using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// UniSelector base class.
    /// </summary>
    public abstract class UniSelector : FormEngineUserControl, IObjectTypeDriven
    {
        #region "Events"

        /// <summary>
        /// OnSelectionChanged event.
        /// </summary>
        public event EventHandler OnSelectionChanged;


        /// <summary>
        /// OnItemsSelected event (for the button mode).
        /// </summary>
        public event EventHandler OnItemsSelected;


        /// <summary>
        /// Fires when special fields are loaded.
        /// </summary>
        public event EventHandler OnSpecialFieldsLoaded;


        /// <summary>
        /// Delegate used for manipulation with items title.
        /// </summary>
        /// <param name="item">Created item</param>
        public delegate void ListItemCreated(ListItem item);


        /// <summary>
        /// Fires after data are retrieved.
        /// </summary>
        public event OnAfterRetrieveData OnAfterRetrieveData;


        /// <summary>
        /// Fires when a list item is created.
        /// </summary>
        public event ListItemCreated OnListItemCreated;


        /// <summary>
        /// Fires when selection dialog script is getting.
        /// </summary>
        public SimpleHandler<GetSelectionDialogScriptEventArgs> OnGetSelectionDialogScript = new SimpleHandler<GetSelectionDialogScriptEventArgs>();


        /// <summary>
        /// Delegate for additional data bound event.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="sourceName">Source name for grid column</param>
        /// <param name="parameter">Column (row) grid values</param>
        /// <param name="value">Value given from classic grid data bound event</param>
        public delegate object AdditionalDataBoundEventHandler(object sender, string sourceName, object parameter, object value);


        /// <summary>
        /// Event for additional manipulation with uni-selector grid.
        /// </summary>
        public event AdditionalDataBoundEventHandler OnAdditionalDataBound;

        #endregion


        #region "Constants"

        /// <summary>
        /// Represents 'none' or 'global' record selected.
        /// </summary>
        public const int US_NONE_RECORD = SpecialFieldValue.NONE;

        /// <summary>
        /// Represents 'all' record selected.
        /// </summary>
        public const int US_ALL_RECORDS = SpecialFieldValue.ALL;

        /// <summary>
        /// Represents 'more' record selected.
        /// </summary>
        public const int US_MORE_RECORDS = -2;

        /// <summary>
        /// Represents 'new' record selected.
        /// </summary>
        public const int US_NEW_RECORD = -3;

        /// <summary>
        /// Represents 'global' record selected.
        /// </summary>
        public const int US_GLOBAL_RECORD = SpecialFieldValue.GLOBAL;

        /// <summary>
        /// Represents 'global or site' record selected.
        /// </summary>
        public const int US_GLOBAL_AND_SITE_RECORD = SpecialFieldValue.GLOBAL_AND_SITE;

        /// <summary>
        /// Constant for formatting of user name
        /// </summary>
        public const string USER_DISPLAY_FORMAT = "##USERDISPLAYFORMAT##";

        /// <summary>
        /// Represents default prefix for resource strings used for the selector options.
        /// </summary>
        private const string DEFAULT_RESOURCE_PREFIX = "general";

        #endregion


        #region "Private variables"

        private static int mDefaultMaxDisplayedItems = -1;
        private static int mDefaultMaxDisplayedTotalItems = -1;
        private static int mDefaultItemsPerPage = -1;

        private bool mEnabled = true;
        private bool mDynamicFirstColumnName = true;

        private string mScriptSafeValueSeparator;

        private GeneralizedInfo mObject;
        private GeneralizedInfo mListingObject;

        private SpecialFieldsDefinition mSpecialFields;
        private List<KeyValuePair<string, string>> mPrioritizeItemsWhere;
        private readonly HashSet<string> mParameters = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        #endregion


        #region "Inner controls"

        /// <summary>
        /// Gets the single select drop down field.
        /// </summary>
        public virtual CMSDropDownList DropDownSingleSelect
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Drop down list selection edit button.
        /// </summary>
        public virtual UniButton ButtonDropDownEdit
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Gets the Select button control.
        /// </summary>
        public virtual UniButton ButtonSelect
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Gets the Clear button control.
        /// </summary>
        public virtual UniButton ButtonClear
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Gets the Remove selected items button.
        /// </summary>
        public virtual CMSMoreOptionsButton ButtonRemoveSelected
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Gets the Add items button control.
        /// </summary>
        public virtual Button ButtonAddItems
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Textbox selection edit button.
        /// </summary>
        public virtual UniButton ButtonEdit
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Multiple selection grid.
        /// </summary>
        public virtual UniGrid UniGrid
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Button selection control.
        /// </summary>
        public virtual LocalizedButton DialogButton
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// New item button.
        /// </summary>
        public virtual UniButton ButtonDropDownNew
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// TextBox control.
        /// </summary>
        public virtual CMSTextBox TextBoxSelect
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Object transformation used in SingleTransformation selection mode.
        /// </summary>
        public virtual ObjectTransformation ObjectTransformation
        {
            get
            {
                return null;
            }
        }

        #endregion


        #region "Protected and private properties"

        /// <summary>
        /// Object of the specific given type.
        /// </summary>
        protected virtual GeneralizedInfo Object
        {
            get
            {
                // Make sure that objects specified by given object type exist
                if ((mObject == null) && !String.IsNullOrEmpty(ObjectType))
                {
                    mObject = ModuleManager.GetObject(ObjectType, true);
                }

                return mObject;
            }
        }


        /// <summary>
        /// Dialog control identifier.
        /// </summary>
        protected virtual string Identifier
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// List with priority items settings - where condition is key and CSS class is value.
        /// </summary>
        private List<KeyValuePair<string, string>> PrioritizeItemsWhere
        {
            get
            {
                if (mPrioritizeItemsWhere == null)
                {
                    mPrioritizeItemsWhere = new List<KeyValuePair<string, string>>();
                    if (!String.IsNullOrEmpty(PriorityCSSClass) && !String.IsNullOrEmpty(PriorityWhereCondition))
                    {
                        mPrioritizeItemsWhere.Add(new KeyValuePair<string, string>(PriorityWhereCondition, PriorityCSSClass));
                    }
                }

                return mPrioritizeItemsWhere;
            }
        }


        /// <summary>
        /// ValueSeparator encoded for JavaScript and encapsulated into "'".
        /// </summary>
        protected string ScriptSafeValueSeparator
        {
            get
            {
                return mScriptSafeValueSeparator ?? (mScriptSafeValueSeparator = ScriptHelper.GetString(ValuesSeparator.ToString()));
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Type of alternative info object used in multiple picker grid view.
        /// </summary>
        public virtual GeneralizedInfo ListingObject
        {
            get
            {
                // Make sure that objects specified by given object type exist
                if ((mListingObject == null) && !String.IsNullOrEmpty(ListingObjectType))
                {
                    mListingObject = ModuleManager.GetObject(ListingObjectType);
                }

                return mListingObject;
            }
        }


        /// <summary>
        /// If true first column is dynamically named by object type info.
        /// </summary>
        public virtual bool DynamicColumnName
        {
            get
            {
                return mDynamicFirstColumnName;
            }
            set
            {
                mDynamicFirstColumnName = value;
            }
        }


        /// <summary>
        /// If true, uni-selector is used in site manager.
        /// </summary>
        public virtual bool IsSiteManager
        {
            get;
            set;
        }


        /// <summary>
        /// Name of alternative grid view listing object type.
        /// </summary>
        public virtual string ListingObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Where condition of alternative grid view listing.
        /// </summary>
        public virtual string ListingWhereCondition
        {
            get;
            set;
        }


        /// <summary>
        /// Zero rows text.
        /// Use only if you need special text that cannot be only retrieved from the resource strings.
        /// Otherwise use ResourcePrefix property and create "ResourcePrefix.nodatafound" resource string.
        /// This property has higher priority than the resource string.
        /// </summary>
        public virtual string ZeroRowsText
        {
            get
            {
                return ValidationHelper.GetString(ViewState["ZeroRowsText"], null);
            }
            set
            {
                ViewState["ZeroRowsText"] = value;
            }
        }


        /// <summary>
        /// Filtered zero rows text.
        /// So far works only for the selection dialog.
        /// Use only if you need special text that cannot be only retrieved from the resource strings.
        /// Otherwise use ResourcePrefix property and create "ResourcePrefix.noitemsfound" resource string.
        /// This property has higher priority than the resource string.
        /// </summary>
        public virtual string FilteredZeroRowsText
        {
            get
            {
                return ValidationHelper.GetString(ViewState["FilteredZeroRowsText"], null);
            }
            set
            {
                ViewState["FilteredZeroRowsText"] = value;
            }
        }


        /// <summary>
        /// Name of the edit window.
        /// </summary>
        public virtual string EditWindowName
        {
            get
            {
                return GetValue("EditWindowName", "EditWindow");
            }
            set
            {
                SetValue("EditWindowName", value);
            }
        }


        /// <summary>
        /// List of items of drop down control
        /// </summary>
        public virtual ListItemCollection DropDownItems
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Gets or sets the width of modal dialog window used for editing.
        /// </summary>
        public virtual int EditDialogWindowWidth
        {
            get
            {
                return GetValue("EditDialogWindowWidth", 1000);
            }
            set
            {
                SetValue("EditDialogWindowWidth", value);
            }
        }


        /// <summary>
        /// Gets or sets the height of modal dialog window used for editing.
        /// </summary>
        public virtual int EditDialogWindowHeight
        {
            get
            {
                return GetValue("EditDialogWindowHeight", 700);
            }
            set
            {
                SetValue("EditDialogWindowHeight", value);
            }
        }


        /// <summary>
        /// Value of the "(all)" DDL record, -1 by default.
        /// </summary>
        public virtual string AllRecordValue
        {
            get
            {
                return ValidationHelper.GetString(ViewState["AllRecordValue"], US_ALL_RECORDS.ToString());
            }
            set
            {
                ViewState["AllRecordValue"] = value;
            }
        }


        /// <summary>
        /// Value of the "(none)" DDL record, 0 by default.
        /// </summary>
        public virtual string NoneRecordValue
        {
            get
            {
                return ValidationHelper.GetString(ViewState["NoneRecordValue"], US_NONE_RECORD.ToString());
            }
            set
            {
                ViewState["NoneRecordValue"] = value;
            }
        }


        /// <summary>
        /// Sets a value indicating whether the New button should be enabled.
        /// </summary>
        public virtual bool? ButtonNewEnabled
        {
            get;
            set;
        }


        /// <summary>
        /// Sets a value indicating whether the Edit button should be enabled.
        /// </summary>
        public virtual bool? ButtonEditEnabled
        {
            get;
            set;
        }


        /// <summary>
        /// Sets a value indicating whether the Remove button should be enabled.
        /// </summary>
        public virtual bool? ButtonRemoveEnabled
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if UniSelector value was set.
        /// </summary>
        public new virtual bool HasValue
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["HasValue"], false);
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether the UniSelector should apply <see cref="WhereCondition"/> for the selected value (default: true). This does not affect the modal dialog.
        /// </summary>
        public virtual bool ApplyValueRestrictions
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ApplyValueRestrictions"], true);
            }
            set
            {
                ViewState["ApplyValueRestrictions"] = value;
            }
        }


        /// <summary>
        /// Gets or sets site name. If set, only objects which belong to specified site are retrieved (if the object has SiteID column). If null or empty, all objects are retrieved.
        /// Use #currentsite or #current for CurrentSite and #global for global objects or #currentandglobal for both.
        /// </summary>
        public virtual string ObjectSiteName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ObjectSiteName"), null);
            }
            set
            {
                SetValue("ObjectSiteName", value);
            }
        }


        /// <summary>
        /// Gets or sets the default value for maximum of displayed total items in the selector. Default is 50. If exceeded, only MaxDisplayedItems is displayed.
        /// </summary>
        public static int DefaultMaxDisplayedTotalItems
        {
            get
            {
                if (mDefaultMaxDisplayedTotalItems < 0)
                {
                    mDefaultMaxDisplayedTotalItems = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSSelectorMaxDisplayedTotalItems"], 50);
                    if (mDefaultMaxDisplayedTotalItems < DefaultMaxDisplayedItems)
                    {
                        mDefaultMaxDisplayedTotalItems = DefaultMaxDisplayedItems;
                    }
                }
                return mDefaultMaxDisplayedTotalItems;
            }
            set
            {
                mDefaultMaxDisplayedTotalItems = value;
            }
        }


        /// <summary>
        /// Gets or sets the default value for maximum of displayed items in the selector.
        /// </summary>
        public static int DefaultMaxDisplayedItems
        {
            get
            {
                if (mDefaultMaxDisplayedItems < 0)
                {
                    mDefaultMaxDisplayedItems = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSSelectorMaxDisplayedItems"], 25);
                }
                return mDefaultMaxDisplayedItems;
            }
            set
            {
                mDefaultMaxDisplayedItems = value;
            }
        }


        /// <summary>
        /// Gets or sets the default value for the items per page.
        /// </summary>
        public static int DefaultItemsPerPage
        {
            get
            {
                if (mDefaultItemsPerPage < 0)
                {
                    mDefaultItemsPerPage = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSSelectorItemsPerPage"], 10);
                }
                return mDefaultItemsPerPage;
            }
            set
            {
                mDefaultItemsPerPage = value;
            }
        }


        /// <summary>
        /// Gets or sets the enabled state of the control.
        /// </summary>
        public override bool Enabled
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
        /// If true, the textbox mode works with the return value and allows editing of the value.
        /// </summary>
        public virtual bool AllowEditTextBox
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("AllowEditTextBox"), false);
            }
            set
            {
                SetValue("AllowEditTextBox", value);
            }
        }


        /// <summary>
        /// Indicates selection mode of the control.
        /// This property should not be changed later than on Page Init as it causes invalid hash.
        /// If you need to change it later than on Page Init, use reload method after changing this.
        /// </summary>
        public virtual SelectionModeEnum SelectionMode
        {
            get
            {
                SelectionModeEnum result = SelectionModeEnum.SingleDropDownList;

                // Value may be stored as integer or enum string...
                object value = GetValue("SelectionMode");

                if (value == null)
                {
                    SetValue("SelectionMode", (int)result);
                }
                else if (value is int || value is SelectionModeEnum)
                {
                    result = (SelectionModeEnum)value;
                }
                else if (value is string)
                {
                    Enum.TryParse(value.ToString(), true, out result);
                }

                return result;
            }
            set
            {
                SetValue("SelectionMode", (int)value);
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
                string value = ValidationHelper.GetString(GetValue("ResourcePrefix"), DEFAULT_RESOURCE_PREFIX);
                if (!string.IsNullOrEmpty(ObjectType) && value.Equals(DEFAULT_RESOURCE_PREFIX, StringComparison.OrdinalIgnoreCase))
                {
                    var obj = ModuleManager.GetReadOnlyObject(ObjectType);
                    string objType = ObjectType;
                    if ((obj != null) && obj.TypeInfo.IsListingObjectTypeInfo)
                    {
                        objType = obj.TypeInfo.OriginalObjectType;
                    }

                    value = TranslationHelper.GetSafeClassName(objType) + ".select";
                    SetValue("ResourcePrefix", value);
                }
                return value;
            }
            set
            {
                SetValue("ResourcePrefix", value);
            }
        }


        /// <summary>
        /// Type of the selected objects.
        /// </summary>
        public virtual string ObjectType
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ObjectType"), null);
            }
            set
            {
                SetValue("ObjectType", value);
            }
        }


        /// <summary>
        /// Column name of the object which value should be returned by the selector. 
        /// If NULL, ID column is used.
        /// </summary>
        public virtual string ReturnColumnName
        {
            get
            {
                if ((SelectionMode == SelectionModeEnum.SingleTransformation) && (Object != null))
                {
                    // Transformation mode requires ID column for value
                    SetValue("ReturnColumnName", Object.TypeInfo.IDColumn);
                }

                return ValidationHelper.GetString(GetValue("ReturnColumnName"), null);
            }
            set
            {
                SetValue("ReturnColumnName", value);
            }
        }


        /// <summary>
        /// Column name where the enabled flag of the object is stored.
        /// </summary>
        public virtual string EnabledColumnName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("EnabledColumnName"), null);
            }
            set
            {
                SetValue("EnabledColumnName", value);
            }
        }


        /// <summary>
        /// Indicates if only special fields should be displayed.
        /// </summary>
        public virtual bool OnlySpecialFields
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("OnlySpecialFields"), false);
            }
            set
            {
                SetValue("OnlySpecialFields", value);
            }
        }


        /// <summary>
        /// Format of the display name.
        /// </summary>
        public virtual string DisplayNameFormat
        {
            get
            {
                return ValidationHelper.GetString(GetValue("DisplayNameFormat"), null);
            }
            set
            {
                SetValue("DisplayNameFormat", value);
            }
        }


        /// <summary>
        /// Default format of the display name. This format is used if the DisplayNameFormat (after resolving macros) is empty.
        /// </summary>
        public virtual string DefaultDisplayNameFormat
        {
            get
            {
                return ValidationHelper.GetString(GetValue("DefaultDisplayNameFormat"), null);
            }
            set
            {
                SetValue("DefaultDisplayNameFormat", value);
            }
        }


        /// <summary>
        /// If there is a multi-selection enabled, the returned values are separated by this separator. 
        /// Default is semicolon ";".
        /// </summary>
        public virtual char ValuesSeparator
        {
            get
            {
                return GetValue("ValuesSeparator", ';');
            }
            set
            {
                SetValue("ValuesSeparator", value);
                // Reset encoded version
                mScriptSafeValueSeparator = null;
            }
        }


        /// <summary>
        /// Specifies, whether the selector allows empty selection. If the dialog allows empty selection, 
        /// it displays the (none) field in the DDL variant and Clear button in the Textbox variant (default true).
        /// For multiple selection it returns empty string, otherwise it returns 0.
        /// </summary>
        public virtual bool AllowEmpty
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("AllowEmpty"), true);
            }
            set
            {
                SetValue("AllowEmpty", value);
            }
        }


        /// <summary>
        /// Specifies, whether the selector allows default item selection. If the dialog allows default selection, 
        /// it displays the (default) field in the DDL variant (default false).
        /// For multiple selection it returns empty string, otherwise it returns 0.
        /// </summary>
        public virtual bool AllowDefault
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("AllowDefault"), false);
            }
            set
            {
                SetValue("AllowDefault", value);
            }
        }


        /// <summary>
        /// Specifies, whether the selector allows selection of all items. If the dialog allows selection of all items, 
        /// it displays the (all) field in the DDL variant.
        /// When property is selected then Uniselector doesn’t load any data from DB, it just returns -1 value 
        /// and external code must handle data loading.
        /// </summary>
        public virtual bool AllowAll
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("AllowAll"), false);
            }
            set
            {
                SetValue("AllowAll", value);
            }
        }


        /// <summary>
        /// Specifies whether the selector should resolve localization macros.
        /// </summary>
        public virtual bool LocalizeItems
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("LocalizeItems"), true);
            }
            set
            {
                SetValue("LocalizeItems", value);
            }
        }


        /// <summary>
        /// Path to the filter control (CMSAbstractBaseFilterControl), which will be used for advanced filtering of 
        /// the objects in the selection dialogs.
        /// </summary>
        public virtual string FilterControl
        {
            get
            {
                return ValidationHelper.GetString(GetValue("FilterControl"), null);
            }
            set
            {
                SetValue("FilterControl", value);
            }
        }


        /// <summary>
        /// If true, name filter is used (default true), it can be disabled when some FilterControl is used.
        /// </summary>
        public virtual bool UseDefaultNameFilter
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("UseDefaultNameFilter"), true);
            }
            set
            {
                SetValue("UseDefaultNameFilter", value);
            }
        }


        /// <summary>
        /// Array with the special field values that should be added to the DropDownList selection, 
        /// between the (none) item and the other items. 
        /// </summary>
        public virtual SpecialFieldsDefinition SpecialFields
        {
            get
            {
                if (mSpecialFields == null)
                {
                    // Get fields definition
                    mSpecialFields = new SpecialFieldsDefinition(ResourcePrefix, FieldInfo, ContextResolver);
                    mSpecialFields.SetUniqueIDs = true;
                    mSpecialFields.LoadFromText(ValidationHelper.GetString(GetValue("SpecialFields"), String.Empty));
                }
                return mSpecialFields;
            }
            set
            {
                SetValue("SpecialFields", ValidationHelper.GetString(value, null));
                mSpecialFields = null;
            }
        }


        /// <summary>
        /// The number of maximum displayed total items in the dropdownlist selection (excluding the special and generic items). Default is 50. If exceeded, only MaxDisplayedItems is displayed.
        /// </summary>
        public virtual int MaxDisplayedTotalItems
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("MaxDisplayedTotalItems"), DefaultMaxDisplayedTotalItems);
            }
            set
            {
                SetValue("MaxDisplayedTotalItems", value);
            }
        }


        /// <summary>
        /// The number of maximum displayed items in the dropdownlist selection (excluding the special and generic items). Default is 25.
        /// </summary>
        public virtual int MaxDisplayedItems
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("MaxDisplayedItems"), DefaultMaxDisplayedItems);
            }
            set
            {
                SetValue("MaxDisplayedItems", value);
            }
        }


        /// <summary>
        /// Indicates whether localized filtering should be used in selection dialog.
        /// </summary>
        public bool AllowLocalizedFilteringInSelectionDialog
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("AllowLocalizedFilteringInSelectionDialog"), false);
            }
            set
            {
                SetValue("AllowLocalizedFilteringInSelectionDialog", value);
            }
        }


        /// <summary>
        ///  Base where condition for the objects selection. Applies to all Base multiple selection grid, 
        ///  dropdownlist, single and multiple selection dialogs.
        /// </summary>
        public virtual string WhereCondition
        {
            get
            {
                return ValidationHelper.GetString(GetValue("WhereCondition"), null);
            }
            set
            {
                SetValue("WhereCondition", value);
            }
        }


        /// <summary>
        /// Base order of the items. Applies to all Base multiple selection grid, 
        /// dropdownlist, single and multiple selection dialogs.
        /// </summary>
        public virtual string OrderBy
        {
            get
            {
                return ValidationHelper.GetString(GetValue("OrderBy"), null);
            }
            set
            {
                SetValue("OrderBy", value);
            }
        }


        /// <summary>
        /// Default number of items per page. Applies to all Base multiple selection grid, 
        /// dropdownlist, single and multiple selection dialogs. Default 25.
        /// </summary>
        public virtual int ItemsPerPage
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ItemsPerPage"), DefaultItemsPerPage);
            }
            set
            {
                SetValue("ItemsPerPage", value);
            }
        }


        /// <summary>
        /// Replaces items which don't have any visible text to be displayed. Default is empty string.
        /// </summary>
        public virtual string EmptyReplacement
        {
            get
            {
                return ValidationHelper.GetString(GetValue("EmptyReplacement"), String.Empty);
            }
            set
            {
                SetValue("EmptyReplacement", value);
            }
        }


        /// <summary>
        /// Grid name (path to the XML) for the multiple selection grid.
        /// </summary>
        public virtual string GridName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("GridName"), "~/CMSAdminControls/UI/UniSelector/ControlItemList.xml");
            }
            set
            {
                SetValue("GridName", value);
            }
        }


        /// <summary>
        /// Dialog grid name (path to the XML).
        /// </summary>
        public virtual string DialogGridName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("DialogGridName"), null);
            }
            set
            {
                SetValue("DialogGridName", value);
            }
        }


        /// <summary>
        /// Additional columns to select.
        /// </summary>
        public virtual string AdditionalColumns
        {
            get
            {
                return ValidationHelper.GetString(GetValue("AdditionalColumns"), null);
            }
            set
            {
                SetValue("AdditionalColumns", value);
            }
        }


        /// <summary>
        /// Additional columns to search in.
        /// </summary>
        public virtual string AdditionalSearchColumns
        {
            get
            {
                return ValidationHelper.GetString(GetValue("AdditionalSearchColumns"), null);
            }
            set
            {
                SetValue("AdditionalSearchColumns", value);
            }
        }


        /// <summary>
        /// Additional URL parameters added to dialogs URLs.
        /// Must start with '&amp;'.
        /// </summary>
        public virtual string AdditionalUrlParameters
        {
            get
            {
                return ContextResolver.ResolveMacros(ValidationHelper.GetString(GetValue("AdditionalUrlParameters"), null));
            }
            set
            {
                SetValue("AdditionalUrlParameters", value);
            }
        }


        /// <summary>
        /// Gets the value that indicates whether current selector in multiple mode displays some data or whether the dropdown contains some data.
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
        /// Callback java-script method name.
        /// </summary>
        public virtual string CallbackMethod
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CallbackMethod"), null);
            }
            set
            {
                SetValue("CallbackMethod", value);
            }
        }


        /// <summary>
        /// Path to the new item page.
        /// </summary>
        public virtual string NewItemPageUrl
        {
            get
            {
                return ValidationHelper.GetString(GetValue("NewItemPageUrl"), null);
            }
            set
            {
                SetValue("NewItemPageUrl", value);
            }
        }


        /// <summary>
        /// Path to the edit item page.
        /// </summary>
        public virtual string EditItemPageUrl
        {
            get
            {
                return ValidationHelper.GetString(GetValue("EditItemPageUrl"), null);
            }
            set
            {
                SetValue("EditItemPageUrl", value);
            }
        }


        /// <summary>
        /// URL of select item dialog.
        /// </summary>
        public virtual string SelectItemPageUrl
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SelectItemPageUrl"), null);
            }
            set
            {
                SetValue("SelectItemPageUrl", value);
            }
        }


        /// <summary>
        /// Gets or sets the resource name of the new/edit pages when defined by UI element names. Resource name determines the module which the UI elements belong to.
        /// </summary>
        public virtual string ElementResourceName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ElementResourceName"), null);
            }
            set
            {
                SetValue("ElementResourceName", value);
            }
        }


        /// <summary>
        /// Gets or sets the type of the column which will be used as a return value when the new/edit item dialog is saved.
        /// </summary>
        public virtual string ReturnColumnType
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ReturnColumnType"), null);
            }
            set
            {
                SetValue("ReturnColumnType", value);
            }
        }


        /// <summary>
        /// Gets or sets the UI element which defines the New item dialog page. Requires the ResourceName property to be defined.
        /// </summary>
        public virtual string NewItemElementName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("NewItemElementName"), null);
            }
            set
            {
                SetValue("NewItemElementName", value);
            }
        }


        /// <summary>
        /// Gets or sets the UI element which defines the Edit item dialog page. Requires the ResourceName property to be defined.
        /// </summary>
        public virtual string EditItemElementName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("EditItemElementName"), null);
            }
            set
            {
                SetValue("EditItemElementName", value);
            }
        }


        /// <summary>
        /// Dialog window name.
        /// </summary>
        public virtual string DialogWindowName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("DialogWindowName"), "SelectionDialog");
            }
            set
            {
                SetValue("DialogWindowName", value);
            }
        }


        /// <summary>
        /// Dialog window width.
        /// </summary>
        public virtual int DialogWindowWidth
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("DialogWindowWidth"), 750);
            }
            set
            {
                SetValue("DialogWindowWidth", value);
            }
        }


        /// <summary>
        /// Dialog window height.
        /// </summary>
        public virtual int DialogWindowHeight
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("DialogWindowHeight"), 650);
            }
            set
            {
                SetValue("DialogWindowHeight", value);
            }
        }


        /// <summary>
        /// Confirmation message for the items removal. To disable confirmation, set this property to an empty string.
        /// </summary>
        public virtual string RemoveConfirmation
        {
            get
            {
                return ValidationHelper.GetString(GetValue("RemoveConfirmation"), null);
            }
            set
            {
                SetValue("RemoveConfirmation", value);
            }
        }


        /// <summary>
        /// Confirmation message for the items selection.
        /// </summary>
        public virtual string SelectionConfirmation
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SelectionConfirmation"), null);
            }
            set
            {
                SetValue("SelectionConfirmation", value);
            }
        }


        /// <summary>
        /// Indicates whether to call check changes script before selecting some value
        /// </summary>
        public virtual bool CheckChanges
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("CheckChanges"), false);
            }
            set
            {
                SetValue("CheckChanges", value);
            }
        }


        /// <summary>
        /// Gets or sets identifiers of disabled items for selection dialog in multiple choice mode. Identifiers are separated by semicolon.
        /// </summary>
        public virtual string DisabledItems
        {
            get
            {
                return ValidationHelper.GetString(GetValue("DisabledItems"), null);
            }
            set
            {
                SetValue("DisabledItems", value);
            }
        }


        /// <summary>
        /// Gets or set the suffix which is added to global objects if AddGlobalObjectSuffix is true. Default is "(global)".
        /// </summary>
        public string GlobalObjectSuffix
        {
            get
            {
                string value = ValidationHelper.GetString(GetValue("GlobalObjectSuffix"), null);
                if (string.IsNullOrEmpty(value))
                {
                    value = GetString("general.global");
                    SetValue("GlobalObjectSuffix", value);
                }
                return value;
            }
            set
            {
                SetValue("GlobalObjectSuffix", value);
            }
        }


        /// <summary>
        /// Indicates whether global objects have suffix "(global)" in the grid.
        /// </summary>
        public bool AddGlobalObjectSuffix
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("AddGlobalObjectSuffix"), false);
            }
            set
            {
                SetValue("AddGlobalObjectSuffix", value);
            }
        }


        /// <summary>
        /// Indicates whether global object names should be selected with prefix '.'
        /// </summary>
        public bool AddGlobalObjectNamePrefix
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("AddGlobalObjectNamePrefix"), false);
            }
            set
            {
                SetValue("AddGlobalObjectNamePrefix", value);
            }
        }


        /// <summary>
        /// Indicates whether to remove multiple commas (can happen when DisplayNameFormat is like {%column1%}, {%column2%}, {column3} and column2 is empty.
        /// </summary>
        public virtual bool RemoveMultipleCommas
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("RemoveMultipleCommas"), false);
            }
            set
            {
                SetValue("RemoveMultipleCommas", value);
            }
        }


        /// <summary>
        /// If true, the selector uses the type condition to get the data
        /// </summary>
        public bool UseTypeCondition
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("UseTypeCondition"), true);
            }
            set
            {
                SetValue("UseTypeCondition", value);
            }
        }


        /// <summary>
        /// Client java-script code before event 'onchange'.
        /// </summary>
        public virtual string OnBeforeClientChanged
        {
            get
            {
                return ValidationHelper.GetString(GetValue("OnBeforeClientChanged"), null);
            }
            set
            {
                SetValue("OnBeforeClientChanged", value);
            }
        }


        /// <summary>
        /// Client java-script code after event 'onchange'.
        /// </summary>
        public virtual string OnAfterClientChanged
        {
            get
            {
                return ValidationHelper.GetString(GetValue("OnAfterClientChanged"), null);
            }
            set
            {
                SetValue("OnAfterClientChanged", value);
            }
        }


        /// <summary>
        /// Gets or sets sort expression of the unigrid in Multiple mode.
        /// </summary>
        protected virtual string SortExpression
        {
            get
            {
                return ValidationHelper.GetString(ViewState["SortExpression"], string.Empty);
            }
            set
            {
                ViewState["SortExpression"] = value;
            }
        }


        /// <summary>
        /// Internal purposes only, do not use.
        /// </summary>
        public virtual bool UseUniSelectorAutocomplete
        {
            get
            {
                return false;
            }
            set
            {
                SetValue("UseAutocomplete", false);
            }
        }


        /// <summary>
        /// Transformation used for output formating. Applied in SingleTransformation mode. If empty, the object display name is returned.
        /// </summary>
        public virtual string Transformation
        {
            get
            {
                return ValidationHelper.GetString(GetValue("Transformation"), null);
            }
            set
            {
                SetValue("Transformation", value);
            }
        }


        /// <summary>
        /// Transformation used in case no object was selected. Applied in SingleTransformation mode.
        /// </summary>
        public virtual string NoDataTransformation
        {
            get
            {
                return ValidationHelper.GetString(GetValue("NoDataTransformation"), null);
            }
            set
            {
                SetValue("NoDataTransformation", value);
            }
        }


        /// <summary>
        /// If true, the output is encoded. Applied in SingleTransformation mode. Default value is true.
        /// </summary>
        public virtual bool EncodeOutput
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("EncodeOutput"), true);
            }
            set
            {
                SetValue("EncodeOutput", value);
            }
        }


        /// <summary>
        /// Gets or sets explanation text if button for adding new items is disabled in Multiple mode.
        /// </summary>
        public virtual string DisabledAddButtonExplanationText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("DisabledAddButtonExplanationText"), null);
            }
            set
            {
                SetValue("DisabledAddButtonExplanationText", value);
            }
        }


        /// <summary>
        /// Where condition for items with high priority.
        /// </summary>
        public string PriorityWhereCondition
        {
            get;
            set;
        }


        /// <summary>
        /// Priority CSS class for items where <see cref="PriorityWhereCondition"/> is applied.
        /// </summary>
        public string PriorityCSSClass
        {
            get;
            set;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Reloads all controls.
        /// </summary>
        /// <param name="forceReload">Indicates if data should be loaded from DB</param>
        public virtual void Reload(bool forceReload)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Overridden method to set a parameter value.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="value">Value</param>
        public override bool SetValue(string propertyName, object value)
        {
            base.SetValue(propertyName, value);

            // Store explicitly set parameter for dialog configuration
            mParameters.Add(propertyName);

            return true;
        }

        #endregion


        #region "Protected and private methods"

        /// <summary>
        /// Returns true if the OnSelectionChanged event is bound.
        /// </summary>
        protected bool OnSelectionChangedAvailable()
        {
            return (OnSelectionChanged != null);
        }


        /// <summary>
        /// Raises the OnSelectionChanged event.
        /// </summary>
        /// /// <param name="registerChangedScript">Indicates whether JavaScript code, which marks this control as changed, should be rendered.</param>
        protected void RaiseSelectionChanged(bool registerChangedScript = true)
        {
            if (OnSelectionChanged != null)
            {
                OnSelectionChanged(this, null);
            }
            else
            {
                if (registerChangedScript)
                {
                    // Mark as changed on client side
                    RegisterChangedScript();
                }
            }
        }


        /// <summary>
        /// Raises the OnItemsSelected event.
        /// </summary>
        protected void RaiseOnItemsSelected()
        {
            if (OnItemsSelected != null)
            {
                OnItemsSelected(this, null);
            }
            else
            {
                // Mark as changed on client side
                RaiseSelectionChanged();
            }
        }


        /// <summary>
        /// Raises the OnSpecialFieldsLoad event.
        /// </summary>
        protected void RaiseOnSpecialFieldsLoaded()
        {
            OnSpecialFieldsLoaded?.Invoke(this, null);
        }


        /// <summary>
        /// Raises the OnAfterRetrieveData event.
        /// </summary>
        /// <param name="ds">Data</param>
        private DataSet RaiseAfterRetrieveData(DataSet ds)
        {
            if (OnAfterRetrieveData != null)
            {
                return OnAfterRetrieveData(ds);
            }

            return ds;
        }


        /// <summary>
        /// Raises the OnListItemCreated event.
        /// </summary>
        /// <param name="item">Created item</param>
        private void RaiseOnListItemCreated(ListItem item)
        {
            OnListItemCreated?.Invoke(item);
        }


        /// <summary>
        /// Registers script notifying page that change occurred.
        /// </summary>
        protected void RegisterChangedScript()
        {
            ScriptHelper.RegisterJQuery(Page);
            ScriptHelper.RegisterStartupScript(Page, typeof(string), "US_Changed", @"$cmsj(document).ready(function() {
  if (window.US_Changed != null) {
    US_Changed();
  }
});", true);
        }


        /// <summary>
        /// Returns data set depending on specified properties.
        /// </summary>
        /// <param name="id">ID(s) of the items to load. If null, all items are loaded</param>
        /// <param name="topN">Only take topN items</param>
        protected DataSet GetResultSet(string id, int topN)
        {
            int totalRecords = 0;

            return GetResultSet(id, topN, 0, 0, ref totalRecords);
        }


        /// <summary>
        /// Returns data set depending on specified properties.
        /// </summary>
        /// <param name="ids">ID(s) of the items to load. If null, all items are loaded</param>
        /// <param name="topN">Only take topN items</param>
        /// <param name="offset">Offset of the items</param>
        /// <param name="maxRecords">Maximum number of the records to get</param>
        /// <param name="totalRecords">Returns total number of records</param>
        protected DataSet GetResultSet(string ids, int topN, int offset, int maxRecords, ref int totalRecords)
        {
            // Only special fields should be displayed
            if (OnlySpecialFields || (Object == null))
            {
                return null;
            }

            DataSet ds = null;

            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();

            using (var condition = new SelectCondition(parameters))
            {
                // Initialize where condition
                string where;
                if (!GetSelectionWhereCondition(condition, ids, out where))
                {
                    return null;
                }

                // Initialize columns
                string columns = GetSelectionColumns();

                // Initialize OrderBy
                string orderBy = GetSelectionOrderBy();

                GeneralizedInfo listingObj = ListingObject ?? Object;

                // Disable license error when selector tries to load objects that are not included in current license
                using (new CMSActionContext { EmptyDataForInvalidLicense = true })
                {
                    try
                    {
                        // Get the data query
                        var q = listingObj.GetDataQuery(
                            UseTypeCondition,
                            s => s
                                .Where(where)
                                .OrderBy(orderBy)
                                .TopN(topN)
                                .Columns(columns),
                            true
                            );

                        q.IncludeBinaryData = false;
                        q.Offset = offset;
                        q.MaxRecords = maxRecords;
                        q.Parameters = parameters;

                        // Get the data and number of total records
                        ds = q.Result;
                        totalRecords = q.TotalRecords;

                        ds = RaiseAfterRetrieveData(ds);
                    }
                    catch (Exception ex)
                    {
                        CoreServices.EventLog.LogException("UniSelector", "GetResultSet", ex);
                    }
                }
            }

            return ds;
        }


        /// <summary>
        /// Returns FALSE if the selection is empty.
        /// </summary>
        private bool GetSelectionWhereCondition(SelectCondition condition, string itemIds, out string where)
        {
            where = null;

            if (Object.TypeInfo.IsVirtualObject)
            {
                condition.InlineLimit = SelectCondition.ALL_INLINE;
            }

            switch (SelectionMode)
            {
                case SelectionModeEnum.Multiple:
                case SelectionModeEnum.MultipleButton:
                case SelectionModeEnum.MultipleTextBox:
                    if (itemIds != null)
                    {
                        // Get where condition for selected items
                        string[] items = itemIds.Split(new[] { ValuesSeparator }, StringSplitOptions.RemoveEmptyEntries);

                        bool isString = !String.Equals(Object.TypeInfo.IDColumn, ReturnColumnName, StringComparison.InvariantCultureIgnoreCase);
                        if (isString)
                        {
                            // Names
                            condition.PrepareCondition(ReturnColumnName, items);
                        }
                        else
                        {
                            var intList = items.Select(s =>
                            {
                                int value;
                                Int32.TryParse(s, out value);
                                return value;
                            }).ToList();
                            // IDs
                            condition.PrepareCondition(ReturnColumnName, intList);
                        }

                        // Do not return anything if the selection is empty
                        if (condition.IsEmpty)
                        {
                            return false;
                        }

                        where = condition.WhereCondition;
                    }
                    break;

                default:
                    // Build where condition
                    if (!String.IsNullOrEmpty(itemIds))
                    {
                        where = String.Format("{0} = '{1}'", ReturnColumnName, SqlHelper.GetSafeQueryString(itemIds, false));
                    }
                    break;
            }

            // Apply value restrictions
            if (ApplyValueRestrictions)
            {
                where = SqlHelper.AddWhereCondition(where, !string.IsNullOrEmpty(ListingWhereCondition) ? ListingWhereCondition : WhereCondition);
            }

            // Apply site restrictions
            string siteWhere = GetSiteWhereCondition();
            if (!String.IsNullOrEmpty(siteWhere))
            {
                where = SqlHelper.AddWhereCondition(where, siteWhere);
            }

            // Resolve macros in where condition
            where = ContextResolver.ResolveMacros(where);

            return true;
        }


        private string GetSelectionColumns()
        {
            string columns;

            if (DisplayNameFormat == USER_DISPLAY_FORMAT)
            {
                // Ensure columns which are needed for USER_DISPLAY_FORMAT
                columns = "UserName,FullName";
            }
            else if (DisplayNameFormat != null)
            {
                columns = DataHelper.GetNotEmpty(MacroProcessor.GetMacros(DisplayNameFormat, true), Object.DisplayNameColumn).Replace(";", ", ");
            }
            else
            {
                columns = Object.DisplayNameColumn;
            }

            // Add the default format name column to the query
            if (DefaultDisplayNameFormat != null)
            {
                string defaultColumn = DataHelper.GetNotEmpty(MacroProcessor.GetMacros(DefaultDisplayNameFormat, true), Object.DisplayNameColumn).Replace(";", ", ");
                columns = SqlHelper.MergeColumns(columns, defaultColumn);
            }

            // Add return column name
            columns = SqlHelper.MergeColumns(columns, ReturnColumnName);

            // Add additional columns
            columns = SqlHelper.MergeColumns(columns, AdditionalColumns);

            // Add enabled column name
            columns = SqlHelper.MergeColumns(columns, EnabledColumnName);

            // Add priority column name
            if (PrioritizeItemsWhere.Count > 0)
            {
                columns = SqlHelper.MergeColumns(columns, SqlHelper.GetCaseColumn(PrioritizeItemsWhere, "CSS"));
            }

            // Ensure SiteID column (for global object prefixes/suffixes)
            if ((AddGlobalObjectNamePrefix || AddGlobalObjectSuffix) && (Object.TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                columns = SqlHelper.MergeColumns(columns, Object.TypeInfo.SiteIDColumn);
            }

            return columns;
        }


        private string GetSelectionOrderBy()
        {
            string orderBy = SortExpression;

            // Priority column order
            if (PrioritizeItemsWhere.Count > 0)
            {
                // Select only WhereConditions
                var cases = PrioritizeItemsWhere.Select(t => t.Key);

                orderBy += SqlHelper.GetCaseOrderBy(cases);
            }

            orderBy = SqlHelper.AddOrderBy(orderBy, String.IsNullOrEmpty(OrderBy) ? Object.DisplayNameColumn : OrderBy);

            return orderBy;
        }


        /// <summary>
        /// Returns site WHERE condition according to ObjectSiteName property.
        /// </summary>
        protected string GetSiteWhereCondition()
        {
            string siteWhere = null;

            if (!string.IsNullOrEmpty(ObjectSiteName))
            {
                int siteId = 0;
                bool global = false;

                switch (ObjectSiteName.ToLowerInvariant())
                {
                    case "#global":
                        global = true;
                        break;

                    case "#currentsite":
                    case "#current":
                        siteId = SiteContext.CurrentSiteID;
                        break;

                    case "#currentandglobal":
                        siteId = SiteContext.CurrentSiteID;
                        global = true;
                        break;

                    default:
                        siteId = SiteInfoProvider.GetSiteID(ObjectSiteName);
                        break;
                }

                siteWhere = Object.TypeInfo.GetSiteWhereCondition(siteId, global).ToString(true);
            }

            return siteWhere;
        }


        /// <summary>
        /// Checks if any method handles the event. If so, raise the event.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="sourceName">Source name for grid column</param>
        /// <param name="parameter">Column (row) grid values</param>
        /// <param name="value">Value given from classic grid data bound event</param>
        /// <returns>Result of the event handler, if set; null otherwise</returns>
        protected object AdditionalDataBound(object sender, string sourceName, object parameter, object value)
        {
            return OnAdditionalDataBound?.Invoke(sender, sourceName, parameter, value);
        }


        /// <summary>
        /// Ensures that the column names are properly set up.
        /// </summary>
        protected void EnsureColumnNames()
        {
            var ti = Object.TypeInfo;

            // Set return column name
            if (String.IsNullOrEmpty(ReturnColumnName))
            {
                ReturnColumnName = ti.IDColumn;
            }

            GeneralizedInfo obj = ListingObject ?? Object;

            // Set enabled column name if not set
            if (String.IsNullOrEmpty(EnabledColumnName) && (obj.TypeInfo.EnabledColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                EnabledColumnName = ti.EnabledColumn;
            }
        }


        /// <summary>
        /// Gets the new item URL
        /// </summary>
        /// <param name="clientId">Selector's client ID</param>
        protected string GetNewUrl(string clientId)
        {
            string newUrl;

            if (!String.IsNullOrEmpty(NewItemPageUrl))
            {
                // New item page defined by URL
                newUrl = GetItemPageUrl(NewItemPageUrl, clientId);
            }
            else
            {
                // New item page defined by UIelement
                newUrl = GetUIElementPageUrl(NewItemElementName, AdditionalUrlParameters, clientId);
            }
            return newUrl;
        }


        /// <summary>
        /// Gets the edit item URL
        /// </summary>
        /// <param name="clientId">Selector's client ID</param>
        protected string GetEditUrl(string clientId)
        {
            string editUrl;

            if (!String.IsNullOrEmpty(EditItemPageUrl))
            {
                // Edit item page defined by URL
                editUrl = GetItemPageUrl(EditItemPageUrl, clientId);
            }
            else
            {
                // Edit item page defined by UIelement
                editUrl = GetUIElementPageUrl(EditItemElementName, AdditionalUrlParameters, clientId, true);
            }
            return editUrl;
        }


        /// <summary>
        /// Returns URL for the item page based on base item page URL. Selector's ID, hash and additional URL parameters are added.
        /// </summary>
        /// <param name="baseItemUrl">Base item page URL</param>
        /// <param name="clientId">Selector's client ID</param>
        private string GetItemPageUrl(string baseItemUrl, string clientId)
        {
            // Item page defined by URL
            string resultUrl = URLHelper.AddParameterToUrl(baseItemUrl, "selectorId", clientId) + AdditionalUrlParameters;

            // Ensure dialog hash if not set externally
            bool hashExists;
            URLHelper.GetQueryValue(resultUrl, "hash", out hashExists);
            if (!hashExists)
            {
                resultUrl = ApplicationUrlHelper.AppendDialogHash(resultUrl);
            }

            return resultUrl;
        }


        /// <summary>
        /// Gets the URL for the New/Edit UI element. URL contains return handler function which is called after the "save" action of the New/Edit dialog.
        /// </summary>
        /// <param name="elementName">Name of the UI element</param>
        /// <param name="additionalQuery">Additional query to append</param>
        /// <param name="clientId">Selector's client ID</param>
        /// <param name="addObjectId">Indicates if "objectid=##ITEMID##" should be added to the result query; it is required for edit item URLs</param>
        private string GetUIElementPageUrl(string elementName, string additionalQuery, string clientId, bool addObjectId = false)
        {
            string query = "returnhandler=US_SelectNewValue_" + clientId + "&returntype=" + ReturnColumnType + additionalQuery;

            query = ApplicationUrlHelper.GetElementDialogUrl(ElementResourceName, elementName, 0, query);
            if (addObjectId)
            {
                query = URLHelper.AddParameterToUrl(query, "objectid", "##ITEMID##");
            }

            return query;
        }


        /// <summary>
        /// Gets the item name for the given item ID (value)
        /// </summary>
        /// <param name="value">Item value</param>
        protected string GetDisplayName(object value)
        {
            string ids = ValidationHelper.GetString(value, null);
            if (!String.IsNullOrEmpty(ids))
            {
                // Load data
                DataSet ds = GetResultSet(ids, 0);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    // Build the list of names
                    List<string> names = new List<string>();
                    HashSet<string> usedValues = new HashSet<string>();

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        string val = ValidationHelper.GetString(dr[ReturnColumnName], String.Empty);
                        if (usedValues.Add(val))
                        {
                            // Add the display name
                            string name = GetItemName(dr);
                            if (!String.IsNullOrEmpty(name))
                            {
                                names.Add(name);
                            }
                        }
                    }

                    // Each name on single line
                    return names.Join("\n");
                }
            }

            return String.Empty;
        }


        /// <summary>
        /// Returns item display name based on DisplayNameFormat.
        /// </summary>
        /// <param name="dr">Source data row</param>    
        protected string GetItemName(DataRow dr)
        {
            string itemName;

            // Special formatted user name
            var obj = Object;

            if (DisplayNameFormat == USER_DISPLAY_FORMAT)
            {
                string userName = DataHelper.GetStringValue(dr, "UserName");
                string fullName = DataHelper.GetStringValue(dr, "FullName");

                itemName = UserInfoProvider.GetFormattedUserName(userName, fullName, null, IsLiveSite);
            }
            else if (DisplayNameFormat == null)
            {
                itemName = ResHelper.LocalizeString(DataHelper.GetStringValue(dr, obj.DisplayNameColumn));
            }
            else
            {
                itemName = MergeText(DisplayNameFormat, dr);
            }

            // Use the DefaultDisplayNameFormat if the resolved DisplayNameFormat is empty
            if (String.IsNullOrEmpty(itemName) && (!String.IsNullOrEmpty(DefaultDisplayNameFormat)))
            {
                itemName = MergeText(DefaultDisplayNameFormat, dr);
            }

            // Add items to unigrid
            if (String.IsNullOrEmpty(itemName))
            {
                itemName = EmptyReplacement;
            }

            if (RemoveMultipleCommas)
            {
                itemName = TextHelper.RemoveMultipleCommas(itemName);
            }

            if (AddGlobalObjectSuffix)
            {
                var ti = obj.TypeInfo;

                if (!String.Equals(ti.SiteIDColumn, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, StringComparison.OrdinalIgnoreCase))
                {
                    itemName += (DataHelper.GetIntValue(dr, ti.SiteIDColumn) > 0 ? String.Empty : " " + GlobalObjectSuffix);
                }
            }

            return itemName;
        }


        /// <summary>
        /// Creates a new list item based on the given DataRow.
        /// </summary>
        protected ListItem NewListItem(DataRow dr)
        {
            string itemname = GetItemName(dr);
            if (LocalizeItems)
            {
                itemname = ResHelper.LocalizeString(itemname);
            }

            // Create new item
            ListItem newItem = NewListItem(itemname, dr[ReturnColumnName].ToString());
            if (newItem != null)
            {
                // Set disabled if disabled
                if (EnabledColumnName != null)
                {
                    bool isEnabled = ValidationHelper.GetBoolean(dr[EnabledColumnName], false);
                    string itemClass = isEnabled ? "DropDownItemEnabled" : "DropDownItemDisabled";

                    newItem.Attributes.Add("class", itemClass);
                }

                if (PrioritizeItemsWhere.Count > 0)
                {
                    if (dr["CSS"] != null)
                    {
                        newItem.Attributes["class"] += dr["CSS"];
                    }
                }
            }

            return newItem;
        }


        /// <summary>
        /// Creates a new list item based on the given text and value.
        /// </summary>
        protected ListItem NewListItem(string text, string value)
        {
            if (String.IsNullOrEmpty(text))
            {
                return null;
            }

            // Create new item
            ListItem newItem = new ListItem(text, value);
            newItem.Attributes.Add(SpecialFieldsDefinition.DATA_HASH_ATTRIBUTE, ValidationHelper.GetHashString(newItem.Value));

            RaiseOnListItemCreated(newItem);

            return newItem;
        }


        /// <summary>
        /// Merges the given text with the given data resolved by macros.
        /// </summary>
        /// <param name="originalText">Template text</param>
        /// <param name="data">DataRow with the data</param>
        private static string MergeText(string originalText, DataRow data)
        {
            if ((originalText != null) && (data != null))
            {
                MacroResolver resolver = MacroResolver.GetInstance();
                resolver.Culture = Thread.CurrentThread.CurrentUICulture.ToString();
                foreach (DataColumn item in data.Table.Columns)
                {
                    resolver.SetNamedSourceData(item.ColumnName, data[item.ColumnName]);
                }
                originalText = resolver.ResolveMacros(originalText);
            }

            return originalText;
        }


        /// <summary>
        /// Gets the display name of the selected value
        /// </summary>
        protected string GetValueDisplayName()
        {
            switch (SelectionMode)
            {
                case SelectionModeEnum.SingleDropDownList:
                    {
                        DropDownList drpList = DropDownSingleSelect;
                        if (drpList != null)
                        {
                            // For dropdown, return the selected text
                            if (drpList.SelectedIndex >= 0)
                            {
                                return drpList.Items[drpList.SelectedIndex].Text;
                            }

                            return drpList.Text;
                        }

                        return null;
                    }

                default:
                    // Other types of selection - Get through the data items
                    return GetDisplayName(Value);
            }
        }


        /// <summary>
        /// Sets the dialog parameters to the context.
        /// </summary>
        protected void SetDialogParameters()
        {
            var p = new Hashtable();

            // Add explicitly set properties
            foreach (var parameter in mParameters)
            {
                p[parameter] = GetValue(parameter);
            }

            p["SelectionMode"] = SelectionMode;
            p["ResourcePrefix"] = ResourcePrefix;
            p["ObjectType"] = ObjectType;
            p["ReturnColumnName"] = ReturnColumnName;
            p["AllowEmpty"] = AllowEmpty;
            p["AllowDefault"] = AllowDefault;
            p["AllowAll"] = AllowAll;
            p["NoneRecordValue"] = NoneRecordValue;
            p["AllRecordValue"] = AllRecordValue;
            p["FilterControl"] = FilterControl;
            p["UseDefaultNameFilter"] = UseDefaultNameFilter;
            p["WhereCondition"] = WhereCondition;
            p["OrderBy"] = OrderBy;
            p["ItemsPerPage"] = ItemsPerPage;
            p["EmptyReplacement"] = EmptyReplacement;
            p["Values"] = String.Format("{0}{1}{0}", ValuesSeparator, Value);
            p["DisplayNameFormat"] = DisplayNameFormat;
            p["DialogGridName"] = DialogGridName;
            p["AdditionalColumns"] = AdditionalColumns;
            p["CallbackMethod"] = CallbackMethod;
            p["AllowEditTextBox"] = AllowEditTextBox;
            p["FireOnChanged"] = OnSelectionChangedAvailable();
            p["DisabledItems"] = DisabledItems;
            p["AddGlobalObjectSuffix"] = AddGlobalObjectSuffix;
            p["AddGlobalObjectNamePrefix"] = AddGlobalObjectNamePrefix;
            p["GlobalObjectSuffix"] = GlobalObjectSuffix;
            p["RemoveMultipleCommas"] = RemoveMultipleCommas;
            p["IsSiteManager"] = IsSiteManager;
            p["FilterMode"] = GetValue("FilterMode");
            p["AdditionalSearchColumns"] = AdditionalSearchColumns;
            p["SiteWhereCondition"] = GetSiteWhereCondition();
            p["UseTypeCondition"] = UseTypeCondition;
            p["AllowLocalizedFiltering"] = AllowLocalizedFilteringInSelectionDialog;
            p["ZeroRowsText"] = ZeroRowsText;
            p["FilteredZeroRowsText"] = FilteredZeroRowsText;
            p["HasDependingFields"] = HasDependingFields;

            WindowHelper.Add(Identifier, p);
        }


        /// <summary>
        /// Loads special fields to drop down.
        /// </summary>
        protected void LoadSpecialFields()
        {
            if (DropDownSingleSelect == null)
            {
                return;
            }

            // Raise event
            RaiseOnSpecialFieldsLoaded();

            // Display '(default)' item
            if (AllowDefault)
            {
                SpecialFields.Insert(0, new SpecialField
                {
                    Value = NoneRecordValue,
                    Text = GetString("general.defaultchoice")
                });
            }

            // Display '(none)' item
            if (AllowEmpty)
            {
                SpecialFields.Insert(0, new SpecialField
                {
                    Value = NoneRecordValue,
                    Text = GetString("general.empty")
                });
            }

            // Display '(all)' item
            if (AllowAll)
            {
                SpecialFields.Insert(0, new SpecialField
                {
                    Value = AllRecordValue,
                    Text = GetString("general.selectall")
                });
            }

            try
            {
                SpecialFields.FillItems(DropDownSingleSelect.Items);
            }
            catch (FormatException)
            {
                ShowError(GetString("TemplateDesigner.ErrorDropDownListOptionsInvalidSpecialMacroFormat"));
            }
        }

        #endregion
    }
}