using System;
using System.Data;

using CMS.DataEngine;

namespace CMS.Membership
{    
    /// <summary>
    /// Class providing ExternalLoginInfo management.
    /// </summary>
    public class ExternalLoginInfoProvider : AbstractInfoProvider<ExternalLoginInfo, ExternalLoginInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public ExternalLoginInfoProvider()
            : base(ExternalLoginInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ExternalLoginInfo objects.
        /// </summary>
        public static ObjectQuery<ExternalLoginInfo> GetExternalLogins()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Sets (updates or inserts) specified ExternalLoginInfo.
        /// </summary>
        /// <param name="infoObj">ExternalLoginInfo to be set</param>
        public static void SetExternalLoginInfo(ExternalLoginInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified ExternalLoginInfo.
        /// </summary>
        /// <param name="infoObj">ExternalLoginInfo to be deleted</param>
        public static void DeleteExternalLoginInfo(ExternalLoginInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }

        #endregion
    }
}