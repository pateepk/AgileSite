using System;

using CMS.ApplicationDashboard.Web.UI.Internal;
using CMS.Helpers;
using CMS.Modules;
using CMS.PortalEngine;
using CMS.UIControls;

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Provides method for creating single application tile.
    /// </summary>
    internal class ApplicationTileModelFactory : ITileModelFactory
    {
        private readonly ITileIconModelProvider mTileIconModelProvider;
        
        /// <summary>
        /// Creates new instance of <see cref="ApplicationTileModelFactory"/>.
        /// </summary>
        /// <param name="tileIconModelProvider">Provides method for retrieving icon model for dashboard tiles</param>
        /// <exception cref="ArgumentNullException"><paramref name="tileIconModelProvider"/> is null</exception>
        public ApplicationTileModelFactory(ITileIconModelProvider tileIconModelProvider)
        {
            if (tileIconModelProvider == null)
            {
                throw new ArgumentNullException("tileIconModelProvider");
            }

            mTileIconModelProvider = tileIconModelProvider;
        }


        /// <summary>
        /// Creates instance of tile model for given <paramref name="dashboardItem"/>.
        /// </summary>
        /// <param name="setting">User dashboard setting for the given <paramref name="dashboardItem"/></param>
        /// <param name="dashboardItem">UI elements for which the tile is created</param>
        /// <exception cref="ArgumentNullException"><paramref name="setting"/> is null -or- <paramref name="dashboardItem"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="dashboardItem"/> has to have <see cref="DashboardItem.Application"/> property set</exception>
        /// <returns>Instance of tile model</returns>
        public ITileModel CreateTileModel(UserDashboardSetting setting, DashboardItem dashboardItem)
        {
            if (setting == null)
            {
                throw new ArgumentNullException("setting");
            }
            if (dashboardItem == null)
            {
                throw new ArgumentNullException("dashboardItem");
            }

            var application = dashboardItem.Application;
            if (application == null)
            {
                throw new ArgumentException("[ApplicationTileModelFactory.CreateTileModel]: Application in dashboard item cannot be null");
            }

            string path = URLHelper.ResolveUrl(
                ApplicationUrlHelper.GetApplicationUrl(ApplicationUrlHelper.GetResourceName(application.ElementResourceID), application.ElementName)
            );

            return new ApplicationTileModel
            {
                DisplayName = application.ElementDisplayName,
                Path = path,
                ListItemCssClass = ApplicationCSSHelper.GetApplicationIconCssClass(application.ElementGUID),
                TileIcon = mTileIconModelProvider.CreateTileIconModel(application),
                ApplicationGuid = application.ElementGUID,
                IsVisible = dashboardItem.IsVisible
            };
        }


        /// <summary>
        /// Creates instance of empty tile model for given <paramref name="dashboardItem"/>. This object
        /// is required in order to maintain order-ability of the dashboard. Dashboard item created with this
        /// method should remain hidden, since they does not contain any information besides the one
        /// needed for saving it to the user settings.
        /// </summary>
        /// <param name="setting">User dashboard setting for the given <paramref name="dashboardItem"/></param>
        /// <param name="dashboardItem">UI elements for which the tile is created</param>
        /// <returns>Instance of tile model</returns>
        public ITileModel CreateEmptyTileModel(UserDashboardSetting setting, DashboardItem dashboardItem)
        {
            if (setting == null)
            {
                throw new ArgumentNullException("setting");
            }

            if (setting.ApplicationGuid == null)
            {
                throw new ArgumentException("[ApplicationTileModelFactory.CreateEmptyTileModel]: Application in dashboard item cannot be null");
            }
            
            return new ApplicationTileModel
            {
                ApplicationGuid = setting.ApplicationGuid,
                IsVisible = false
            };
        }
    }
}