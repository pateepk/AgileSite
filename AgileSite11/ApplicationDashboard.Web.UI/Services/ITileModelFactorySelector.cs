using CMS;
using CMS.ApplicationDashboard.Web.UI;
using CMS.ApplicationDashboard.Web.UI.Internal;

[assembly: RegisterImplementation(typeof(ITileModelFactorySelector), typeof(TileModelFactorySelector), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Provides methods for retrieving either tile model factory or tile model updater.
    /// </summary>
    internal interface ITileModelFactorySelector
    {
        /// <summary>
        /// Gets model factory for given <paramref name="tileModelType"/>.
        /// </summary>
        /// <param name="tileModelType">Type of the tile</param>
        /// <returns>Factory for given <paramref name="tileModelType"/></returns>
        ITileModelFactory GetTileModelFactory(TileModelTypeEnum tileModelType);

        
        /// <summary>
        /// Gets model updated for given <paramref name="tileModelType"/>.
        /// </summary>
        /// <param name="tileModelType">Type of the tile</param>
        /// <returns>Updater for given <paramref name="tileModelType"/></returns>
        ITileModelUpdater GetTileModelUpdater(TileModelTypeEnum tileModelType);
    }
}