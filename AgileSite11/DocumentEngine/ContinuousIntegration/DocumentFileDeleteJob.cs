using System;
using System.Collections.Generic;
using System.Linq;

using CMS.ContinuousIntegration;
using CMS.ContinuousIntegration.Internal;
using CMS.DataEngine;
using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class designated for removal of serialized TreeNode objects from the file system.
    /// </summary>
    public class DocumentFileDeleteJob : FileSystemDeleteJob
    {
        /// <summary>
        /// Creates a new document file delete job with given repository configuration.
        /// </summary>
        /// <param name="configuration">Repository configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        public DocumentFileDeleteJob(FileSystemRepositoryConfiguration configuration)
            : base(configuration)
        {
        }
        

        /// <summary>
        /// Selects files to be deleted in order to delete given <paramref name="baseInfo"/> from the repository.
        /// </summary>
        /// <param name="baseInfo">Base info which will be deleted.</param>
        /// <returns>Collection of file paths to deleted.</returns>
        /// <remarks>Passed <paramref name="baseInfo"/> is never null.</remarks>
        protected override IEnumerable<string> SelectFilesToDelete(BaseInfo baseInfo)
        {
            var node = (TreeNode)baseInfo;

            var locationCollections = RepositoryPathHelper.GetExistingSerializationFiles(node).ToList();
            RepositoryLocationsCollection documentCollection = null;

            if (!node.IsLink)
            {
                // Get correct document collection
                var documentPartPath = RepositoryPathHelper.GetFilePath(baseInfo, node.DocumentCulture, "document");
                documentCollection = locationCollections.FirstOrDefault(col => col.Contains(documentPartPath));
            }

            var nodeMainPath = RepositoryPathHelper.GetFilePath(node);
            
            // Check if main node data file should be deleted (last localization is present)
            bool deleteMainFile = locationCollections.Count(collection => collection.MainLocations.Contains(nodeMainPath)) == 1;

            var relativePaths = new HashSet<string>();
            if (documentCollection != null)
            {
                if (deleteMainFile)
                {
                    relativePaths.AddRange(documentCollection);
                }
                else
                {
                    var structuredPaths = documentCollection
                        .StructuredLocations
                        .Where(location => !location.MainLocation.EqualsCSafe(nodeMainPath, true))
                        .ToList();

                    // Add document parts paths
                    relativePaths.AddRange(structuredPaths.Select(location => location.MainLocation));

                    // Add separated field's file parts
                    relativePaths.AddRange(structuredPaths.SelectMany(location => location));
                }
            }
            else if (deleteMainFile)
            {
                relativePaths.Add(nodeMainPath);
                relativePaths.AddRange(locationCollections.SelectMany(collection => collection.StructuredLocations)
                                                          .Where(location => location.MainLocation.EqualsCSafe(nodeMainPath, true))
                                                          .SelectMany(location => location));
            }

            // Delete the files
            return relativePaths;
        }
    }
}
