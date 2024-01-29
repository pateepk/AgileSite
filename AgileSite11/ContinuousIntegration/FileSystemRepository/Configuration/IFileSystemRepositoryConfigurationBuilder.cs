namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Builder for file system repository configuration.
    /// </summary>
    internal interface IFileSystemRepositoryConfigurationBuilder
    {
        /// <summary>
        /// Builds file system repository configuration.
        /// </summary>
        /// <returns>File system repository configuration.</returns>
        FileSystemRepositoryConfiguration Build();
    }
}