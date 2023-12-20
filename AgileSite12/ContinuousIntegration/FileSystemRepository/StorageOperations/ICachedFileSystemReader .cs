using System.Collections.Generic;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Describes data reading access in repository with caching support.
    /// </summary>
    internal interface ICachedFileSystemReader : IFileSystemReader
    {
        /// <summary>
        /// Removes cached content of each path in <paramref name="relativePaths"/> collection
        /// from internal caches and also from <see cref="RepositoryHashManager"/>.
        /// </summary>
        /// <param name="relativePaths">Collection of relative path of the file.</param>
        /// <remarks>Only non-empty and not-null paths are processed.</remarks>
        void RemoveFromCache(IEnumerable<string> relativePaths);
    }
}
