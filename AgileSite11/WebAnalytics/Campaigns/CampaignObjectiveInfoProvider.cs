using System;

using CMS.DataEngine;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Class providing <see cref="CampaignObjectiveInfo"/> management.
    /// </summary>
    public class CampaignObjectiveInfoProvider : AbstractInfoProvider<CampaignObjectiveInfo, CampaignObjectiveInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public CampaignObjectiveInfoProvider()
            : base(CampaignObjectiveInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the <see cref="CampaignObjectiveInfo"/> objects.
        /// </summary>
        public static ObjectQuery<CampaignObjectiveInfo> GetCampaignObjectives()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="CampaignObjectiveInfo"/> with specified ID.
        /// </summary>
        /// <param name="id">CampaignObjectiveInfo ID</param>
        public static CampaignObjectiveInfo GetCampaignObjectiveInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns <see cref="CampaignObjectiveInfo"/> with specified GUID.
        /// </summary>
        /// <param name="guid">CampaignObjectiveInfo GUID</param>                
        public static CampaignObjectiveInfo GetCampaignObjectiveInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="CampaignObjectiveInfo"/>.
        /// </summary>
        /// <param name="infoObj">CampaignObjectiveInfo to be set</param>
        public static void SetCampaignObjectiveInfo(CampaignObjectiveInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="CampaignObjectiveInfo"/>.
        /// </summary>
        /// <param name="infoObj">CampaignObjectiveInfo to be deleted</param>
        public static void DeleteCampaignObjectiveInfo(CampaignObjectiveInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="CampaignObjectiveInfo"/> with specified ID.
        /// </summary>
        /// <param name="id">CampaignObjectiveInfo ID</param>
        public static void DeleteCampaignObjectiveInfo(int id)
        {
            CampaignObjectiveInfo infoObj = GetCampaignObjectiveInfo(id);
            DeleteCampaignObjectiveInfo(infoObj);
        }

        #endregion
    }
}