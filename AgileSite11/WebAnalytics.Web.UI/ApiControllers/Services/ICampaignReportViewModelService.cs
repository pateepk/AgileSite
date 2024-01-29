using CMS;
using CMS.WebAnalytics.Web.UI;

[assembly: RegisterImplementation(typeof(ICampaignReportViewModelService), typeof(CampaignReportViewModelService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Service providing <see cref="CampaignReportViewModel"/> for campaign.
    /// </summary>
    public interface ICampaignReportViewModelService
    {
        /// <summary>
        /// Provides view model for campaign report.
        /// </summary>
        /// <param name="campaign">Campaign to create view model for.</param>
        CampaignReportViewModel GetViewModel(CampaignInfo campaign);
    }
}
