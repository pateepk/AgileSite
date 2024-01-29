namespace CMS.WebAnalytics
{
    /// <summary>
    /// Service that provides method to work with the <see cref="CampaignAssetViewModel"/> objects.
    /// </summary>
    public interface ICampaignAssetModelService
    {
        /// <summary>
        /// Returns specific campaign asset model service strategy.
        /// </summary>
        /// <param name="type">Type of asset to work with.</param>
        ICampaignAssetModelStrategy GetStrategy(string type);


        /// <summary>
        /// Registers asset model service strategy <paramref name="strategy"/> under key <paramref name="key"/> .
        /// </summary>
        /// <param name="key">Key name under that the asset service model strategy is stored.</param>
        /// <param name="strategy">Asset model service strategy instance.</param>
        void RegisterAssetModelStrategy(string key, ICampaignAssetModelStrategy strategy);
    }
}
