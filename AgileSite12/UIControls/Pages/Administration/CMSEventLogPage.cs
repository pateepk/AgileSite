using CMS.Base;
using CMS.EventLog;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the event log tab of administration pages to apply global settings to the pages.
    /// </summary>
    public abstract class CMSEventLogPage : CMSAdministrationPage
    {
        /// <summary>
        /// Checks the access permissions.
        /// </summary>
        /// <param name="redirectOnError">If set to <c>true</c> then redirect to error permission page when not authorised</param>
        public static bool CheckPermissions(bool redirectOnError = true)
        {
            bool authorised = true;

            CurrentUserInfo user = MembershipContext.AuthenticatedUser;
            if (user == null)
            {
                authorised = false;
            }
            else
            {
                // Check site availability
                if (!user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                {
                    if (!ResourceSiteInfoProvider.IsResourceOnSite(EventLogInfo.OBJECT_TYPE, SiteContext.CurrentSiteName))
                    {
                        authorised = false;
                        if (redirectOnError)
                        {
                            RedirectToAccessDeniedResourceNotAvailableOnSite(EventLogInfo.OBJECT_TYPE);
                        }
                    }
                }

                // Check UI profile
                if (!CheckUIElementAccessHierarchical(EventLogInfo.OBJECT_TYPE, "EventLog", null, 0, false))
                {
                    authorised = false;
                    if (redirectOnError)
                    {
                        RedirectToUIElementAccessDenied(EventLogInfo.OBJECT_TYPE, "EventLog");
                    }
                }

                // Check "read" permission
                if (!user.IsAuthorizedPerResource(EventLogInfo.OBJECT_TYPE, "Read"))
                {
                    authorised = false;
                    if (redirectOnError)
                    {
                        RedirectToAccessDenied(EventLogInfo.OBJECT_TYPE, "Read");
                    }
                }
            }
            return authorised;
        }
    }
}