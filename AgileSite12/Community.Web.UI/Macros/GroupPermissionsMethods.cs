using System;

using CMS;
using CMS.Community.Web.UI;
using CMS.MacroEngine;
using CMS.Membership;

[assembly: RegisterExtension(typeof(GroupPermissionsMethods), typeof(UserInfo))]

namespace CMS.Community.Web.UI
{
    /// <summary>
    /// Container for Group permissions related macro methods.
    /// </summary>
    internal class GroupPermissionsMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns <c>true</c> if the user is administrator of the group or the user has given permission 
        /// for Groups module on the given site. If given site is different from the site the group belongs to, it returns <c>false</c>.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), @"Returns true if the user is administrator of the group given by groupId parameter or the user has permission given by permissionName parameter 
for Groups module on the given site. Otherwise returns false", 3, IsHidden = true)]
        [MacroMethodParam(0, "user", typeof(object), "User info object.")]
        [MacroMethodParam(1, "groupId", typeof(int), "Community group ID.")]
        [MacroMethodParam(2, "permissionName", typeof(string), "Permission name.")]
        [MacroMethodParam(3, "siteId", typeof(int), "Site ID.")]
        public static object IsAuthorizedPerGroup(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length < 4)
            {
                throw new NotSupportedException();
            }

            var userInfo = parameters[0] as UserInfo;

            if (userInfo == null)
            {
                return false;
            }

            int groupId = GetIntParam(parameters[1]);
            string permissionName = GetStringParam(parameters[2]);
            int siteId = GetIntParam(parameters[3]);

            return GroupSecurityHelper.IsUserAuthorizedPerGroup(userInfo, groupId, permissionName, siteId);
        }
    }
}
