using System;

using CMS;
using CMS.ApplicationDashboard.Web.UI;
using CMS.Modules;

[assembly: RegisterImplementation(typeof(ILiveTileModelProviderFactory), typeof(LiveTileModelProviderFactory), Priority = CMS.Core.RegistrationPriority.Fallback, Lifestyle = CMS.Core.Lifestyle.Transient)]

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Provides methods for obtaining live model providers for applications.
    /// </summary>
    internal interface ILiveTileModelProviderFactory
    {
        /// <summary>
        /// Gets live model provider for given application.
        /// </summary>
        /// <param name="uiElementInfo">UI element representing the application</param>
        /// <returns>Instance of live model provider</returns>
        ILiveTileModelProvider GetLiveTileModelProvider(UIElementInfo uiElementInfo);


        /// <summary>
        /// Gets whether there is available live model provider for given application or not.
        /// </summary>
        /// <param name="uiElementInfo">UI element representing the application</param>
        /// <returns>True if live model provider can be loaded, false otherwise</returns>
        bool CanLoadLiveTileModelProvider(UIElementInfo uiElementInfo);
    }
}