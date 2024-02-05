using System;
using System.Linq;
using System.Text;

using CMS;
using CMS.ApplicationDashboard.Web.UI;
using CMS.Base;
using CMS.Membership;
using CMS.Modules;

[assembly: RegisterImplementation(typeof(ILiveTileModelLoader), typeof(LiveTileModelLoader), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Provides method for loading live tile model for specific <see cref="UIElementInfo"/>.
    /// </summary>
    internal interface ILiveTileModelLoader
    {
        /// <summary>
        /// Gets <see cref="LiveTileModel"/> for an application (UI element) with given <paramref name="uiElementGuid"/>. Uses <see cref="ILiveTileModelProvider"/> that is set up in
        /// the UIElement.
        /// </summary>
        /// <param name="uiElementGuid">Guid of a UI element for which <see cref="LiveTileModel"/> will be returned.</param>
        /// <param name="user">User for which the model will be returned. Is used for security reasons.</param>
        /// <param name="site">Site for which to display <see cref="LiveTileModel"/> for.</param>
        LiveTileModel LoadLiveTileModel(Guid uiElementGuid, ISiteInfo site, UserInfo user);
    }
}
