using CMS;
using CMS.ApplicationDashboard.Web.UI;
using CMS.ApplicationDashboard.Web.UI.Internal;
using CMS.Modules;

[assembly: RegisterImplementation(typeof(ITileIconModelProvider), typeof(TileIconModelProvider), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Provides method for retrieving icon model for dashboard tiles.
    /// </summary>
    internal interface ITileIconModelProvider
    {
        /// <summary>
        /// Creates icon model for given <paramref name="uiElement"/>.
        /// </summary>
        /// <param name="uiElement">UI element the icon model is retrieved for</param>
        /// <returns>Icon model for given <paramref name="uiElement"/></returns>
        TileIconModel CreateTileIconModel(UIElementInfo uiElement);
    }
}