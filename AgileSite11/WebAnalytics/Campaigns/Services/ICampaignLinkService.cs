using CMS;
using CMS.WebAnalytics;
using CMS.WebAnalytics.Internal;

[assembly: RegisterImplementation(typeof(ICampaignLinkService), typeof(CampaignLinkService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.WebAnalytics.Internal
{
    /// <summary>
    /// Provides methods for obtaining link to single campaign object.
    /// </summary>
    public interface ICampaignLinkService
    {
        /// <summary>
        /// Gets link leading to the given <paramref name="campaign"/>. Takes the <paramref name="campaign"/> status into account
        /// and changes the target tab accordingly.
        /// </summary>
        /// <param name="campaign">Campaign object the link is being obtained for</param>
        /// <returns>Link leading to the given <paramref name="campaign"/></returns>
        string GetCampaignLink(CampaignInfo campaign);
    }
}