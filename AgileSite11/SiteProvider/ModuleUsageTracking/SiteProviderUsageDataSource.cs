using CMS.Base;
using CMS.Core;
using CMS.SiteProvider;

[assembly: RegisterModuleUsageDataSource(typeof(SiteProviderUsageDataSource))]

namespace CMS.SiteProvider
{
    /// <summary>
    /// Module usage data for site provider.
    /// </summary>
    internal class SiteProviderUsageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Site provider data source name.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.SiteProvider";
            }
        }


        /// <summary>
        /// Get site provider usage data.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var result = ObjectFactory<IModuleUsageDataCollection>.New();

            // Get count of content only sites
            result.Add("SiteIsContentOnlyCount", SiteInfoProvider.GetSites().WhereTrue("SiteIsContentOnly")
                .Count);

            // Get count of sites using presentation URL
            result.Add("SiteUsingPresentationUrlCount", SiteInfoProvider.GetSites().WhereNotEmpty("SitePresentationURL")
                .Count);

            return result;
        }
    }
}
