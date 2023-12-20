using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Synchronization;

using Newtonsoft.Json;

[assembly: RegisterModuleUsageDataSource(typeof(VersioningUsageDataSource))]

namespace CMS.Synchronization
{
    /// <summary>
    /// Module usage tracking data source for staging.
    /// </summary>
    internal class VersioningUsageDataSource : IModuleUsageDataSource
    {
        protected const string VERSIONING_SETTING_KEY_PREFIX = "CMSEnableVersioning";

        /// <summary>
        /// Staging data source name.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.Versioning";
            }
        }


        /// <summary>
        /// Get module usage data.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var result = ObjectFactory<IModuleUsageDataCollection>.New();

            // Get information whether versioning is enabled
            if (!SettingsKeyInfoProvider.GetBoolValue("CMSEnableObjectsVersioning"))
            {
                result.Add("VersioningEnabled", false);
                return result;
            }

            // Get list of setting keys regarding versioning
            var versioningSettingKeys = SettingsKeyInfoProvider.GetSettingsKeys()
                .WhereStartsWith("KeyName", VERSIONING_SETTING_KEY_PREFIX)
                .WhereNull("SiteID")
                .WhereEquals("KeyValue", "True");

            if (!versioningSettingKeys.TypedResult.Any())
            {
                result.Add("VersioningEnabled", false);
                return result;
            }

            result.Add("VersioningEnabled", true);
            result.Add("VersionedObjects", JsonConvert.SerializeObject(versioningSettingKeys.TypedResult.Select(s => s.KeyName.Substring(VERSIONING_SETTING_KEY_PREFIX.Length)), new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeHtml }));

            return result;
        }
    }
}
