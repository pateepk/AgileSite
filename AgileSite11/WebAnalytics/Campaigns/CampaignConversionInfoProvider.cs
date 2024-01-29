using CMS.DataEngine;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Class providing CampaignConversionInfo management.
    /// </summary>
    public class CampaignConversionInfoProvider : AbstractInfoProvider<CampaignConversionInfo, CampaignConversionInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public CampaignConversionInfoProvider()
            : base(CampaignConversionInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                UseWeakReferences = true
            })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the CampaignConversionInfo objects.
        /// </summary>
        public static ObjectQuery<CampaignConversionInfo> GetCampaignConversions()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns CampaignConversionInfo with specified ID.
        /// </summary>
        /// <param name="id">CampaignConversionInfo ID</param>
        public static CampaignConversionInfo GetCampaignConversionInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified CampaignConversionInfo.
        /// </summary>
        /// <param name="infoObj">CampaignConversionInfo to be set</param>
        public static void SetCampaignConversionInfo(CampaignConversionInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified CampaignConversionInfo.
        /// </summary>
        /// <param name="infoObj">CampaignConversionInfo to be deleted</param>
        public static void DeleteCampaignConversionInfo(CampaignConversionInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes CampaignConversionInfo with specified ID.
        /// </summary>
        /// <param name="id">CampaignConversionInfo ID</param>
        public static void DeleteCampaignConversionInfo(int id)
        {
            var infoObj = GetCampaignConversionInfo(id);
            DeleteCampaignConversionInfo(infoObj);
        }

        #endregion
    }
}
