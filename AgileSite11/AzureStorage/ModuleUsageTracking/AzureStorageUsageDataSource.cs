using CMS.AzureStorage;
using CMS.Base;
using CMS.Core;
using CMS.Helpers;

[assembly: RegisterModuleUsageDataSource(typeof(AzureStorageUsageDataSource))]

namespace CMS.AzureStorage
{
    /// <summary>
    /// Module usage data for Azure storage.
    /// </summary>
    public class AzureStorageUsageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Identifies Windows Application Gallery Kentico package.
        /// </summary>
        private const string WWAG_KEY = "CMSWWAGInstallation";

        /// <summary>
        /// Azure storage usage data source name.
        /// </summary>
        public string Name => "CMS.AzureStorage";


        /// <summary>
        /// Get Azure storage usage data.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var result = ObjectFactory<IModuleUsageDataCollection>.New();

            // Check if Azure storage is used
            result.Add("IsAzureStorageUsed", !string.IsNullOrEmpty(SettingsHelper.AppSettings["CMSAzureAccountName"]));

            // Check if Azure CDN is used
            result.Add("IsUsingAzureCDN", !string.IsNullOrEmpty(SettingsHelper.AppSettings["CMSAzureCDNEndpoint"]));

            // Check if is running on Azure
            result.Add("IsRunningOnAzure", SystemContext.IsRunningOnAzure);

            // Check if is running as Windows application gallery package
            result.Add("IsRunningAsWindowsApplicationGallerySite", ValidationHelper.GetBoolean(SettingsHelper.AppSettings[WWAG_KEY], false));

            return result;
        }
    }
}
