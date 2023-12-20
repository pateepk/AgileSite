using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using CMS.ApplicationDashboard.Web.UI.Internal;
using CMS.Core;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(PinHandlerController))]

namespace CMS.ApplicationDashboard.Web.UI.Internal
{
    /// <summary>
    /// Handles pinning and unpinning tiles.
    /// </summary>
    /// <remarks>
    /// Only authorized users (editors) are allowed to pin or unpin tile.
    /// Exceptions thrown on execution are automatically handled.
    /// </remarks>
    /// <exclude />
    [AllowOnlyEditor]
    [HandleExceptions]
    public sealed class PinHandlerController : CMSApiController
    {
        private readonly IDefaultDashboardItemsLoader mDefaultDashboardItemsLoader;
        private readonly IUserSpecificDashboardItemsLoader mUserSpecificDashboardItemsLoader;
        
        private readonly CurrentUserInfo mCurrentUser;
        private readonly SiteInfo mCurrentSite;
        

        /// <summary>
        /// Constructor.
        /// </summary>
        public PinHandlerController()
        {
            mCurrentUser = MembershipContext.AuthenticatedUser;
            mCurrentSite = SiteContext.CurrentSite;

            mDefaultDashboardItemsLoader = Service.Resolve<IDefaultDashboardItemsLoader>();
            mUserSpecificDashboardItemsLoader = Service.Resolve<IUserSpecificDashboardItemsLoader>();
        }


        /// <summary>
        /// Pin application or single object.
        /// </summary>
        [HttpPost]
        public HttpResponseMessage Pin([FromBody] DashboardItemPinSettingsModel model)
        {
            return DoPin(model, true);
        }


        /// <summary>
        /// Unpin application or single object.
        /// </summary>
        [HttpPost]
        public HttpResponseMessage Unpin([FromBody] DashboardItemPinSettingsModel model)
        {
            return DoPin(model, false);
        }


        /// <summary>
        /// Pins or unpins either application or single object.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="model"/> is null</exception>
        private HttpResponseMessage DoPin(DashboardItemPinSettingsModel model, bool pin)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            if (Guid.Empty == model.ApplicationGuid)
            {
                throw new ArgumentException("[PinHandlerController.DoPin]: Model has to have ApplicationGuid specified", "model");
            }

            if (UIElementInfoProvider.GetUIElementInfo(model.ApplicationGuid) == null)
            {
                throw new ArgumentException("[PinHandlerController.DoPin]: model.ApplicationGuid has to refer the existing UIElement", "model");
            }

            if ((model.ElementGuid.HasValue) && UIElementInfoProvider.GetUIElementInfo(model.ElementGuid.Value) == null)
            {
                throw new ArgumentException("[PinHandlerController.DoPin]: model.ElementGuid has to be either null or refer the existing UIElement", "model");
            }

            var settings = GetUserSettings();

            // Single application case
            UpdateSettings(model, settings, pin);

            mUserSpecificDashboardItemsLoader.SaveUserSpecificDashboardSettings(mCurrentUser, settings);
            return Request.CreateResponse(HttpStatusCode.OK);
        }


        /// <summary>
        /// Update given <paramref name="settings"/> with the <paramref name="model"/>. If <paramref name="pin"/> is true, <paramref name="model"/> is added; otherwise, removed.
        /// </summary>
        /// <param name="model">Model to be used</param>
        /// <param name="settings">Collection containing tiles to be displayed in the application dashboard</param>
        /// <param name="pin">True if <paramref name="model"/> should be added; otherwise, false</param>
        private void UpdateSettings(DashboardItemPinSettingsModel model, List<UserDashboardSetting> settings, bool pin)
        {
            if (model.ObjectName != null)
            {
                RemoveSingleObjectFromSettings(model, settings);

                if (pin)
                {
                    AddSingleObjectToSettings(model, settings);
                }
            }
            else
            {
                RemoveApplicationFromSettings(model, settings);

                if (pin)
                {
                    AddApplicationToSettings(model, settings);
                }
            }
        }


        /// <summary>
        /// Removes given <paramref name="model"/> representing application from <paramref name="settings"/>.
        /// </summary>
        /// <param name="model">Model to be removed</param>
        /// <param name="settings">Collection containing tiles to be displayed in the application dashboard</param>
        private static void RemoveApplicationFromSettings(DashboardItemPinSettingsModel model, List<UserDashboardSetting> settings)
        {
            settings.RemoveAll(s => (s.ApplicationGuid == model.ApplicationGuid) && (!s.ElementGuid.HasValue) && string.IsNullOrEmpty(s.ObjectName));
        }


        /// <summary>
        /// Adds given <paramref name="model"/> representing application to <paramref name="settings"/>.
        /// </summary>
        /// <param name="model">Model to be added</param>
        /// <param name="settings">Collection containing tiles to be displayed in the application dashboard</param>
        private static void AddApplicationToSettings(DashboardItemPinSettingsModel model, ICollection<UserDashboardSetting> settings)
        {
            settings.Add(new UserDashboardSetting
            {
                ApplicationGuid = model.ApplicationGuid,
            });
        }


        /// <summary>
        /// Removes given <paramref name="model"/> representing single object from <paramref name="settings"/>.
        /// </summary>
        /// <param name="model">Model to be removed</param>
        /// <param name="settings">Collection containing tiles to be displayed in the application dashboard</param>
        private void RemoveSingleObjectFromSettings(DashboardItemPinSettingsModel model, List<UserDashboardSetting> settings)
        {
            settings.RemoveAll(s => 
                (s.ApplicationGuid == model.ApplicationGuid) && 
                (s.ElementGuid == model.ElementGuid) && 
                (s.ObjectName == model.ObjectName) && 
                (s.ObjectSiteName == model.ObjectSiteName)
            );
        }


        /// <summary>
        /// Adds given <paramref name="model"/> representing single object to <paramref name="settings"/>.
        /// </summary>
        /// <param name="model">Model to be added</param>
        /// <param name="settings">Collection containing tiles to be displayed in the application dashboard</param>
        private void AddSingleObjectToSettings(DashboardItemPinSettingsModel model, ICollection<UserDashboardSetting> settings)
        {
            settings.Add(new UserDashboardSetting
            {
                ApplicationGuid = model.ApplicationGuid,
                ElementGuid = model.ElementGuid,
                ObjectName = model.ObjectName,
                ObjectSiteName = model.ObjectSiteName
            });
        }


        /// <summary>
        /// Gets dashboard settings for <see cref="mCurrentUser"/>.
        /// </summary>
        /// <remarks>
        /// If no settings are available for <see cref="mCurrentUser"/>, default ones are selected.
        /// </remarks>
        /// <returns>Collection of tiles to be displayed in the dashboard</returns>
        private List<UserDashboardSetting> GetUserSettings()
        {
            var settings = mUserSpecificDashboardItemsLoader.GetUserSpecificDashboardItems(mCurrentUser) ?? 
                mDefaultDashboardItemsLoader.GetDefaultDashboardItems(mCurrentUser, mCurrentSite);

            return settings.Keys.ToList();
        }
    }
}