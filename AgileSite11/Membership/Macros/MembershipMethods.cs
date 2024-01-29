using System;

using CMS;
using CMS.Base;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.SiteProvider;

[assembly: RegisterExtension(typeof(MembershipMethods), typeof(UserInfo))]

namespace CMS.Membership
{
    /// <summary>
    /// Membership methods - wrapping methods for macro resolver.
    /// </summary>
    public class MembershipMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns true if given user is granted with specified permission
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if given user is granted with specified permission.", 2)]
        [MacroMethodParam(0, "user", typeof(object), "User info object.")]
        [MacroMethodParam(1, "resource", typeof(string), "Resource name to test.")]
        [MacroMethodParam(2, "permission", typeof(string), "Permission to test.")]
        public static object IsAuthorizedPerResource(EvaluationContext context, params object[] parameters)
        {
            UserInfo ui = parameters[0] as UserInfo;
            if (ui != null)
            {
                String resource = ValidationHelper.GetString(parameters[1], String.Empty);
                String permission = ValidationHelper.GetString(parameters[2], String.Empty);

                return ui.IsAuthorizedPerResource(resource, permission, SiteContext.CurrentSiteName, false);
            }

            return false;
        }


        /// <summary>
        /// Returns true if current user is granted with permission to given UI element.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if current user is granted with permission to given UI element.", 2)]
        [MacroMethodParam(0, "user", typeof(object), "User info object.")]
        [MacroMethodParam(1, "resource", typeof(string), "Resource name to test.")]
        [MacroMethodParam(2, "elementName", typeof(string), "UI element name to test.")]
        public static object IsAuthorizedPerUIElement(EvaluationContext context, params object[] parameters)
        {
            CurrentUserInfo ui = parameters[0] as CurrentUserInfo;
            if (ui != null)
            {
                String resource = ValidationHelper.GetString(parameters[1], String.Empty);
                String elementName = ValidationHelper.GetString(parameters[2], String.Empty);

                return ui.IsAuthorizedPerUIElement(resource, elementName, false);
            }

            return false;
        }


        /// <summary>
        /// Returns true if user fulfils the required privilege level (the higher level contains all children: GlobalAdmin -> Admin -> Editor -> None).
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if user fulfils the required privilege level (the higher level contains all children: GlobalAdmin -> Admin -> Editor -> None)", 2)]
        [MacroMethodParam(0, "user", typeof(object), "User info object.")]
        [MacroMethodParam(1, "privilegeLevel", typeof(UserPrivilegeLevelEnum), "Required privilege level.")]
        public static object CheckPrivilegeLevel(EvaluationContext context, params object[] parameters)
        {
            UserInfo ui = parameters[0] as UserInfo;
            if (ui != null)
            {
                UserPrivilegeLevelEnum privilegeLevel = (UserPrivilegeLevelEnum)parameters[1];
                return ui.CheckPrivilegeLevel(privilegeLevel);
            }
            return false;
        }


        /// <summary>
        /// Tests whether user is in role
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if user is in role. If a period '.' is added as a prefix to the role code name, the method will only check global roles. Otherwise it checks both global and site roles.", 2)]
        [MacroMethodParam(0, "user", typeof (object), "User info object.")]
        [MacroMethodParam(1, "userRole", typeof (string), "Name(s) of the role(s) (separated with semicolon) to test whether user is in.")]
        [MacroMethodParam(2, "allRoles", typeof (bool), "If true, user has to be in all specified roles. If false, it is sufficient if the user is at least in one of the roles.")]
        public static object IsInRole(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length < 2)
            {
                throw new NotSupportedException();
            }

            // Get resolved user info from first parameter
            UserInfo ui = parameters[0] as UserInfo;
            if (ui != null)
            {
                string roles = ValidationHelper.GetString(parameters[1], "");
                if (roles.Contains(";"))
                {
                    bool all = (parameters.Length == 3 ? ValidationHelper.GetBoolean(parameters[2], false) : false);
                    string[] roleNames = roles.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string name in roleNames)
                    {
                        bool isInRole = ui.IsInRole(name, SiteContext.CurrentSiteName);
                        if (isInRole && !all)
                        {
                            return true;
                        }
                        if (!isInRole && all)
                        {
                            return false;
                        }
                    }

                    return all;
                }
                else
                {
                    return ui.IsInRole(roles, SiteContext.CurrentSiteName);
                }
            }

            return false;
        }


        /// <summary>
        /// Tests whether user is in group
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if user is in group.", 2)]
        [MacroMethodParam(0, "user", typeof(object), "User info object.")]
        [MacroMethodParam(1, "userGroup", typeof (string), "Name of the group to test whether user is in.")]
        public static object IsInGroup(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    // Get resolved user info from first parameter
                    UserInfo ui = parameters[0] as UserInfo;
                    CurrentUserInfo cui = MembershipContext.AuthenticatedUser;
                    if (ui != null)
                    {
                        // For caching reasons - use current user info, if user is the same
                        if ((cui == null) || (cui.UserID != ui.UserID))
                        {
                            cui = new CurrentUserInfo(ui, true);
                        }

                        return cui.IsGroupMember(parameters[1].ToString(), SiteContext.CurrentSiteName);
                    }

                    return false;

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Tests whether user has any membership
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if user has any membership.", 1)]
        [MacroMethodParam(0, "user", typeof (object), "User info object.")]
        public static object HasAnyMembership(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length < 1)
            {
                throw new NotSupportedException();
            }

            // Get resolved user info from first parameter
            UserInfo ui = parameters[0] as UserInfo;
            if (ui != null)
            {
                var membership = ui.MembershipsInternal;
                return (membership != null) && (membership.Count > 0);
            }

            return false;
        }


        /// <summary>
        /// Tests whether user has membership
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if user is in membership. If a period '.' is added as a prefix to the membership code name, the method will only check global memberships. Otherwise it checks both global and site memberships.", 2)]
        [MacroMethodParam(0, "user", typeof(object), "User info object.")]
        [MacroMethodParam(1, "userMemberships", typeof (string), "Name(s) of the membership(s) (separated with semicolon) to test whether user is in.")]
        [MacroMethodParam(2, "allMemberships", typeof (bool), "If true, user has to in all specified memberships. If false, it is sufficient if the user is at least in one of the memberships.")]
        public static object HasMembership(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length < 2)
            {
                throw new NotSupportedException();
            }

            // Get resolved user info from first parameter
            UserInfo ui = parameters[0] as UserInfo;
            if (ui != null)
            {
                string memberships = ValidationHelper.GetString(parameters[1], "");
                if (memberships.Contains(";"))
                {
                    bool all = (parameters.Length == 3 ? ValidationHelper.GetBoolean(parameters[2], false) : false);
                    string[] membershipNames = memberships.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string name in membershipNames)
                    {
                        bool isInMembership = ui.IsInMembership(name, SiteContext.CurrentSiteName);
                        if (isInMembership && !all)
                        {
                            return true;
                        }
                        if (!isInMembership && all)
                        {
                            return false;
                        }
                    }

                    return all;
                }
                else
                {
                    return ui.IsInMembership(memberships, SiteContext.CurrentSiteName);
                }
            }

            return false;
        }


        /// <summary>
        /// Returns formatted username in format: full name (nickname) if nickname specified otherwise full name (username).
        /// Allows you to customize how the usernames will look like throughout the admin UI. 
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns formatted username in format: full name (nickname) if nickname specified otherwise full name (username).", 1)]
        [MacroMethodParam(0, "user", typeof(object), "User info object.")]
        [MacroMethodParam(1, "isLiveSite", typeof(bool), "Indicates if returned username should be displayed on live site")]
        public static object GetFormattedUserName(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length < 1)
            {
                throw new NotSupportedException();
            }

            // Get resolved user info from first parameter
            UserInfo ui = parameters[0] as UserInfo;
            bool isLiveSite = false;
            if (parameters.Length > 1)
            {
                isLiveSite = ValidationHelper.GetBoolean(parameters[1], false);
            }

            if (ui != null)
            {
                return HTMLHelper.HTMLEncode(ui.GetFormattedUserName(isLiveSite));
            }

            return false;
        }
    }
}