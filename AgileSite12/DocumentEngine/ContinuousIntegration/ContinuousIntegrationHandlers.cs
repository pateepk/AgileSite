using CMS.Base;
using CMS.ContinuousIntegration.Internal;
using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    internal class ContinuousIntegrationHandlers
    {
        /// <summary>
        /// Initializes the events handlers.
        /// </summary>
        public static void Init()
        {
            // Include type-specific documents to continuous integration
            RepositoryConfigurationEvaluator.AddObjectTypeTransformation(DocumentHelper.DOCUMENT_PREFIX, _ => TreeNode.OBJECT_TYPE);

            // Register custom job to compose TreeNode from partial classes
            FileSystemStoreJobFactory.RegisterJob(PredefinedObjectType.DOCUMENT, config => new DocumentFileStoreJob(config));
            FileSystemUpsertObjectsByTypeJobFactory.RegisterJob(PredefinedObjectType.DOCUMENT, config => new FileSystemUpsertDocumentsByTypeJob(config));
            FileSystemDeleteObjectsByTypeJobFactory.RegisterJob(PredefinedObjectType.DOCUMENT, config => new FileSystemDeleteDocumentsByTypeJob(config));

            // Register custom job to correct removing TreeNode on the file system
            FileSystemDeleteJobFactory.RegisterJob(PredefinedObjectType.DOCUMENT, config => new DocumentFileDeleteJob(config));

            // Register enumerator to serve all pages
            DatabaseObjectsEnumeratorFactory.RegisterObjectEnumerator(PredefinedObjectType.DOCUMENT, (objectType, where) => new GetDocumentsEnumerator(objectType, where));

            // Register custom field processor for ACLs
            CustomProcessorFactory.RegisterProcessor(AclInfo.OBJECT_TYPE, job => new ACLCustomProcessor(job));

            // Register custom processor for documents
            CustomProcessorFactory.RegisterProcessor(DocumentNodeDataInfo.OBJECT_TYPE, job => new DocumentCustomProcessor());
            CustomProcessorFactory.RegisterProcessor(DocumentCultureDataInfo.OBJECT_TYPE, job => new DocumentCustomProcessor());

            DocumentEvents.Insert.After += SerializeDocumentOnInsert;
            DocumentEvents.UpdateInner.Before += SerializeDocumentOnUpdate;
            DocumentEvents.InsertNewCulture.Before += SerializeDocumentOnTranslation;
            DocumentEvents.Delete.After += SerializeDocumentOnDelete;
            DocumentEvents.InsertLink.After += SerializeDocumentOnInsertLink;
            DocumentEvents.ChangeToLink.Before += SerializeDocumentOnChangeToLink;
            DocumentEvents.ChangeDocumentType.Before += SerializeDocumentOnTypeChange;
            DocumentTypeInfo.TYPEINFODOCUMENTTYPE.Events.Update.Before += SerializeDocumentsOnFormDefinitionChange;

            ContentStagingTaskCollection.TasksCollected.Execute += LogDocumentChangesOnLogTasksCollected;
        }


        #region "Handler methods"

        private static void LogDocumentChangesOnLogTasksCollected(object sender, CMSEventArgs<TasksCollectedEventArgs> e)
        {
            new ContentStagingTaskLogger().LogDocumentChanges(e.Parameter.Tasks);
        }


        private static void SerializeDocumentOnTranslation(object sender, DocumentEventArgs e)
        {
            var node = e.Node;

            // Inserting new culture version may change the node alias, in that case we must refresh all culture versions of the document in CI repository
            if (node.ItemChanged("NodeAlias"))
            {
                // Wrap the update into the bulk action which deletes and logs all culture version data. Child documents are handled by the underlying API calls of update.
                var ids = DocumentContinuousIntegrationHelper.StartBulkUpdate(true, TreeProvider.GetCultureVersionsWhereCondition(node.NodeID));

                e.CallWhenFinished(() => DocumentContinuousIntegrationHelper.FinishBulkUpdate(ids));
            }

            e.CallWhenFinished(() => ContinuousIntegrationEventHandling.BaseInfoInsertAfter(node));
        }


        private static void SerializeDocumentOnInsertLink(object sender, DocumentEventArgs e)
        {
            var link = e.Node;
            if (!link.IsInPublishStep)
            {
                // Make sure published data is serialized to the repository
                var node = GetLinkWithPublishedData(link);
                ContinuousIntegrationEventHandling.BaseInfoInsertAfter(node);
            }
            else
            {
                ContinuousIntegrationEventHandling.BaseInfoInsertAfter(link);
            }
        }


        private static TreeNode GetLinkWithPublishedData(TreeNode link)
        {
            var node = TreeNode.New(link.ClassName);
            node.NodeData = link.NodeData;
            node.CultureData = DocumentCultureDataInfoProvider.GetDocumentCultureInfo(link.DocumentID);
            if (link.IsCoupled)
            {
                node.CoupledData = DocumentFieldsInfoProvider.GetDocumentFieldsInfo(link.DocumentForeignKeyValue, link.ClassName);
            }
            return node;
        }


        private static void SerializeDocumentOnInsert(object sender, DocumentEventArgs e)
        {
            ContinuousIntegrationEventHandling.BaseInfoInsertAfter(e.Node);
        }


        private static void SerializeDocumentOnUpdate(object sender, DocumentEventArgs e)
        {
            var node = e.Node;

            // If site contains more language versions, we need to handle all language versions of the document in case parent or path changes
            if (CultureSiteInfoProvider.IsSiteMultilingual(node.NodeSiteName) &&
                (node.ItemChanged("NodeParentID") || node.ItemChanged("NodeAlias")))
            {
                // Wrap the update into the bulk action which deletes and logs all culture version data. Child documents are handled by the underlying API calls of update.
                var ids = DocumentContinuousIntegrationHelper.StartBulkUpdate(true, TreeProvider.GetCultureVersionsWhereCondition(node.NodeID));

                e.CallWhenFinished(() => DocumentContinuousIntegrationHelper.FinishBulkUpdate(ids));
            }
            else
            {
                // When only single culture exists, handle just as single object
                ContinuousIntegrationEventHandling.BaseInfoUpdateBefore(node, e);
            }
        }


        private static void SerializeDocumentOnDelete(object sender, DocumentEventArgs e)
        {
            ContinuousIntegrationEventHandling.BaseInfoDeleteAfter(e.Node);
        }


        private static void SerializeDocumentOnChangeToLink(object sender, DocumentEventArgs e)
        {
            var node = e.Node;

            var bulkWhere = TreeProvider.GetCultureVersionsWhereCondition(node.NodeID);
            if (!node.IsLink)
            {
                // Delete related CI repository data
                RepositoryBulkOperations.DeleteObjects(TreeNode.TYPEINFO, bulkWhere);

                // Disable further CI actions until the end of the bulk update
                var ac = new CMSActionContext { ContinuousIntegrationAllowObjectSerialization = false };

                // Restore new CI repository data for link, only node data remains
                e.CallWhenFinished(() =>
                {
                    // Enable CI actions again
                    ac.Dispose();
                    ac = null;

                    RepositoryBulkOperations.StoreObjects(TreeNode.TYPEINFO, bulkWhere.WhereEquals("DocumentCulture", node.DocumentCulture));
                });

                e.CallOnDispose(() =>
                {
                    // Make sure to finalize the context in case of error
                    if (ac != null)
                    {
                        ac.Dispose();
                    }
                });
            }
        }


        private static void SerializeDocumentOnTypeChange(object sender, DocumentEventArgs e)
        {
            var node = e.Node;

            // Refresh related CI repository data
            var bulkWhere = TreeProvider.GetCultureVersionsWhereCondition(node.NodeID);
            var updatedIds = DocumentContinuousIntegrationHelper.StartBulkUpdate(true, bulkWhere);

            // Disable further CI actions until the end of the bulk update
            var ac = new CMSActionContext
            {
                ContinuousIntegrationAllowObjectSerialization = false
            };

            e.CallWhenFinished(() =>
            {
                // Enable CI actions again
                ac.Dispose();
                ac = null;

                // Restore new CI repository data with the new document type
                DocumentContinuousIntegrationHelper.FinishBulkUpdate(updatedIds);
            });

            e.CallOnDispose(() =>
            {
                // Make sure to finalize the context in case of error
                if (ac != null)
                {
                    ac.Dispose();
                }
            });
        }


        private static void SerializeDocumentsOnFormDefinitionChange(object sender, ObjectEventArgs e)
        {
            DocumentTypeInfo info = (DocumentTypeInfo)e.Object;

            // If form definition is changed, we need to refresh all pages based on this page type on file system
            if (!ContinuousIntegrationEventHandling.RequiresFormDefinitionChangeRepositoryRefresh(info))
            {
                return;
            }

            // Refresh related CI repository data
            var bulkWhere = new WhereCondition().WhereEquals("NodeClassID", info.ClassID);
            var updatedIds = DocumentContinuousIntegrationHelper.StartBulkUpdate(true, bulkWhere);

            e.CallWhenFinished(() =>
            {
                // Restore new CI repository data with the new document type
                DocumentContinuousIntegrationHelper.FinishBulkUpdate(updatedIds);
            });
        }

        #endregion
    }
}
