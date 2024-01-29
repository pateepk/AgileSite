using CMS.ContinuousIntegration.Internal;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Loader for Continuous Integration repository configuration file.
    /// </summary>
    internal interface IRepositoryConfigurationLoader
    {
        /// <summary>
        /// Loads configuration file for the repository.
        /// </summary>
        /// <param name="path">Path to the configuration file.</param>
        /// <returns>Configuration file loaded from given <paramref name="path"/> or new instance of <see cref="RepositoryConfigurationFile"/> if configuration is not defined.</returns>
        RepositoryConfigurationFile Load(string path);
    }
}