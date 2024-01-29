using CMS;
using CMS.WebAnalytics;

[assembly: RegisterImplementation(typeof(ICampaignService), typeof(CampaignService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides access to the campaign related to the current request via the implementation of <see cref="ICampaignPersistentStorage"/>.
    /// </summary>
    public interface ICampaignService
    {
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
        string CampaignCode
        {
            get;
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
        string CampaignSourceName
        {
            get;
        }


        /// <summary>
        /// Gets/sets the visitor's content from/to a cookie.
        /// </summary>
        /// <example>
        /// Following example shows how to use the <see cref="CampaignContent"/> property in the case <see cref="CookieCampaignPersistentStorage"/> is used as implementation of <see cref="ICampaignPersistentStorage"/>.
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
        string CampaignContent
        {
            get;
        }


        /// <summary>
        /// If <see cref="CampaignInfo"/> with given <paramref name="campaignCode"/> exists and is running, sets <paramref name="campaignCode"/>, <paramref name="source"/> and <paramref name="content"/> to their 
        /// properties (<see cref="CampaignCode"/>, <see cref="CampaignSourceName"/> or <see cref="CampaignContent"/> respectively).
        /// </summary>
        /// <remarks>
        /// If Javascript logging is enabled, hit logging will be performed asynchronously on the following request.
        /// </remarks>
        /// <param name="campaignCode">Campaign UTM code</param>
        /// <param name="siteName">Name of the site the campaign belongs to</param>
        /// <param name="source">Name of the campaign UTM source</param>
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
        void SetCampaign(string campaignCode, string siteName, string source = null, string content = null);
    }
}