using System;
using System.Xml.Serialization;

using CMS.ContinuousIntegration.Internal;
using CMS.IO;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Loads the repository configuration stored in physical file location.
    /// </summary>
    internal class RepositoryConfigurationFileLoader : IRepositoryConfigurationLoader
    {
        /// <summary>
        /// Loads configuration file for the repository.
        /// </summary>
        /// <param name="path">Path to the configuration file.</param>
        /// <returns>Configuration file loaded from given <paramref name="path"/>, null if given path does not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is null.</exception>
        public RepositoryConfigurationFile Load(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!File.Exists(path))
            {
                return new RepositoryConfigurationFile();
            }

            using (var sr = StreamReader.New(path))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(RepositoryConfigurationFile));

                return serializer.Deserialize(sr) as RepositoryConfigurationFile;
            }
        }
    }
}