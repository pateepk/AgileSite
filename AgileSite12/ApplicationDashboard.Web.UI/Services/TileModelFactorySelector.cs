using System;

using CMS.ApplicationDashboard.Web.UI.Internal;
using CMS.Base;
using CMS.Modules;
using CMS.PortalEngine.Internal;


namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Provides methods for retrieving either tile model factory or tile model updater.
    /// </summary>
    internal class TileModelFactorySelector : ITileModelFactorySelector
    {
        private readonly IUIElementObjectPropertiesProvider mUIElementObjectPropertiesProvider;
        private readonly ITileIconModelProvider mTileIconModelProvider;
        private readonly IUserSpecificDashboardItemsLoader mUserSpecificDashboardItemsLoader;
        private readonly IUILinkProvider mUILinkProvider;
        private readonly ILiveTileModelLoader mLiveTileModelLoader;
        private readonly ISiteService mSiteService;

        private ApplicationTileModelFactory mApplicationTileModelFactory;
        private ApplicationLiveTileModelFactory mApplicationLiveTileModelFactory;
        private SingleObjectTileModelFactory mSingleObjectTileModelFactory;

        private ApplicationLiveTileModelUpdater mApplicationLiveTileModelUpdater;


        /// <summary>
        /// Creates new instance of <see cref="TileModelFactorySelector"/>.
        /// </summary>
        /// <param name="uiElementObjectPropertiesProvider">Provides method for obtaining display name for given <see cref="UIElementInfo"/></param>
        /// <param name="tileIconModelProvider">Provides method for retrieving icon model for dashboard tiles</param>
        /// <param name="userSpecificDashboardItemsLoader">Handles loading of applications from the user settings</param>
        /// <param name="uiLinkProvider"></param>
        /// <param name="liveTileModelLoader">Provides method for loading live tile model for specific <see cref="UIElementInfo"/></param>
        /// <param name="siteService">Provides method for obtaining <see cref="ISiteInfo"/> for current context</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="uiElementObjectPropertiesProvider"/> is null -or-
        /// <paramref name="tileIconModelProvider"/> is null -or-
        /// <paramref name="userSpecificDashboardItemsLoader"/> is null -or-
        /// <paramref name="uiLinkProvider"/> is null -or-
        /// <paramref name="liveTileModelLoader"/> is null -or-
        /// <paramref name="siteService"/> is null 
        /// </exception>
        public TileModelFactorySelector(
            IUIElementObjectPropertiesProvider uiElementObjectPropertiesProvider, 
            ITileIconModelProvider tileIconModelProvider, 
            IUserSpecificDashboardItemsLoader userSpecificDashboardItemsLoader, 
            IUILinkProvider uiLinkProvider, 
            ILiveTileModelLoader liveTileModelLoader,
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

            if (liveTileModelLoader == null)
            {
                throw new ArgumentNullException("liveTileModelLoader");
            }

            if (siteService == null)
            {
                throw new ArgumentNullException("siteService");
            }

            mUIElementObjectPropertiesProvider = uiElementObjectPropertiesProvider;
            mTileIconModelProvider = tileIconModelProvider;
            mUserSpecificDashboardItemsLoader = userSpecificDashboardItemsLoader;
            mUILinkProvider = uiLinkProvider;
            mLiveTileModelLoader = liveTileModelLoader;
            mSiteService = siteService;
        }


        /// <value>
        /// Provides method for creating single application tile.
        /// </value>
        private ApplicationTileModelFactory ApplicationTileModelFactory
        {
            get
            {
                return mApplicationTileModelFactory ?? (mApplicationTileModelFactory = new ApplicationTileModelFactory(mTileIconModelProvider));
            }
        }


        /// <value>
        /// Provides method for creating single application tile with live data provider.
        /// </value>
        private ApplicationLiveTileModelFactory ApplicationLiveTileModelFactory
        {
            get
            {
                return mApplicationLiveTileModelFactory ?? (mApplicationLiveTileModelFactory = new ApplicationLiveTileModelFactory(mTileIconModelProvider));
            }
        }


        /// <value>
        /// Provides method for creating single object tile.
        /// </value>
        private SingleObjectTileModelFactory SingleObjectTileModelFactory
        {
            get
            {
                return mSingleObjectTileModelFactory ?? (mSingleObjectTileModelFactory = new SingleObjectTileModelFactory(
                    mUIElementObjectPropertiesProvider,
                    mTileIconModelProvider,
                    mUserSpecificDashboardItemsLoader,
                    mUILinkProvider,
                    mSiteService)
                );
            }
        }


        /// <value>
        /// Provides method for retrieving updated dashboard tile.
        /// </value>
        private ApplicationLiveTileModelUpdater ApplicationLiveTileModelUpdater
        {
            get
            {
                return mApplicationLiveTileModelUpdater ?? (mApplicationLiveTileModelUpdater = new ApplicationLiveTileModelUpdater(mLiveTileModelLoader, mSiteService));
            }
        }


        /// <summary>
        /// Gets model factory for given <paramref name="tileModelType"/>.
        /// </summary>
        /// <param name="tileModelType">Type of the tile</param>
        /// <returns>Factory for given <paramref name="tileModelType"/></returns>
        /// <exception cref="ArgumentException"><paramref name="tileModelType"/> is not supported</exception>
        public ITileModelFactory GetTileModelFactory(TileModelTypeEnum tileModelType)
        {
            switch (tileModelType)
            {
                case TileModelTypeEnum.ApplicationTileModel:
                    return ApplicationTileModelFactory;
                case TileModelTypeEnum.ApplicationLiveTileModel:
                    return ApplicationLiveTileModelFactory;
                case TileModelTypeEnum.SingleObjectTileModel:
                    return SingleObjectTileModelFactory;
                default:
                    throw new ArgumentException("[TileModelServiceSelector.GetTileModelFactory]: Given type is not supported", "tileModelType");
            }
        }


        /// <summary>
        /// Gets model updated for given <paramref name="tileModelType"/>.
        /// </summary>
        /// <param name="tileModelType">Type of the tile</param>
        /// <returns>Updater for given <paramref name="tileModelType"/></returns>
        /// <exception cref="ArgumentException"><paramref name="tileModelType"/> is not supported</exception>
        public ITileModelUpdater GetTileModelUpdater(TileModelTypeEnum tileModelType)
        {
            switch (tileModelType)
            {
                case TileModelTypeEnum.ApplicationLiveTileModel:
                    return ApplicationLiveTileModelUpdater;
                default:
                    throw new ArgumentException("[TileModelServiceSelector.GetTileModelUpdater]: Given type is not supported", "tileModelType");
            }
        }
    }
}