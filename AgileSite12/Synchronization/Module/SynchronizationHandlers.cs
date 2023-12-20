using System;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Synchronization handlers
    /// </summary>
    internal static class SynchronizationHandlers
    {
        /// <summary>
        /// Initializes the synchronization handlers
        /// </summary>
        public static void Init()
        {
            ObjectEvents.Update.Before += Update_Before;

            ObjectEvents.Insert.Before += Insert_Before;
            ObjectEvents.Insert.After += Insert_After;

            ObjectEvents.Delete.Before += Delete_Before;
            ObjectEvents.Delete.After += Delete_After;

            ObjectTypeInfo.OnLogGlobalObjectChange += SynchronizationHelper.LogObjectChange;

            RequestEvents.Finalize.Execute += ProcessInternalTasksAsync_Execute;
        }


        /// <summary>
        /// Processes integration tasks asynchronously for all touched connectors.
        /// </summary>
        /// <remarks>
        /// Ideally integration tasks should be inserted and connectors touched asynchronously through <see cref="SynchronizationQueueWorker"/>.
        /// However if anyone would want to log integration tasks synchronously from request, touching connectors is done here.
        /// </remarks>
        private static void ProcessInternalTasksAsync_Execute(object sender, EventArgs e)
        {
            IntegrationHelper.ProcessInternalTasksAsync(IntegrationHelper.TouchedConnectorNames);
        }


        /// <summary>
        /// Actions that execute before object deletion
        /// </summary>
        [ExcludeFromDebug]
        private static void Delete_Before(object sender, ObjectEventArgs e)
        {
            var obj = StartSynchronizationHandler(e);

            // Log object change
            SynchronizationHelper.LogObjectChange(obj, TaskTypeEnum.DeleteObject);

            var ti = obj.TypeInfo;
            if (ti.HasObjectSettings)
            {
                // Remove object settings
                ObjectSettingsInfoProvider.DeleteSettingsForObject(obj);
            }

            if (ti.SupportsVersioning)
            {
                // Remove object versions
                SynchronizationHelper.RemoveObjectVersions(obj);
            }
        }


        /// <summary>
        /// Actions that execute after object deletion
        /// </summary>
        [ExcludeFromDebug]
        private static void Delete_After(object sender, ObjectEventArgs e)
        {
            var obj = e.Object;

            // Touch the parent to log parent
            SynchronizationHelper.TouchParent(obj, TaskTypeEnum.DeleteObject);
        }


        /// <summary>
        /// Actions that execute before object insert
        /// </summary>
        [ExcludeFromDebug]
        private static void Insert_Before(object sender, ObjectEventArgs e)
        {
            StartSynchronizationHandler(e);
        }


        /// <summary>
        /// Actions that execute after object insert
        /// </summary>
        [ExcludeFromDebug]
        private static void Insert_After(object sender, ObjectEventArgs e)
        {
            var obj = e.Object;
            var genObj = obj.Generalized;

            // Connect object explicitly (because object is in the same state in memory as in DB)
            genObj.Reconnect();

            // Touch the parent to log parent
            SynchronizationHelper.TouchParent(obj, TaskTypeEnum.CreateObject);

            // Log synchronization
            SynchronizationHelper.LogObjectChange(obj, TaskTypeEnum.CreateObject);
        }


        /// <summary>
        /// Actions that execute before object update
        /// </summary>
        private static void Update_Before(object sender, ObjectEventArgs e)
        {
            var obj = StartSynchronizationHandler(e);
            var genObj = obj.Generalized;

            // Ensure version with original data for versioned object
            SynchronizationHelper.EnsureObjectVersion(obj);

            bool dataChanged = obj.Generalized.DataChanged();

            e.CallWhenFinished(() =>
                {
                    // Connect object explicitly (because object is in the same state in memory as in DB)
                    genObj.Reconnect();

                    // Touch the parent to log parent
                    SynchronizationHelper.TouchParent(obj, TaskTypeEnum.UpdateObject);

                    // Log synchronization
                    var settings = new LogObjectChangeSettings(obj, TaskTypeEnum.UpdateObject)
                        {
                            DataChanged = dataChanged
                        };

                    SynchronizationHelper.LogObjectChange(settings);
                });
        }


        /// <summary>
        /// Starts the synchronization handler (actions that are common for all handlers)
        /// </summary>
        /// <param name="e">Event arguments</param>
        private static BaseInfo StartSynchronizationHandler(ObjectEventArgs e)
        {
            var obj = e.Object;
            var genObj = obj.Generalized;

            // Allow caching of the parent data, restore on dispose
            var originalCache = genObj.CacheParentData;
            e.CallOnDispose(() => genObj.CacheParentData = originalCache);

            genObj.CacheParentData = true;

            return obj;
        }


    }
}
