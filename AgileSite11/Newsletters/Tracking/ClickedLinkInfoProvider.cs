using System;
using System.Data;

using CMS.DataEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class providing ClickedLinkInfo management.
    /// </summary>
    public class ClickedLinkInfoProvider : AbstractInfoProvider<ClickedLinkInfo, ClickedLinkInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public ClickedLinkInfoProvider()
            : base(ClickedLinkInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ClickedLinkInfo objects.
        /// </summary>
        public static ObjectQuery<ClickedLinkInfo> GetClickedLinks()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns ClickedLinkInfo with specified ID.
        /// </summary>
        /// <param name="id">ClickedLinkInfo ID</param>
        public static ClickedLinkInfo GetClickedLinkInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns ClickedLinkInfo with specified GUID.
        /// </summary>
        /// <param name="guid">ClickedLinkInfo GUID</param>                
        public static ClickedLinkInfo GetClickedLinkInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified ClickedLinkInfo.
        /// </summary>
        /// <param name="infoObj">ClickedLinkInfo to be set</param>
        public static void SetClickedLinkInfo(ClickedLinkInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified ClickedLinkInfo.
        /// </summary>
        /// <param name="infoObj">ClickedLinkInfo to be deleted</param>
        public static void DeleteClickedLinkInfo(ClickedLinkInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes ClickedLinkInfo with specified ID.
        /// </summary>
        /// <param name="id">ClickedLinkInfo ID</param>
        public static void DeleteClickedLinkInfo(int id)
        {
            ClickedLinkInfo infoObj = GetClickedLinkInfo(id);
            DeleteClickedLinkInfo(infoObj);
        }

        #endregion
    }
}