using System;

using CMS;
using CMS.Helpers;
using CMS.WebAnalytics.Internal;
using CMS.WebAnalytics.Web.UI.Internal;
using CMS.WebAnalytics.Web.UI;

[assembly: RegisterImplementation(typeof(ICampaignListItemViewModelService), typeof(CampaignListItemViewModelService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Service that provides method to fill all the properties in <see cref="CampaignListItemViewModel"/> objects.
    /// </summary>
    internal class CampaignListItemViewModelService : ICampaignListItemViewModelService
    {
        private readonly ICampaignLinkService mCampaignLinkService;
        private readonly ICampaignStatisticsService mStatisticsService;


        /// <summary>
        /// Instantiates new instance of <see cref="CampaignListItemViewModelService"/>.
        /// </summary>
        /// <param name="campaignLinkService">Provides methods for obtaining link to single campaign object</param>
        /// <param name="statisticsService">Provides methods for obtaining campaign statistics</param>
        public CampaignListItemViewModelService(ICampaignLinkService campaignLinkService, ICampaignStatisticsService statisticsService)
        {
            mCampaignLinkService = campaignLinkService;
            mStatisticsService = statisticsService;
        }


        /// <summary>
        /// Method takes <see cref="CampaignInfo"/> and creates new instance of <see cref="CampaignListItemViewModel"/> containing all values
        /// required for communication between Web API and javascript modules.
        /// </summary>
        /// <param name="campaign">Campaign info obtained from the database</param>
        /// <param name="dateTime">Datetime, used to calculate the current status of campaign</param>
        /// <exception cref="ArgumentNullException"><paramref name="campaign"/> is null</exception>
        public CampaignListItemViewModel GetModel(CampaignInfo campaign, DateTime dateTime)
        {
            if (campaign == null)
            {
                throw new ArgumentNullException("campaign");
            }

            var model = new CampaignListItemViewModel();
            var status = campaign.GetCampaignStatus(dateTime);

            model.CampaignID = campaign.CampaignID;
            model.DisplayName = campaign.CampaignDisplayName;
            model.Status = status.ToString();

            if (campaign.CampaignOpenFrom != DateTimeHelper.ZERO_TIME)
            {
                model.OpenFrom = campaign.CampaignOpenFrom;

                var now = dateTime.Date;
                var campaignStart = campaign.CampaignOpenFrom.Date;
                model.DaysToStart = campaignStart.Subtract(now).Days;
            }

            if (campaign.CampaignOpenTo != DateTimeHelper.ZERO_TIME)
            {
                model.OpenTo = campaign.CampaignOpenTo;
            }

            model.DetailLink = mCampaignLinkService.GetCampaignLink(campaign);

            if ((status == CampaignStatusEnum.Running) || (status == CampaignStatusEnum.Finished))
            {
                ComputeStatistics(campaign, model);
            }

            return model;
        }


        private void ComputeStatistics(CampaignInfo campaign, CampaignListItemViewModel viewModel)
        {
            var objectiveStatistics = mStatisticsService.GetObjectiveStatistics(campaign.CampaignID);

            viewModel.Conversions = mStatisticsService.ComputeConversionsCount(campaign.CampaignID);
            viewModel.Objective = (objectiveStatistics != null) ? (decimal?)objectiveStatistics.ResultPercent : null;
            viewModel.Visitors = campaign.CampaignVisitors;
        }
    }
}
