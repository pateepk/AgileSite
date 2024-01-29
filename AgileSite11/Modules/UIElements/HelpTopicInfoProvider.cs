using System;
using System.Data;

using CMS.DataEngine;

namespace CMS.Modules
{
    /// <summary>
    /// Class providing HelpTopicInfo management.
    /// </summary>
    public class HelpTopicInfoProvider : AbstractInfoProvider<HelpTopicInfo, HelpTopicInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the HelpTopicInfo objects.
        /// </summary>
        public static ObjectQuery<HelpTopicInfo> GetHelpTopics()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns HelpTopicInfo with specified ID.
        /// </summary>
        /// <param name="id">HelpTopicInfo ID</param>
        public static HelpTopicInfo GetHelpTopicInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified HelpTopicInfo.
        /// </summary>
        /// <param name="infoObj">HelpTopicInfo to be set</param>
        public static void SetHelpTopicInfo(HelpTopicInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified HelpTopicInfo.
        /// </summary>
        /// <param name="infoObj">HelpTopicInfo to be deleted</param>
        public static void DeleteHelpTopicInfo(HelpTopicInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes HelpTopicInfo with specified ID.
        /// </summary>
        /// <param name="id">HelpTopicInfo ID</param>
        public static void DeleteHelpTopicInfo(int id)
        {
            HelpTopicInfo infoObj = GetHelpTopicInfo(id);
            DeleteHelpTopicInfo(infoObj);
        }

        #endregion
    }
}