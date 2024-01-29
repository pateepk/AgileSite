using System;
using System.Collections.Generic;

using CMS;
using CMS.WebAnalytics;

[assembly: RegisterImplementation(typeof(ICampaignAssetModelService), typeof(CampaignAssetModelService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Service that provides method to work with the <see cref="CampaignAssetViewModel"/> objects.
    /// </summary>
    public class CampaignAssetModelService : ICampaignAssetModelService
    {
        private readonly Dictionary<string, ICampaignAssetModelStrategy> mRegisteredStrategies = new Dictionary<string, ICampaignAssetModelStrategy>();

        /// <summary>
        /// Returns specific campaign asset model service strategy.
        /// </summary>
        /// <param name="type">Type of asset to work with.</param>
        public ICampaignAssetModelStrategy GetStrategy(string type)
        {
            ICampaignAssetModelStrategy service;
            mRegisteredStrategies.TryGetValue(type, out service);

            if (service == null)
            {
                throw new ArgumentException("Invalid asset type.");
            }

            return service;

        }


        /// <summary>
        /// Registers asset model service strategy <paramref name="strategy"/> under key <paramref name="key"/> .
        /// </summary>
        /// <param name="key">Key name under that the asset service model strategy is stored.</param>
        /// <param name="strategy">Asset model service strategy instance.</param>
        public void RegisterAssetModelStrategy(string key, ICampaignAssetModelStrategy strategy)
        {
            if (!mRegisteredStrategies.ContainsKey(key))
            {
                mRegisteredStrategies.Add(key, strategy);
            }
        }
    }
}