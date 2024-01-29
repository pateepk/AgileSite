using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Provides utility methods for <see cref="UserMacroIdentityInfo"/>.
    /// </summary>
    public class UserMacroIdentityHelper : AbstractHelper<UserMacroIdentityHelper>
    {
        /// <summary>
        /// Assigns <see cref="MacroIdentityInfo"/> identified by <paramref name="macroIdentityId"/> to <paramref name="userInfo"/>.
        /// Clears user's identity when <paramref name="macroIdentityId"/> is 0.
        /// </summary>
        /// <param name="userInfo">User whom to assign the macro identity.</param>
        /// <param name="macroIdentityId">Macro identity identifier.</param>
        public static void SetMacroIdentity(IUserInfo userInfo, int macroIdentityId)
        {
            HelperObject.SetMacroIdentityInternal(userInfo, macroIdentityId);
        }


        /// <summary>
        /// Assigns <see cref="MacroIdentityInfo"/> identified by <paramref name="macroIdentityId"/> to <paramref name="userInfo"/>.
        /// Clears user's identity when <paramref name="macroIdentityId"/> is 0.
        /// </summary>
        /// <param name="userInfo">User whom to assign the macro identity.</param>
        /// <param name="macroIdentityId">Macro identity identifier.</param>
        protected virtual void SetMacroIdentityInternal(IUserInfo userInfo, int macroIdentityId)
        {
            if (macroIdentityId == 0)
            {
                UserMacroIdentityInfoProvider.GetUserMacroIdentityInfo(userInfo)?.Delete();
            }
            else
            {
                using (var transaction = new CMSLateBoundTransaction())
                { 
                    var userMacroIdentity = UserMacroIdentityInfoProvider.GetUserMacroIdentityInfo(userInfo) ?? new UserMacroIdentityInfo { UserMacroIdentityUserID = userInfo.UserID };
                    userMacroIdentity.UserMacroIdentityMacroIdentityID = macroIdentityId;
                    UserMacroIdentityInfoProvider.SetUserMacroIdentityInfo(userMacroIdentity);

                    transaction.Commit();
                }
            }
        }
    }
}
