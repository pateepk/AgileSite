using System.Collections.Generic;

using CMS.Base;
using CMS.ContinuousIntegration.Internal;
using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    internal class FileSystemDeleteDocumentsByTypeJob : FileSystemDeleteObjectsByTypeJob
    {
        /// <summary>
        /// Creates a new instance of FileSystemRestoreDocumentsByTypeJob that restores all documents to the database.
        /// </summary>
        /// <param name="configuration">Repository configuration.</param>
        public FileSystemDeleteDocumentsByTypeJob(FileSystemRepositoryConfiguration configuration)
            : base(configuration)
        {
        }


        /// <summary>
        /// Gets collection of all file locations that are used for serialization of given object.
        /// </summary>
        /// <param name="obj">File locations will be returned for given object.</param>
        /// <returns>Collection of all file locations that are used for serialization of given object.</returns>
        protected override RepositoryLocationsCollection GetObjectFilePaths(BaseInfo obj)
        {
            var paths = new List<string>();

            var node = (TreeNode)obj;
            paths.Add(RepositoryPathHelper.GetFilePath(node));

            if (node.IsLink)
            {
                return new RepositoryLocationsCollection(paths);
            }

            paths.Add(RepositoryPathHelper.GetFilePath(node, node.DocumentCulture, "document"));

            if (!node.IsCoupled)
            {
                return new RepositoryLocationsCollection(paths);
            }

            paths.Add(RepositoryPathHelper.GetFilePath(node, node.DocumentCulture, "fields"));

            return new RepositoryLocationsCollection(paths);
        }


        /// <summary>
        /// Gets enumeration of all objects of given type. Only objects that are covered by continuous integration solution are returned.
        /// </summary>
        /// <param name="objectType">Returned objects' type.</param>
        /// <returns>Enumeration of all objects of given type.</returns>
        protected override IEnumerable<BaseInfo> GetObjectsInDatabase(string objectType)
        {
            return new GetDocumentsEnumerator(objectType);
        }


        /// <summary>
        /// Deletes object that is missing in the repository from the database.
        /// </summary>
        /// <param name="obj">Object to delete.</param>
        protected override void DeleteMissingObject(BaseInfo obj)
        {
            // Document engine does not reflect the action context's CreateVersion property
            // Allow to delete the root page completely in context of Continuous integration 
            using (new DocumentActionContext { ForceDestroyHistory = !CMSActionContext.CurrentCreateVersion, AllowRootDeletion = true, LogSynchronization = false })
            {
                ContentStagingTaskCollection.AddTaskForDeleteDocument((TreeNode) obj);

                obj.Delete();
            }
        }
    }
}
