using System;

namespace CMS.WebAnalytics.Web.UI.Internal
{
    /// <summary>
    /// Service that provides method to work with the <see cref="CampaignListItemViewModel"/> objects.
    /// </summary>
    public interface ICampaignListItemViewModelService
    {
        /// <summary>
        /// Method takes <see cref="CampaignInfo"/> and creates new instance of <see cref="CampaignListItemViewModel"/> containing all values
        /// required for communication between Web API and javascript modules.
        /// </summary>
        /// <param name="campaign">Campaign info obtained from the database</param>
        /// <param name="dateTime">Datetime, used to calculate the current status of campaign</param>
        /// <exception cref="ArgumentNullException"><paramref name="campaign"/> is null</exception>
        CampaignListItemViewModel GetModel(CampaignInfo campaign, DateTime dateTime);
    }
}