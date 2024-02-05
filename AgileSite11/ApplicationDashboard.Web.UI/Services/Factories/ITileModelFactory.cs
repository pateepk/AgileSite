using System;
using System.Linq;
using System.Text;

using CMS.ApplicationDashboard.Web.UI.Internal;

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Provides method for creating dashboard tile.
    /// </summary>
    internal interface ITileModelFactory
    {
        /// <summary>
        /// Creates instance of tile model for given <paramref name="dashboardItem"/>.
        /// </summary>
        /// <param name="setting">User dashboard setting for the given <paramref name="dashboardItem"/></param>
        /// <param name="dashboardItem">UI elements for which the tile is created</param>
        /// <returns>Instance of tile model</returns>
        ITileModel CreateTileModel(UserDashboardSetting setting, DashboardItem dashboardItem);


        /// <summary>
        /// Creates instance of empty tile model for given <paramref name="dashboardItem"/>. This object
        /// is required in order to maintain order-ability of the dashboard. Dashboard item created with this
        /// method should remain hidden, since they does not contain any information besides the one
        /// needed for saving it to the user settings.
        /// </summary>
        /// <param name="setting">User dashboard setting for the given <paramref name="dashboardItem"/></param>
        /// <param name="dashboardItem">UI elements for which the tile is created</param>
        /// <returns>Instance of tile model</returns>
        ITileModel CreateEmptyTileModel(UserDashboardSetting setting, DashboardItem dashboardItem);
    }
}
