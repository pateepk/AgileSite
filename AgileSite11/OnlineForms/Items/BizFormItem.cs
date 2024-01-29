using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.LicenseProvider;

namespace CMS.OnlineForms
{
    /// <summary>
    /// <see cref="BizFormInfo"/> stores information about General, Autoresponder, Email notification and similar tabs. 
    /// <see cref="FormClassInfo"/> stores the structure of the form. That means for example form definition (Fields tab) and search fields settings.
    /// <see cref="BizFormItem"/> stores the data that visitors fill on the website.
    /// </summary>
    public class BizFormItem : AbstractInfoBase<BizFormItem>
    {
        #region "Variables"

        private DataClassInfo mDataClassInfo;

        #endregion


        #region "Properties"

        /// <summary>
        /// DataClass info
        /// </summary>
        protected DataClassInfo DataClassInfo
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
                        throw new Exception("[BizFormItem.DataClassInfo]: Class name '" + className + "' not found!");
                    }

                    if (!dci.ClassIsForm)
                    {
                        throw new Exception("[BizFormItem.DataClassInfo]: Class name '" + className + "' is not form class!");
                    }

                    return mDataClassInfo = dci;
                }

                return mDataClassInfo;
            }
        }


        /// <summary>
        /// BizForm which this item belongs to.
        /// </summary>
        public BizFormInfo BizFormInfo
        {
            get
            {
                return BizFormInfoProvider.GetBizForms().WhereEquals("FormClassID", DataClassInfo.ClassID).FirstObject;
            }
        }


        /// <summary>
        /// Gets class name of BizForm item.
        /// </summary>
        public virtual string BizFormClassName
        {
            get
            {
                return ClassName;
            }
        }


        /// <summary>
        /// Gets or sets BizForm item ID.
        /// </summary>
        public int ItemID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue(TypeInfo.IDColumn), 0);
            }
            set
            {
                SetValue(TypeInfo.IDColumn, value);
            }
        }


        /// <summary>
        /// Gets or sets BizForm item insertion date.
        /// </summary>
        public DateTime FormInserted
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("FormInserted"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                if (value == DateTimeHelper.ZERO_TIME)
                {
                    SetValue("FormInserted", null);
                }
                else
                {
                    SetValue("FormInserted", value);
                }
            }
        }


        /// <summary>
        /// Gets or sets date of modification of BizForm item.
        /// </summary>
        public DateTime FormUpdated
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("FormUpdated"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                if (value == DateTimeHelper.ZERO_TIME)
                {
                    SetValue("FormUpdated", null);
                }
                else
                {
                    SetValue("FormUpdated", value);
                }
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Object display name.
        /// </summary>
        protected override string ObjectDisplayName
        {
            get
            {
                return BizFormItemProvider.GetItemName(this, DataClassInfo.ClassDisplayName);
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
            BizFormItemProvider.DeleteItem(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            BizFormItemProvider.SetItem(this);
        }
        

        /// <summary>
        /// Creates a clone of BizForm item
        /// </summary>
        public override BizFormItem Clone()
        {
            // Get new instance
            BizFormItem item = New(ClassName);

            item.LoadData(new LoadDataSettings(this));
            item.IsClone = true;

            return item;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Update BizForm item DataClass.
        /// </summary>
        protected override void UpdateData()
        {
            using (var h = BizFormItemEvents.Update.StartEvent(this))
            {
                h.DontSupportCancel();

                if (h.CanContinue())
                {
                    // Check license
                    if (!String.IsNullOrEmpty(RequestContext.CurrentDomain))
                    {
                        LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.BizForms);
                    }

                    // Set specific fields if should be updated
                    if (CMSActionContext.CurrentUpdateSystemFields)
                    {
                        FormUpdated = DateTime.Now;
                    }

                    // Update
                    base.UpdateData();
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Insert BizForm item DataClass.
        /// </summary>
        protected override void InsertData()
        {
            using (var h = BizFormItemEvents.Insert.StartEvent(this))
            {
                h.DontSupportCancel();

                if (h.CanContinue())
                {
                    // Check license
                    if (!String.IsNullOrEmpty(RequestContext.CurrentDomain))
                    {
                        LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.BizForms);
                    }

                    // Set specific fields if should be updated
                    if (CMSActionContext.CurrentUpdateSystemFields)
                    {
                        FormInserted = DateTime.Now;
                    }

                    // Insert
                    base.InsertData();
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Delete BizForm item DataClass.
        /// </summary>
        protected override void DeleteData()
        {
            using (var h = BizFormItemEvents.Delete.StartEvent(this))
            {
                h.DontSupportCancel();

                if (h.CanContinue())
                {
                    // Delete
                    base.DeleteData();
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Converts permissions enum to permission code name when CheckPermission() is called.
        /// </summary>
        /// <param name="permission">Permissions enum</param>
        protected override string GetPermissionName(PermissionsEnum permission)
        {
            switch (permission)
            {
                case PermissionsEnum.Read:
                    return "ReadData";

                case PermissionsEnum.Create:
                case PermissionsEnum.Modify:
                    return "EditData";

                case PermissionsEnum.Delete:
                case PermissionsEnum.Destroy:
                    return "DeleteData";

                default:
                    return base.GetPermissionName(permission);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly.")]
        public BizFormItem()
            : this(null)
        {
        }


        /// <summary>
        /// Base constructor for inherited classes and internal purposes
        /// </summary>
        /// <param name="className">Class name of the document</param>
        /// <param name="dr">Data row with the data</param>
        public BizFormItem(string className, DataRow dr = null)
        {
            if (!String.IsNullOrEmpty(className))
            {
                TypeInfo = BizFormItemProvider.GetTypeInfo(className);
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
            var className = BizFormItemProvider.GetClassName(settings.ObjectType);

            return New<BizFormItem>(className, settings.Data);
        }


        /// <summary>
        /// Creates new BizFormItem instance which must inherit the BizFormItem class.
        /// </summary>
        /// <param name="className">Class name in format application.class</param>
        /// <param name="dataRow">Data row containing both tree node and coupled table</param>
        public static BizFormItem New(string className, DataRow dataRow = null)
        {
            return New<BizFormItem>(className, dataRow);
        }


        /// <summary>
        /// Creates new document instance.
        /// </summary>
        /// <param name="className">Class name in format application.class</param>
        /// <param name="dataRow">Data row containing both tree node and coupled table</param>
        public static ItemType New<ItemType>(string className, DataRow dataRow)
            where ItemType : BizFormItem, new()
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
        /// Creates new document instance.
        /// </summary>
        /// <param name="className">Class name in format application.class</param>
        /// <param name="data">Data container containing both tree node and coupled table</param>
        public static ItemType New<ItemType>(string className, IDataContainer data)
            where ItemType : BizFormItem, new()
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
            where ItemType : BizFormItem, new()
        {
            ItemType result = (typeof(ItemType) == typeof(BizFormItem)) ? BizFormItemGenerator.NewInstance<ItemType>(className) : new ItemType();

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
                TypeInfo = BizFormItemProvider.GetTypeInfo(className);
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
                TypeInfo = BizFormItemProvider.GetTypeInfo(className);
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


        #region "IDataContainer Members"

        /// <summary>
        /// Gets BizForm item value.
        /// </summary>
        public override object GetValue(string columnName)
        {
            // Check if specific columns exists
            switch (columnName.ToLowerCSafe())
            {
                case "forminserted":
                case "formupdated":
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
                    case "forminserted":
                    case "formupdated":
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
                    case "forminserted":
                    case "formupdated":
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
        /// Gets the id column value which is used as search id by default.
        /// </summary>
        public override string GetSearchID()
        {
            return ItemID + ";" + TypeInfo.ObjectType;
        }


        /// <summary>
        /// Returns URL to current search result item.
        /// </summary>
        /// <param name="id">ID of the item</param>
        /// <param name="image">Image</param>
        public override string GetSearchImageUrl(string id, string image)
        {
            if (!string.IsNullOrEmpty(image))
            {
                string[] parts = image.Split('/');
                if (ImageHelper.IsImage(Path.GetExtension(parts[0])))
                {
                    return "~/CMSPages/GetBizFormFile.aspx?filename=" + parts[0];
                }
            }
            return base.GetSearchImageUrl(id, image);
        }

        #endregion
    }
}