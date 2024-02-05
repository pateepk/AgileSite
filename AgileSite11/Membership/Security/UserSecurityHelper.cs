using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Modules;
using CMS.Base;

namespace CMS.Membership
{
    /// <summary>
    /// Helper class to check the user security
    /// </summary>
    public static class UserSecurityHelper
    {
        /// <summary>
        /// Checks whether the user is authorized for given object with given permission.
        /// </summary>
        /// <param name="permission">Permission to check</param>
        /// <param name="obj">Object to check</param>
        /// <param name="siteName">Site name</param>
        /// <param name="userInfo">User to check</param>
        /// <param name="exceptionOnFailure">If true, <see cref="PermissionCheckException"/> is thrown whenever a permission check fails</param>
        /// <exception cref="PermissionCheckException">Thrown when permission check fail and exception is allowed by <paramref name="exceptionOnFailure"/> parameter.</exception>
        public static bool IsAuthorizedPerObject(PermissionsEnum permission, BaseInfo obj, string siteName, UserInfo userInfo, bool exceptionOnFailure = false)
        {
            if (obj == null)
            {
                return false;
            }

            // Reserve the log item
            DataRow dr = SecurityDebug.StartSecurityOperation("IsAuthorizedPerObject");

            bool result = false;

            // Do not allow in read-only context
            if (!MembershipContext.AllowOnlyRead || IsReadOnlyPermission(permission))
            {
                result = userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin) || UserInfoProvider.IsAuthorizedPerObject(obj, permission, siteName, userInfo, exceptionOnFailure);
            }

            // Log the operation
            if (dr != null)
            {
                string permissionName = GetPermissionName(permission);
                SecurityDebug.FinishSecurityOperation(dr, userInfo.UserName, obj.TypeInfo.ObjectType, permissionName, result, siteName);
            }

            return result;
        }


        /// <summary>
        /// Checks whether the user is authorized per object with given object type and permission.
        /// </summary>
        /// <param name="permission">Permission to check</param>
        /// <param name="objectType">Object type</param>
        /// <param name="siteName">Site name</param>
        /// <param name="userInfo">User to check</param>
        /// <param name="exceptionOnFailure">If true, <see cref="PermissionCheckException"/> is thrown whenever a permission check fails</param>
        /// <exception cref="PermissionCheckException">Thrown when permission check fail and exception is allowed by <paramref name="exceptionOnFailure"/> parameter.</exception>
        public static bool IsAuthorizedPerObject(PermissionsEnum permission, string objectType, string siteName, UserInfo userInfo, bool exceptionOnFailure = false)
        {
            // Reserve the log item
            DataRow dr = SecurityDebug.StartSecurityOperation("IsAuthorizedPerObject");

            bool result = false;

            // Do not allow in read-only context
            if (!MembershipContext.AllowOnlyRead || IsReadOnlyPermission(permission))
            {
                result = userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || UserInfoProvider.IsAuthorizedPerObject(objectType, permission, siteName, userInfo, exceptionOnFailure);
            }

            // Log the operation
            if (dr != null)
            {
                string permissionName = GetPermissionName(permission);
                SecurityDebug.FinishSecurityOperation(dr, userInfo.UserName, objectType, permissionName, result, siteName);
            }

            return result;
        }


        /// <summary>
        /// Checks whether the user is authorized per meta file for given object type and permission.
        /// </summary>
        /// <param name="permission">Permission to check</param>
        /// <param name="objectType">Object type</param>
        /// <param name="siteName">Site name</param>
        /// <param name="userInfo">User to check</param>
        public static bool IsAuthorizedPerMetaFile(PermissionsEnum permission, string objectType, string siteName, UserInfo userInfo)
        {
            // Reserve the log item
            DataRow dr = SecurityDebug.StartSecurityOperation("IsAuthorizedPerMetaFile");

            bool result = false;

            // Do not allow in read-only context
            if (!MembershipContext.AllowOnlyRead || IsReadOnlyPermission(permission))
            {
                result = userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || UserInfoProvider.IsAuthorizedPerMetaFile(objectType, permission, siteName, userInfo);
            }

            // Log the operation
            if (dr != null)
            {
                string permissionName = GetPermissionName(permission);
                SecurityDebug.FinishSecurityOperation(dr, userInfo.UserName, objectType, permissionName, result, siteName);
            }

            return result;
        }


        /// <summary>
        /// Checks whether the user is authorized for given resource name and permission, returns true if so.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="permissionName">Permission name to check</param>
        /// <param name="siteName">Site name</param>
        /// <param name="userInfo">User to check</param>
        /// <param name="exceptionOnFailure">If true, <see cref="PermissionCheckException"/> is thrown whenever a permission check fails</param>
        /// <exception cref="PermissionCheckException">Thrown when permission check fail and exception is allowed by <paramref name="exceptionOnFailure"/> parameter.</exception>
        public static bool IsAuthorizedPerResource(string resourceName, string permissionName, string siteName, UserInfo userInfo, bool exceptionOnFailure = false)
        {
            // Reserve the log item
            DataRow dr = SecurityDebug.StartSecurityOperation("IsAuthorizedPerResource");
            bool result = false;

            // Do not allow in read-only context
            if (!MembershipContext.AllowOnlyRead || IsReadOnlyPermission(permissionName))
            {
                result = userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin) || UserInfoProvider.IsAuthorizedPerResource(resourceName, permissionName, siteName, userInfo);
            }

            // Log the operation
            if (dr != null)
            {
                SecurityDebug.FinishSecurityOperation(dr, userInfo.UserName, resourceName, permissionName, result, siteName);
            }

            if (exceptionOnFailure && !result)
            {
                throw new PermissionCheckException(resourceName, permissionName, siteName);
            }

            return result;
        }


        /// <summary>
        /// Returns true if the given permission is a read-only permission
        /// </summary>
        /// <param name="permission">Permission</param>
        private static bool IsReadOnlyPermission(PermissionsEnum permission)
        {
            return (permission == PermissionsEnum.Read);
        }


        /// <summary>
        /// Returns true if the given permission is a read-only permission
        /// </summary>
        /// <param name="permissionName">Permission name</param>
        private static bool IsReadOnlyPermission(string permissionName)
        {
            // Permission is considered a read permission in case it contains the "read" word
            return
                (permissionName.IndexOfCSafe("read", true) >= 0) ||
                (permissionName.IndexOfCSafe("explore", true) >= 0) ||
                (permissionName.IndexOfCSafe("view", true) >= 0);
        }


        /// <summary>
        /// Checks whether the user is authorized for given UI element of the specified resource, returns true if so.
        /// </summary>
        /// <param name="resourceName">Resource code name</param>
        /// <param name="elementName">UI element code name</param>
        /// <param name="siteAvailabilityRequired">Indicates if site availability of the corresponding resource (resource with name in format "cms.[ElementName]") is required. Takes effect only when corresponding resource exists</param>        
        /// <param name="siteName">Site code name</param>
        /// <param name="userInfo">User to check</param>
        public static bool IsAuthorizedPerUIElement(string resourceName, string elementName, bool siteAvailabilityRequired, string siteName, UserInfo userInfo)
        {
            // Reserve the log item
            DataRow dr = SecurityDebug.StartSecurityOperation("IsAuthorizedPerUIElement");

            bool authorized = false;

            if (UserInfoProvider.IsAuthorizedPerUIElement(resourceName, elementName, siteName, userInfo))
            {
                authorized = true;

                if (siteAvailabilityRequired)
                {
                    // Check module availability for the specified site                                                
                    authorized = ResourceSiteInfoProvider.IsResourceOnSite(resourceName, siteName);
                }
            }

            // Log the operation
            if (dr != null)
            {
                SecurityDebug.FinishSecurityOperation(dr, userInfo.UserName, resourceName, elementName, authorized, siteName);
            }

            return authorized;
        }


        /// <summary>
        /// Checks whether the user is authorized for given resource name and UIElements, returns true if so.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="elementNames">UIElement names to check</param>
        /// <param name="siteName">Site name</param>
        /// <param name="userInfo">User to check</param>
        public static bool IsAuthorizedPerUIElement(string resourceName, string[] elementNames, string siteName, UserInfo userInfo)
        {
            // Reserve the log item
            DataRow dr = SecurityDebug.StartSecurityOperation("IsAuthorizedPerUIElement");

            bool authorized = UserInfoProvider.IsAuthorizedPerUIElement(resourceName, elementNames, siteName, userInfo);

            // Log the operation
            if (dr != null)
            {
                SecurityDebug.FinishSecurityOperation(dr, userInfo.UserName, resourceName, String.Join(";", elementNames), authorized, siteName);
            }

            return authorized;
        }


        /// <summary>
        /// Checks whether the user is authorized for given class name and permission, returns true if so.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="permissionName">Permission name to check</param>
        /// <param name="siteName">Site name</param>
        /// <param name="userInfo">User to check</param>
        /// <param name="exceptionOnFailure">If true, <see cref="PermissionCheckException"/> is thrown whenever a permission check fails</param>
        /// <exception cref="PermissionCheckException">Thrown when permission check fail and exception is allowed by <paramref name="exceptionOnFailure"/> parameter.</exception>
        public static bool IsAuthorizedPerClassName(string className, string permissionName, string siteName, UserInfo userInfo, bool exceptionOnFailure = false)
        {
            // Reserve the log item
            DataRow dr = SecurityDebug.StartSecurityOperation("IsAuthorizedPerClassName");

            bool result = false;

            // Do not allow in read-only context
            if (!MembershipContext.AllowOnlyRead || IsReadOnlyPermission(permissionName))
            {
                result = userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || UserInfoProvider.IsAuthorizedPerClass(className, permissionName, siteName, userInfo, exceptionOnFailure);
            }

            // Log the operation
            if (dr != null)
            {
                SecurityDebug.FinishSecurityOperation(dr, userInfo.UserName, className, permissionName, result, siteName);
            }

            return result;
        }


        /// <summary>
        /// Returns name of the permission specified by the enumeration value.
        /// </summary>
        /// <param name="permission">Enumeration value of the permission</param>
        public static string GetPermissionName(PermissionsEnum permission)
        {
            string permissionName = null;

            switch (permission)
            {
                case PermissionsEnum.Read:
                    permissionName = "read";
                    break;

                case PermissionsEnum.Modify:
                    permissionName = "modify";
                    break;

                case PermissionsEnum.Create:
                    permissionName = "create";
                    break;

                case PermissionsEnum.Delete:
                    permissionName = "delete";
                    break;
            }

            return permissionName;
        }
    }
}
