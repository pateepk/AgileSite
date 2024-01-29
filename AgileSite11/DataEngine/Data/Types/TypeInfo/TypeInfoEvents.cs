using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Object events for particular type info
    /// </summary>
    public class TypeInfoEvents
    {
        #region "Variables"

        // Base name for the events
        private readonly string baseName;

        // Indicates whether parent type info is dynamic and in this case static events shouldn't be used.
        private readonly bool parentIsDynamicTypeInfo;

        private ObjectHandler mUpdate;
        private ObjectHandler mUpdateInner;
        private ObjectHandler mInsert;
        private ObjectHandler mDelete;
        private BulkInsertHandler mBulkInsert;
        private BulkUpdateHandler mBulkUpdate;
        private BulkDeleteHandler mBulkDelete;
        private ObjectSortHandler mSort;
        private ObjectChangeOrderHandler mChangeOrder;
        private ObjectDataEventHandler mGetData;
        private SimpleObjectHandler mGetContent;
        private LogObjectChangeHandler mLogChange;
        private ObjectSecurityHandler mCheckPermissions;

        private readonly object lockObject = new object();

        #endregion


        #region "Properties"

        /// <summary>
        /// Fires when object is updated
        /// </summary>
        public ObjectHandler Update
        {
            get
            {
                return EnsureEvent(ref mUpdate, ObjectEvents.Update, "Update");
            }
        }


        /// <summary>
        /// Fires when the object is updated in the database.
        /// This is a read-only event, it is forbidden to change the object passed as event argument. If you need to modify the object, use <see cref="Update"/> event.
        /// </summary>
        /// <remarks>
        /// <see cref="Update"/>.Before event is fired before this event and <see cref="Update"/>.After event is fired after this event.
        /// This API supports the framework infrastructure and is not intended to be used directly from your code.
        /// </remarks>
        internal ObjectHandler UpdateInner
        {
            get
            {
                return EnsureEvent(ref mUpdateInner, ObjectEvents.UpdateInner, "UpdateInner");
            }
        }


        /// <summary>
        /// Fires when object is inserted
        /// </summary>
        public ObjectHandler Insert
        {
            get
            {
                return EnsureEvent(ref mInsert, ObjectEvents.Insert, "Insert");
            }
        }


        /// <summary>
        /// Fires when object is deleted
        /// </summary>
        public ObjectHandler Delete
        {
            get
            {
                return EnsureEvent(ref mDelete, ObjectEvents.Delete, "Delete");
            }
        }


        /// <summary>
        /// Fires when object is inserted in a bulk
        /// </summary>
        public BulkInsertHandler BulkInsert
        {
            get
            {
                return EnsureEvent(ref mBulkInsert, ObjectEvents.BulkInsert, "BulkInsert");
            }
        }


        /// <summary>
        /// Fires when object is updated in a bulk
        /// </summary>
        public BulkUpdateHandler BulkUpdate
        {
            get
            {
                return EnsureEvent(ref mBulkUpdate, ObjectEvents.BulkUpdate, "BulkUpdate");
            }
        }


        /// <summary>
        /// Fires when object is deleted in a bulk
        /// </summary>
        public BulkDeleteHandler BulkDelete
        {
            get
            {
                return EnsureEvent(ref mBulkDelete, ObjectEvents.BulkDelete, "BulkDelete");
            }
        }


        /// <summary>
        /// Fires when sorting of a objects is requested
        /// </summary>
        public ObjectSortHandler Sort
        {
            get
            {
                return EnsureEvent(ref mSort, ObjectEvents.Sort, "Sort");
            }
        }


        /// <summary>
        /// Fires when change order is requested on the object
        /// </summary>
        public ObjectChangeOrderHandler ChangeOrder
        {
            get
            {
                return EnsureEvent(ref mChangeOrder, ObjectEvents.ChangeOrder, "ChangeOrder");
            }
        }


        /// <summary>
        /// Fires when data are retrieved
        /// </summary>
        public ObjectDataEventHandler GetData
        {
            get
            {
                return EnsureEvent(ref mGetData, ObjectEvents.GetData, "GetData");
            }
        }


        /// <summary>
        /// Fires when search content is requested. You can modify content value which is saved to the search index.
        /// </summary>
        public SimpleObjectHandler GetContent
        {
            get
            {
                return EnsureEvent(ref mGetContent, ObjectEvents.GetContent, "GetContent");
            }
        }


        /// <summary>
        /// Fires when object tasks are logged. You can disable tasks logging for staging, integration etc.
        /// </summary>
        public LogObjectChangeHandler LogChange
        {
            get
            {
                return EnsureEvent(ref mLogChange, ObjectEvents.LogChange, "LogChange");
            }
        }


        /// <summary>
        /// Fires when permissions are checked on the object
        /// </summary>
        public ObjectSecurityHandler CheckPermissions
        {
            get
            {
                return EnsureEvent(ref mCheckPermissions, ObjectEvents.CheckPermissions, "CheckPermissions");
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="typeInfo">Type info</param>
        internal TypeInfoEvents(ObjectTypeInfo typeInfo)
        {
            baseName = string.Format("TypeInfo({0}).Events.", typeInfo.ObjectType);

            // Redirect parent of events to original type info if set
            if (typeInfo.OriginalTypeInfo != null)
            {
                SetParent(typeInfo.OriginalTypeInfo.Events);
            }

            parentIsDynamicTypeInfo = (typeInfo is DynamicObjectTypeInfo);
        }


        /// <summary>
        /// Ensures the event object
        /// </summary>
        /// <param name="field">Event field</param>
        /// <param name="parent">Event parent</param>
        /// <param name="name">Event name</param>
        private T EnsureEvent<T>(ref T field, T parent, string name)
            where T : AbstractHandler, new()
        {
            if (field == null)
            {
                lock (lockObject)
                {
                    if (field == null)
                    {
                        field = CreateEvent(parent, name);
                    }
                }
            }

            return field;
        }


        /// <summary>
        /// Creates an event
        /// </summary>
        /// <param name="parent">Event parent</param>
        /// <param name="name">Event name</param>
        private T CreateEvent<T>(T parent, string name)
            where T : AbstractHandler, new()
        {
            // Create the event instance
            var ev = new T();

            ev.SetParent(parent);
            ev.Name = baseName + name;

            // Make sure that events of dynamic type info are not behaving as static
            ev.IsStatic = !parentIsDynamicTypeInfo;

            return ev;
        }


        /// <summary>
        /// Sets the parent for the events
        /// </summary>
        /// <param name="parentEvents">Parent events</param>
        private void SetParent(TypeInfoEvents parentEvents)
        {
            Delete.Parent = parentEvents.Delete;
            Insert.Parent = parentEvents.Insert;
            Update.Parent = parentEvents.Update;
            BulkInsert.Parent = parentEvents.BulkInsert;
            BulkUpdate.Parent = parentEvents.BulkUpdate;
            BulkDelete.Parent = parentEvents.BulkDelete;
            Sort.Parent = parentEvents.Sort;
            ChangeOrder.Parent = parentEvents.ChangeOrder;
            UpdateInner.Parent = parentEvents.UpdateInner;
            LogChange.Parent = parentEvents.LogChange;
            GetContent.Parent = parentEvents.GetContent;
            GetData.Parent = parentEvents.GetData;
        }

        #endregion
    }
}