using System.Collections.Generic;

namespace CMS.WebAnalytics.Web.UI.Internal
{
    internal class CampaignConversionItemFilterContainer : ICampaignConversionItemFilterContainer
    {
        private readonly Dictionary<string, ICampaignConversionItemFilter> mFilters = new Dictionary<string, ICampaignConversionItemFilter>();


        public void RegisterFilter(string objectType, ICampaignConversionItemFilter filter)
        {
            mFilters.Add(objectType, filter);
        }


        public ICampaignConversionItemFilter GetFilter(string objectType)
        {
            return GetFilterSafe(objectType);
        }


        private ICampaignConversionItemFilter GetFilterSafe(string objectType)
        {
            return mFilters.ContainsKey(objectType) ? mFilters[objectType] : null;
        }
    }
}