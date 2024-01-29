using CMS;
using CMS.DocumentEngine.Macros;

using CMS.MacroEngine;
using CMS.Membership;
using CMS.SiteProvider;

[assembly: RegisterExtension(typeof(ContentMembershipMethods), typeof(UserInfo))]

namespace CMS.DocumentEngine.Macros
{
    /// <summary>
    /// Content membership methods - wrapping methods for macro resolver.
    /// </summary>
    internal class ContentMembershipMethods: MacroMethodContainer
    {
        /// <summary>
        /// Returns true if user has access to Pages application.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if user has access to Pages application", 1, IsHidden = true)]
        [MacroMethodParam(0, "user", typeof(object), "User info object.")]
        public static object IsAuthorizedPerContent(EvaluationContext context, params object[] parameters)
        {
            UserInfo ui = parameters[0] as UserInfo;
            if (ui != null)
            {
                return DocumentSecurityHelper.IsUserAuthorizedPerContent(SiteContext.CurrentSiteName, ui);
            }

            return false;
        }
    }
}
