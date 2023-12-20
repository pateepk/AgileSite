using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading;

using CMS.Core;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;

namespace CMS.DataEngine
{
    using ClearCacheMethodsDictionary = IDictionary<string, Action<BaseInfo>>;

    /// <summary>
    /// Abstract object info class.
    /// </summary>
    [Serializable]
    public abstract partial class AbstractInfoBase<TInfo> : BaseInfo, ISearchable
        where TInfo : AbstractInfoBase<TInfo>, new()
    {
        #region "Variables"

        // Local properties
        private List<string> mLocalProperties;

        /// <summary>
        /// Column names, cached either by call to <seealso cref="CombineColumnNames(string[])"/> (or its override) or in the code of inherited class.
        /// </summary>
        /// <seealso cref="CombineColumnNames(System.Collections.ICollection[])"/>
        /// <seealso cref="CombineColumnNames(string[])"/>
        protected static List<string> mCombinedColumnNames;

        // Collection of external column settings (cached settings indexed by column name which are registered in specific InfoObjects).
        private static StringSafeDictionary<ExternalColumnSettings<TInfo>> mExternalColumnSettings;

        // Local column names
        private List<string> mLocalColumnNames;

        private IDataClass mDataClass;
        private bool mDataClassInitialized;
        private bool mDataClassInitializationInProgress;
        private Exception mDataClassInitializationFailureException;
        private string mDataClassInitializationFailureStackTrace;

        // If true, version GUID of the object is updated when saved.
        private bool mUpdateVersionGUID = true;

        // Collection of registered properties per object type.
        private static readonly StringSafeDictionary<RegisteredProperties<TInfo>> mRegisteredProperties = new StringSafeDictionary<RegisteredProperties<TInfo>>();

        // If true, all physical files will be deleted when object will be deleted.
        private bool mDeleteFiles = true;

        private bool mIsInfoReadOnly;
        private static readonly object lockObject = new object();
        private readonly object mEnsureDataLock = new object();
        private readonly object mUseOriginalDataLock = new object();

        #endregion


        #region "Static variables"

        /// <summary>
        /// Object generator
        /// </summary>
        protected static ObjectGenerator<TInfo> mGenerator = new ObjectGenerator<TInfo>(new ObjectFactory<TInfo>());

        /// <summary>
        /// Dictionary with the methods for clearing the internal object cache [columnName] => [clearCacheAction]
        /// </summary>
        private static ClearCacheMethodsDictionary mClearCacheMethods;

        #endregion


        #region "General properties (protected, accessible from Generalized)"

        /// <summary>
        /// Data class with the info object data.
        /// </summary>
        protected virtual IDataClass DataClass
        {
            get
            {
                if (!mDataClassInitialized && (mTypeInfo.ObjectClassName != ObjectTypeInfo.VALUE_UNKNOWN))
                {
                    // Create container for the object data
                    EnsureData();
                }

                return mDataClass;
            }
            set
            {
                mDataClass = value;
                mDataClassInitialized = true;
            }
        }


        /// <summary>
        /// If true, version GUID of the object is updated when saved.
        /// </summary>
        protected virtual bool UpdateVersionGUID
        {
            get
            {
                return mUpdateVersionGUID;
            }
            set
            {
                mUpdateVersionGUID = value;
            }
        }


        /// <summary>
        /// Indicates if all physical files should be deleted when object will be deleted.
        /// </summary>
        protected virtual bool DeleteFiles
        {
            get
            {
                return mDeleteFiles;
            }
            set
            {
                mDeleteFiles = value;
            }
        }


        /// <summary>
        /// Returns the original object code name
        /// </summary>
        protected virtual string OriginalObjectCodeName
        {
            get
            {
                if (CodeNameColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    return null;
                }

                return ValidationHelper.GetString(GetOriginalValue(CodeNameColumn), "");
            }
        }


        /// <summary>
        /// Returns whether the object code name changed or not
        /// </summary>
        protected virtual bool CodeNameChanged
        {
            get
            {
                if (CodeNameColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    return false;
                }

                return ItemChanged(CodeNameColumn);
            }
        }

        #endregion


        #region "Generalized properties"

        /// <summary>
        /// Returns the class name of this object
        /// </summary>
        [DatabaseMapping(false)]
        public virtual string ClassName
        {
            get
            {
                return DataClass.ClassName;
            }
        }


        /// <summary>
        /// Generalized interface of this object.
        /// </summary>
        public new GeneralizedAbstractInfo<TInfo> Generalized
        {
            get
            {
                return (GeneralizedAbstractInfo<TInfo>)base.Generalized;
            }
        }


        /// <summary>
        /// Gets the generalized info for this object
        /// </summary>
        protected override GeneralizedInfo GetGeneralizedInfo()
        {
            return new GeneralizedAbstractInfo<TInfo>(this);
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Dictionary with the methods for clearing the internal object cache [columnName] => [clearCacheAction]
        /// </summary>
        protected internal sealed override ClearCacheMethodsDictionary ClearCacheMethods
        {
            get
            {
                // Initializes with the main type info of the object
                return LockHelper.Ensure(ref mClearCacheMethods, () => GetClearCacheMethods(TypeInfo.OriginalTypeInfo ?? TypeInfo), lockObject);
            }
        }


        /// <summary>
        /// If true, original data is used instead of the actual data.
        /// </summary>
        internal virtual bool UseOriginalData
        {
            get
            {
                return DataClass.UseOriginalData;
            }
            set
            {
                DataClass.UseOriginalData = value;
            }
        }


        /// <summary>
        /// Returns true if the object is complete (has all columns).
        /// </summary>
        public override bool IsComplete
        {
            get
            {
                return DataClass.IsComplete;
            }
        }


        /// <summary>
        /// If true, the object allows partial updates.
        /// </summary>
        [DatabaseMapping(false)]
        public override bool AllowPartialUpdate
        {
            get
            {
                return DataClass.AllowPartialUpdate;
            }
            set
            {
                DataClass.AllowPartialUpdate = value;
            }
        }


        /// <summary>
        /// Returns true if the object changed.
        /// </summary>
        public override bool HasChanged
        {
            get
            {
                if (DataClass == null)
                {
                    return false;
                }

                return DataClass.HasChanged;
            }
        }


        /// <summary>
        /// Properties of the object
        /// </summary>
        public override List<string> Properties
        {
            get
            {
                if (mLocalProperties == null)
                {
                    List<string> names = base.Properties;

                    // Gets the list of the local column names
                    List<string> local = GetLocalProperties();
                    if (local != null)
                    {
                        // Build the new list of local joined with default
                        names = new List<string>(names);
                        names.AddRange(local);
                    }

                    mLocalProperties = names;
                }

                return mLocalProperties;
            }
        }


        /// <summary>
        /// Column names.
        /// </summary>
        /// <remarks>
        /// Consists of column names for this class and column names for this particular object.
        /// </remarks>
        public sealed override List<string> ColumnNames
        {
            get
            {
                if (mLocalColumnNames == null)
                {
                    var names = GetColumnNames();

                    // Gets the list of the local column names
                    var local = GetLocalColumnNames();
                    if (local != null)
                    {
                        // Build the new list of local joined with default
                        names = new List<string>(names);
                        names.AddRange(local);
                    }

                    mLocalColumnNames = names;
                }

                return mLocalColumnNames;
            }
        }


        /// <summary>
        /// Registered properties
        /// </summary>
        protected virtual RegisteredProperties<TInfo> RegisteredProperties
        {
            get
            {
                if (mRegisteredProperties[TypeInfo.ObjectType] == null)
                {
                    lock (mRegisteredProperties)
                    {
                        if (mRegisteredProperties[TypeInfo.ObjectType] == null)
                        {
                            mRegisteredProperties[TypeInfo.ObjectType] = new RegisteredProperties<TInfo>(RegisterProperties);
                        }
                    }
                }

                return mRegisteredProperties[TypeInfo.ObjectType];
            }
        }


        /// <summary>
        /// Collection of external column settings (cached settings indexed by column name which are registered in specific InfoObjects).
        /// </summary>
        private StringSafeDictionary<ExternalColumnSettings<TInfo>> ExternalColumnSettings
        {
            get
            {
                if (mExternalColumnSettings == null)
                {
                    lock (lockObject)
                    {
                        if (mExternalColumnSettings == null)
                        {
                            mExternalColumnSettings = new StringSafeDictionary<ExternalColumnSettings<TInfo>>();
                            RegisterExternalColumns();
                        }
                    }
                }

                return mExternalColumnSettings;
            }
        }


        /// <summary>
        /// Returns true if the object has it's data storage initialized already
        /// </summary>
        protected override bool HasData
        {
            get
            {
                return mDataClassInitialized;
            }
        }


        internal IInfoCacheProvider<TInfo> InfoCacheProvider
        {
            get
            {
                return TypeInfo.ProviderObject as IInfoCacheProvider<TInfo>;
            }
        }

        #endregion


        #region "Protected generalized methods (accessible from Generalized wrapper)"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            DeleteData();
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            if (TypeInfo.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                // Update by ID presence
                if (ObjectID > 0)
                {
                    UpdateData();
                }
                else
                {
                    InsertData();
                }
            }
            else
            {
                // Update by existing object (objects without single ID - bindings)
                GeneralizedInfo existing = GetExisting();
                if (existing == null)
                {
                    InsertData();
                }
                else
                {
                    UpdateData();
                }
            }
        }


        /// <summary>
        /// Deletes the object.
        /// </summary>
        protected virtual void DeleteData()
        {
            using (var tr = TransactionScopeFactory.GetTransactionScope(TypeInfo.ProviderType))
            {
                // Handle the event
                using (var h = TypeInfo.Events.Delete.StartEvent(this))
                {
                    h.DontSupportCancel();

                    if (h.CanContinue())
                    {
                        if (TypeInfo.CheckDependenciesOnDelete)
                        {
                            if (CheckDependencies(false))
                            {
                                throw new CheckDependenciesException(this, "You are attempting to delete an object which currently has some required dependencies. Remove the dependencies first.");
                            }
                        }

                        // Delete related data
                        DeleteRelatedData();

                        // Delete the object data
                        DeleteExternalColumns(false);

                        DeleteDataInternal();

                        DeleteFromHashtables();

                        ObjectStatus = ObjectStatusEnum.WasDeleted;
                    }

                    // Touch cache dependencies
                    if (TouchCacheDependencies)
                    {
                        TouchKeys();
                    }

                    // Finish the event
                    h.FinishEvent();
                }

                tr.Commit();
            }
        }


        /// <summary>
        /// Deletes the object data from the database
        /// </summary>
        protected virtual void DeleteDataInternal()
        {
            DataClass.Delete(true);
        }


        /// <summary>
        /// Deletes the related data of the object
        /// </summary>
        private void DeleteRelatedData()
        {
            DeleteMetafiles();

            RemoveObjectDependencies();
        }


        /// <summary>
        /// Deletes the metafiles related to this object
        /// </summary>
        protected virtual void DeleteMetafiles()
        {
            if (TypeInfo.HasMetaFiles)
            {
                using (CMSActionContext context = new CMSActionContext())
                {
                    // Don't touch parent, it's not necessary
                    context.LogSynchronization = false;

                    MetaFileInfoProvider.DeleteFiles(this, null);
                }
            }
        }


        /// <summary>
        /// Updates or inserts the object to the database.
        /// </summary>
        protected virtual void SetData()
        {
            if (ObjectID > 0)
            {
                UpdateData();
            }
            else if (TypeInfo.UseUpsert)
            {
                UpsertData();
            }
            else
            {
                InsertData();
            }
        }


        /// <summary>
        /// Updates or inserts the object to the database.
        /// </summary>
        protected virtual void UpsertData()
        {
            EnsureSystemFields();

            var ti = TypeInfo;

            // Handle the event
            using (var h = ti.Events.Insert.StartEvent(this))
            {
                h.DontSupportCancel();

                if (h.CanContinue())
                {
                    SaveExternalColumns(true, true);

                    // Get where condition which determines whether the object exists or not
                    var existingWhere = GetExistingWhereCondition(true);

                    UpsertDataInternal(existingWhere);
                    UpdateProviderHashtable();

                    FinalizeSaving();
                }

                // Finish the event
                h.FinishEvent();
            }
        }


        /// <summary>
        /// Upserts the data to the database
        /// </summary>
        /// <param name="existingWhere">Existing data where condition</param>
        protected virtual void UpsertDataInternal(WhereCondition existingWhere)
        {
            DataClass.Upsert(existingWhere);
        }


        /// <summary>
        /// Inserts the object to the database.
        /// </summary>
        protected virtual void InsertData()
        {
            EnsureSystemFields();

            var ti = TypeInfo;

            // Handle the event
            using (var h = ti.Events.Insert.StartEvent(this))
            {
                h.DontSupportCancel();

                if (h.CanContinue())
                {
                    SaveExternalColumns(false, false);

                    // Insert the data
                    InsertDataInternal();

                    // Ensure IDPath and Level after the ID has been already assigned to the object and save the data
                    if ((ti.ObjectIDPathColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) || (ti.ObjectNamePathColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
                    {
                        EnsureHierarchyColumns();

                        // Save the modified object
                        UpdateDataInternal();
                    }

                    UpdateProviderHashtable();

                    FinalizeSaving();
                }

                // Finish the event
                h.FinishEvent();
            }
        }


        /// <summary>
        /// Inserts the object data to the database
        /// </summary>
        protected virtual void InsertDataInternal()
        {
            DataClass.Insert();
        }


        /// <summary>
        /// Ensures the system fields for the object
        /// </summary>
        internal void EnsureSystemFields()
        {
            // Ensure GUID
            EnsureGUID();

            // Update time stamp
            EnsureLastModified();

            // Ensure the code name
            EnsureCodeName();
        }


        /// <summary>
        /// Updates the object to the database.
        /// </summary>
        protected virtual void UpdateData()
        {
            EnsureSystemFields();

            var ti = TypeInfo;

            // Handle the event
            using (var h = ti.Events.Update.StartEvent(this))
            {
                h.DontSupportCancel();

                if (h.CanContinue())
                {
                    SaveExternalColumns(true, true);

                    using (var hInner = ti.Events.UpdateInner.StartEvent(this))
                    {
                        hInner.DontSupportCancel();

                        // Ensure IDPath and Level
                        EnsureHierarchyColumns();

                        UpdateDataInternal();
                        UpdateProviderHashtable();
                        FinalizeSaving();

                        hInner.FinishEvent();
                    }
                }

                // Finish the event
                h.FinishEvent();
            }
        }


        /// <summary>
        /// Updates the object data to the database
        /// </summary>
        protected virtual void UpdateDataInternal()
        {
            // Insert the data
            DataClass.Update();
        }


        /// <summary>
        /// Finalizes saving of the object by performing extra actions
        /// </summary>
        private void FinalizeSaving()
        {
            IsClone = false;
            ObjectStatus = ObjectStatusEnum.Unchanged;

            // Reset the changes
            ResetChanges();

            // Touch cache dependencies
            TouchKeys();
        }


        /// <summary>
        /// Copies memory properties
        /// </summary>
        /// <param name="infoObj">Target object instance</param>
        protected virtual void CopyProperties(TInfo infoObj)
        {
            infoObj.UpdateVersionGUID = mUpdateVersionGUID;
            infoObj.DeleteFiles = mDeleteFiles;

            CopyMemoryProperties(infoObj);
        }


        /// <summary>
        /// Creates a clone of object's data class and properties.
        /// </summary>
        private TInfo CloneCore()
        {
            // Create new object of the same type
            var newObj = (TInfo)NewObject(new LoadDataSettings((DataRow)null, TypeInfo.ObjectType));

            newObj.IsClone = true;

            // Copy memory properties
            CopyProperties(newObj);

            if (mDataClass != null)
            {
                // Copy the standard data
                mDataClass.CopyDataTo(newObj.DataClass);

                // Copy original data
                mDataClass.CopyOriginalDataTo(newObj.DataClass);
            }

            return newObj;
        }


        /// <summary>
        /// Creates a clone of the object
        /// </summary>
        public virtual TInfo Clone()
        {
            // Create new object of the same type
            var newObj = CloneCore();

            if (mDataClass != null)
            {
                // Copy directly external columns to prioritize values stored on file system.
                CopyExternalColumns(newObj);
            }

            return newObj;
        }


        /// <summary>
        /// Creates a clone of the object
        /// </summary>
        /// <param name="clear">If true, the object is cleared to be able to create new object</param>
        public virtual TInfo Clone(bool clear)
        {
            TInfo newObj = Clone();

            // Reset the changes in case of clearing
            if (clear)
            {
                // Clear the IDs
                var genObj = newObj.Generalized;

                genObj.ObjectID = 0;
                genObj.ObjectGUID = Guid.NewGuid();
                genObj.IsClone = false;

                // Reset remembered changes
                newObj.ResetChanges();
            }

            return newObj;
        }


        /// <summary>
        /// Creates a new object of the given object type
        /// </summary>
        /// <param name="objectType">Object type</param>
        protected static TInfo NewInternal(string objectType = null)
        {
            var result = mGenerator.CreateNewObject(objectType);
            if ((result != null) && (objectType != null))
            {
                // Apply type condition to the object to properly match the given object type
                var ti = ObjectTypeManager.GetTypeInfo(objectType);
                if (ti != null)
                {
                    ti.ApplyTypeCondition(result);
                }
            }

            return result;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Static constructor
        /// </summary>
        static AbstractInfoBase()
        {
            TypeManager.RegisterGenericType(typeof(AbstractInfoBase<TInfo>));
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        protected AbstractInfoBase()
            : this(null, false)
        {
        }


        /// <summary>
        /// Constructor - Initializes the type dependent values.
        /// </summary>
        /// <param name="typeInfo">Type information</param>
        protected AbstractInfoBase(ObjectTypeInfo typeInfo)
            : this(typeInfo, false)
        {
        }


        /// <summary>
        /// Constructor - Initializes the type dependent values.
        /// </summary>
        /// <param name="typeInfo">Type information</param>
        /// <param name="createData">If true, data structure of the object is created</param>
        protected AbstractInfoBase(ObjectTypeInfo typeInfo, bool createData)
            : base(typeInfo)
        {
            if (createData && (typeInfo.ObjectClassName != ObjectTypeInfo.VALUE_UNKNOWN))
            {
                EnsureData();
            }

            ObjectStatus = ObjectStatusEnum.Unchanged;
        }


        /// <summary>
        /// Constructor - Initializes the type dependent values.
        /// </summary>
        /// <param name="typeInfo">Type information</param>
        /// <param name="dr">DataRow with the source data</param>
        protected AbstractInfoBase(ObjectTypeInfo typeInfo, DataRow dr)
            : base(typeInfo)
        {
            if ((dr != null) && (mTypeInfo.ObjectClassName != ObjectTypeInfo.VALUE_UNKNOWN))
            {
                EnsureData();
                mDataClass.LoadData(dr.AsDataContainer());
            }

            ObjectStatus = ObjectStatusEnum.Unchanged;
        }


        /// <summary>
        /// Constructor - Initializes the object with source data.
        /// </summary>
        /// <param name="sourceData">Source data</param>
        protected AbstractInfoBase(IDataClass sourceData)
            : this(null, sourceData, false)
        {
        }


        /// <summary>
        /// Constructor - Initializes the object with source data.
        /// </summary>
        /// <param name="typeInfo">Type information</param>
        /// <param name="sourceData">Source data</param>
        /// <param name="keepSourceData">If true, source data are kept</param>
        protected AbstractInfoBase(ObjectTypeInfo typeInfo, IDataClass sourceData, bool keepSourceData)
            : this(typeInfo, false)
        {
            if (sourceData == null)
            {
                throw new ArgumentNullException(nameof(sourceData));
            }

            if (keepSourceData)
            {
                // Keep the source class as the data (must be the same class name)
                if (!sourceData.ClassName.Equals(typeInfo.ObjectClassName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("The source data must be the same class as the new object.");
                }

                mDataClass = sourceData;
                mDataClassInitialized = true;

                ApplyDataClassFlags();
            }
            else
            {
                // Initialize the data structure
                EnsureData();

                sourceData.CopyDataTo(mDataClass);
            }
        }


        /// <summary>
        /// Applies necessary flags to the internal data container
        /// </summary>
        private void ApplyDataClassFlags()
        {
            if (mIsInfoReadOnly)
            {
                mDataClass?.SetReadOnly();
            }
        }


        /// <summary>
        /// Serialization constructor.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Context</param>
        protected AbstractInfoBase(SerializationInfo info, StreamingContext context)
            : this(info, context, null)
        {
            SerializationNotSupported();
        }


        /// <summary>
        /// Serialization constructor.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Context</param>
        /// <param name="typeInfos">Type infos that the object may need</param>
        protected AbstractInfoBase(SerializationInfo info, StreamingContext context, params ObjectTypeInfo[] typeInfos)
            : base(info, context, typeInfos)
        {
            mIsInfoReadOnly = Convert.ToBoolean(ObjectHelper.GetSerializedData(info, "Locked", typeof(bool), false));
            mDataClass = (IDataClass)ObjectHelper.GetSerializedData(info, "DataClass", typeof(IDataClass), null);
            mDataClassInitialized = true;
            mUpdateVersionGUID = Convert.ToBoolean(ObjectHelper.GetSerializedData(info, "UpdateVersionGUID", typeof(bool), false));
        }


        /// <summary>
        /// Object serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Context</param>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Locked", mIsInfoReadOnly);
            info.AddValue("DataClass", mDataClass);
            info.AddValue("UpdateVersionGUID", mUpdateVersionGUID);
        }


        /// <summary>
        /// Loads the default data to the object.
        /// </summary>
        protected virtual void LoadDefaultData()
        {
            // Nothing in base class
        }


        /// <summary>
        /// Creates a new data class container within the object
        /// </summary>
        /// <param name="loadDefault">If true, the process loads the default data</param>
        /// <param name="applyTypeCondition">If true, type condition is applied to the object</param>
        protected void EnsureData(bool loadDefault = true, bool applyTypeCondition = true)
        {
            if (mDataClassInitialized)
            {
                return;
            }

            lock (mEnsureDataLock)
            {
                ThrowIfInitializationFailed();

                if (mDataClassInitialized)
                {
                    return;
                }

                if (mDataClassInitializationInProgress)
                {
                    // Current thread is initializing the data class. The LoadDefaultData() method typically accesses the DataClass properties, which causes a loop
                    return;
                }

                try
                {
                    mDataClassInitializationInProgress = true;

                    mDataClass = DataClassFactory.NewDataClass(mTypeInfo.ClassStructureInfo);

                    // Load default data
                    if (loadDefault)
                    {
                        LoadDefaultData();

                        // Type condition is stronger than default data to ensure consistency in ObjectType
                        if (applyTypeCondition)
                        {
                            mTypeInfo.ApplyTypeCondition(mDataClass);
                        }

                        ResetChanges();
                    }

                    ApplyDataClassFlags();

                    mDataClassInitialized = true;
                }
                catch(Exception ex)
                {
                    var stackTrace = new StackTrace(true);
                    mDataClassInitializationFailureStackTrace = stackTrace.ToString();
                    mDataClassInitializationFailureException = ex;

                    throw new InvalidOperationException($"The initialization of {typeof(AbstractInfoBase<TInfo>).FullName} failed. See the inner exception for details.", ex);
                }
            }
        }


        private void ThrowIfInitializationFailed()
        {
            if (mDataClassInitializationFailureException != null)
            {
                throw new InvalidOperationException($"The initialization of {typeof(AbstractInfoBase<TInfo>).FullName} has already failed and the object is in an invalid state. See the inner exception for details of the original failure. The stack trace from the time of the original failure follows.{Environment.NewLine}{mDataClassInitializationFailureStackTrace}-- END OF STACK TRACE --{Environment.NewLine}", mDataClassInitializationFailureException);
            }
        }


        /// <summary>
        /// Executes the given action without using external column data.
        /// All object values will be used directly from the object and not loaded from external data.
        /// </summary>
        /// <param name="action">Action to execute</param>
        private void ExecuteWithoutExternalColumnData(Action action)
        {
            var originalValue = IgnoreExternalColumns;

            try
            {
                IgnoreExternalColumns = true;

                action();
            }
            finally
            {
                IgnoreExternalColumns = originalValue;
            }
        }


        /// <summary>
        /// Loads the object using the given settings
        /// </summary>
        /// <param name="settings">Data settings</param>
        protected internal override void LoadData(LoadDataSettings settings)
        {
            EnsureData(settings.DataIsExternal, !settings.DataIsExternal);

            mDataClass.LoadData(settings.Data, !settings.DataIsExternal);
        }

        #endregion


        #region "New methods"

        /// <summary>
        /// Creates the clone of the object
        /// </summary>
        /// <param name="clear">If true, the object is cleared to be able to create new object</param>
        public override BaseInfo CloneObject(bool clear = false)
        {
            return Clone(clear);
        }


        /// <summary>
        /// Initiates the system to replace the info type with a specified type that must inherit from the original info type
        /// </summary>
        public static IConditionalObjectFactory ReplaceWith<TNewInfo>()
            where TNewInfo : TInfo, new()
        {
            var fact = new ConditionalObjectFactory<TNewInfo>();

            mGenerator.RegisterDefaultFactory(fact, true);

            return fact;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the list of local properties for particular object
        /// </summary>
        protected virtual List<string> GetLocalProperties()
        {
            return GetLocalColumnNames();
        }


        /// <summary>
        /// Gets the default list of column names for this class
        /// </summary>
        protected override List<string> GetColumnNames()
        {
            // Get the names from structure definition
            var structure = TypeInfo.ClassStructureInfo;

            return structure?.ColumnNames;
        }


        /// <summary>
        /// Gets the list of local column names for particular object
        /// </summary>
        protected virtual List<string> GetLocalColumnNames()
        {
            return null;
        }


        /// <summary>
        /// Gets the type of the given property
        /// </summary>
        /// <param name="propertyName">Property name</param>
        protected override Type GetPropertyType(string propertyName)
        {
            return
                RegisteredProperties.GetPropertyType(propertyName) ??
                base.GetPropertyType(propertyName) ??
                TypeInfo.ClassStructureInfo?.GetColumnType(propertyName);
        }


        /// <summary>
        /// Combines column names from supplied collections into a list and caches it in <see cref="mCombinedColumnNames"/>.
        /// Does nothing when column names are already cached unless <see cref="BaseInfo.TypeInfo"/> has <see cref="ObjectTypeInfo.ColumnsInvalidated"/> set to true.
        /// Returns combined column names.
        /// </summary>
        /// <param name="names">An array of collections containing column names.</param>
        /// <returns>Combined column names.</returns>
        /// <seealso cref="mCombinedColumnNames"/>
        protected List<string> CombineColumnNames(params ICollection[] names)
        {
            if ((mCombinedColumnNames == null) || TypeInfo.ColumnsInvalidated)
            {
                TypeInfo.ColumnsInvalidated = false;
                mCombinedColumnNames = TypeHelper.NewList(names);
            }

            return mCombinedColumnNames;
        }


        /// <summary>
        /// Combines supplied column names into a list and caches it in <see cref="mCombinedColumnNames"/>.
        /// Does nothing when column names are already cached unless <see cref="BaseInfo.TypeInfo"/> has <see cref="ObjectTypeInfo.ColumnsInvalidated"/> set to true.
        /// Returns combined column names.
        /// </summary>
        /// <param name="names">A string array containing column names.</param>
        /// <returns>Combined column names.</returns>
        /// <seealso cref="mCombinedColumnNames"/>
        public List<string> CombineColumnNames(params string[] names)
        {
            if ((mCombinedColumnNames == null) || TypeInfo.ColumnsInvalidated)
            {
                TypeInfo.ColumnsInvalidated = false;
                mCombinedColumnNames = new List<string>(names);
            }

            return mCombinedColumnNames;
        }


        /// <summary>
        /// Returns true if the object contains given column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override bool ContainsColumn(string columnName)
        {
            bool result = false;

            // Check the DataClass
            if (ColumnNames != null)
            {
                result = ColumnNames.Contains(columnName, StringComparer.InvariantCultureIgnoreCase);
            }

            // Check the related data
            object related = RelatedData;
            if (!result && (related is IDataContainer))
            {
                result = ((IDataContainer)related).ContainsColumn(columnName);
            }

            return result;
        }


        /// <summary>
        /// Sets the field value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        public override bool SetValue(string columnName, object value)
        {
            bool result = false;

            // Ensure correct value for foreign key column
            if (TypeInfo.IsForeignKey(columnName))
            {
                var val = ValidationHelper.GetInteger(value, 0);
                if (val < 0)
                {
                    value = null;
                }
            }

            ClearCachedValues(columnName);

            // Try to set the DataClass value
            if (DataClass != null)
            {
                result = DataClass.SetValue(columnName, value);
            }

            // Try to set the related data value
            object related = RelatedData;
            if (!result && (related is IDataContainer))
            {
                result = ((IDataContainer)related).SetValue(columnName, value);
            }

            if (result)
            {
                // Change status to changed object in case the object wasn't deleted
                if (ObjectStatus != ObjectStatusEnum.WasDeleted)
                {
                    ObjectStatus = ObjectStatusEnum.Changed;
                }
            }

            return result;
        }


        /// <summary>
        /// Locks the object as a read-only
        /// </summary>
        protected internal override void SetReadOnly()
        {
            mDataClass?.SetReadOnly();

            mIsInfoReadOnly = true;
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public override bool TryGetValue(string columnName, out object value)
        {
            bool result = false;
            value = null;

            // Try to get value from external column, but only if the given column didn't change
            // The external column is updated on filesystem when the object is set to database, so prior to that we want to return the changed data
            if (mTypeInfo.HasExternalColumns && !IgnoreExternalColumns && !ItemChanged(columnName))
            {
                object externalData = GetExternalColumnData(columnName, true);
                if (externalData != null)
                {
                    value = externalData;
                    return true;
                }
            }

            // Try to get value from the data class with given column name
            if (DataClass != null)
            {
                result = DataClass.TryGetValue(columnName, out value);
            }

            if (!result)
            {
                // Try to get value from the related data
                var related = RelatedData as IDataContainer;
                if (related != null)
                {
                    result = related.TryGetValue(columnName, out value);
                }
            }

            return result;
        }


        /// <summary>
        /// Makes the object data complete.
        /// </summary>
        /// <param name="loadFromDb">If true, the data to complete the object is loaded from database</param>
        public override void MakeComplete(bool loadFromDb)
        {
            DataClass.MakeComplete(loadFromDb);
        }


        /// <summary>
        /// Gets the field value converted to a specified type.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value to use if the value is empty or not convertible to a specified type</param>
        public T GetValue<T>(string columnName, T defaultValue)
        {
            return ValidationHelper.GetValue(GetValue(columnName), defaultValue);
        }


        /// <summary>
        /// Gets the field value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override object GetValue(string columnName)
        {
            // Get the value
            object value;
            TryGetValue(columnName, out value);

            return value;
        }


        /// <summary>
        /// Touches the cache dependency keys of the object to flush the dependent cache items.
        /// </summary>
        public void TouchKeys()
        {
            if (TouchCacheDependencies)
            {
                CacheHelper.TouchKeys(GetCacheDependencies(), LogWebFarmTasks, false);
            }
        }


        /// <summary>
        /// Reverts the object changes to the original values.
        /// </summary>
        public override void RevertChanges()
        {
            DataClass.RevertChanges();
        }


        /// <summary>
        /// Resets the object changes and keeps the new values as unchanged according to the asUnchanged parameter.
        /// </summary>
        public override void ResetChanges()
        {
            DataClass.ResetChanges();
        }


        /// <summary>
        /// Returns the original value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override object GetOriginalValue(string columnName)
        {
            return DataClass.GetOriginalValue(columnName);
        }


        /// <summary>
        /// Returns true if the object changed.
        /// </summary>
        /// <param name="excludedColumns">List of columns excluded from change (separated by ';')</param>
        public override bool DataChanged(string excludedColumns)
        {
            return DataClass.DataChanged(excludedColumns);
        }


        /// <summary>
        /// Returns true if any of specified columns changed.
        /// </summary>
        /// <param name="columnNames">Column names</param>
        public bool AnyItemChanged(params string[] columnNames)
        {
            return columnNames.Any(ItemChanged);
        }


        /// <summary>
        /// Returns true if the item on specified column name changed.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override bool ItemChanged(string columnName)
        {
            return DataClass.ItemChanged(columnName);
        }


        /// <summary>
        /// Returns list of column names which values were changed.
        /// </summary>
        /// <returns>List of column names</returns>
        public override List<string> ChangedColumns()
        {
            return DataClass.ChangedColumns();
        }


        /// <summary>
        /// Method which is called after the order of the object was changed. Generates staging tasks and webfarm tasks by default.
        /// </summary>
        protected override void SetObjectOrderPostprocessing()
        {
            base.SetObjectOrderPostprocessing();

            if (TouchCacheDependencies)
            {
                TouchKeys();
            }
        }


        private void UpdateProviderHashtable()
        {
            InfoCacheProvider?.UpdateObjectInHashtables((TInfo)this);
        }


        private void DeleteFromHashtables()
        {
            InfoCacheProvider?.DeleteObjectFromHashtables((TInfo)this);
        }

        #endregion


        #region "Property methods"

        /// <summary>
        /// Registers the given property to the object
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="ex">Lambda expression</param>
        protected PropertySettings<TInfo> RegisterProperty<TProperty>(string propertyName, Func<TInfo, object> ex)
        {
            return RegisteredProperties.Add<TProperty>(propertyName, ex, null);
        }


        /// <summary>
        /// Registers the given parametrized property to the object.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="parameter">Parameter for the lambda expression</param>
        /// <param name="ex">Lambda expression</param>
        protected PropertySettings<TInfo> RegisterProperty(string propertyName, object parameter, Func<TInfo, object, object> ex)
        {
            return RegisteredProperties.Add<object>(propertyName, parameter, ex, null);
        }


        /// <summary>
        /// Registers the given property to the object
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="ex">Lambda expression</param>
        protected PropertySettings<TInfo> RegisterProperty(string propertyName, Func<TInfo, object> ex)
        {
            return RegisteredProperties.Add<object>(propertyName, ex, null);
        }


        /// <summary>
        /// Registers the properties of this object
        /// </summary>
        protected virtual void RegisterProperties()
        {
            RegisteredProperties.CollectProperties(GetType());

            var ti = TypeInfo;

            // Children
            if ((ti.ChildObjectTypes != null) && (ti.ChildObjectTypes.Count > 0))
            {
                RegisterProperty("Children", m => m.Children);

                // Register all the children collections as properties of current object
                int i = 0;
                foreach (string colName in Children.CollectionNames)
                {
                    if (!ContainsColumn(colName))
                    {
                        string name = colName;

                        RegisterProperty(colName, i++, (m, index) => m.Children[name]);
                    }
                }
            }

            // Child dependencies
            if (!String.IsNullOrEmpty(ti.ChildDependencyColumns))
            {
                RegisterProperty("ChildDependencies", m => m.ChildDependencies);
            }

            // Parent
            if (!String.IsNullOrEmpty(ParentObjectType))
            {
                var prop = RegisterProperty("Parent", m => m.Parent);
                prop.EmptyObjectFactory = new InfoObjectFactory(ParentObjectType);
            }

            // Site
            if (ti.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                var prop = RegisterProperty("Site", m => m.Site);
                prop.EmptyObjectFactory = new InfoObjectFactory(PredefinedObjectType.SITE);
            }

            // Bindings
            if ((ti.BindingObjectTypes != null) && (ti.BindingObjectTypes.Count > 0))
            {
                RegisterProperty("Bindings", m => m.Bindings);
            }

            // Other bindings
            if ((ti.OtherBindingObjectTypes != null) && (ti.OtherBindingObjectTypes.Count > 0))
            {
                RegisterProperty("OtherBindings", m => m.OtherBindings);
            }

            // Site binding
            if (!String.IsNullOrEmpty(ti.SiteBinding))
            {
                RegisterProperty("SiteBindings", m => m.AssignedSites);
            }

            // Meta files
            if (ti.HasMetaFiles)
            {
                RegisterProperty("MetaFiles", m => m.MetaFiles);

                // Thumbnail
                if (ti.ThumbnailGUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    var prop = RegisterProperty("Thumbnail", m => m.ObjectThumbnail);
                    prop.EmptyObjectFactory = new InfoObjectFactory(MetaFileInfo.OBJECT_TYPE);
                }

                // Icon
                if (ti.IconGUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    var prop = RegisterProperty("Icon", m => m.ObjectIcon);
                    prop.EmptyObjectFactory = new InfoObjectFactory(MetaFileInfo.OBJECT_TYPE);
                }
            }

            // Processes of object
            if (ti.HasProcesses)
            {
                RegisterProperty("Processes", m => m.Processes);
            }

            // Scheduled tasks of object
            if (ti.HasScheduledTasks)
            {
                RegisterProperty("ScheduledTasks", m => m.ScheduledTasks);
            }

            RegisterProperty("ReferringObjects", m => m.ReferringObjects);

            // Automatically evaluated properties
            if (ti.HasAutomaticProperties)
            {
                if (ti.ObjectDependencies != null)
                {
                    foreach (var dep in ti.ObjectDependencies)
                    {
                        // If column found, return its object type
                        string colName = dep.DependencyColumn;
                        if (colName.EndsWith("id", StringComparison.OrdinalIgnoreCase) && !colName.EndsWith("guid", StringComparison.OrdinalIgnoreCase))
                        {
                            colName = colName.Substring(0, colName.Length - 2);

                            // Do not override registered property when it's registered manually.
                            if (!RegisteredProperties.Contains((colName)))
                            {
                                // Register the automatic property
                                var prop = RegisterProperty(colName, m => m.GetAutomaticProperty(colName));

                                if (!string.IsNullOrEmpty(dep.DependencyObjectType))
                                {
                                    prop.EmptyObjectFactory = new InfoObjectFactory(dep.DependencyObjectType);
                                }
                            }
                        }
                    }
                }
            }

            RegisterProperty("DisplayName", m => m.Generalized.ObjectDisplayName);
            if (ti.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                RegisterProperty("SiteID", m => m.Generalized.ObjectSiteID);
            }
            if (ti.CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                RegisterProperty("CodeName", m => m.Generalized.ObjectCodeName);
            }
            if (ti.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                RegisterProperty("ID", m => m.Generalized.ObjectID);
            }
            if (ti.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                RegisterProperty("GUID", m => m.Generalized.ObjectGUID);
            }

            if (ti.ParentIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                RegisterProperty("ParentID", m => m.Generalized.ObjectParentID);
                RegisterProperty("Parent", m => m.Generalized.ObjectParent);
            }

            RegisterProperty("TypeInfo", m => m.TypeInfo);
        }


        /// <summary>
        /// Returns value of property.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public override bool TryGetProperty(string columnName, out object value)
        {
            // Ensure the registered properties
            GetRegisteredProperties();

            // Registered properties
            bool result = RegisteredProperties.Evaluate((TInfo)this, columnName, out value);

            if (!result)
            {
                // Try through regular methods
                result = base.TryGetProperty(columnName, out value);
            }

            return result;
        }


        /// <summary>
        /// Gets list of registered properties.
        /// </summary>
        protected override List<string> GetRegisteredProperties()
        {
            return RegisteredProperties.GetRegisteredProperties();
        }


        /// <summary>
        /// Tries to get the automatic property value for underlying object
        /// </summary>
        /// <param name="columnName">Column name</param>
        protected object GetAutomaticProperty(string columnName)
        {
            object result = null;
            TryGetAutomaticProperty(columnName, ref result);

            return result;
        }


        /// <summary>
        /// Tries to get the automatic property value for underlying object
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returning value</param>
        protected bool TryGetAutomaticProperty(string columnName, ref object value)
        {
            // Search by property name + ID (such as SiteDefaultStylesheet[ID])
            columnName = columnName + "ID";
            if (ContainsColumn(columnName))
            {
                var dep = TypeInfo.GetDependencyForColumn(columnName);
                if (dep != null)
                {
                    string dependencyType = dep.DependencyObjectType ?? GetStringValue(dep.ObjectTypeColumn, null);
                    if (!String.IsNullOrEmpty(dependencyType))
                    {
                        int bindingId = ValidationHelper.GetInteger(GetProperty(columnName), 0);
                        if (bindingId > 0)
                        {
                            // Get the object from provider
                            value = ProviderHelper.GetInfoById(dependencyType, bindingId);
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        #endregion


        #region "External column storage methods"

        /// <summary>
        /// Returns settings for external storage of the column. Returns null by default (which causes the default settings to be used in the external column data storage process).
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        protected ExternalColumnSettings<TInfo> GetExternalColumnSettings(string columnName)
        {
            return ExternalColumnSettings[columnName];
        }


        /// <summary>
        /// Registers external column settings to the hash table.
        /// </summary>
        protected void RegisterExternalColumn(string columnName, ExternalColumnSettings<TInfo> settings)
        {
            ExternalColumnSettings[columnName] = settings;
        }


        /// <summary>
        /// Registers external column settings to the hash table.
        /// </summary>
        protected virtual void RegisterExternalColumns()
        {
            // Does nothing by default
        }


        /// <summary>
        /// Returns path to the external storage with data of the given column. If the column data is not stored in the external storage (not enabled), and the check for enabled is allowed, returns null.
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        /// <param name="settings">Settings of the external storage of the column data (if null, default settings are used)</param>
        /// <param name="checkEnabled">If true, checks if the external column is enabled</param>
        protected string GetExternalPath(string columnName, ExternalColumnSettings<TInfo> settings, bool checkEnabled = true)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                return null;
            }

            if (settings == null)
            {
                settings = new ExternalColumnSettings<TInfo>();
            }

            string path = null;

            // Check the setting if exists
            if (!checkEnabled || settings.StoreInExternalStorage(ObjectSiteName))
            {
                path = settings.StoragePath((TInfo)this);
            }

            return path;
        }


        /// <summary>
        /// Returns FileInfo object from external storage if exists.
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        public FileInfo GetFileInfo(string columnName)
        {
            var settings = GetExternalColumnSettings(columnName);
            if (settings == null)
            {
                return null;
            }

            string path = GetExternalPath(columnName, settings);
            path = GetPhysicalPath(path);

            if (!string.IsNullOrEmpty(path))
            {
                // Use the provider to get the data
                AbstractStorageProvider provider = settings.StorageProvider ?? StorageHelper.GetStorageProvider(path);
                return provider.GetFileInfo(path);
            }

            return null;
        }


        /// <summary>
        /// Returns physical path or null if can't be retrieved.
        /// </summary>
        /// <param name="path">Path to process</param>
        private static string GetPhysicalPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            try
            {
                return URLHelper.GetPhysicalPath(path);
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// Returns the column data from external storage if exists.
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        /// <param name="applyGetTransformation">If true, GetTransformation is applied before the result is returned</param>
        protected object GetExternalColumnData(string columnName, bool applyGetTransformation)
        {
            var settings = GetExternalColumnSettings(columnName);
            if (settings == null)
            {
                return null;
            }

            if (!AllowExternalColumn(columnName))
            {
                return null;
            }

            // Get the data path from original object data because fields which influence the path might have changed
            var clone = CloneCore();
            clone.RevertChanges();

            string path = clone.GetExternalPath(columnName, settings);

            path = GetPhysicalPath(path);

            AbstractStorageProvider provider = null;
            FileInfo file = null;
            if (!string.IsNullOrEmpty(path))
            {
                // Use the provider to get the data
                provider = settings.StorageProvider ?? StorageHelper.GetStorageProvider(path);
                file = provider.GetFileInfo(path);
            }
            if ((file != null) && file.Exists)
            {
                byte[] buffer;

                using (FileStream fileStream = file.OpenRead())
                {
                    int count, sum = 0;
                    int length = (int)fileStream.Length;
                    buffer = new byte[length];

                    while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                    {
                        sum += count;
                    }
                }

                // Ensure correct data type
                Type type = GetPropertyType(columnName);
                if (type == typeof(byte[]))
                {
                    return buffer;
                }

                object result = provider.FileProviderObject.ReadAllText(file.FullName, Encoding.UTF8);

                // Use the transformation if requested
                if (applyGetTransformation)
                {
                    if (settings.GetDataTransformation != null)
                    {
                        result = settings.GetDataTransformation((TInfo)this, result);
                    }
                }

                return result;
            }

            return null;
        }


        /// <summary>
        /// Saves the column data to the external storage if exists.
        /// Returns true if data was stored in external storage and should not be stored also in DB.
        /// If data should be stored in DB, returns false.
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        /// <param name="data">Data to save externally</param>
        /// <param name="deleteUnusedFiles">If true, the old files are deleted when the path of the columnFile has changed</param>
        protected bool SetExternalColumnData(string columnName, object data, bool deleteUnusedFiles)
        {
            // Get the settings
            var settings = GetExternalColumnSettings(columnName);
            if (settings == null)
            {
                return false;
            }

            if (!AllowExternalColumn(columnName))
            {
                return false;
            }

            // Get the file path
            string path = GetExternalPath(columnName, settings);
            path = GetPhysicalPath(path);

            // Delete the old file if exists
            if (deleteUnusedFiles)
            {
                string originalPath = null;

                ExecuteWithOriginalData(() =>
                {
                    originalPath = GetExternalPath(columnName, settings, false);
                });

                originalPath = URLHelper.GetPhysicalPath(originalPath);

                if (!string.IsNullOrEmpty(originalPath))
                {
                    if (!originalPath.Equals(path, StringComparison.OrdinalIgnoreCase))
                    {
                        // Delete original file
                        if (File.Exists(originalPath))
                        {
                            File.Delete(originalPath);
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(path))
            {
                // Use transformation method when defined
                object newData = TransformExternalColumnData(settings, data, false);

                // Do not create empty folders
                if (!Directory.Exists(Path.GetDirectoryName(path)) && String.IsNullOrEmpty(ValidationHelper.GetString(newData, String.Empty)))
                {
                    return false;
                }

                // Ensure storage path
                DirectoryHelper.EnsureDiskPath(path, SystemContext.WebApplicationPhysicalPath);

                // Ensure correct data type
                var bytes = newData as byte[];
                if (bytes != null)
                {
                    // If the input data is binary, just write it to the file
                    File.WriteAllBytes(path, bytes);
                }
                else if ((newData is string) || FileHelper.IsTextFileExtension(Path.GetExtension(path)))
                {
                    // When data is text, ensure non-empty value and normalize line endings, otherwise do not write the file at all
                    var stringData = NormalizeLineEndings(ValidationHelper.GetString(newData, ""));

                    File.WriteAllText(path, stringData, Encoding.UTF8);
                }
                else if (File.Exists(path))
                {
                    // Delete the file, as empty binary file is not allowed
                    File.Delete(path);
                }

                // Check if the data should be stored in database
                return !settings.StoreInDatabase(ObjectSiteName);
            }

            return false;
        }


        /// <summary>
        /// Executes the given <paramref name="action"/> using original data of the object.
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <remarks>
        /// This operation temporarily modifies the object's state and is inherently not thread-safe
        /// even when <paramref name="action"/> is a read-only operation.
        /// </remarks>
        public override void ExecuteWithOriginalData(Action action)
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(mUseOriginalDataLock, ref lockTaken);
                if (!lockTaken)
                {
                    // Concurrency within this method indicates improper use, log it to event log
                    LogExecuteWithOriginalDataConcurrency();
                }

                ExecuteWithOriginalDataCore(action);
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(mUseOriginalDataLock);
                }
            }
        }


        /// <summary>
        /// Executes the given <paramref name="action"/> using original data of the object.
        /// </summary>
        private void ExecuteWithOriginalDataCore(Action action)
        {
            var originalValue = UseOriginalData;

            try
            {
                // Make the object use its original data
                UseOriginalData = true;
                ClearCachedValues();

                action();
            }
            finally
            {
                // Restore the object state to normal
                UseOriginalData = originalValue;
                ClearCachedValues();
            }
        }


        /// <summary>
        /// Logs warning to event log regarding multiple threads executing action with original data on one object's instance.
        /// </summary>
        private void LogExecuteWithOriginalDataConcurrency()
        {
            CoreServices.EventLog.LogEvent("W", "AbstractInfoBase", "UseOriginalDataConcurrency",
                String.Format("Concurrency was detected when executing an action with original data on info object '{0}'. The stack strace of current thread follows: {1}{1}{2}",
                ToString(), Environment.NewLine, new StackTrace(true)));
        }


        /// <summary>
        /// Returns the list of columns registered as the external columns.
        /// </summary>
        protected override List<string> GetExternalColumns()
        {
            return ExternalColumnSettings?.TypedKeys.ToList();
        }


        /// <summary>
        /// Goes through the columns which are stored externally and updates DB versions with the data from external storage.
        /// </summary>
        protected override void UpdateExternalColumns()
        {
            using (CMSActionContext context = new CMSActionContext())
            {
                context.CheckLicense = false;

                if (ExternalColumnSettings != null)
                {
                    if (IsModifiedExternally())
                    {
                        foreach (string col in ExternalColumnSettings.Keys)
                        {
                            object data = GetExternalColumnData(col, true);
                            if (data != null)
                            {
                                SetValue(col, data);
                            }
                        }

                        SetObject();
                    }
                }
            }
        }


        /// <summary>
        /// Goes through the columns which are stored externally and deletes all the files.
        /// </summary>
        /// <param name="updateDB">If true, DB is updated with the data from the file before it's deleted.</param>
        protected override void DeleteExternalColumns(bool updateDB)
        {
            if (ExternalColumnSettings != null)
            {
                foreach (string col in ExternalColumnSettings.Keys)
                {
                    if (updateDB)
                    {
                        // Transfer data to the object
                        object data = GetExternalColumnData(col, true);
                        if (data != null)
                        {
                            SetValue(col, data);
                        }
                    }

                    DeleteExternalColumn(col);
                }

                if (updateDB)
                {
                    SetObject();
                }
            }
        }


        /// <summary>
        /// Deletes the external column data
        /// </summary>
        /// <param name="col">Column name</param>
        private void DeleteExternalColumn(string col)
        {
            // Get the settings
            var settings = GetExternalColumnSettings(col);
            if (settings == null)
            {
                return;
            }

            string path = GetExternalPath(col, settings, false);
            path = GetPhysicalPath(path);

            if (!string.IsNullOrEmpty(path))
            {
                // Get the provider
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }


        /// <summary>
        /// Goes through the columns which are stored externally and returns the list of particular files this object uses.
        /// </summary>
        protected override List<string> GetExternalFiles()
        {
            if (ExternalColumnSettings.Count == 0)
            {
                return null;
            }

            List<string> files = new List<string>();

            // Process all external columns
            foreach (string col in ExternalColumnSettings.Keys)
            {
                var settings = GetExternalColumnSettings(col);
                var path = settings.StoragePath((TInfo)this);

                files.Add(path);
            }

            return files;
        }


        /// <summary>
        /// Goes through the columns which are stored externally and ensures them in the external storage.
        /// </summary>
        /// <param name="deleteUnusedFiles">If true, the old files are deleted when the path of the columnFile has changed</param>
        /// <param name="onlyIfChanged">Tries to modify the external file only if the column is marked as changed</param>
        protected internal override void SaveExternalColumns(bool deleteUnusedFiles, bool onlyIfChanged)
        {
            if (TypeInfo.HasExternalColumns && !IgnoreExternalColumns)
            {
                using (CMSActionContext context = new CMSActionContext())
                {
                    context.CheckLicense = false;

                    foreach (string col in ExternalColumnSettings.Keys)
                    {
                        var colName = col;
                        var settings = GetExternalColumnSettings(colName);

                        // Try to set to external storage
                        if (!onlyIfChanged || ItemChanged(colName) || ((CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && ItemChanged(CodeNameColumn))
                            || ColumnsChanged(settings.DependencyColumns))
                        {
                            object val = null;

                            // Try to get data from the object (existing database data or new data) first
                            ExecuteWithoutExternalColumnData(() => val = GetValue(colName));

                            // If the data is not stored in database but stored on file system, we want to get it from original location in file system
                            if (val == null)
                            {
                                ExecuteWithOriginalData(() =>
                                {
                                    var siteName = ObjectSiteName;

                                    if (!settings.StoreInDatabase(siteName) && settings.StoreInExternalStorage(siteName))
                                    {
                                        val = GetValue(colName);
                                    }
                                });
                            }

                            if (SetExternalColumnData(colName, val, deleteUnusedFiles))
                            {
                                // If not stored in DB, remove value from data class
                                SetValue(colName, null);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Returns true if at least one column of the column list provided as the first argument was changed.
        /// </summary>
        /// <param name="columns">Column list</param>
        protected bool ColumnsChanged(List<string> columns)
        {
            if (columns != null)
            {
                foreach (string column in columns)
                {
                    if (ItemChanged(column))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Goes through the columns which are stored externally and checks if the data in DB is the same as in external storage. If all the columns are same returns true, otherwise false.
        /// </summary>
        protected override bool IsModifiedExternally()
        {
            return ExternalColumnSettings.TypedKeys.Any(IsColumnModifiedExternally);
        }


        private bool IsColumnModifiedExternally(string columnName)
        {
            object fsData = GetExternalColumnData(columnName, false);
            object dbData = GetTransformedDatabaseData(columnName);

            return !ExternalColumnDataEquals(fsData, dbData);
        }


        /// <summary>
        /// Returns true if both given values are equal.
        /// </summary>
        /// <remarks>
        /// Values are considered equal also if one of them is null.
        /// </remarks>
        /// <param name="externalValue">Value to be compared</param>
        /// <param name="databaseValue">Value to be compared</param>
        private bool ExternalColumnDataEquals(object externalValue, object databaseValue)
        {
            if ((externalValue == null) || (databaseValue == null))
            {
                return true;
            }

            if ((externalValue is string) && (databaseValue is string))
            {
                string normalizedA = NormalizeLineEndings((string)externalValue);
                string normalizedB = NormalizeLineEndings((string)databaseValue);

                return normalizedA.Equals(normalizedB, StringComparison.Ordinal);
            }

            return externalValue.Equals(databaseValue);
        }


        private object GetTransformedDatabaseData(string columnName)
        {
            var data = DataClass[columnName];
            var settings = GetExternalColumnSettings(columnName);

            return TransformExternalColumnData(settings, data, true);
        }


        private object TransformExternalColumnData(ExternalColumnSettings<TInfo> settings, object data, bool readOnly)
        {
            if (settings?.SetDataTransformation == null)
            {
                return data;
            }

            return settings.SetDataTransformation((TInfo)this, data, readOnly);
        }


        /// <summary>
        /// Indicates whether a given column is allowed to be saved externally.
        /// </summary>
        /// <param name="columnName">Column name</param>
        protected virtual bool AllowExternalColumn(string columnName)
        {
            return true;
        }


        /// <summary>
        /// Returns text with normalized line endings (\r\n).
        /// </summary>
        /// <param name="text">Text to process</param>
        private string NormalizeLineEndings(string text)
        {
            return text.Replace("\n", "\r\n").Replace("\r\r", "\r");
        }

        #endregion


        #region "ISearchable Members"

        /// <summary>
        /// Gets the search type name.
        /// </summary>
        [DatabaseMapping(false)]
        public virtual string SearchType
        {
            get
            {
                return TypeInfo.ObjectClassName;
            }
        }


        /// <summary>
        /// Returns search fields collection. When existing collection is passed as argument, fields will be added to that collection.
        /// When collection is not passed, new collection will be created and return.
        /// Collection will contain field values only when collection with StoreValues property set to true is passed to the method.
        /// When method creates new collection, it is created with StoreValues property set to false.
        /// </summary>
        /// <param name="index">Search index</param>
        /// <param name="searchFields">Search fields collection</param>
        public virtual ISearchFields GetSearchFields(ISearchIndexInfo index, ISearchFields searchFields = null)
        {
            // If search fields collection is not given, create new collection that doesn't store values.
            searchFields = searchFields ?? Service.Resolve<ISearchFields>();

            var ti = TypeInfo;

            // Get data class
            var dataClass = DataClassInfoProvider.GetDataClassInfo(ti.ObjectClassName);
            var fieldsSettings = dataClass?.ClassSearchSettingsInfos;

            // When DataClass or fields search settings are null return empty list
            if (fieldsSettings == null)
            {
                return searchFields;
            }

            // Add ID column name
            searchFields.Add(SearchFieldFactory.Instance.Create(SearchFieldsConstants.IDCOLUMNNAME, typeof(string), CreateSearchFieldOption.SearchableAndRetrievable), () => ti.IDColumn);

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


        /// <summary>
        /// Returns index document for current object with dependencies on search index info.
        /// </summary>
        /// <param name="index">Search index info object</param>
        public virtual SearchDocument GetSearchDocument(ISearchIndexInfo index)
        {
            // Get data class
            DataClassInfo dataClass = DataClassInfoProvider.GetDataClassInfo(TypeInfo.ObjectClassName);

            // Return null when data class cannot be retrieved
            if (dataClass == null)
            {
                return null;
            }

            // Set default document creation
            DateTime documentCreated = DateTimeHelper.ZERO_TIME;

            // Check whether creation column is defined
            if (!String.IsNullOrEmpty(dataClass.ClassSearchCreationDateColumn))
            {
                // Check whether column exists in document collection
                if (ContainsColumn(dataClass.ClassSearchCreationDateColumn))
                {
                    documentCreated = ValidationHelper.GetDateTime(GetValue(dataClass.ClassSearchCreationDateColumn), DateTimeHelper.ZERO_TIME);
                }
            }

            // Create search document
            var doc = new SearchDocument();
            doc.Initialize(index, SearchType, GetSearchID(), documentCreated);

            // Create search fields collection
            var searchFields = Service.Resolve<ISearchFields>();
            searchFields.StoreValues = true;

            foreach (var field in GetSearchFields(index, searchFields).Items)
            {
                // When field is content, raise event
                if (field.FieldName == SearchFieldsConstants.CONTENT)
                {
                    // Get content
                    string content = field.Value.ToString();

                    // Raise the custom event
                    TypeInfo.Events.GetContent.StartEvent(this, ref content);
                    field.Value = content;
                }

                doc.AddSearchField(field);
            }

            // Return document
            return doc;
        }


        /// <summary>
        /// Returns URL for a search result item image.
        /// </summary>
        /// <param name="image">Image</param>
        public virtual string GetSearchImageUrl(string image)
        {
            // By default do not transform the image url
            return image;
        }

        #endregion
    }
}