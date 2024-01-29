using CMS;
using CMS.WebAnalytics.Web.UI;

[assembly: RegisterImplementation(typeof(ICampaignObjectiveService), typeof(CampaignObjectiveService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.WebAnalytics.Web.UI
{
    internal class CampaignObjectiveService : ICampaignObjectiveService
    {
        /// <summary>
        /// Returns view model from specified objective (<paramref name="info"/>).
        /// </summary>
        /// <param name="info">Campaign objective info</param>
        public CampaignObjectiveViewModel GetObjectiveViewModel(CampaignObjectiveInfo info)
        {
            return CreateViewModel(info);
        }


        /// <summary>
        /// Sets data from campaign objective model <paramref name="model"/> and returns updated view model.
        /// </summary>
        /// <param name="model">Campaign objective view model</param>
        public CampaignObjectiveViewModel SaveObjective(CampaignObjectiveViewModel model)
        {
            var info = CampaignObjectiveInfoProvider.GetCampaignObjectiveInfo(model.ID) ?? new CampaignObjectiveInfo();
            info.CampaignObjectiveCampaignID = model.CampaignID;
            info.CampaignObjectiveCampaignConversionID = model.ConversionID;
            info.CampaignObjectiveValue = model.Value;

            CampaignObjectiveInfoProvider.SetCampaignObjectiveInfo(info);

            return CreateViewModel(info);
        }


        /// <summary>
        /// Returns view model from specified objective (<paramref name="info"/>).
        /// </summary>
        /// <param name="info">Campaign objective info</param>
        private static CampaignObjectiveViewModel CreateViewModel(CampaignObjectiveInfo info)
        {
            return new CampaignObjectiveViewModel
            {
                ID = info.CampaignObjectiveID,
                CampaignID = info.CampaignObjectiveCampaignID,
                ConversionID = info.CampaignObjectiveCampaignConversionID,
                Value = info.CampaignObjectiveValue
            };
        }
    }
}
