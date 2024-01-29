using CMS.Base;
using CMS.Core;
using CMS.LicenseProvider;

[assembly: RegisterModuleUsageDataSource(typeof(LicenseUsageDataSource))]

namespace CMS.LicenseProvider
{
    /// <summary>
    /// Class for retrieving statistical information about current web application instance license.
    /// </summary>
    internal class LicenseUsageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Get the name of the license data source.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.LicenseProvider";
            }
        }


        /// <summary>
        /// Get information about current web application instance license.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var result = ObjectFactory<IModuleUsageDataCollection>.New();

            var bestLicense = LicenseKeyInfoProvider.GetBestLicense();

            // Handle missing license
            var edition = (bestLicense == null) ? "None" : bestLicense.Edition.ToString();

            result.Add("HighestLicenseEdition", edition);

            return result;
        }
    }
}
