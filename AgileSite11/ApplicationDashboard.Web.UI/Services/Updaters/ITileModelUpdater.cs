using CMS.ApplicationDashboard.Web.UI.Internal;
using CMS.Membership;

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Provides method for retrieving updated dashboard tile.
    /// </summary>
    internal interface ITileModelUpdater
    {
        /// <summary>
        /// Creates instance of updated tile model for given <paramref name="userDashboardSetting"/>.
        /// </summary>
        /// <param name="userDashboardSetting">User settings for which the updated tile is retrieved for</param>
        /// <param name="user">User the model is updated for</param>
        /// <returns>Instance of updated tile model</returns>
        ITileModel GetTileModel(UserDashboardSetting userDashboardSetting, UserInfo user);
    }
}