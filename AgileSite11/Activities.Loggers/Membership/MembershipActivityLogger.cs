using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;

namespace CMS.Activities.Loggers
{
    /// <summary>
    /// Provides methods for logging membership activities.
    /// </summary>
    public class MembershipActivityLogger
    {
        /// <summary>
        /// Logs login activity.
        /// </summary>
        /// <param name="userName">User name of current user</param>
        /// <param name="currentDocument">Current document tree representation, for MVC should be null</param>
        public static void LogLogin(string userName, ITreeNode currentDocument = null)
        {
            var user = UserInfoProvider.GetUserInfo(userName);

            if ((user == null) || !user.UserSettings.UserLogActivities)
            {
                return;
            }

            var contactID = ModuleCommands.OnlineMarketingGetUserLoginContactID(user);
            var userLoginActivity = new UserLoginActivityInitializer(user, currentDocument, contactID);

            var activityLogService = Service.Resolve<IActivityLogService>();
            activityLogService.Log(userLoginActivity, CMSHttpContext.Current.Request);
        }


        /// <summary>
        /// Logs registration activity.
        /// </summary>
        /// <param name="userName">User name of current user</param>
        /// <param name="currentDocument">Current document tree representation, for MVC should be null</param>
        /// <param name="checkViewModel"><c>True</c> if activities should not be logged in administration</param>
        public static void LogRegistration(string userName, ITreeNode currentDocument = null, bool checkViewModel = true)
        {
            var user = UserInfoProvider.GetUserInfo(userName);

            if ((user == null) || !user.UserSettings.UserLogActivities)
            {
                return;
            }

            var contactID = ModuleCommands.OnlineMarketingGetUserLoginContactID(user);
            var registrationActivity = new RegistrationActivityInitializer(user, currentDocument, contactID);

            var activityLogService = Service.Resolve<IActivityLogService>();
            activityLogService.Log(registrationActivity, CMSHttpContext.Current.Request, checkViewModel);
        }
    }
}