using System;

using CMS.DataEngine;
using CMS.Membership;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Translates <see cref="UserInfo"/> to human readable format.
    /// </summary>
    public static class UserNameFormatter
    {
        /// <summary>
        /// Joins user name consisting of first, middle, last and e-mail address in one string.
        /// </summary>
        /// <param name="userId">User ID</param>
        public static string GetFriendlyUserName(int userId)
        {
            if (userId <= 0)
            {
                return null;
            }

            UserInfo ui = UserInfoProvider.GetUserInfo(userId);
            return GetFriendlyUserName(ui);
        }


        /// <summary>
        /// Joins user name consisting of first, middle, last and e-mail address in one string.
        /// </summary>
        /// <param name="ui">User info</param>
        public static string GetFriendlyUserName(UserInfo ui)
        {
            if (ui == null)
            {
                return null;
            }

            return GetFriendlyUserName(ui.FirstName, null, ui.LastName, ui.Email, ui.UserName);
        }


        /// <summary>
        /// Joins user name consisting of first, middle, last and e-mail address in one string.
        /// </summary>
        /// <param name="ui">User info</param>
        public static string GetFriendlyUserName(BaseInfo ui)
        {
            if (ui == null)
            {
                return null;
            }
            string firstName = ui.GetStringValue("FirstName", null);
            string lastName = ui.GetStringValue("LastName", null);
            string email = ui.GetStringValue("Email", null);
            string userName = ui.GetStringValue("UserName", null);
            return GetFriendlyUserName(firstName, null, lastName, email, userName);
        }


        /// <summary>
        /// Joins user name consisting of first, middle, last and e-mail address in one string.
        /// </summary>
        /// <param name="first">First name</param>
        /// <param name="middle">Middle name</param>
        /// <param name="last">Last name</param>
        /// <param name="emailAddress">E-mail address</param>
        /// <param name="userName">User name</param>
        public static string GetFriendlyUserName(string first, string middle, string last, string emailAddress, string userName)
        {
            string result = first;

            if (!String.IsNullOrEmpty(middle))
            {
                if (!String.IsNullOrEmpty(result))
                {
                    result += " ";
                }
                result += middle;
            }

            if (!String.IsNullOrEmpty(last))
            {
                if (!String.IsNullOrEmpty(result))
                {
                    result += " ";
                }
                result += last;
            }

            if (!String.IsNullOrEmpty(emailAddress))
            {
                if (!String.IsNullOrEmpty(result))
                {
                    result += " ";
                }
                result += "(" + emailAddress + ")";
            }

            if (String.IsNullOrEmpty(result))
            {
                result = userName;
            }

            return result;
        }
    }
}