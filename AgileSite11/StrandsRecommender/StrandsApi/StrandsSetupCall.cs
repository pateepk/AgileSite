using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.StrandsRecommender
{
    /// <summary>
    /// Loads settings related with automatic catalog upload, creates call data object and performs setup call to Strands Recommender.
    /// </summary>
    public class StrandsSetupCall
    {
        #region "Public methods"

        /// <summary>
        /// Updates Strands Recommender settings with automatic setup call. Uses values provided in settings or the default, hard-coded ones.
        /// </summary>
        public void DoWithDefaults()
        {
            string currentSiteName = SiteContext.CurrentSiteName;

            if (!StrandsSettings.IsStrandsEnabled(currentSiteName))
            {
                throw new StrandsException("[StrandsSetupCall.DoWithDefaults]: Strands Recommender is not enabled.");
            }

            string cultureSuffix = SettingsKeyInfoProvider.GetValue(currentSiteName + ".CMSDefaultCultureCode").Replace("-", "").ToLowerInvariant();

            var data = CreateDefaultSettingsData(cultureSuffix);

            data.ValidationToken = StrandsSettings.GetValidationToken(currentSiteName);
            data.FeedActive = StrandsSettings.IsAutomaticCatalogUploadEnabled(currentSiteName);
            data.FeedFrequency = StrandsSettings.GetAutomaticCatalogUploadFrequency(currentSiteName);
            data.CatalogFeedUsername = StrandsSettings.GetCatalogFeedUsername(currentSiteName);
            data.CatalogFeedPassword = StrandsSettings.GetCatalogFeedPassword(currentSiteName);

            var apiClient = new StrandsApiClient(StrandsSettings.GetApiID(currentSiteName));

            try
            {
                var task = apiClient.DoSetupCallAsync(data);

                task.ContinueWith(CMSThread.Wrap<Task<StrandsSetupCallStatusCodeEnum>>(LogSetupCallResult));
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("Strands Recommender", "SETUPCALL", ex, additionalMessage: "[StrandsSetupCall.DoWithDefaults]: Could not synchronize settings with Strands server. Try saving Strands settings in Site Manager again or set it manually on Strands server.");
            }
        }


        /// <summary>
        /// Logs the setup call result
        /// </summary>
        /// <param name="task">Setup call task</param>
        private static void LogSetupCallResult(Task<StrandsSetupCallStatusCodeEnum> task)
        {
            try
            {
                LogSetupCallResult(task.Result);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("Strands Recommender", "SETUPCALL", ex, additionalMessage: "[StrandsSetupCall.DoWithDefaults]: Could not synchronize settings with Strands server. Try saving Strands settings in Site Manager again or set it manually on Strands server.");
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Creates new instance of StrandsSetupCallData and fills it with default values (the ones, that cannot be changed in settings).
        /// </summary>
        /// <param name="cultureSuffix">Culture suffix used in definition of field mappings</param>
        /// <returns>Instance of StrandsSetupCallData filled with predefined values</returns>
        private StrandsSetupCallData CreateDefaultSettingsData(string cultureSuffix)
        {
            return new StrandsSetupCallData
            {
                Type = "kentico",
                Version = "1.0",
                Tracking = new List<string>
                {
                    "visited",
                    "purchased",
                    "addshoppingcart",
                    "addtofavorites",
                    "addwishlist",
                    "searched"
                },
                Fields = new Dictionary<string, string>
                {
                    {"id", "id"},
                    {"category", "category"},
                    {"image_link", "image_link"},
                    {"price", "price"},
                    {"title", "title_" + cultureSuffix},
                    {"link", "url_" + cultureSuffix},
                    {"description", "description_" + cultureSuffix}
                },
                FeedUrl = URLHelper.GetAbsoluteUrl("~/CMSModules/StrandsRecommender/CMSPages/StrandsCatalogFeed.ashx"),
                Pagination = 2000 // Generating feed with 2000 products per page performs the best as shown by testing
            };
        }


        /// <summary>
        /// Handles setup call response and perform info/exception logging.
        /// </summary>
        /// <param name="response">Response given by Strands Recommender after setup call is performed</param>
        private static void LogSetupCallResult(StrandsSetupCallStatusCodeEnum response)
        {
            string type = EventType.ERROR;
            string eventMessage = response.ToStringRepresentation();

            if (response == StrandsSetupCallStatusCodeEnum.Success)
            {
                type = EventType.INFORMATION;
                eventMessage = "Automatic setup call was successfully performed.";
            }

            EventLogProvider.LogEvent(type, "Strands Recommender", "SETUPCALL", eventMessage);
        } 

        #endregion
    }
}