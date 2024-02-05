using System.Linq;

using CMS.Base;
using CMS.Core;

using CMS.ModuleLicenses;

[assembly: RegisterModuleUsageDataSource(typeof(ModuleLicensesUsageDataSource))]

namespace CMS.ModuleLicenses
{
    /// <summary>
    /// Data source for providing statistical information about Module licenses.
    /// </summary>
    internal class ModuleLicensesUsageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Get the data source name.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.ModuleLicenses";
            }
        }


        /// <summary>
        /// Returns data collection with count of licensed modules and count of all module license keys of all modules.
        /// </summary>
        /// <returns>Collected data</returns>
        public IModuleUsageDataCollection GetData()
        {
            var result = ObjectFactory<IModuleUsageDataCollection>.New();

            var query = ModuleLicenseKeyInfoProvider.GetModuleLicenseKeys().Column("ModuleLicenseKeyResourceID");
            var resourceIDs = query.GetListResult<int>();

            if (resourceIDs.Count <= 0)
            {
                return result;
            }
            
            result.Add("AllLicenseKeysCount", resourceIDs.Count);
            result.Add("LicensedModulesCount", resourceIDs.Distinct().Count());
            return result;
        }
    }
}
