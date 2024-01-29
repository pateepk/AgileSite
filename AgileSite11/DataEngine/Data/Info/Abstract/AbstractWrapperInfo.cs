using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;

using CMS.Base;

namespace CMS.DataEngine.Internal
{
    /// <summary>
    /// Base class for info object which wraps other info object and is able to add additional capabilities to its members.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is intended for internal usage only.
    /// </para>
    /// <para>
    /// <see cref="AbstractWrapperInfo{T}"/> does not have its own internal data class for storing data. Data management is ensured by each internal info.
    /// </para>
    /// </remarks>
    public abstract class AbstractWrapperInfo<TInfo> : AbstractInfoBase<TInfo>
        where TInfo : AbstractWrapperInfo<TInfo>, new()
    {
        #region "Properties"

        /// <summary>
        /// Data class is not used within wrapper instance. 
        /// Data is managed by the internal instance.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when property is used. The <see cref="DataClass"/> property is not internally used within wrapper instance.</exception>
        protected override sealed IDataClass DataClass
        {
            get
            {
                throw new NotSupportedException("The DataClass property is not internally used within wrapper instance and is not initialized. Depending code needs to be overridden and internal info class must be used.");
            }
        }
        

        /// <summary>
        /// Indicates whether the object changed.
        /// </summary>   
        public override bool HasChanged
        {
            get
            {
                return WrappedInfo.HasChanged;
            }
        }


        /// <summary>
        /// Indicates whether the object has loaded values in all columns.
        /// </summary>       
        public override bool IsComplete
        {
            get
            {
                return WrappedInfo.IsComplete;
            }
        }


        /// <summary>
        /// If true, the object allows partial updates.
        /// </summary>
        public override bool AllowPartialUpdate
        {
            get
            {
                return WrappedInfo.AllowPartialUpdate;
            }
            set
            {
                WrappedInfo.AllowPartialUpdate = value;
            }
        }


        /// <summary>
        /// Object type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                var typeInfo = base.TypeInfo;
                if (typeInfo == InfoHelper.UNKNOWN_TYPEINFO)
                {
                    return WrappedInfo.TypeInfo;
                }

                return typeInfo;
            }
            protected internal set
            {
                base.TypeInfo = value;
            }
        }


        /// <summary>
        /// Returns the wrapped info object
        /// </summary>
        protected abstract IInfo WrappedInfo
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance of <see cref="AbstractWrapperInfo{T}"/>.
        /// </summary>
        protected AbstractWrapperInfo()
        {
        }


        /// <summary>
        /// Creates new instance of <see cref="AbstractWrapperInfo{T}"/> with type dependent values initialization.
        /// </summary>
        /// <param name="typeInfo">Type information.</param>
        protected AbstractWrapperInfo(ObjectTypeInfo typeInfo)
            : base(typeInfo)
        {
        }


        /// <summary>
        /// Special constructor is used to deserialize values.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Context</param>
        /// <exception cref = "NotSupportedException" >Thrown when constructor is used. Serialization is not support within the scope of abstract class.</exception>
        protected AbstractWrapperInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            SerializationNotSupported();
        }


        /// <summary>
        /// Special constructor is used to deserialize values.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Context</param>
        /// <param name="typeInfos">Type infos that the object may need</param>
        protected AbstractWrapperInfo(SerializationInfo info, StreamingContext context, params ObjectTypeInfo[] typeInfos)
            : base(info, context, typeInfos)
        {
            DeserializeComponents(info);
        }


        /// <summary>
        /// Deserializes the internal components.
        /// </summary>
        /// <param name="info">Serialization info</param>
        private void DeserializeComponents(SerializationInfo info)
        {
            WrappedInfo = (IInfo)info.GetValue("WrappedInfo", typeof(IInfo));
        }


        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">  The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("WrappedInfo", WrappedInfo);
        }

        #endregion


        #region "Clone methods"

        /// <summary>
        /// Creates a clone of the object.
        /// </summary>
        /// <param name="clear">If true, the object is cleared to be able to create new object.</param>
        public override TInfo Clone(bool clear)
        {
            TInfo clone = base.Clone(clear);

            clone.WrappedInfo = WrappedInfo.CloneObject(clear);

            return clone;
        }


        /// <summary>
        /// Creates a clone of the object.
        /// </summary>
        public override TInfo Clone()
        {
            TInfo clone = base.Clone();

            clone.WrappedInfo = WrappedInfo.CloneObject(false);

            return clone;
        }

        #endregion


        #region "CRUD methods"

        /// <summary>
        /// Updates the object data to the database.
        /// </summary>
        protected override void UpdateDataInternal()
        {
            WrappedInfo.Update();
        }


        /// <summary>
        /// Deletes the object data from the database.
        /// </summary>
        protected override void DeleteDataInternal()
        {
            WrappedInfo.Delete();
        }


        /// <summary>
        /// Inserts the object data to the database.
        /// </summary>
        protected override void InsertDataInternal()
        {
            using (CMSActionContext ctx = new CMSActionContext())
            {
                // Do not touch parent since it is being updated
                ctx.TouchParent = false;

                WrappedInfo.Insert();
            }
        }


        /// <summary>
        /// Updates or inserts the object to the database.
        /// </summary>
        /// <exception cref="NotSupportedException">Upsert is not allowed on a wrapped object.</exception>
        protected override void UpsertData()
        {
            // Upsert is not implemented, because it is not supported for standard objects and we don't want to make it public at this point
            throw new NotSupportedException("Upsert is not allowed on a wrapped object.");
        }


        /// <summary>
        /// Upserts the data to the database.
        /// </summary>
        /// <param name="existingWhere">Existing data where condition</param>
        /// <exception cref="NotSupportedException">Upsert is not allowed on a composite object.</exception>
        protected override void UpsertDataInternal(WhereCondition existingWhere)
        {
            // Upsert is not implemented, because it is not supported for standard objects and we don't want to make it public at this point
            throw new NotSupportedException("Upsert is not allowed on a wrapped object.");
        }

        #endregion


        #region "IAdvancedDataContainer methods"

        /// <summary>
        /// Obtains value of given column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Result value</param>
        /// <returns>Returns <c>true</c> if the operation was successful (the value was present).</returns>
        public override bool TryGetValue(string columnName, out object value)
        {
            return WrappedInfo.TryGetValue(columnName, out value);
        }


        /// <summary>
        /// Sets value of the specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        /// <returns>Returns <c>true</c> if the operation was successful</returns>
        public override bool SetValue(string columnName, object value)
        {
            return WrappedInfo.SetValue(columnName, value);
        }


        /// <summary>
        /// Returns true if the object contains specific column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override bool ContainsColumn(string columnName)
        {
            return WrappedInfo.ContainsColumn(columnName);
        }


        /// <summary>
        /// Returns true if the item on specified column name changed.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override bool ItemChanged(string columnName)
        {
            return WrappedInfo.ItemChanged(columnName);
        }


        /// <summary>
        /// Returns the original value of given column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override object GetOriginalValue(string columnName)
        {
            return WrappedInfo.GetOriginalValue(columnName);
        }


        /// <summary>
        /// Resets the object changes and keeps the new values as unchanged.
        /// </summary>
        public override void ResetChanges()
        {
            WrappedInfo.ResetChanges();
        }


        /// <summary>
        /// Reverts the object changes and keeps the original values.
        /// </summary>
        public override void RevertChanges()
        {
            WrappedInfo.RevertChanges();
        }


        /// <summary>
        /// Makes the object data complete.
        /// </summary>
        /// <param name="loadFromDb">If true, the data to complete the object is loaded from database.</param>
        public override void MakeComplete(bool loadFromDb)
        {
            WrappedInfo.MakeComplete(loadFromDb);
        }


        /// <summary>
        /// Indicates whether object was changed.
        /// </summary>
        /// <param name="excludedColumns">Collection of columns excluded from change (separated by ';').</param>
        public override bool DataChanged(string excludedColumns)
        {
            return WrappedInfo.DataChanged(excludedColumns);
        }


        /// <summary>
        /// Executes the given action using original data of the object.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        public override void ExecuteWithOriginalData(Action action)
        {
            WrappedInfo.ExecuteWithOriginalData(action);
        }


        /// <summary>
        /// Makes sure that the binary data is loaded into binary column of the object when StoreToFileSystem is true.
        /// </summary>
        protected override byte[] EnsureBinaryData()
        {
            return WrappedInfo.Generalized.EnsureBinaryData();
        }

        #endregion


        #region "IHierarchicalDataContainer methods"

        /// <summary>
        /// Returns the type of given property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        protected override Type GetPropertyType(string propertyName)
        {
            return WrappedInfo.Generalized.GetPropertyType(propertyName);            
        }


        /// <summary>
        /// Returns collection of column names which values were changed.
        /// </summary>
        public override List<string> ChangedColumns()
        {
            return WrappedInfo.ChangedColumns();
        }

        #endregion


        #region "Load methods"

        /// <summary>
        /// Loads wrapped info data from the given data source.
        /// </summary>
        /// <param name="data">Source data to load.</param>
        /// <remarks>Method is used when <see cref="LoadData(LoadDataSettings)"/> is called.</remarks>
        protected abstract void LoadWrappedData(IDataContainer data);


        /// <summary>
        /// Loads the object data from given data container defined in <see cref="LoadDataSettings.Data"/>.
        /// </summary>
        /// <param name="settings">Data settings</param>
        protected internal override void LoadData(LoadDataSettings settings)
        {
            if ((settings == null) || (settings.Data == null))
            {
                return;
            }

            LoadWrappedData(settings.Data);
        }


        /// <summary>
        /// Sets the object default values.
        /// </summary>
        /// <remarks>
        /// This method is not called for objects inherited from <see cref="AbstractWrapperInfo{TInfo}"/>.
        /// </remarks>
        protected override sealed void LoadDefaultData()
        {
            base.LoadDefaultData();
        }

        #endregion


        #region "Other overriden methods"

        /// <summary>
        /// Checks whether the specified user has permissions for this object. Outcome of this method is determined by combining results of CheckPermissions event and CheckPermissionsInternal method.
        /// </summary>
        /// <param name="permission">Permission to perform this operation will be checked</param>
        /// <param name="currentSiteName">Name of the current context site. Permissions are checked on this site only when the site name cannot be obtained directly from the info object (from SiteIDColumn or site binding).</param>
        /// <param name="userInfo">Permissions of this user will be checked</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        /// <returns>True if user is allowed to perform specified operation on the this object; otherwise false</returns>
        protected override bool CheckPermissionsWithHandler(PermissionsEnum permission, string currentSiteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            return WrappedInfo.Generalized.CheckPermissions(permission, currentSiteName, userInfo, exceptionOnFailure);
        }


        /// <summary>
        /// Checks the object license. Returns true if the licensing conditions for this object were matched
        /// </summary>
        /// <param name="action">Object action</param>
        /// <param name="domainName">Domain name, if not set, uses current domain</param>
        protected override bool CheckLicense(ObjectActionEnum action = ObjectActionEnum.Read, string domainName = null)
        {
            return WrappedInfo.Generalized.CheckLicense(action, domainName);
        }


        /// <summary>
        /// Inserts the object as a new object to the DB with inner data and structure (according to given settings) cloned from the original.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Result of the cloning - messages in this object will be altered by processing this method</param>
        /// <returns>Returns the newly created clone</returns>
        protected override BaseInfo InsertAsClone(CloneSettings settings, CloneResult result)
        {
            var clone = WrappedInfo.Generalized.InsertAsClone(settings, result);

            var clonedWrapper = new TInfo();
            clonedWrapper.WrappedInfo = clone;

            return clonedWrapper;
        }

        #endregion
    }
}
