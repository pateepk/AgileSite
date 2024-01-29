using System;
using System.Linq;

using CMS.ContinuousIntegration.Internal;
using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    internal static class ContentStagingTaskCollectionExtensions
    {
        /// <summary>
        /// Adds <see cref="TaskTypeEnum.CreateDocument"/>, <see cref="TaskTypeEnum.UpdateDocument"/> or <see cref="TaskTypeEnum.MoveDocument"/> task 
        /// or both <see cref="TaskTypeEnum.UpdateDocument"/> and <see cref="TaskTypeEnum.MoveDocument"/> tasks
        /// for changed document represented by the <paramref name="node"/> into the collection.
        /// </summary>
        /// <param name="stagingTaskCollection">Staging task collection.</param>
        /// <param name="node">Tree node.</param>
        public static void AddTaskForChangeDocument(this ContentStagingTaskCollection stagingTaskCollection, TreeNode node)
        {
            TaskTypeEnum taskType = GetTaskType(node);

            if (IsNodeMoved(node, taskType))
            {
                stagingTaskCollection.Add(node, TaskTypeEnum.MoveDocument);

                if (HasChangedOnlyNodeParentID(node))
                {
                    // Do not log update task
                    return;
                }
            }

            stagingTaskCollection.Add(node, taskType);
        }


        /// <summary>
        /// Adds <see cref="TaskTypeEnum.DeleteDocument"/> task for document represented by the <paramref name="node"/> into the collection.
        /// </summary>
        /// <param name="stagingTaskCollection">Staging task collection.</param>
        /// <param name="node">Tree node.</param>
        public static void AddTaskForDeleteDocument(this ContentStagingTaskCollection stagingTaskCollection, TreeNode node)
        {
            stagingTaskCollection.Add(node, TaskTypeEnum.DeleteDocument);
        }
        

        private static TaskTypeEnum GetTaskType(TreeNode node)
        {
            // To decide whether the node is new (i.e. is created) or existing (i.e. is updated), we need to look into different fields.
            // For Links - when the link is new, the document already exists, so we need to look at NodeID.
            // For Documents - when NodeID exists, document can be altrough new (e.g. new language version for already existing node), 
            //                 so we need to look at DocumentID.

            var nodeIdentifier = node.IsLink ? node.NodeID : node.DocumentID;

            return nodeIdentifier > 0
                ? TaskTypeEnum.UpdateDocument
                : TaskTypeEnum.CreateDocument;
        }


        private static bool IsNodeMoved(TreeNode node, TaskTypeEnum taskType)
        {
            return (taskType == TaskTypeEnum.UpdateDocument) && node.ItemChanged(nameof(node.NodeParentID));
        }


        private static bool HasChangedOnlyNodeParentID(TreeNode node)
        {
            var changedColumns = node.ChangedColumns();
            
            return (changedColumns.Count == 1) && changedColumns.First().Equals(nameof(node.NodeParentID), StringComparison.OrdinalIgnoreCase);
        }
    }
}
