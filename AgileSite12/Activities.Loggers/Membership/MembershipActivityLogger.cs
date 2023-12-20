using System;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Activities.Loggers;

[assembly: RegisterImplementation(typeof(IMembershipActivityLogger), typeof(MembershipActivityLogger), Priority = RegistrationPriority.SystemDefault)]

namespace CMS.Activities.Loggers
{
    /// <summary>
    /// Provides methods for logging membership activities.
    /// </summary>
    public class MembershipActivityLogger : IMembershipActivityLogger
    {
        /// <summary>
        /// Logs login activity.
        /// </summary>
        /// <param name="userName">User name of current user</param>
        /// <param name="currentDocument">Current document tree representation, for MVC should be null</param>
        [Obsolete("Use CMS.Core.Service.Resolve<CMS.Activities.Loggers.IMembershipActivityLogger>().LogLogin(string, CMS.Base.ITreeNode) instead.")]
        public static void LogLogin(string userName, ITreeNode currentDocument = null)
        {
            Service.Resolve<IMembershipActivityLogger>().LogLogin(userName, currentDocument);
        }


        /// <summary>
        /// Logs registration activity.
        /// </summary>
        /// <param name="userName">User name of current user</param>
        /// <param name="currentDocument">Current document tree representation, for MVC should be null</param>
        /// <param name="checkViewModel"><c>True</c> if activities should not be logged in administration</param>
        [Obsolete("Use CMS.Core.Service.Resolve<CMS.Activities.Loggers.IMembershipActivityLogger>().LogLogin(string, CMS.Base.ITreeNode, bool) instead.")]
        public static void LogRegistration(string userName, ITreeNode currentDocument = null, bool checkViewModel = true)
        {
            Service.Resolve<IMembershipActivityLogger>().LogRegistration(userName, currentDocument, checkViewModel);
        }


        /// <summary>
        /// Logs login activity.
        /// </summary>
        /// <param name="userName">User name of current user</param>
        void IMembershipActivityLogger.LogLogin(string userName)
        {
            var current = (IMembershipActivityLogger)this;
            current.LogLogin(userName, null);
        }


        /// <summary>
        /// Logs login activity.
        /// </summary>
        /// <param name="userName">User name of current user</param>
        /// <param name="currentDocument">Current document tree representation, for MVC should be null</param>
        void IMembershipActivityLogger.LogLogin(string userName, ITreeNode currentDocument)
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
        void IMembershipActivityLogger.LogRegistration(string userName)
        {
            var current = (IMembershipActivityLogger)this;
            current.LogRegistration(userName, null);
        }

        
        /// <summary>
        /// Logs registration activity.
        /// </summary>
        /// <param name="userName">User name of current user</param>
        /// <param name="currentDocument">Current document tree representation, for MVC should be null</param>
        void IMembershipActivityLogger.LogRegistration(string userName, ITreeNode currentDocument)
        {
            var current = (IMembershipActivityLogger)this;
            current.LogRegistration(userName, currentDocument, true);
        }


        /// <summary>
        /// Logs registration activity.
        /// </summary>
        /// <param name="userName">User name of current user</param>
        /// <param name="currentDocument">Current document tree representation, for MVC should be null</param>
        /// <param name="checkViewModel"><c>True</c> if activities should not be logged in administration</param>
        void IMembershipActivityLogger.LogRegistration(string userName, ITreeNode currentDocument, bool checkViewModel)
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