using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Search;
using CMS.Base;
using CMS.Synchronization;

namespace CMS.CustomTables
{
    /// <summary>
    /// Class providing access to custom table data.
    /// </summary>
    public class CustomTableItem : AbstractInfoBase<CustomTableItem>
    {
        #region "Variables"

        private DataClassInfo mDataClassInfo;

        #endregion


        #region "Properties"

        /// <summary>
        /// DataClass info
        /// </summary>
        public DataClassInfo DataClassInfo
        {
            get
            {
                if (mDataClassInfo == null)
                {
                    string className = ClassName;

                    // Get the data class info
                    var dci = DataClassInfoProvider.GetDataClassInfo(className);

                    // Verify the data class
                    if (dci == null)
                    {
                        throw new Exception("[CustomTableItem.DataClassInfo]: Class name '" + className + "' not found!");
                    }

                    if (!dci.ClassIsCustomTable)
                    {
                        throw new Exception("[CustomTableItem.DataClassInfo]: Class name '" + className + "' is not custom table!");
                    }

                    mDataClassInfo = dci;
                }

                return mDataClassInfo;
            }
        }


        /// <summary>
        /// Gets class name of custom table item.
        /// </summary>
        public virtual string CustomTableClassName
        {
            get
            {
                return ClassName;
            }
        }


        /// <summary>
        /// Indicates if custom table contains ItemOrder column.
        /// </summary>
        public virtual bool OrderEnabled
        {
            get
            {
                return ContainsColumn("ItemOrder");
            }
        }


        /// <summary>
        /// Gets or sets custom table item ID.
        /// </summary>
        public int ItemID
        {
            get
            {
                return ObjectID;
            }
            set
            {
                ObjectID = value;
            }
        }


        /// <summary>
        /// Gets or sets custom table item GUID.
        /// </summary>
        public Guid ItemGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("ItemGUID"), Guid.Empty);
            }
            set
            {
                SetValue("ItemGUID", value);
            }
        }


        /// <summary>
        /// Gets or sets id of user who created custom table item.
        /// </summary>
        public int ItemCreatedBy
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ItemCreatedBy"), 0);
            }
            set
            {
                SetValue("ItemCreatedBy", value);
            }
        }


        /// <summary>
        /// Gets or sets custom table item creation date.
        /// </summary>
        public DateTime ItemCreatedWhen
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("ItemCreatedWhen"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ItemCreatedWhen", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Gets or sets is of user who modified the custom table item.
        /// </summary>
        public int ItemModifiedBy
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ItemModifiedBy"), 0);
            }
            set
            {
                SetValue("ItemModifiedBy", value);
            }
        }


        /// <summary>
        /// Gets or sets date of modification of custom table item.
        /// </summary>
        public DateTime ItemModifiedWhen
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("ItemModifiedWhen"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ItemModifiedWhen", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Gets or sets custom table item order.
        /// </summary>
        public int ItemOrder
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ItemOrder"), 0);
            }
            set
            {
                SetValue("ItemOrder", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// If true, synchronization tasks are logged on the object update.
        /// </summary>
        protected override SynchronizationTypeEnum LogSynchronization
        {
            get
            {
                // Do not log synchronization for items without GUID column
                if ((ItemGUID == Guid.Empty) || !StagingTaskInfoProvider.LogDataChanges())
                {
                    return SynchronizationTypeEnum.None;
                }

                return base.LogSynchronization;
            }
            set
            {
                base.LogSynchronization = value;
            }
        }


        /// <summary>
        /// Object display name.
        /// </summary>
        protected override string ObjectDisplayName
        {
            get
            {
                return CustomTableItemProvider.GetItemName(this, DataClassInfo.ClassDisplayName);
            }
            set
            {
                base.ObjectDisplayName = value;
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            CustomTableItemProvider.DeleteItem(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            CustomTableItemProvider.SetItem(this);
        }


        /// <summary>
        /// Creates a clone of custom table item
        /// </summary>
        public override CustomTableItem Clone()
        {
            // Get new instance
            CustomTableItem item = New(ClassName);

            item.LoadData(new LoadDataSettings(this));
            item.IsClone = true;

            return item;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly.")]
        public CustomTableItem()
            : this(null)
        {
        }


        /// <summary>
        /// Base constructor for inherited classes and internal purposes
        /// </summary>
        /// <param name="className">Class name of the custom table</param>
        /// <param name="dr">DataRow with the data</param>
        public CustomTableItem(string className, DataRow dr = null)
        {
            if (!String.IsNullOrEmpty(className))
            {
                TypeInfo = CustomTableItemProvider.GetTypeInfo(className);
            }
            
            if (dr != null)
            {
                LoadData(new LoadDataSettings(dr));
            }
        }

        #endregion


        #region "New methods"

        /// <summary>
        /// Creates a new object from the given DataRow
        /// </summary>
        /// <param name="settings">Data settings</param>
        protected override BaseInfo NewObject(LoadDataSettings settings)
        {
            // Transform object type back to class name and create object by class name
            var className = CustomTableItemProvider.GetClassName(settings.ObjectType);

            return New<CustomTableItem>(className, settings.Data);
        }


        /// <summary>
        /// Creates new CustomTableItem instance which must inherit the CustomTableItem class.
        /// </summary>
        /// <param name="className">Class name in format application.class</param>
        /// <param name="dataRow">Data row containing both tree node and coupled table</param>
        public static CustomTableItem New(string className, DataRow dataRow = null)
        {
            return New<CustomTableItem>(className, dataRow);
        }


        /// <summary>
        /// Creates new custom table item instance.
        /// </summary>
        /// <param name="className">Class name in format application.class</param>
        /// <param name="dataRow">Data row containing both tree node and coupled table</param>
        public static ItemType New<ItemType>(string className, DataRow dataRow) 
            where ItemType : CustomTableItem, new()
        {
            ItemType result = NewInstance<ItemType>(className);

            // If type not provided load default type
            if (result != null)
            {
                result.Initialize(className, dataRow);
            }

            return result;
        }


        /// <summary>
        /// Creates new custom table item instance.
        /// </summary>
        /// <param name="className">Class name in format application.class</param>
        /// <param name="data">Data container containing both tree node and coupled table</param>
        public static ItemType New<ItemType>(string className, IDataContainer data) 
            where ItemType : CustomTableItem, new()
        {
            ItemType result = NewInstance<ItemType>(className);

            // If type not provided load default type
            if (result != null)
            {
                result.Initialize(className, data);
            }

            return result;
        }


        /// <summary>
        /// Creates a new instance of the given type
        /// </summary>
        /// <param name="className">Class name</param>
        private static ItemType NewInstance<ItemType>(string className)
            where ItemType : CustomTableItem, new()
        {
            ItemType result = (typeof(ItemType) == typeof(CustomTableItem)) ? CustomTableItemGenerator.NewInstance<ItemType>(className) : new ItemType();

            return result;
        }


        /// <summary>
        /// Initializes the object created with default constructor. Use it to load existing item from data row.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="dr">Data row containing all page data</param>
        protected void Initialize(string className, DataRow dr)
        {
            if (!String.IsNullOrEmpty(className))
            {
                TypeInfo = CustomTableItemProvider.GetTypeInfo(className);
                LoadData(new LoadDataSettings(dr));
            }
        }


        /// <summary>
        /// Initializes the object created with default constructor. Use it to load existing item from data row.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="data">Data container containing all page data</param>
        protected void Initialize(string className, IDataContainer data)
        {
            if (!String.IsNullOrEmpty(className))
            {
                TypeInfo = CustomTableItemProvider.GetTypeInfo(className);
                LoadData(new LoadDataSettings(data));
            }
        }


        /// <summary>
        /// Updates instance with given type info
        /// </summary>
        /// <param name="typeInfo">Type info</param>
        internal void UpdateTypeInfo(ObjectTypeInfo typeInfo)
        {
            TypeInfo = typeInfo;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Update custom table item DataClass.
        /// </summary>
        protected override void UpdateData()
        {
            using (var h = CustomTableItemEvents.Update.StartEvent(this))
            {
                h.DontSupportCancel();

                if (h.CanContinue())
                {
                    // Check license
                    if (!String.IsNullOrEmpty(RequestContext.CurrentDomain))
                    {
                        LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.CustomTables);
                    }

                    // Set specific fields if should be updated
                    if (CMSActionContext.CurrentUpdateSystemFields)
                    {
                        ItemModifiedBy = CMSActionContext.CurrentUser.UserID;
                    }

                    // Update
                    base.UpdateData();
                }

                // Create search task explicitly
                if (LogSearchTask())
                {
                    int classID = 0;
                    DataClassInfo classInfo = DataClassInfoProvider.GetDataClassInfo(CustomTableClassName);
                    if (classInfo != null)
                    {
                        classID = classInfo.ClassID;
                    }

                    SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Process, CustomTableInfo.OBJECT_TYPE_CUSTOMTABLE, TypeInfo.IDColumn, ItemID + ";" + CustomTableClassName.ToLowerCSafe(), classID);
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Insert custom table item DataClass.
        /// </summary>
        protected override void InsertData()
        {
            using (var h = CustomTableItemEvents.Insert.StartEvent(this))
            {
                h.DontSupportCancel();

                if (h.CanContinue())
                {
                    // Check license
                    if (!String.IsNullOrEmpty(RequestContext.CurrentDomain))
                    {
                        LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.CustomTables);
                    }

                    // Set specific fields if should be updated
                    if (CMSActionContext.CurrentUpdateSystemFields)
                    {
                        var user = CMSActionContext.CurrentUser;
                        ItemCreatedBy = user.UserID;
                        ItemModifiedBy = user.UserID;
                        ItemCreatedWhen = DateTime.Now;
                    }

                    // Insert
                    base.InsertData();
                }

                // Create search task explicitly
                if (LogSearchTask())
                {
                    int classID = 0;
                    DataClassInfo classInfo = DataClassInfoProvider.GetDataClassInfo(CustomTableClassName);
                    if (classInfo != null)
                    {
                        classID = classInfo.ClassID;
                    }

                    SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Process, CustomTableInfo.OBJECT_TYPE_CUSTOMTABLE, TypeInfo.IDColumn, ItemID + ";" + CustomTableClassName.ToLowerCSafe(), classID);
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Returns true if given custom table has any search index defined.
        /// Uses only cached values - this operation has to be very fast since it's called in each update/insert of the custom table item.
        /// </summary>
        private bool LogSearchTask()
        {
            if (SearchIndexInfoProvider.SearchEnabled)
            {
                if (SearchHelper.IsClassSearchEnabled(CustomTableClassName))
                {
                    // Get all the custom table indexes
                    var indexes = SearchIndexInfoProvider.GetIndexIDs(new List<string>() { CustomTableInfo.OBJECT_TYPE_CUSTOMTABLE });
                    if ((indexes != null) && (indexes.Count > 0))
                    {
                        foreach (var id in indexes)
                        {
                            SearchIndexInfo info = SearchIndexInfoProvider.GetSearchIndexInfo(id);
                            if (info?.IndexSettings?.Items?.Values != null)
                            {
                                // Each custom table index can contain several custom tables indexed
                                // These are stored in the IndexSettings object each item has it's property ClassNames where the name of custom table is stored
                                foreach (var table in info.IndexSettings.Items.Values)
                                {
                                    if (CustomTableClassName.EqualsCSafe(table.ClassNames, true))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Delete custom table item DataClass.
        /// </summary>
        protected override void DeleteData()
        {
            using (var h = CustomTableItemEvents.Delete.StartEvent(this))
            {
                h.DontSupportCancel();

                if (h.CanContinue())
                {
                    // Delete
                    base.DeleteData();
                }

                // Create search task explicitly
                if (LogSearchTask())
                {
                    int classID = 0;
                    DataClassInfo classInfo = DataClassInfoProvider.GetDataClassInfo(CustomTableClassName);
                    if (classInfo != null)
                    {
                        classID = classInfo.ClassID;
                    }

                    SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Process, CustomTableInfo.OBJECT_TYPE_CUSTOMTABLE, TypeInfo.IDColumn, ItemID + ";" + CustomTableClassName.ToLowerCSafe(), classID);
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            if (OrderEnabled)
            {
                // Get the item count to save the item at the end of list
                ItemOrder = CustomTableItemProvider.GetLastItemOrder(CustomTableClassName) + 1;
            }

            Insert();
        }


        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            return DataClassInfo.CheckPermissions(permission, siteName, userInfo, exceptionOnFailure);
        }

        #endregion


        #region "IDataContainer overridden members"

        /// <summary>
        /// Gets custom table item value.
        /// </summary>
        public override object GetValue(string columnName)
        {
            // Check if specific columns exists
            switch (columnName.ToLowerCSafe())
            {
                case "itemcreatedby":
                case "itemcreatedwhen":
                case "itemmodifiedby":
                case "itemmodifiedwhen":
                case "itemorder":
                case "itemguid":
                    if (!ContainsColumn(columnName))
                    {
                        return null;
                    }
                    break;
            }

            return base.GetValue(columnName);
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public override bool TryGetValue(string columnName, out object value)
        {
            if (columnName == null)
            {
                value = null;
                return false;
            }
            else
            {
                // Check if specific columns exists
                switch (columnName.ToLowerCSafe())
                {
                    case "itemcreatedby":
                    case "itemcreatedwhen":
                    case "itemmodifiedby":
                    case "itemmodifiedwhen":
                    case "itemorder":
                    case "itemguid":
                        if (!ContainsColumn(columnName))
                        {
                            value = null;
                            return false;
                        }
                        break;
                }
                return base.TryGetValue(columnName, out value);
            }
        }


        /// <summary>
        /// Sets value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Column value</param> 
        public override bool SetValue(string columnName, object value)
        {
            if (columnName == null)
            {
                return false;
            }
            else
            {
                // Check if specific columns exists
                switch (columnName.ToLowerCSafe())
                {
                    case "itemcreatedby":
                    case "itemcreatedwhen":
                    case "itemmodifiedby":
                    case "itemmodifiedwhen":
                    case "itemorder":
                    case "itemguid":
                        if (!ContainsColumn(columnName))
                        {
                            return false;
                        }
                        break;
                }

                return base.SetValue(columnName, value);
            }
        }

        #endregion


        #region "ISearchable Members"

        /// <summary>
        /// Gets the type of current object.
        /// </summary>
        public override string SearchType
        {
            get
            {
                return CustomTableInfo.OBJECT_TYPE_CUSTOMTABLE;
            }
        }


        /// <summary>
        /// Gets the id column value which is used as search id by default.
        /// </summary>
        public override string GetSearchID()
        {
            return ItemID + ";" + CustomTableClassName.ToLowerCSafe();
        }


        /// <summary>
        /// Returns search fields collection. When existing collection is passed as argument, fields will be added to that collection.
        /// When collection is not passed, new collection will be created and return. 
        /// Collection will contain field values only when collection with StoreValues property set to true is passed to the method.
        /// When method creates new collection, it is created with StoreValues property set to false.
        /// </summary>
        /// <param name="index">Search index</param>
        /// <param name="searchFields">Search fields collection</param>
        public override ISearchFields GetSearchFields(ISearchIndexInfo index, ISearchFields searchFields = null)
        {
            // If search fields collection is not given, create new collection that doesn't store values.
            searchFields = searchFields ?? Service.Resolve<ISearchFields>();

            // Get data class
            var dataClass = DataClassInfoProvider.GetDataClassInfo(ClassName);
            SearchSettings fieldsSettings;

            // When DataClass or fields search settings are null return empty list
            if (dataClass == null || ((fieldsSettings = dataClass.ClassSearchSettingsInfos) == null))
            {
                return searchFields;
            }

            // Add class name
            searchFields.Add(SearchFieldFactory.Instance.Create("_customtablename", typeof(string), CreateSearchFieldOption.SearchableAndRetrievable), () => CustomTableClassName);

            // Add empty content field to ensure that content field will be created
            searchFields.EnsureContentField();

            // Loop through all general columns
            foreach (var setting in fieldsSettings)
            {
                searchFields.AddContentField(this, index, setting);
                searchFields.AddIndexField(this, index, setting, dataClass.GetSearchColumnType(setting.Name));
            }

            return searchFields;
        }

        #endregion
    }
}