using System;
using System.Collections.Generic;

using CMS.ContinuousIntegration.Internal;
using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides methods to simplify code for integration of documents with continuous integration
    /// </summary>
    internal class DocumentContinuousIntegrationHelper
    {
        /// <summary>
        /// Handles the necessary continuous integration actions before and after the bulk update action.
        /// </summary>
        /// <param name="allowContinuousIntegrationActions">If true, continuous integration actions are allowed. If false, only bulk update action runs.</param>
        /// <param name="bulkWhere">Where condition which defines the scope of the bulk action. Where condition identifies the influenced objects before the bulk action occurs.</param>
        /// <param name="bulkUpdate">Bulk update action</param>
        public static void HandleBulkUpdate(bool allowContinuousIntegrationActions, IWhereCondition bulkWhere, Action bulkUpdate)
        {
            var updatedIds = StartBulkUpdate(allowContinuousIntegrationActions, bulkWhere);

            bulkUpdate();

            FinishBulkUpdate(updatedIds);
        }


        /// <summary>
        /// Starts the bulk update action
        /// </summary>
        /// <param name="allowContinuousIntegrationActions">If true, continuous integration actions are allowed. If false, only bulk update action runs.</param>
        /// <param name="bulkWhere">Where condition which defines the scope of the bulk action. Where condition identifies the influenced objects before the bulk action occurs.</param>
        public static ICollection<int> StartBulkUpdate(bool allowContinuousIntegrationActions, IWhereCondition bulkWhere)
        {
            ICollection<int> updatedIds = null;

            // Delete old versions of objects before the action
            if (allowContinuousIntegrationActions)
            {
                updatedIds = RepositoryBulkOperations.DeleteObjects(TreeNode.TYPEINFO, bulkWhere);
            }

            return updatedIds;
        }


        /// <summary>
        /// Finishes the bulk update of documents
        /// </summary>
        /// <param name="updatedIds">Updated document IDs returned from the StartBulkUpdate method</param>
        public static void FinishBulkUpdate(ICollection<int> updatedIds)
        {
            // Store new versions of objects after bulk action
            if (updatedIds != null)
            {
                RepositoryBulkOperations.StoreObjects(
                    TreeNode.TYPEINFO,
                    new WhereCondition().WhereIn(TreeNode.TYPEINFO.IDColumn, updatedIds)
                );
            }
        }
    }
}
