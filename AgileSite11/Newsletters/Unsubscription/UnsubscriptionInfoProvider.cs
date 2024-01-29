using System;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class providing access to unsubscriptions.
    /// If false, having null in the newsletter ID column means unsubscribed from all newsletters.
    /// </summary>
    public class UnsubscriptionInfoProvider : AbstractInfoProvider<UnsubscriptionInfo, UnsubscriptionInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public UnsubscriptionInfoProvider()
            : base(UnsubscriptionInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the UnsubscriptionInfo objects.
        /// </summary>
        public static ObjectQuery<UnsubscriptionInfo> GetUnsubscriptions()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns UnsubscriptionInfo with specified ID.
        /// </summary>
        /// <param name="id">UnsubscriptionInfo ID</param>
        public static UnsubscriptionInfo GetUnsubscriptionInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns UnsubscriptionInfo with specified GUID.
        /// </summary>
        /// <param name="guid">UnsubscriptionInfo GUID</param>                
        public static UnsubscriptionInfo GetUnsubscriptionInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified UnsubscriptionInfo.
        /// </summary>
        /// <param name="infoObj">UnsubscriptionInfo to be set</param>
        public static void SetUnsubscriptionInfo(UnsubscriptionInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified UnsubscriptionInfo.
        /// </summary>
        /// <param name="infoObj">UnsubscriptionInfo to be deleted</param>
        public static void DeleteUnsubscriptionInfo(UnsubscriptionInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes UnsubscriptionInfo with specified ID.
        /// </summary>
        /// <param name="id">UnsubscriptionInfo ID</param>
        public static void DeleteUnsubscriptionInfo(int id)
        {
            UnsubscriptionInfo infoObj = GetUnsubscriptionInfo(id);
            DeleteUnsubscriptionInfo(infoObj);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(UnsubscriptionInfo info)
        {
            if ((info.UnsubscriptionID == 0) && (info.UnsubscriptionCreated == DateTimeHelper.ZERO_TIME))
            {
                info.UnsubscriptionCreated = DateTime.Now;
            }

            base.SetInfo(info);
        }

        #endregion
    }
}