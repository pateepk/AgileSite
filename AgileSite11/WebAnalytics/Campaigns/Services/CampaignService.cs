using System;

using CMS.Core;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides access to the campaign related to the current request via the implementation of <see cref="ICampaignPersistentStorage"/>.
    /// </summary>
    public class CampaignService : ICampaignService
    {
        private readonly ICampaignPersistentStorage mCampaignPersistentStorage;

        /// <summary>
        /// Creates new instance of <see cref="CampaignService"/>.
        /// </summary>
        /// <param name="campaignPersistentStorage">Provides access to the persistent storage of campaign and campaign source name.</param>
        public CampaignService(ICampaignPersistentStorage campaignPersistentStorage)
        {
            mCampaignPersistentStorage = campaignPersistentStorage;
        }


        /// <summary>
        /// Gets/sets the visitor's campaign from/to a persistent storage.
        /// </summary>
        /// <example>
        /// Following example shows how to use the <see cref="CampaignCode"/> property in the case <see cref="CookieCampaignPersistentStorage"/> is used as implementation of <see cref="ICampaignPersistentStorage"/>.
        /// <code>
        /// 
        /// var campaignService = new CampaignService();
        /// 
        /// ...
        ///  
        /// // for request with cookie Campaign: test_campaign returns test_campaign
        /// // for request without cookies returns null
        /// public string GetCampaignCode() 
        /// {
        ///     return campaignService.CampaignCode;
        /// }
        /// 
        /// </code>
        /// </example>
        public string CampaignCode
        {
            get
            {
                return mCampaignPersistentStorage.CampaignUTMCode;
            }
            private set
            {
                mCampaignPersistentStorage.CampaignUTMCode = value;
            }
        }


        /// <summary>
        /// Gets/sets the visitor's source from/to a cookie.
        /// </summary>
        /// <example>
        /// Following example shows how to use the <see cref="CampaignSourceName"/> property in the case <see cref="CookieCampaignPersistentStorage"/> is used as implementation of <see cref="ICampaignPersistentStorage"/>.
        /// <code>
        /// 
        /// var campaignService = new CampaignService();
        /// 
        /// ...
        ///  
        /// // for request with cookie Source: test_source returns test_source
        /// // for request without cookies returns null
        /// public string GetSourceName() 
        /// {
        ///     return campaignService.CampaignSourceName;
        /// }
        /// 
        /// </code>
        /// </example>
        public string CampaignSourceName
        {
            get
            {
                return mCampaignPersistentStorage.SourceName;
            }
            private set
            {
                mCampaignPersistentStorage.SourceName = value;
            }
        }


        /// <summary>
        /// Gets/sets the visitor's content from/to a cookie.
        /// </summary>
        /// <example>
        /// Following example shows how to use the <see cref="ICampaignService.CampaignContent"/> property in the case <see cref="CookieCampaignPersistentStorage"/> is used as implementation of <see cref="ICampaignPersistentStorage"/>.
        /// <code>
        /// 
        /// var campaignService = new CampaignService();
        /// 
        /// ...
        ///  
        /// // for request with cookie Content: test_content returns test_content
        /// // for request without cookies returns null
        /// public string GetCampaignContent() 
        /// {
        ///     return campaignService.CampaignContent;
        /// }
        /// 
        /// </code>
        /// </example>
        public string CampaignContent
        {
            get
            {
                return mCampaignPersistentStorage.CampaignUTMContent;
            }
            private set
            {
                mCampaignPersistentStorage.CampaignUTMContent = value;
            }
        }


        /// <summary>
        /// If <see cref="CampaignInfo"/> with given <paramref name="campaignCode"/> exists, is running and combination of source and content is valid (when content is not null, source can't be null),
        /// sets <paramref name="campaignCode"/>, <paramref name="source"/> and <paramref name="content"/> to their properties (<see cref="CampaignCode"/>, <see cref="CampaignSourceName"/> or <see cref="CampaignContent"/> respectively).
        /// </summary>
        /// <param name="campaignCode">Name of the campaign to be set</param>
        /// <param name="siteName">Name of the site the <paramref name="campaignCode"/> belongs to</param>
        /// <param name="source">Name of the campaign source</param>
        /// <param name="content">Campaign UTM content</param>
        /// <example>
        /// Following example shows how to use the <see cref="SetCampaign"/> method.
        /// <code>
        /// 
        /// var campaignService = new CampaignService();
        /// 
        /// ...
        /// 
        /// // stores myCampaign to the <see cref="CampaignCode"/>, mySource to the <see cref="CampaignSourceName"/> and myContent to the <see cref="CampaignContent"/>
        /// campaignService.SetCampaign("myCampaign", "mySite", "mySource", "myContent");
        /// 
        /// ...
        /// 
        /// // for all following requests of the same visitor
        /// // returns myCampaign
        /// public string GetCampaignCode() 
        /// {
        ///     return campaignService.CampaignCode;
        /// }
        /// 
        /// 
        /// // returns mySource
        /// public string GetSourceName() 
        /// {
        ///     return campaignService.CampaignSourceName;
        /// }
        /// 
        ///  
        /// // returns myContent
        /// public string GetCampaignContent() 
        /// {
        ///     return campaignService.CampaignSourceName;
        /// }
        /// 
        /// </code>
        /// </example>
        /// <exception cref="ArgumentException"><paramref name="campaignCode"/> is null -or- <paramref name="siteName"/> is null or empty</exception>
        public void SetCampaign(string campaignCode, string siteName, string source = null, string content = null)
        {
            if (string.IsNullOrEmpty(campaignCode))
            {
                throw new ArgumentException("[CampaignService.SetCampaign]: Campaign code cannot be empty", nameof(campaignCode));
            }

            if (string.IsNullOrEmpty(siteName))
            {
                throw new ArgumentException("[CampaignService.SetCampaign]: Site name cannot be empty", nameof(siteName));
            }

            var campaign = GetRunningCampaign(campaignCode, siteName);
            if (campaign == null)
            {
                return;
            }

            UpdateCampaignCookies(campaignCode, source, content);

            if (AnalyticsHelper.JavascriptLoggingEnabled(siteName))
            {
                Service.Resolve<ITrackedCampaignsService>().AddTrackedCampaign(campaign.CampaignUTMCode);
            }
        }


        private static CampaignInfo GetRunningCampaign(string campaignCode, string siteName)
        {
            var campaign = CampaignInfoProvider.GetCampaignByUTMCode(campaignCode, siteName);
            if (campaign == null || !CampaignInfoProvider.CampaignIsRunning(campaign))
            {
                return null;
            }

            return campaign;
        }


        /// <summary>
        /// Updates cookies only if campaign code, source or content has changed.
        /// Sets content cookie to null when source is <c>null</c>.
        /// </summary>
        /// <param name="campaignCode">Campaign UTM code</param>
        /// <param name="source">Campaign UTM source</param>
        /// <param name="content">Campaign UTM content</param>
        private void UpdateCampaignCookies(string campaignCode, string source, string content)
        {
            if (CampaignCode != campaignCode)
            {
                CampaignCode = campaignCode;
            }

            if (CampaignSourceName != source)
            {
                CampaignSourceName = source;
            }

            // Do not use content value when source is not provided
            if (String.IsNullOrEmpty(source) && !String.IsNullOrEmpty(content))
            {
                content = null;
            }

            if (CampaignContent != content)
            {
                CampaignContent = content;
            }
        }
    }
}
