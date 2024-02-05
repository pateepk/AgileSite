using System.Linq;

using CMS.DataEngine;

namespace CMS.WebAnalytics
{

    /// <summary>
    /// Class providing ExitPageInfo management.
    /// </summary>
    public class ExitPageInfoProvider : AbstractInfoProvider<ExitPageInfo, ExitPageInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ExitPageInfo objects.
        /// </summary>
        public static ObjectQuery<ExitPageInfo> GetExitPages()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns ExitPageInfo with specified session identificator.
        /// </summary>
        /// <param name="sessionIdentificator">Exit page session identificator.</param>
        public static ExitPageInfo GetExitPageInfo(string sessionIdentificator)
        {
            return ProviderObject.GetExitPageInfoInternal(sessionIdentificator);
        }


        /// <summary>
        /// Sets (updates or inserts) specified ExitPageInfo.
        /// </summary>
        /// <param name="infoObj">ExitPageInfo to be set.</param>
        public static void SetExitPageInfo(ExitPageInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified ExitPageInfo.
        /// </summary>
        /// <param name="infoObj">ExitPageInfo to be deleted.</param>
        public static void DeleteExitPageInfo(ExitPageInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes ExitPageInfo with specified name.
        /// </summary>
        /// <param name="name">ExitPageInfo name.</param>
        public static void DeleteExitPageInfo(string name)
        {
            ExitPageInfo infoObj = GetExitPageInfo(name);
            DeleteExitPageInfo(infoObj);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns ExitPageInfo with specified session identificator.
        /// </summary>
        /// <param name="sessionIdentificator">Exit page session identificator.</param>        
        protected virtual ExitPageInfo GetExitPageInfoInternal(string sessionIdentificator)
        {
            return GetObjectQuery().WhereEquals("SessionIdentificator", sessionIdentificator).TopN(1).FirstOrDefault();
        }

        #endregion
    }
}