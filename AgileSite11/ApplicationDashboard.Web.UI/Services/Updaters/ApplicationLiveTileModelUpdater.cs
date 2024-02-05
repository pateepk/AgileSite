using System;
using System.Linq;
using System.Text;

using CMS.ApplicationDashboard.Web.UI.Internal;
using CMS.Base;
using CMS.Membership;

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Provides method for retrieving updated dashboard tile.
    /// </summary>
    internal class ApplicationLiveTileModelUpdater : ITileModelUpdater
    {
        private readonly ILiveTileModelLoader mLiveTileModelLoader;
        private readonly ISiteService mSiteService;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="liveTileModelLoader">Service for loading live tile model for given UI element</param>
        /// <param name="siteService">Service for loading context of the current site</param>
        /// <exception cref="ArgumentNullException"><paramref name="liveTileModelLoader"/> is null -or- <paramref name="siteService"/> is null</exception>
        public ApplicationLiveTileModelUpdater(ILiveTileModelLoader liveTileModelLoader, ISiteService siteService)
        {
            if (liveTileModelLoader == null)
            {
                throw new ArgumentNullException("liveTileModelLoader");
            }

            if (siteService == null)
            {
                throw new ArgumentNullException("siteService");
            }
            
            mLiveTileModelLoader = liveTileModelLoader;
            mSiteService = siteService;
        }


        /// <summary>
        /// Creates instance of updated tile model for given <paramref name="userDashboardSetting"/>.
        /// </summary>
        /// <param name="userDashboardSetting">User settings for which the updated tile is retrieved for</param>
        /// <param name="user">User the model is updated for</param>
        /// <exception cref="ArgumentNullException"><paramref name="userDashboardSetting"/> is null -or- <paramref name="user"/> is null</exception>
        /// <returns>Instance of updated tile model</returns>
        public ITileModel GetTileModel(UserDashboardSetting userDashboardSetting, UserInfo user)
        {
            if (userDashboardSetting == null)
            {
                throw new ArgumentNullException("userDashboardSetting");
            }

            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (mSiteService.CurrentSite == null)
            {
                return null;
            }

            var model = mLiveTileModelLoader.LoadLiveTileModel(userDashboardSetting.ApplicationGuid, mSiteService.CurrentSite, user);
            if (model == null)
            {
                return null;
            }

            return new ApplicationLiveTileModel
            {
                Value = model.Value,
                Description = model.Description
            };
        }
    }
}