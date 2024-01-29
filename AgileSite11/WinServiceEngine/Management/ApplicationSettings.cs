using System.Configuration;

namespace CMS.WinServiceEngine
{

    /// <summary>
    /// Provides application settings from arbitrary configuration file.
    /// </summary>
    internal sealed class ApplicationSettings
    {

        #region "Variables"

        /// <summary>
        /// The configuration file path.
        /// </summary>
        private readonly string mConfigurationFilePath;
        
        
        /// <summary>
        /// The configuration file representation.
        /// </summary>
        private Configuration mApplicationConfiguration;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the configuration file representation.
        /// </summary>
        private Configuration ApplicationConfiguration
        {
            get
            {
                if (mApplicationConfiguration == null)
                {
                    mApplicationConfiguration = GetApplicationConfiguration(mConfigurationFilePath);
                }

                return mApplicationConfiguration;
            }
        }

        
        /// <summary>
        /// Gets the application setting with the specified name.
        /// </summary>
        /// <param name="name">The name of the application setting to locate.</param>
        /// <returns>A string that contains the value associated with the specified name, if found; otherwise, null.</returns>
        public string this[string name]
        {
            get
            {
                var element = ApplicationConfiguration.AppSettings.Settings[name];
                if (element == null)
                {
                    return null;
                }

                return element.Value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the ApplicationSettings class.
        /// </summary>
        /// <param name="configurationFilePath">The configuration file path.</param>
        public ApplicationSettings(string configurationFilePath)
        {
            mConfigurationFilePath = configurationFilePath;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates a representation of the configuration file with the specified path, and returns it.
        /// </summary>
        /// <param name="configurationFilePath">The configuration file path.</param>
        /// <returns>The representation of the configuration file with the specified path.</returns>
        private Configuration GetApplicationConfiguration(string configurationFilePath)
        {
            var configurationFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = configurationFilePath
            };

            return ConfigurationManager.OpenMappedExeConfiguration(configurationFileMap, ConfigurationUserLevel.None);
        }

        #endregion

    }

}