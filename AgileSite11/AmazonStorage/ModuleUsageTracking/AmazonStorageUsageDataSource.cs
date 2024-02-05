using System;
using System.Linq;
using System.Text;

using CMS.AmazonStorage;
using CMS.Base;
using CMS.Core;

[assembly: RegisterModuleUsageDataSource(typeof(AmazonStorageUsageDataSource))]

namespace CMS.AmazonStorage
{
    /// <summary>
    /// Module usage data for Amazon storage.
    /// </summary>
    public class AmazonStorageUsageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Amazon storage usage data source name.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.AmazonStorage";
            }
        }


        /// <summary>
        /// Get Amazon storage usage data.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var result = ObjectFactory<IModuleUsageDataCollection>.New();

            // Check if Azure storage is used
            result.Add("IsAmazonStorageUsed", !string.IsNullOrEmpty(SettingsHelper.AppSettings["CMSAmazonBucketName"]));

            // Check if Azure CDN is used
            result.Add("IsUsingAmazonCDN", !string.IsNullOrEmpty(SettingsHelper.AppSettings["CMSAmazonEndPoint"]));

            return result;
        }
    }
}
