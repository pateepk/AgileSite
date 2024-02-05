using System;

using CMS.Modules;

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Provides methods for obtaining live model providers for applications.
    /// </summary>
    internal class LiveTileModelProviderFactory : ILiveTileModelProviderFactory
    {
        /// <summary>
        /// Gets live model provider for given application.
        /// </summary>
        /// <param name="uiElementInfo">UI element representing the application</param>
        /// <exception cref="ArgumentNullException"><paramref name="uiElementInfo"/> is null</exception>
        /// <returns>Instance of live model provider</returns>
        public ILiveTileModelProvider GetLiveTileModelProvider(UIElementInfo uiElementInfo)
        {
            if (uiElementInfo == null)
            {
                throw new ArgumentNullException("uiElementInfo");
            }

            return LiveTileModelProviderContainer.GetProvider(uiElementInfo);
        }


        /// <summary>
        /// Gets whether there is available live model provider for given application or not.
        /// </summary>
        /// <param name="uiElementInfo">UI element representing the application</param>
        /// <exception cref="ArgumentNullException"><paramref name="uiElementInfo"/> is null</exception>
        /// <returns>True if live model provider can be loaded, false otherwise</returns>
        public bool CanLoadLiveTileModelProvider(UIElementInfo uiElementInfo)
        {
            if (uiElementInfo == null)
            {
                throw new ArgumentNullException("uiElementInfo");
            }

            return GetLiveTileModelProvider(uiElementInfo) != null;
        }
    }
}