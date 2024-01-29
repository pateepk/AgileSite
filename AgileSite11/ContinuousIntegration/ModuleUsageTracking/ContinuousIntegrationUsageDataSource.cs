using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.ContinuousIntegration;
using CMS.ContinuousIntegration.Internal;

[assembly: RegisterModuleUsageDataSource(typeof(ContinuousIntegrationUsageDataSource))]

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Module usage data for continuous integration.
    /// </summary>
    internal class ContinuousIntegrationUsageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Continuous integration usage data source name.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.ContinuousIntegration";
            }
        }


        /// <summary>
        /// Get Continuous integration usage data.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var result = ObjectFactory<IModuleUsageDataCollection>.New();

            result.Add("CIEnabled", SettingsKeyInfoProvider.GetBoolValue(ContinuousIntegrationHelper.ENABLED_CI_KEY));

            var configFile = GetRepositoryConfigurationFile();
            result.Add("ConfigFileIncludedTypesCount", configFile.IncludedObjectTypes.Count);
            result.Add("ConfigFileExcludedTypesCount", configFile.ExcludedObjectTypes.Count);
            result.Add("ConfigFileExcludedCodeNamesCount", configFile.ExcludedCodeNames.Count);
            
            return result;
        }


        private RepositoryConfigurationFile GetRepositoryConfigurationFile()
        {
            try
            {
                var builder = new FileSystemRepositoryConfigurationBuilder();

                return builder.LoadConfigurationFile();
            }
            catch (RepositoryConfigurationException)
            {
                return new RepositoryConfigurationFile();
            }
        }
    }
}
