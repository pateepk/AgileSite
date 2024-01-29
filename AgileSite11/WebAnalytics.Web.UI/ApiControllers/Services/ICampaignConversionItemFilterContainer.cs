using CMS.Base.Web.UI;

using CMS;
using CMS.WebAnalytics.Web.UI.Internal;

[assembly: RegisterImplementation(typeof(ICampaignConversionItemFilterContainer), typeof(CampaignConversionItemFilterContainer), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.WebAnalytics.Web.UI.Internal
{
    /// <summary>
    /// Enables to register implementation of <see cref="ICampaignConversionItemFilter"/> to corresponding object type. These filters will be used 
    /// before creating the <see cref="BaseSelectorViewModel"/>.
    /// </summary>
    public interface ICampaignConversionItemFilterContainer
    {
        /// <summary>
        /// Register given <paramref name="filter"/> to the given <paramref name="objectType"/>.
        /// </summary>
        /// <param name="objectType">Object type the <paramref name="filter"/> is registered for</param>
        /// <param name="filter">Filter to be registered</param>
        void RegisterFilter(string objectType, ICampaignConversionItemFilter filter);


        /// <summary>
        /// Gets the previously registered <see cref="ICampaignConversionItemFilter"/>. If no filter is found, returns <c>null</c>.
        /// </summary>
        /// <param name="objectType">Object type the <see cref="ICampaignConversionItemFilter"/> is obtained for</param>
        /// <returns>Gets previously registered filter for given <paramref name="objectType"/></returns>
        ICampaignConversionItemFilter GetFilter(string objectType);
    }
}