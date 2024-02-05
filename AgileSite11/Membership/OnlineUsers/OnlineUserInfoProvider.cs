using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.Membership
{
    internal class OnlineUserInfoProvider : AbstractInfoProvider<OnlineUserInfo, OnlineUserInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all online users.
        /// </summary>   
        internal static ObjectQuery<OnlineUserInfo> GetOnlineUsers()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Updates the data in the database based on the given values and where condition.
        /// </summary>
        internal static void UpdateData(IWhereCondition where, IDictionary<string, object> values)
        {
            ProviderObject.UpdateData(where, values, false);
        }

        #endregion
    }
}
