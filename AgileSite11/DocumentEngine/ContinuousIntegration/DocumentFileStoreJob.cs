using System;

using CMS.ContinuousIntegration.Internal;
using CMS.DataEngine;
using CMS.DocumentEngine.Internal;

namespace CMS.DocumentEngine
{
    internal class DocumentFileStoreJob : FileSystemStoreJob
    {
        /// <summary>
        /// Creates a new file system store job with given repository configuration.
        /// </summary>
        /// <param name="configuration">Repository configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        public DocumentFileStoreJob(FileSystemRepositoryConfiguration configuration)
            : base(configuration)
        {
        }


        /// <summary>
        /// Stores given <paramref name="baseInfo"/> to the repository by serializing it to proper repository location.
        /// </summary>
        /// <param name="baseInfo">Base info which will be stored.</param>
        /// <remarks>Passed <paramref name="baseInfo"/> is never null.</remarks>
        protected override void RunInternal(BaseInfo baseInfo)
        {
            var source = (TreeNode)baseInfo;
            var node = source;

            // Store node data
            var nodeInfo = node.NodeData;
            var nodePath = RepositoryPathHelper.GetFilePath(node);
            StoreBaseInfo(nodeInfo, nodePath);

            // Culture and coupled data need to be stored in context of the original node to use correct paths
            if (source.IsLink)
            {
                node = TreeNode.New(source.ClassName);
                node.NodeData = DocumentNodeDataInfoProvider.GetDocumentNodeDataInfo(source.OriginalNodeID);
                node.CultureData = source.CultureData;
                if (source.IsCoupled)
                {
                    node.CoupledData = source.CoupledData;
                }
            }

            // Store document data
            var documentInfo = node.CultureData;
            var documentPath = RepositoryPathHelper.GetFilePath(node, node.DocumentCulture, "document");
            StoreBaseInfo(documentInfo, documentPath);

            // There is no coupled data for container type of document
            if (!source.IsCoupled)
            {
                return;
            }

            // Store coupled data
            var coupledDataInfo = node.CoupledData;
            var coupledDataPath = RepositoryPathHelper.GetFilePath(node, node.DocumentCulture, "fields");
            StoreBaseInfo(coupledDataInfo, coupledDataPath);
        }
    }
}
