using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

using CMS.Helpers;
using CMS.Membership;

namespace CMS.WebApi
{
    /// <summary>
    /// Filter that sets correct thread UI culture based on user preferred UI culture.
    /// </summary>
    internal class ThreadUICultureActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            Thread.CurrentThread.CurrentUICulture = CultureHelper.GetCultureInfo(MembershipContext.AuthenticatedUser.PreferredUICultureCode);
            base.OnActionExecuting(actionContext);
        }
    }
}