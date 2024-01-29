using System.Web;

using CMS.Helpers;
using CMS.Base;

namespace CMS.Newsletters
{
    /// <summary>
    /// Helper methods for logging activities for opened e-mails and link tracking.
    /// </summary>
    public class ActivityTrackingHelper : AbstractHelper<ActivityTrackingHelper>
    {
        /// <summary>
        /// Responds to ping request by sending <paramref name="ping"/> value to the client.
        /// </summary>
        internal static void RespondToPing(HttpContextBase context, int ping)
        {
            HelperObject.RespondToPingInternal(context, ping);
        }


        /// <summary>
        /// Responds to ping request by sending <paramref name="ping"/> value to the client.
        /// </summary>
        internal virtual void RespondToPingInternal(HttpContextBase context, int ping)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write(ping);
            RequestHelper.EndResponse();
        }
    }
}