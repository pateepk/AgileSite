using System;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;

using CMS.DataEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Abstract class providing common functionality for ecommerce objects selectors.
    /// </summary>
    public abstract class BaseObjectSelector : FormEngineUserControl
    {
        #region "Variables"

        private BaseInfo mReadOnlyObject;

        private bool mAddAllItemsRecord = true;
        private bool mDisplayOnlyEnabled = true;
        private bool mUseNameForSelection = true;
        private bool mAddNoneRecord;
        private bool mEnsureSelectedItem;

        private string mAdditionalItems = "";
        private bool mAllowCreate;
        private int mSiteId = -1;

        #endregion


        #region "Object properties"

        /// <summary>
        /// Object type name of selected objects.
        /// </summary>
        protected string ObjectType
        {
            get
            {
                return UniSelector.ObjectType;
            }
        }


        /// <summary>
        /// Empty info object of the same type as selected objects.
        /// </summary>
        protected BaseInfo EmptyObject
        {
            get
            {
                return mReadOnlyObject ?? (mReadOnlyObject = ModuleManager.GetReadOnlyObject(ObjectType));
            }
        }


        /// <summary>
        /// Name of the column containing code name.
        /// </summary>
        protected string CodeNameColumn
        {
            get
            {
                return EmptyObject.Generalized.CodeNameColumn;
            }
        }


        /// <summary>
        /// Name of the column containing ID.
        /// </summary>
        protected string IDColumn
        {
            get
            {
                return EmptyObject.TypeInfo.IDColumn;
            }
        }


        /// <summary>
        /// Name of the column containing ID of the site.
        /// </summary>
        protected string SiteIDColumn
        {
            get
            {
                return EmptyObject.TypeInfo.SiteIDColumn;
            }
        }


        /// <summary>
        /// Name of the column containing enabled flag.
        /// </summary>
        protected string EnabledColumn
        {
            get
            {
                return EmptyObject.TypeInfo.EnabledColumn;
            }
        }


        /// <summary>
        /// Indicates if objects can be combined site and global within one site. 
        /// False means that site can use only global or only site records.
        /// </summary>
        protected bool AllowCombineSiteAndGlobal
        {
            get
            {
                return EmptyObject.TypeInfo.NameGloballyUnique;
            }
        }

        #endregion


        #region "General form control properties"

        /// <summary>
        /// Getter to be overridden. Gets uniselector object used for selecting objects.
        /// </summary>
        public abstract UniSelector UniSelector
        {
            get;
        }


        /// <summary>
        /// If true, control does not process the data.
        /// </summary>
        public override bool StopProcessing
        {
            get
            {
                return base.StopProcessing;
            }
            set
            {
                base.StopProcessing = value;
                UniSelector.StopProcessing = value;
            }
        }


        /// <summary>
        /// Gets or sets the enabled state of the selector.
        /// </summary>
        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;
                UniSelector.Enabled = value;
            }
        }


        /// <summary>
        /// Indicates if selector has data.
        /// </summary>
        public virtual bool HasData
        {
            get
            {
                return UniSelector.HasData;
            }
        }


        /// <summary>
        /// Returns ClientID of the dropdown list.
        /// </summary>
        public override string ValueElementID
        {
            get
            {
                return UniSelector.DropDownSingleSelect.ClientID;
            }
        }

        #endregion


        #region "Filtering and special items handling properties"

        /// <summary>
        /// Allows to display only enabled items. Default value is true.
        /// </summary>
        public virtual bool DisplayOnlyEnabled
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("DisplayOnlyEnabled"), mDisplayOnlyEnabled);
            }
            set
            {
                mDisplayOnlyEnabled = value;
                SetValue("DisplayOnlyEnabled", value);
            }
        }


        /// <summary>
        /// Gets or sets the value which determines, whether to add 'all' item record to the selector.
        /// </summary>
        public virtual bool AddAllItemsRecord
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("AddAllItemsRecord"), mAddAllItemsRecord);
            }
            set
            {
                mAddAllItemsRecord = value;
                SetValue("AddAllItemsRecord", value);
            }
        }


        /// <summary>
        /// Determines whether to add 'none' item record to the selector.
        /// </summary>
        public virtual bool AddNoneRecord
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("AddNoneRecord"), mAddNoneRecord);
            }
            set
            {
                mAddNoneRecord = value;
                SetValue("AddNoneRecord", value);
            }
        }


        /// <summary>
        /// Indicates whether selected item must be on the list despite of other settings. Default value is false.
        /// </summary>
        public virtual bool EnsureSelectedItem
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("EnsureSelectedItem"), mEnsureSelectedItem);
            }
            set
            {
                mEnsureSelectedItem = value;
                SetValue("EnsureSelectedItem", value);
            }
        }


        /// <summary>
        /// ID of items which have to be displayed. Use ',' or ';' as separator if more IDs required.
        /// </summary>
        public virtual string AdditionalItems
        {
            get
            {
                return mAdditionalItems;
            }
            set
            {
                // Prevent from setting null value
                mAdditionalItems = value?.Replace(';', ',') ?? "";
            }
        }


        /// <summary>
        /// Indicates whether new button will be visible and creating of new object allowed.
        /// </summary>
        public virtual bool AllowCreate
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("AllowCreate"), mAllowCreate);
            }
            set
            {
                mAllowCreate = value;
                SetValue("AllowCreate", value);
            }
        }


        /// <summary>
        /// Allows to display records only for specified site ID.
        /// </summary>
        public virtual int SiteID
        {
            get
            {
                if (mSiteId < 0)
                {
                    mSiteId = SiteContext.CurrentSiteID;
                }

                return mSiteId;
            }
            set
            {
                mSiteId = value;
            }
        }

        #endregion


        #region "Selected value handling"

        /// <summary>
        /// Determines whether the AutoPostBack property of dropdown list should be enabled.
        /// </summary>
        public virtual bool AutoPostBack
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether code name is to be used for selecting items.
        /// </summary>
        public virtual bool UseNameForSelection
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("UseNameForSelection"), mUseNameForSelection);
            }
            set
            {
                SetValue("UseNameForSelection", value);
                mUseNameForSelection = value;
            }
        }


        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        public override object Value
        {
            get
            {
                if (UseNameForSelection)
                {
                    return SelectedName;
                }

                return (SelectedID == 0) ? null : (object)SelectedID;
            }
            set
            {
                if (UseNameForSelection)
                {
                    SelectedName = ValidationHelper.GetString(value, "");
                }
                else
                {
                    SelectedID = ValidationHelper.GetInteger(value, 0);
                }
            }
        }


        /// <summary>
        /// Gets or sets the ID of selected item.
        /// </summary>
        public virtual int SelectedID
        {
            get
            {
                if (UseNameForSelection)
                {
                    // Convert name to ID
                    return GetObjectID(ValidationHelper.GetString(UniSelector.Value, ""), SiteInfoProvider.GetSiteName(SiteID));
                }

                return ValidationHelper.GetInteger(UniSelector.Value, 0);
            }
            set
            {
                if (UseNameForSelection)
                {
                    // Convert ID to codename
                    UniSelector.Value = GetCodeName(value);
                }
                else
                {
                    UniSelector.Value = value;
                }
            }
        }


        /// <summary>
        /// Gets or sets the codename of selected item.
        /// </summary>
        public virtual string SelectedName
        {
            get
            {
                if (UseNameForSelection)
                {
                    return ValidationHelper.GetString(UniSelector.Value, "");
                }

                // Convert ID to codename
                return GetCodeName(ValidationHelper.GetInteger(UniSelector.Value, 0));
            }
            set
            {
                if (UseNameForSelection)
                {
                    UniSelector.Value = value;
                }
                else
                {
                    // Convert codename to ID
                    UniSelector.Value = GetObjectID(value, SiteInfoProvider.GetSiteName(SiteID));
                }
            }
        }


        /// <summary>
        /// Gets the display name of the value item. Returns null if display name is not available.
        /// </summary>
        public override string ValueDisplayName
        {
            get
            {
                return UniSelector.ValueDisplayName;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes of the selector.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            UniSelector.OnSelectionChanged += (sender, args) => RaiseOnChanged();
            UniSelector.OnListItemCreated += OnListItemCreated;
            UniSelector.OnAfterRetrieveData += OnAfterRetrieveData;

            if (StopProcessing)
            {
                UniSelector.StopProcessing = true;
            }
            else
            {
                InitSelector();
            }
        }


        /// <summary>
        /// Load data for selector.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UniSelector.WhereCondition = GenerateWhereCondition();
        }


        /// <summary>
        /// Called after selector data are retrieved.
        /// </summary>
        /// <param name="ds">Original selector data.</param>
        protected virtual DataSet OnAfterRetrieveData(DataSet ds)
        {
            return ds;
        }


        /// <summary>
        /// Called when item in selector is created.
        /// </summary>
        /// <param name="item">Created list item.</param>
        protected virtual void OnListItemCreated(ListItem item)
        {
        }


        /// <summary>
        /// Reloads selector.
        /// </summary>
        /// <param name="forceReload">Indicates if data should be loaded from DB.</param>
        public virtual void Reload(bool forceReload = true)
        {
            InitSelector();
            UniSelector.WhereCondition = GenerateWhereCondition();

            // Reload selector
            UniSelector.Reload(forceReload);
        }


        /// <summary>
        /// Initializes the selector.
        /// </summary>
        protected virtual void InitSelector()
        {
            UniSelector.IsLiveSite = IsLiveSite;
            UniSelector.AllowEmpty = AddNoneRecord;
            UniSelector.AllowAll = AddAllItemsRecord;
            UniSelector.ReturnColumnName = UseNameForSelection ? CodeNameColumn : IDColumn;
            UniSelector.DropDownSingleSelect.AutoPostBack = AutoPostBack;

            if (AllowCreate)
            {
                if (SiteID > 0)
                {
                    UniSelector.NewItemPageUrl = GetNewItemUrl();
                }
            }
        }


        /// <summary>
        /// Returns url of quick add dialog.
        /// </summary>
        protected string GetNewItemUrl()
        {
            string parameters = "?objectType=" + ObjectType + "&site=" + SiteID;
            string url = "~/CMSModules/Ecommerce/FormControls/QuickAdd.aspx" + parameters;
            url = URLHelper.AddParameterToUrl(url, "hash", QueryHelper.GetHash(parameters));

            return url;
        }

        #endregion


        #region "Code name/ID conversion methods"

        /// <summary>
        /// Converts ID of the object to its codename.
        /// </summary>
        /// <param name="id">ID of the object, which code name is to be returned</param>
        protected virtual string GetCodeName(int id)
        {
            BaseInfo bi = ProviderHelper.GetInfoById(ObjectType, id);

            return bi?.Generalized.ObjectCodeName;
        }


        /// <summary>
        /// Converts object name to ID on the specific site
        /// </summary>
        /// <param name="name">Name of the object to find ID for.</param>
        /// <param name="siteName">Name of the site.</param>
        protected virtual int GetID(string name, string siteName)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Converts object name to ID on the specific site. If object is not found on specific site, global object are searched. 
        /// </summary>
        /// <param name="name">Name of the object to find ID for.</param>
        /// <param name="siteName"></param>
        private int GetObjectID(string name, string siteName)
        {
            int objectID = GetID(name, siteName);

            if ((objectID == 0) && AllowCombineSiteAndGlobal)
            {
                objectID = GetID(name, null);
            }

            return objectID;
        }

        #endregion


        #region "Where condition generation"

        /// <summary>
        /// Generates where condition for uniselector.
        /// </summary>
        protected virtual string GenerateWhereCondition()
        {
            string where = AppendExclusiveWhere(string.Empty);

            where = AppendInclusiveWhere(where);

            return SqlHelper.AddWhereCondition(where, GenerateBaseWhere());
        }


        /// <summary>
        /// Returns base where condition. This condition is applied always. 
        /// Returns empty where condition by default - override this method to generate custom condition.
        /// </summary>
        protected virtual string GenerateBaseWhere()
        {
            return string.Empty;
        }


        /// <summary>
        /// Appends restrictive part of where condition to given where condition.
        /// </summary>
        protected virtual string AppendExclusiveWhere(string where)
        {
            // Restrict to enabled records only
            where = AppendEnabledWhere(where);

            return where;
        }


        /// <summary>
        /// Appends widening part of where condition to given where condition.
        /// </summary>
        protected virtual string AppendInclusiveWhere(string where)
        {
            // Add additional items
            where = AppendAdditionalItemsWhere(where);

            // Add selected value
            where = AppendSelectedItemWhere(where);

            return where;
        }


        /// <summary>
        /// Appends where restricting to enabled items.
        /// </summary>
        /// <param name="where">Original where condition to append enabled where to.</param>
        protected virtual string AppendEnabledWhere(string where)
        {
            if (DisplayOnlyEnabled && (EnabledColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                return new WhereCondition(where).WhereTrue(EnabledColumn).ToString(true);
            }

            return where;
        }


        /// <summary>
        /// Appends where adding additional items.
        /// </summary>
        /// <param name="where">Original where condition to append additional where to.</param>
        protected virtual string AppendAdditionalItemsWhere(string where)
        {
            // Add items which have to be on the list
            var whereCondition = new WhereCondition();
            var additionalIds = AdditionalItems.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).OfType<object>().ToArray();
            var additionalItems = ValidationHelper.GetIntegers(additionalIds, 0);

            if ((!string.IsNullOrEmpty(where)) && (additionalItems.Length > 0))
            {
                return whereCondition.Where(where).Or().WhereIn(IDColumn, additionalItems).ToString(true);
            }

            return where;
        }


        /// <summary>
        /// Appends where condition including selected item.
        /// </summary>
        /// <param name="where">Original where condition to append where to.</param>
        protected virtual string AppendSelectedItemWhere(string where)
        {
            // Selected value must be on the list
            if (EnsureSelectedItem && (!string.IsNullOrEmpty(where)) && (SelectedID > 0))
            {
                return new WhereCondition(where).Or(w => w.WhereEquals(IDColumn, SelectedID)).ToString(true);
            }

            return where;
        }

        #endregion
    }
}