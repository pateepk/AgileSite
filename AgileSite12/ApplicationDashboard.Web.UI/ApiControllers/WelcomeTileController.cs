using System.Net;
using System.Net.Http;
using System.Web.Http;

using CMS.ApplicationDashboard.Web.UI.Internal;
using CMS.Core;
using CMS.Membership;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(WelcomeTileController))]

namespace CMS.ApplicationDashboard.Web.UI.Internal
{
    /// <summary>
    /// Handles obtaining of the welcome tile data from the server.
    /// </summary>
    /// <remarks>
    /// Only authorized users (editors) are allowed to obtain live tile data.
    /// Exceptions thrown on execution are automatically handled.
    /// </remarks>
    /// <exclude />
    [AllowOnlyEditor]
    [HandleExceptions]
    public sealed class WelcomeTileController : CMSApiController
    {
        private readonly CurrentUserInfo mCurrentUser;
        private readonly ILocalizationService mLocalizationService;


        /// <summary>
        /// Constructor.
        /// </summary>
        public WelcomeTileController()
        {
            mLocalizationService = CoreServices.Localization;

            mCurrentUser = MembershipContext.AuthenticatedUser;
        }


        /// <summary>
        /// Gets the welcome tile.
        /// </summary>
        /// <returns>Welcome tile</returns>
        public WelcomeTileModel Get()
        {
            return new WelcomeTileModel
            {
                Visible = mCurrentUser.UserSettings.UserShowIntroductionTile,
                Header = mLocalizationService.GetString("cms.dashboard.introduction.welcome"),
                Description = mLocalizationService.GetString("cms.dashboard.introduction.info"),
                BrowseApplicationsText = mLocalizationService.GetString("cms.dashboard.introduction.applications"),
                OpenHelpText = mLocalizationService.GetString("cms.dashboard.introduction.help"),
            };
        }


        /// <summary>
        /// Saves whether the welcome tile should be visible for the current user or not.
        /// </summary>
        /// <param name="visible">Visibility of the welcome tile</param>
        public HttpResponseMessage Post([FromBody] bool visible)
        {
            mCurrentUser.UserSettings.UserShowIntroductionTile = visible;
            mCurrentUser.UserSettings.Update();
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}