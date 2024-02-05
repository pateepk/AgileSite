using CMS;
using CMS.Activities;
using CMS.DataEngine;
using CMS.WebAnalytics.Web.UI;

[assembly: RegisterImplementation(typeof(ICampaignConversionService), typeof(CampaignConversionService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Service that provides method to work with the <see cref="CampaignConversionViewModel"/> objects.
    /// Provides methods for conversion of the <see cref="CampaignConversionInfo"/> objects to <see cref="CampaignConversionViewModel"/>
    /// and for saving the <see cref="CampaignConversionViewModel"/> objects into the database.
    /// </summary>
    internal class CampaignConversionService : ICampaignConversionService
    {
        /// <summary>
        /// Returns view model for given campaign conversion info.
        /// </summary>
        /// <param name="info">Campaign conversion info.</param>
        public CampaignConversionViewModel GetConversionViewModel(CampaignConversionInfo info)
        {
            var activityType = ActivityTypeInfoProvider.GetActivityTypeInfo(info.CampaignConversionActivityType);
            return CreateViewModel(info, activityType);
        }


        /// <summary>
        /// Sets data from campaign conversion model and returns updated view model.
        /// </summary>
        /// <param name="model">Campaign conversion view model.</param>
        public CampaignConversionViewModel SaveConversion(CampaignConversionViewModel model)
        {
            var info = GetInfo(model);
            CampaignConversionInfoProvider.SetCampaignConversionInfo(info);

            return GetConversionViewModel(info);
        }


        /// <summary>
        /// Creates view model composed of campaign conversion and activity type properties.
        /// </summary>
        /// <param name="conversion">Campaign conversion.</param>
        /// <param name="activity">Activity type.</param>
        private CampaignConversionViewModel CreateViewModel(CampaignConversionInfo conversion, ActivityTypeInfo activity)
        {
            var viewModel = new CampaignConversionViewModel
            {
                CampaignID = conversion.CampaignConversionCampaignID,
                ID = conversion.CampaignConversionID,
                Name = conversion.CampaignConversionDisplayName,
                ActivityType = conversion.CampaignConversionActivityType,
                ItemID = conversion.CampaignConversionItemID,
                Order = conversion.CampaignConversionOrder,
                IsFunnelStep = conversion.CampaignConversionIsFunnelStep,
                Url = conversion.CampaignConversionURL
            };

            if (activity != null)
            {
                viewModel.ActivityName = activity.ActivityTypeDisplayName;
            }

            return viewModel;
        }


        /// <summary>
        /// Returns campaign conversion info from view model.
        /// </summary>
        /// <remarks>
        /// If conversion with the same ID as in view model already exists, it's properties are modified according to the view model.
        /// Otherwise new campaign info is created.
        /// </remarks>
        /// <param name="model">Campaign conversion view model sent from client.</param>
        private CampaignConversionInfo GetInfo(CampaignConversionViewModel model)
        {
            var activityType = ActivityTypeInfoProvider.GetActivityTypeInfo(model.ActivityType);
            var info = CampaignConversionInfoProvider.GetCampaignConversionInfo(model.ID) ?? new CampaignConversionInfo();

            info.CampaignConversionActivityType = (activityType != null) ? activityType.ActivityTypeName : string.Empty;
            info.CampaignConversionDisplayName = model.Name;

            info.CampaignConversionItemID = model.ItemID.GetValueOrDefault();
            info.CampaignConversionIsFunnelStep = model.IsFunnelStep;
            info.CampaignConversionURL = model.Url;

            EnsureConversionCampaignID(info, model.CampaignID);
            EnsureConversionName(info);
            EnsureConversionOrder(info);

            return info;
        }


        private void EnsureConversionName(CampaignConversionInfo conversion)
        {
            if (string.IsNullOrEmpty(conversion.CampaignConversionName))
            {
                conversion.CampaignConversionName = InfoHelper.CODENAME_AUTOMATIC;
            }
        }


        private void EnsureConversionOrder(CampaignConversionInfo conversion)
        {
            if (conversion.CampaignConversionOrder == 0)
            {
                conversion.CampaignConversionOrder = conversion.Generalized.GetLastObjectOrder();
            }
        }


        private void EnsureConversionCampaignID(CampaignConversionInfo conversion, int campaignId)
        {
            if (conversion.CampaignConversionCampaignID == 0)
            {
                conversion.CampaignConversionCampaignID = campaignId;
            }
        }
    }
}
