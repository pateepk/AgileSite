using System;
using System.Collections.Generic;
using System.Linq;

using CMS.ContinuousIntegration.Internal;
using CMS.DataEngine;
using CMS.DataEngine.Serialization;
using CMS.DocumentEngine.Internal;

namespace CMS.DocumentEngine
{
    internal class FileSystemUpsertDocumentsByTypeJob : FileSystemUpsertObjectsByTypeJob
    {
        /// <summary>
        /// Set of skipped mappings for DocumentNodeID to ensure correct creation of nodes
        /// </summary>
        private readonly HashSet<string> mSkippedMappings = new HashSet<string>();


        /// <summary>
        /// Creates a new instance of FileSystemRestoreDocumentsByTypeJob that restores all documents to the database.
        /// </summary>
        /// <param name="configuration">Repository configuration.</param>
        public FileSystemUpsertDocumentsByTypeJob(FileSystemRepositoryConfiguration configuration)
            : base(configuration)
        {
        }


        /// <summary>
        /// Composes multiple partial sub-objects into one or more main objects.
        /// </summary>
        /// <param name="deserializedObjects">Deserialized objects indexed by their location.</param>
        /// <returns>Compound object or objects.</returns>
        protected override IEnumerable<DeserializationResult> GetMainObjects(IDictionary<string, IEnumerable<DeserializationResult>> deserializedObjects)
        {
            DocumentNodeDataInfo nodeData = null;
            DocumentCultureDataInfo cultureData = null;
            DocumentFieldsInfo coupledData = null;

            // Configure provider
            var provider = new TreeProvider
            {
                // Do not generate new GUID for culture versions
                GenerateNewGuid = false,

                // Synchronize time stamps and user information
                UpdateUser = false,
                UpdateTimeStamps = false,

                // Keep data exactly in the format as in file system
                EnsureSafeNodeAlias = false,
                AutomaticallyUpdateDocumentAlias = false,
                CheckUniqueNames = false,
                HandleACLs = false,
                UseAutomaticOrdering = false,
                UpdateNodeName = false,

                // CI is unable to perform move to other site as move, it does delete + insert instead, so page reference to original may change during this operation
                CheckLinkConsistency = false,

                // Do not generate document aliases, they will be handled once they are supported by CI
                EnableDocumentAliases = false,

                // Do not update URL path, take the value as it is
                UpdateUrlPath = false
            };

            var node = GetNewNodeInstance(provider);

            var result = new DeserializationResult(node);

            foreach (var objects in deserializedObjects)
            {
                var partialResult = objects.Value.First();

                if (partialResult.DeserializedInfo is DocumentCultureDataInfo)
                {
                    cultureData = (DocumentCultureDataInfo)partialResult.DeserializedInfo;
                }
                else if (partialResult.DeserializedInfo is DocumentFieldsInfo)
                {
                    coupledData = (DocumentFieldsInfo)partialResult.DeserializedInfo;
                }
                else if (partialResult.DeserializedInfo is DocumentNodeDataInfo)
                {
                    nodeData = (DocumentNodeDataInfo)partialResult.DeserializedInfo;
                }
                else
                {
                    throw new Exception("[FileSystemRestoreDocumentsByTypeJob.ComposeObjects]: Unknown deserialization data.");
                }

                // Collect failed fields
                foreach (var failedFields in partialResult.FailedFields)
                {
                    if (!result.FailedFields.Contains(failedFields))
                    {
                        result.FailedFields.Add(failedFields);
                    }
                }

                // Collect failed mappings
                foreach (var failedMappings in partialResult.FailedMappings)
                {
                    if (!result.FailedMappings.ContainsField(failedMappings.FieldName))
                    {
                        result.FailedMappings.Add(failedMappings);
                    }
                }
            }

            // Compose result from partial classes
            ComposeResult(nodeData, cultureData, coupledData, result);

            // Validate failed mappings
            ValidateFailedMappings(result);

            return new List<DeserializationResult> { result };
        }


        private static TreeNode GetNewNodeInstance(TreeProvider provider)
        {
            var node = TreeNode.New(null, provider);

            // Use data loader to be able to load missing data for link documents
            node.DataLoader = new ContinuousIntegrationComponentsDataLoader(node);

            return node;
        }


        /// <summary>
        /// Sets changed object to the database.
        /// </summary>
        /// <param name="baseInfo">Changed object</param>
        protected override void SetChangedObject(BaseInfo baseInfo)
        {
            var node = (TreeNode)baseInfo;

            ContentStagingTaskCollection.AddTaskForChangeDocument(node);

            node.SetDocumentInternal(false);

            ReplaceTranslationRecord(node.NodeData);
            ReplaceTranslationRecord(node.CultureData);
            if (node.IsCoupled)
            {
                ReplaceTranslationRecord(node.CoupledData);
            }
        }


        /// <summary>
        /// Validates failed mappings and ensure correction of the mappings
        /// </summary>
        /// <param name="result">Deserialized result</param>
        private void ValidateFailedMappings(DeserializationResult result)
        {
            // Skip only first failed translation of DocumentNodeID field
            var failedMapping = result.FailedMappings.SelectField("DocumentNodeID").FirstOrDefault();
            if (failedMapping == null)
            {
                // DocumentNodeID translation succeeded
                return;
            }

            var failedMappingKey = failedMapping.TranslationReference.ToString();
            if (mSkippedMappings.Add(failedMappingKey))
            {
                // When the DocumentNodeID field is processed for the first time, skip the failed field
                // since tree node is being inserted and the ID is not needed yet
                result.FailedMappings.Remove(failedMapping);
            }
        }


        /// <summary>
        /// Composes result from partial classes
        /// </summary>
        /// <param name="nodeData">Node data</param>
        /// <param name="cultureData">Culture data</param>
        /// <param name="coupledData">Coupled fields data</param>
        /// <param name="result">Deserialized result with tree node instance to compose</param>
        private static void ComposeResult(DocumentNodeDataInfo nodeData, DocumentCultureDataInfo cultureData, DocumentFieldsInfo coupledData, DeserializationResult result)
        {
            if (nodeData == null)
            {
                throw new ArgumentNullException(nameof(nodeData), "Could not compose document from deserialized parts. The node data part is missing.");
            }

            var treeNode = (TreeNode)result.DeserializedInfo;
            treeNode.NodeData = nodeData;

            // Propagate translated class ID from partial class
            if (!result.FailedMappings.ContainsField("NodeClassID"))
            {
                treeNode.SetValue("NodeClassID", nodeData.NodeClassID);
            }

            if (nodeData.IsLink || result.FailedMappings.ContainsField("NodeLinkedNodeID"))
            {
                return;
            }

            if (cultureData == null)
            {
                throw new ArgumentNullException(nameof(cultureData), "Could not compose document from deserialized parts. The culture data part is missing.");
            }
            treeNode.CultureData = cultureData;

            if (treeNode.IsCoupled && (coupledData == null))
            {
                throw new ArgumentNullException(nameof(coupledData), "Could not compose document from deserialized parts. The coupled data part is missing.");
            }
            treeNode.CoupledData = coupledData;
        }
    }
}
