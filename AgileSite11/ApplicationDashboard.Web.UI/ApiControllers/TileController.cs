using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using CMS.ApplicationDashboard.Web.UI.Internal;
using CMS.Core;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(TileController))]

namespace CMS.ApplicationDashboard.Web.UI.Internal
{
    /// <summary>
    /// Handles obtaining of tile data from the server.
    /// </summary>
    /// <remarks>
    /// Only authorized users (editors) are allowed to obtain live tile data.
    /// Exceptions thrown on execution are automatically handled.
    /// </remarks>
    /// <exclude />
    [AllowOnlyEditor]
    [HandleExceptions]
    public sealed class TileController : CMSApiController
    {
        private readonly IDashboardItemProvider mDashboardItemProvider;
        private readonly ILiveTileModelProviderFactory mLiveTileModelProviderFactory;
        private readonly IDefaultDashboardItemsLoader mDefaultDashboardItemsLoader;
        private readonly IUserSpecificDashboardItemsLoader mUserSpecificDashboardItemsLoader;
        private readonly ITileModelFactorySelector mTileModelFactorySelector;

        private readonly SiteInfo mCurrentSite;
        private readonly CurrentUserInfo mCurrentUser;


        /// <summary>
        /// Constructor.
        /// </summary>
        public TileController()
        {
            mDashboardItemProvider = Service.Resolve<IDashboardItemProvider>();
            mLiveTileModelProviderFactory = Service.Resolve<ILiveTileModelProviderFactory>();
            mDefaultDashboardItemsLoader = Service.Resolve<IDefaultDashboardItemsLoader>();
            mUserSpecificDashboardItemsLoader = Service.Resolve<IUserSpecificDashboardItemsLoader>();
            mTileModelFactorySelector = Service.Resolve<ITileModelFactorySelector>();

            mCurrentSite = SiteContext.CurrentSite;
            mCurrentUser = MembershipContext.AuthenticatedUser;
        }


        /// <summary>
        /// Gets collection of applications specific for the user. If no items are found, uses default settings for user's role.
        /// </summary>
        /// <returns>User specified list of application. If not item is found, returns empty collection</returns>
        public IEnumerable<ITileModel> Get()
        {
            var loadedApplications = mUserSpecificDashboardItemsLoader.GetUserSpecificDashboardItems(mCurrentUser) ??
                mDefaultDashboardItemsLoader.GetDefaultDashboardItems(mCurrentUser, mCurrentSite);

            return loadedApplications == null ?
                Enumerable.Empty<ITileModel>() :
                loadedApplications.Select(application => CreateTileModel(application.Key, application.Value))
                                  .Where(tileModel => tileModel != null)
                                  .ToList();
        }


        /// <summary>
        /// Gets the tile for given ApplicationGuid.
        /// </summary>
        /// <param name="guid">ApplicationGuid of the tile</param>
        /// <returns>Tile with the guid</returns>
        [HttpGet]
        public ITileModel LoadTile([FromUri] Guid guid)
        {
            var uiElement = mDashboardItemProvider.GetFilteredApplicationByGuid(mCurrentUser, guid);

            if (uiElement == null)
            {
                return null;
            }

            return CreateTileModel(new UserDashboardSetting
            {
                ApplicationGuid = guid
            }, new DashboardItem
            {
                Application = uiElement,
                IsVisible = true
            });
        }


        /// <summary>
        /// Gets updated tile of type in <paramref name="tileModelType"/> for given <paramref name="userDashboardSetting"/>.
        /// </summary>
        /// <param name="userDashboardSetting">Setting specifying which tile will be proceeded</param>
        /// <param name="tileModelType">Type of the tile</param>
        /// <returns>Tile for the given <paramref name="userDashboardSetting"/></returns>
        [HttpPost]
        public ITileModel UpdateTile([FromBody] UserDashboardSetting userDashboardSetting, [FromUri] TileModelTypeEnum tileModelType)
        {
            return mTileModelFactorySelector.GetTileModelUpdater(tileModelType).GetTileModel(userDashboardSetting, mCurrentUser);
        }


        /// <summary>
        /// Saves given application list for the current user.
        /// </summary>
        /// <remarks>
        /// Order of the application within the list determines order of the loaded tiles.
        /// </remarks>
        /// <param name="userDashboardSettings">Collection of user dashboard settings to be saved</param>
        public HttpResponseMessage Save([FromBody] List<UserDashboardSetting> userDashboardSettings)
        {
            mUserSpecificDashboardItemsLoader.SaveUserSpecificDashboardSettings(mCurrentUser, userDashboardSettings);
            return Request.CreateResponse(HttpStatusCode.OK);
        }


        /// <summary>
        /// Creates new tile for the given UI element.
        /// </summary>
        /// <param name="userDashboardSetting">Setting specifying which tile will be proceeded</param>
        /// <param name="dashboardItem">UI elements the tile should be created for</param>
        /// <returns>Tile created for the given UI element</returns>
        private ITileModel CreateTileModel(UserDashboardSetting userDashboardSetting, DashboardItem dashboardItem)
        {
            TileModelTypeEnum tileModelType;

            if ((dashboardItem == null) || dashboardItem.Application == null)
            {
                return null;
            }

            if (userDashboardSetting.ElementGuid.HasValue && !string.IsNullOrEmpty(userDashboardSetting.ObjectName))
            {
                tileModelType = TileModelTypeEnum.SingleObjectTileModel;
            }
            else
            {
                tileModelType = mLiveTileModelProviderFactory.CanLoadLiveTileModelProvider(dashboardItem.Application) ?
                    TileModelTypeEnum.ApplicationLiveTileModel :
                    TileModelTypeEnum.ApplicationTileModel;
            }

            var tileModelFactory = mTileModelFactorySelector.GetTileModelFactory(tileModelType);
            return dashboardItem.IsVisible ?
                tileModelFactory.CreateTileModel(userDashboardSetting, dashboardItem) :
                tileModelFactory.CreateEmptyTileModel(userDashboardSetting, dashboardItem);
        }
    }
}