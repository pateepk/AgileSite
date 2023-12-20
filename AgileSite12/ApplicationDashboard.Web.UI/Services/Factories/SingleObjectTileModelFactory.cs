using System;
using System.Linq;
using System.Text;

using CMS.ApplicationDashboard.Web.UI.Internal;
using CMS.Base;
using CMS.Helpers;
using CMS.Membership;
using CMS.Modules;
using CMS.PortalEngine;
using CMS.PortalEngine.Internal;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Provides method for creating single object tile.
    /// </summary>
    internal class SingleObjectTileModelFactory : ITileModelFactory
    {
        private readonly IUIElementObjectPropertiesProvider mUIElementObjectPropertiesProvider;
        private readonly ITileIconModelProvider mTileIconModelProvider;
        private readonly IUserSpecificDashboardItemsLoader mUserSpecificDashboardItemsLoader;
        private readonly IUILinkProvider mUILinkProvider;
        private readonly ISiteService mSiteService;
        
        /// <summary>
        /// Creates new instance of <see cref="SingleObjectTileModelFactory"/>.
        /// </summary>
        /// <param name="uiElementObjectPropertiesProvider">Service providing method for obtaining display name for given <see cref="UIElementInfo"/></param>
        /// <param name="tileIconModelProvider">Service providing method for retrieving icon model for dashboard tiles</param>
        /// <param name="userSpecificDashboardItemsLoader">Handles loading of applications from the user settings</param>
        /// <param name="uiLinkProvider">Provides methods for generating links to access single objects within the module (e.g. single Site)</param>
        /// <param name="siteService">Service for loading context of the current site</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="uiElementObjectPropertiesProvider"/> is null -or-
        /// <paramref name="tileIconModelProvider"/> is null -or-
        /// <paramref name="userSpecificDashboardItemsLoader"/> is null -or-
        /// <paramref name="uiLinkProvider"/> is null
        /// </exception>
        public SingleObjectTileModelFactory(
            IUIElementObjectPropertiesProvider uiElementObjectPropertiesProvider, 
            ITileIconModelProvider tileIconModelProvider,
            IUserSpecificDashboardItemsLoader userSpecificDashboardItemsLoader,
            IUILinkProvider uiLinkProvider, 
            ISiteService siteService)
        {
            if (uiElementObjectPropertiesProvider == null)
            {
                throw new ArgumentNullException("uiElementObjectPropertiesProvider");
            }

            if (tileIconModelProvider == null)
            {
                throw new ArgumentNullException("tileIconModelProvider");
            }

            if (userSpecificDashboardItemsLoader == null)
            {
                throw new ArgumentNullException("userSpecificDashboardItemsLoader");
            }

            if (uiLinkProvider == null)
            {
                throw new ArgumentNullException("uiLinkProvider");
            }

            if (siteService == null)
            {
                throw new ArgumentNullException("siteService");
            }

            mUIElementObjectPropertiesProvider = uiElementObjectPropertiesProvider;
            mTileIconModelProvider = tileIconModelProvider;
            mUserSpecificDashboardItemsLoader = userSpecificDashboardItemsLoader;
            mUILinkProvider = uiLinkProvider;
            mSiteService = siteService;
        }


        /// <summary>
        /// Creates instance of single object tile model for given <paramref name="dashboardItem"/>.
        /// </summary>
        /// <param name="setting">User dashboard setting for the given <paramref name="dashboardItem"/></param>
        /// <param name="dashboardItem">UI elements for which the tile is created</param>
        /// <exception cref="ArgumentNullException"><paramref name="setting"/> is null -or- <paramref name="dashboardItem"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="setting"/> has to have <see cref="UserDashboardSetting.ElementGuid"/> and <see cref="UserDashboardSetting.ObjectName"/> properties set -and-
        /// <paramref name="dashboardItem"/> has to have <see cref="DashboardItem.Application"/> and <see cref="DashboardItem.SingleObject"/> properties set</exception>
        /// <returns>Instance of single object tile model</returns>
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

            if (setting.ElementGuid == null)
            {
                throw new ArgumentException("[SingleObjectTileModelFactory.CreateTileModel]: ElementGuid in setting cannot be null");
            }

            if (setting.ObjectName == null)
            {
                throw new ArgumentException("[SingleObjectTileModelFactory.CreateTileModel]: ObjectName in setting cannot be null");
            }

            var application = dashboardItem.Application;
            var singleObject = dashboardItem.SingleObject;

            if (application == null)
            {
                throw new ArgumentException("[SingleObjectTileModelFactory.CreateTileModel]: Application in dashboard item cannot be null");
            }
            if (singleObject == null)
            {
                throw new ArgumentException("[SingleObjectTileModelFactory.CreateTileModel]: SingleObject in dashboard item cannot be null");
            }

            var siteID = SiteInfoProvider.GetSiteID(setting.ObjectSiteName);
            var displayName = mUIElementObjectPropertiesProvider.GetDisplayName(setting.ObjectName, siteID, singleObject);
            if (string.IsNullOrEmpty(displayName))
            {
                mUserSpecificDashboardItemsLoader.RemoveSpecificUserDashboardSetting(MembershipContext.AuthenticatedUser, setting);
                return null;
            }

            return new SingleObjectTileModel
            {
                ApplicationDisplayName = application.ElementDisplayName,
                Path = URLHelper.GetAbsoluteUrl(mUILinkProvider.GetSingleObjectLink(singleObject, new ObjectDetailLinkParameters
                {
                    AllowNavigationToListing = true,
                    ObjectIdentifier = setting.ObjectName
                })),
                ListItemCssClass = ApplicationCSSHelper.GetApplicationIconCssClass(application.ElementGUID),
                TileIcon = mTileIconModelProvider.CreateTileIconModel(application),
                ApplicationGuid = setting.ApplicationGuid,
                ObjectDisplayName = displayName,
                UIElementGuid = setting.ElementGuid.Value,
                ObjectName = setting.ObjectName,
                ObjectSiteName = setting.ObjectSiteName,
                IsVisible = (setting.ObjectSiteName == null) || (mSiteService.CurrentSite.SiteName == setting.ObjectSiteName)
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

            if (dashboardItem == null)
            {
                throw new ArgumentNullException("dashboardItem");
            }

            if (setting.ApplicationGuid == null)
            {
                throw new ArgumentException("[SingleObjectTileModelFactory.CreateEmptyTileModel]: Application in dashboard item cannot be null");
            }

            if (setting.ElementGuid == null)
            {
                throw new ArgumentException("[SingleObjectTileModelFactory.CreateEmptyTileModel]: ElementGuid in setting cannot be null");
            }
            
            if (setting.ObjectName == null)
            {
                throw new ArgumentException("[SingleObjectTileModelFactory.CreateEmptyTileModel]: ObjectName in setting cannot be null");
            }

            return new SingleObjectTileModel
            {
                ApplicationGuid = setting.ApplicationGuid,
                UIElementGuid = setting.ElementGuid.Value,
                ObjectName = setting.ObjectName,
                ObjectSiteName = setting.ObjectSiteName,
                IsVisible = false
            };
        }
    }
}