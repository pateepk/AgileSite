using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Object events
    /// </summary>
    public static class ObjectEvents
    {
        /// <summary>
        /// Fires when object is updated
        /// </summary>
        public static ObjectHandler Update = new ObjectHandler { Name = "ObjectEvents.Update" };


        /// <summary>
        /// Fires when the object is updated in the database. 
        /// This is a read-only event, it is forbidden to change the object passed as event argument. If you need to modify the object, use <see cref="Update"/> events.
        /// </summary>
        /// <remarks>
        /// <see cref="Update"/>.Before event is fired before this event and <see cref="Update"/>.After event is fired after this event.
        /// This API supports the framework infrastructure and is not intended to be used directly from your code.
        /// </remarks>
        internal static readonly ObjectHandler UpdateInner = new ObjectHandler() { Name = "ObjectEvents.UpdateInner"};


        /// <summary>
        /// Fires when object is inserted
        /// </summary>
        public static ObjectHandler Insert = new ObjectHandler { Name = "ObjectEvents.Insert" };


        /// <summary>
        /// Fires when object is deleted
        /// </summary>
        public static ObjectHandler Delete = new ObjectHandler { Name = "ObjectEvents.Delete" };


        /// <summary>
        /// Fires when object data are being retrieved.
        /// </summary>
        public static ObjectDataEventHandler GetData = new ObjectDataEventHandler { Name = "ObjectEvents.GetData" };


        /// <summary>
        /// Fires when search content is requested. You can modify content value which is saved to the search index.
        /// </summary>
        public static SimpleObjectHandler GetContent = new SimpleObjectHandler { Name = "ObjectEvents.GetContent" };


        /// <summary>
        /// Fires when object tasks are logged. You can disable tasks logging for staging, integration etc.
        /// </summary>
        public static LogObjectChangeHandler LogChange = new LogObjectChangeHandler { Name = "ObjectEvents.LogChange" };


        /// <summary>
        /// Fires when permissions are checked on the object
        /// </summary>
        public static ObjectSecurityHandler CheckPermissions = new ObjectSecurityHandler { Name = "ObjectEvents.CheckPermissions" };


        /// <summary>
        /// Fires when change order is requested on the object
        /// </summary>
        public static ObjectChangeOrderHandler ChangeOrder = new ObjectChangeOrderHandler { Name = "ObjectEvents.ChangeOrder" };


        /// <summary>
        /// Fires when sorting of a objects is requested
        /// </summary>
        public static ObjectSortHandler Sort = new ObjectSortHandler { Name = "ObjectEvents.Sort" };


        /// <summary>
        /// Fires when objects are inserted in bulk.
        /// </summary>
        public static readonly BulkInsertHandler BulkInsert = new BulkInsertHandler { Name = "ObjectEvents.BulkInsert" };


        /// <summary>
        /// Fires when objects are updated in bulk.
        /// </summary>
        public static BulkUpdateHandler BulkUpdate = new BulkUpdateHandler { Name = "ObjectEvents.BulkUpdate" };


        /// <summary>
        /// Fires when objects are deleted in bulk.
        /// </summary>
        public static readonly BulkDeleteHandler BulkDelete = new BulkDeleteHandler { Name = "ObjectEvents.BulkDelete" };


        /// <summary>
        /// Fires before a depending object, which doesn't have deletion type specified, is deleted. You can decide if the object will be deleted with API processing all the events or with generated DB query.
        /// </summary>
        internal static SimpleObjectTypeInfoHandler RequireEventHandling = new SimpleObjectTypeInfoHandler { Name = "ObjectEvents.RequireEventHandling" };
    }
}