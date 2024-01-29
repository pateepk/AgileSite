using System;

using CMS.Base;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;


namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Provides method for loading live tile model for specific <see cref="UIElementInfo"/>.
    /// </summary>
    internal class LiveTileModelLoader : ILiveTileModelLoader
    {
        private readonly IDashboardItemProvider mDashboardItemProvider;
        private readonly ILiveTileModelProviderFactory mLiveTileModelProviderFactory;


        /// <summary>
        /// Creates new instance of <see cref="LiveTileModelLoader"/>.
        /// </summary>
        /// <param name="dashboardItemProvider">Contains methods for obtaining filtered lists of dashboard items.</param>
        /// <param name="liveTileModelProviderFactory">Provides methods for obtaining live model providers for applications.</param>
        /// <exception cref="ArgumentNullException"><paramref name="dashboardItemProvider"/> is null -or- <paramref name="liveTileModelProviderFactory"/> is null</exception>
        public LiveTileModelLoader(IDashboardItemProvider dashboardItemProvider, ILiveTileModelProviderFactory liveTileModelProviderFactory)
        {
            if (dashboardItemProvider == null)
            {
                throw new ArgumentNullException("dashboardItemProvider");
            }

            if (liveTileModelProviderFactory == null)
            {
                throw new ArgumentNullException("liveTileModelProviderFactory");
            }

            mDashboardItemProvider = dashboardItemProvider;
            mLiveTileModelProviderFactory = liveTileModelProviderFactory;
        }


        /// <summary>
        /// Gets <see cref="LiveTileModel"/> for an application (UI element) with given <paramref name="uiElementGuid"/>. Uses <see cref="ILiveTileModelProvider"/> that is set up in
        /// the UIElement.
        /// </summary>
        /// <param name="uiElementGuid">Guid of a UI element for which <see cref="LiveTileModel"/> will be returned</param>
        /// <param name="user">User for which the model will be returned. Is used for security reasons</param>
        /// <param name="site">Site for which to display <see cref="LiveTileModel"/> for</param>
        /// <exception cref="UnauthorizedAccessException">User does not have permissions for the UI element</exception>
        /// <exception cref="ArgumentNullException"><paramref name="site"/> or <paramref name="user"/> is null</exception>
        public LiveTileModel LoadLiveTileModel(Guid uiElementGuid, ISiteInfo site, UserInfo user)
        {
            if (site == null)
            {
                throw new ArgumentNullException("site");
            }
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var application = mDashboardItemProvider.GetFilteredApplicationByGuid(user, uiElementGuid);
            if (application == null)
            {
                // If application was not found, user most probably doesn't have permissions to see it
                throw new UnauthorizedAccessException();
            }

            var liveTileModelProvider = mLiveTileModelProviderFactory.GetLiveTileModelProvider(application);
            if (liveTileModelProvider == null)
            {
                return null;
            }

            return liveTileModelProvider.GetModel(
                new LiveTileContext
                {
                    SiteInfo = SiteInfoProvider.GetSiteInfo(site.SiteName),
                    UserInfo = user
                });
        }
    }
}