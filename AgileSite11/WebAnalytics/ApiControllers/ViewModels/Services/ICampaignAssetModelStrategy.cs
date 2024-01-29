namespace CMS.WebAnalytics
{
    /// <summary>
    /// Interface that provides service methods to work with the <see cref="CampaignAssetViewModel"/> objects.
    /// </summary>
    public interface ICampaignAssetModelStrategy
    {
        /// <summary>
        /// Returns asset info from view model.
        /// </summary>
        /// <param name="model">Asset view model.</param>
        CampaignAssetInfo GetAssetInfo(CampaignAssetViewModel model);


        /// <summary>
        /// Returns view model from given asset info.
        /// </summary>
        /// <param name="info">Asset info.</param>
        CampaignAssetViewModel GetAssetViewModel(CampaignAssetInfo info);


        /// <summary>
        /// Sets data from asset model and returns updated view model.
        /// This method is called when new asset is added to campaign.
        /// </summary>
        /// <param name="model">Asset view model.</param>
        CampaignAssetViewModel SetAssetInfo(CampaignAssetViewModel model);
    }
}