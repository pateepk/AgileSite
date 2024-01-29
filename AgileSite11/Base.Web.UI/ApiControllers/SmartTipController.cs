using System.Net;
using System.Net.Http;
using System.Web.Http;

using CMS.Base.Web.UI;
using CMS.Membership;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(SmartTipController))]

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Handles the smart tips.
    /// </summary>
    [HandleExceptions]
    public sealed class SmartTipController : CMSApiController
    {
        /// <summary>
        /// Toggles state of the smart tip identified by <paramref name="smartTipIdentifier"/>.
        /// If smart tip is dismissed, removes it from dismissed list.
        /// If not, smart tip is added to dismissed list.
        /// </summary>
        /// <param name="smartTipIdentifier">Unique identifier of the smart tip</param>
        [HttpPost]
        public void Toggle([FromBody] string smartTipIdentifier)
        {
            if (string.IsNullOrEmpty(smartTipIdentifier))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Unique identifier of the smart tip must be defined"));
            }

            var user = MembershipContext.AuthenticatedUser;
            if (!user.IsPublic())
            {
                var manager = new UserSmartTipDismissalManager(user);
                manager.ToggleSmartTipState(smartTipIdentifier);
            }
        }
    }
}
