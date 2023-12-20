using System;

using CMS.Base;
using CMS.ContinuousIntegration.Internal;
using CMS.DataEngine;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Event handling for continuous integration processes
    /// </summary>
    internal class ContinuousIntegrationHandlers
    {
        /// <summary>
        /// Initializes the events handlers.
        /// </summary>
        public static void Init()
        {
            ObjectEvents.Insert.After += InsertAfter;

            ObjectEvents.UpdateInner.Before += UpdateInnerBefore;

            ObjectEvents.Delete.Before += DeleteBefore;

            ObjectEvents.Delete.After += DeleteAfter;

            ObjectEvents.BulkInsert.After += BulkInsertAfter;

            ObjectEvents.BulkUpdate.Before += BulkUpdateBefore;

            ObjectEvents.BulkDelete.Before += BulkDeleteBefore;

            SettingsKeyInfoProvider.OnSettingsKeyChanged += OnSettingsChanged;
        }


        #region "Handler methods"

        private static void InsertAfter(object sender, ObjectEventArgs e)
        {
            ContinuousIntegrationEventHandling.BaseInfoInsertAfter(e.Object);
        }


        private static void UpdateInnerBefore(object sender, ObjectEventArgs e)
        {
            ContinuousIntegrationEventHandling.BaseInfoUpdateBefore(e.Object, e);
        }


        private static void DeleteAfter(object sender, ObjectEventArgs e)
        {
            ContinuousIntegrationEventHandling.BaseInfoDeleteAfter(e.Object);
        }


        private static void OnSettingsChanged(object sender, SettingsKeyChangedEventArgs e)
        {
            if (e.KeyName.Equals(ContinuousIntegrationHelper.ENABLED_CI_KEY, StringComparison.OrdinalIgnoreCase))
            {
                // File metadata are not up to date
                FileMetadataInfoProvider.DeleteAllFileMetadataInfos();

                if (!RepositoryActionContext.CurrentIsRestoreOperationRunning && e.KeyValue.ToBoolean(false))
                {
                    ContinuousIntegrationEventHandling.RebuildConfiguration();
                }
            }
        }


        private static void BulkInsertAfter(object sender, BulkInsertEventArgs e)
        {
            ContinuousIntegrationEventHandling.BulkInsertAfter(e);
        }


        private static void BulkUpdateBefore(object sender, BulkUpdateEventArgs e)
        {
            ContinuousIntegrationEventHandling.BulkUpdateBefore(e);
        }


        private static void BulkDeleteBefore(object sender, BulkDeleteEventArgs e)
        {
            ContinuousIntegrationEventHandling.BulkDeleteBefore(e);
        }


        private static void DeleteBefore(object sender, ObjectEventArgs objectEventArgs)
        {
            if (RepositoryActionContext.CurrentIsRestoreOperationRunning || !ContinuousIntegrationHelper.IsObjectSerializationEnabled)
            {
                return;
            }

            ContinuousIntegrationEventHandling.RemoveBindingsWithOptimalization(objectEventArgs.Object);
        }

        #endregion
    }
}
