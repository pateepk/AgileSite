using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.ContinuousIntegration.Internal;
using CMS.Synchronization;

namespace CMS.DocumentEngine
{
    internal sealed class ContentStagingTaskLogger
    {
        /// <summary>
        /// Node IDs affected by related objects (like ACL)
        /// </summary>
        private readonly HashSet<int> nodeIds = new HashSet<int>();


        /// <summary>
        /// Document IDs affected by related objects (like ACL)
        /// </summary>
        private readonly HashSet<int> documentIds = new HashSet<int>();


        /// <summary>
        /// Filters and logs given <paramref name="tasksToLog"/> for staging.
        /// </summary>
        /// <param name="tasksToLog">Staging tasks.</param>
        public void LogDocumentChanges(IReadOnlyList<StagingTask> tasksToLog)
        {
            foreach (var processedTask in tasksToLog)
            {
                var info = processedTask.Object;

                if (HasLogSynchronizationSet(info))
                {
                    continue;
                }

                var node = info as TreeNode;
                if (node != null)
                {
                    LogDocumentChange(node, processedTask.TaskType);
                    continue;
                }

                StoreNodeIdOrDocumentIdForTreeNodeChildObjects(info);
                StoreNodeIdsAffectedByAclChange(info);
            }

            LogDocumentChangesForAffectedTreeNodes();
        }


        private static bool HasLogSynchronizationSet(BaseInfo info)
        {
            return info.TypeInfo.SynchronizationSettings.LogSynchronization == SynchronizationTypeEnum.LogSynchronization;
        }


        private void StoreNodeIdsAffectedByAclChange(BaseInfo info)
        {
            switch (info.TypeInfo.ObjectType)
            {
                case AclInfo.OBJECT_TYPE:
                case AclItemInfo.OBJECT_TYPE:
                    var nodeAclId = info.GetIntegerValue("ACLID", 0);
                    var nodeOwner = DocumentNodeDataInfoProvider.GetDocumentNodes()
                        .WhereEquals("NodeACLID", nodeAclId)
                        .WhereTrue("NodeIsACLOwner")
                        .Column("NodeID")
                        .GetScalarResult<int>();

                    nodeIds.Add(nodeOwner);
                    break;

                default:
                    break;
            }
        }


        private void StoreNodeIdOrDocumentIdForTreeNodeChildObjects(BaseInfo info)
        {
            // Check if object's parent is part of TreeNode
            switch (info.TypeInfo.ParentObjectType)
            {
                case DocumentNodeDataInfo.OBJECT_TYPE:
                    nodeIds.Add(info.Generalized.ObjectParentID);
                    break;

                case DocumentCultureDataInfo.OBJECT_TYPE:
                    documentIds.Add(info.Generalized.ObjectParentID);
                    break;

                default:
                    // Do nothing
                    break;
            }
        }


        private void LogDocumentChangesForAffectedTreeNodes()
        {
            if (nodeIds.Any() || documentIds.Any())
            {
                var pageTypes = DataClassInfoProvider.GetClasses().Where(x => x.ClassIsDocumentType).Select(x => x.ClassName);

                foreach(var pageType in pageTypes)
                {
                    var nodes = new DocumentQuery(pageType)
                        .WhereIn("NodeID", nodeIds).Or().WhereIn("DocumentID", documentIds)
                        .OrderByAscending("NodeLevel", "NodeOrder");

                    nodes.ForEachObject(node => LogDocumentChange(node, TaskTypeEnum.UpdateDocument));
                }
            }
        }

        
        private static void LogDocumentChange(TreeNode node, TaskTypeEnum taskType)
        {
            DocumentSynchronizationHelper.LogDocumentChange(node, taskType, node.TreeProvider, SynchronizationInfoProvider.ENABLED_SERVERS, null, false);
        }
    }
}
