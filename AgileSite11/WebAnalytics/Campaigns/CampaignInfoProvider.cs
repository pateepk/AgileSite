using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Class providing CampaignInfo management.
    /// </summary>
    public class CampaignInfoProvider : AbstractInfoProvider<CampaignInfo, CampaignInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the CampaignInfo objects.
        /// </summary>
        public static ObjectQuery<CampaignInfo> GetCampaigns()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns campaign with specified ID.
        /// </summary>
        /// <param name="campaignId">Campaign ID</param>        
        public static CampaignInfo GetCampaignInfo(int campaignId)
        {
            return ProviderObject.GetInfoById(campaignId);
        }


        /// <summary>
        /// Returns campaign with specified name.
        /// </summary>
        /// <param name="campaignName">Campaign name</param>                
        /// <param name="siteName">Site name</param>                
        public static CampaignInfo GetCampaignInfo(string campaignName, string siteName)
        {
            return ProviderObject.GetInfoByCodeName(campaignName, SiteInfoProvider.GetSiteID(siteName));
        }


        /// <summary>
        /// Sets (updates or inserts) specified campaign.
        /// </summary>
        /// <param name="campaignObj">Campaign to be set</param>
        public static void SetCampaignInfo(CampaignInfo campaignObj)
        {
            ProviderObject.SetInfo(campaignObj);
        }


        /// <summary>
        /// Deletes specified campaign.
        /// </summary>
        /// <param name="campaignObj">Campaign to be deleted</param>
        public static void DeleteCampaignInfo(CampaignInfo campaignObj)
        {
            ProviderObject.DeleteInfo(campaignObj);
        }


        /// <summary>
        /// Deletes campaign with specified ID.
        /// </summary>
        /// <param name="campaignId">Campaign ID</param>
        public static void DeleteCampaignInfo(int campaignId)
        {
            CampaignInfo campaignObj = GetCampaignInfo(campaignId);
            DeleteCampaignInfo(campaignObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns true if the campaign is valid for current date time.
        /// </summary>
        /// <param name="campaignName">Campaign name</param>
        /// <param name="siteName">Site name</param>
        public static bool CampaignIsRunning(string campaignName, string siteName)
        {
            return ProviderObject.CampaignIsRunningInternal(campaignName, siteName);
        }


        /// <summary>
        /// Returns true if the campaign is valid for current date time.
        /// </summary>
        /// <param name="campaignInfo">Campaign info object</param>
        public static bool CampaignIsRunning(CampaignInfo campaignInfo)
        {
            return ProviderObject.CampaignIsRunningInternal(campaignInfo);
        }


        /// <summary>
        /// Returns campaign with given UTM code.
        /// </summary>
        /// <param name="utmCode">UTM code.</param>
        /// <param name="siteName">Site name.</param>
        public static CampaignInfo GetCampaignByUTMCode(string utmCode, string siteName)
        {
            return ProviderObject.GetCampaignByUTMCodeInternal(utmCode, siteName);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(CampaignInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            base.DeleteInfo(info);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns true if the campaign is valid for current date time.
        /// </summary>
        /// <param name="campaignName">Campaign name</param>
        /// <param name="siteName">Site name</param>
        protected virtual bool CampaignIsRunningInternal(string campaignName, string siteName)
        {
            CampaignInfo ci = GetCampaignInfo(campaignName, siteName);
            return CampaignIsRunningInternal(ci);
        }


        /// <summary>
        /// Returns true if the campaign is valid for current date time.
        /// </summary>
        /// <param name="campaignInfo">Campaign info object</param>
        protected virtual bool CampaignIsRunningInternal(CampaignInfo campaignInfo)
        {
            return campaignInfo.GetCampaignStatus(DateTime.Now) == CampaignStatusEnum.Running;
        }


        /// <summary>
        /// Returns campaign with given UTM code.
        /// </summary>
        /// <param name="utmCode">UTM code.</param>
        /// <param name="siteName">Site name.</param>
        protected virtual CampaignInfo GetCampaignByUTMCodeInternal(string utmCode, string siteName)
        {
            if (string.IsNullOrEmpty(utmCode))
            {
                return null;
            }

            return CacheHelper.Cache(cs =>
                {
                    var campaign = GetCampaigns().OnSite(siteName)
                                                 .WhereEquals("CampaignUTMCode", utmCode)
                                                 .TopN(1)
                                                 .FirstObject;

                    string dependencyKey;
                    if (campaign == null)
                    {
                        dependencyKey = CampaignInfo.OBJECT_TYPE + "|all";
                    }
                    else
                    {
                        dependencyKey = CampaignInfo.OBJECT_TYPE + "|byid|" + campaign.CampaignID;
                    }
                    cs.CacheDependency = CacheHelper.GetCacheDependency(dependencyKey);

                    return campaign;
                },
                new CacheSettings(10, "GetCampaignByUTMCodeResult", utmCode, siteName)
            );
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor using ID and codename Hashtables.
        /// </summary>
        public CampaignInfoProvider() : base(CampaignInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
					Name = true,
					Load = LoadHashtableEnum.All
				})
        {
        }

        #endregion
    }
}