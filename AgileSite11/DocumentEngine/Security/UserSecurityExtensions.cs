using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Security extensions for users
    /// </summary>
    public static class UserSecurityExtensions
    {
        /// <summary>
        /// Checks whether the user is authorized for the given document and permission, returns the authorization result. Doesn't check the allowed cultures of an user.
        /// </summary>
        /// <param name="user">User to check</param>
        /// <param name="nodeId">Document node id</param>
        /// <param name="permission">Permission to check</param>
        public static AuthorizationResultEnum IsAuthorizedPerTreeNode(this UserInfo user, int nodeId, NodePermissionsEnum permission)
        {
            return IsAuthorizedPerTreeNode(user, nodeId, permission, CultureHelper.GetPreferredCulture());
        }

        
        /// <summary>
        /// Checks whether the user is authorized for the given document and permission, returns the authorization result. Doesn't check the allowed cultures of an user.
        /// </summary>
        /// <param name="user">User to check</param>
        /// <param name="nodeId">Document node id</param>
        /// <param name="permission">Permission to check</param>
        /// <param name="culture">Document culture code. If the culture is null the user allowed cultures check is not performed</param>
        public static AuthorizationResultEnum IsAuthorizedPerTreeNode(this UserInfo user, int nodeId, NodePermissionsEnum permission, string culture)
        {
            return DocumentSecurityHelper.IsAuthorizedPerTreeNode(nodeId, permission, culture, user);
        }

        
        /// <summary>
        /// Checks whether the user is authorized for the given document and permission, returns the authorization result.
        /// </summary>
        /// <param name="user">User to check</param>
        /// <param name="node">Document node</param>
        /// <param name="permission">Permission to check</param>
        /// <param name="checkUserAllowedCultures">Indicates if the allowed cultures of the user should be checked</param>
        public static AuthorizationResultEnum IsAuthorizedPerTreeNode(this UserInfo user, TreeNode node, NodePermissionsEnum permission, bool checkUserAllowedCultures = true)
        {
            return IsAuthorizedPerTreeNode(user, node, permission, null, checkUserAllowedCultures);
        }


        /// <summary>
        /// Checks whether the user is authorized for the given document and permission, returns the authorization result. Doesn't check the allowed cultures of an user.
        /// </summary>
        /// <param name="user">User to check</param>
        /// <param name="node">Document node</param>
        /// <param name="permission">Permission to check</param>
        /// <param name="culture">Document culture code. If the culture is null the user allowed cultures check is not performed</param>
        public static AuthorizationResultEnum IsAuthorizedPerTreeNode(this UserInfo user, TreeNode node, NodePermissionsEnum permission, string culture)
        {
            return DocumentSecurityHelper.IsAuthorizedPerTreeNode(node, permission, culture, user);
        }
       

        /// <summary>
        /// Checks whether the user is authorized for the given document and permission, returns the authorization result.
        /// </summary>
        /// <param name="user">User to check</param>
        /// <param name="node">Document node</param>
        /// <param name="permission">Permission to check</param>
        /// <param name="culture">Culture of the document to be checked</param>
        /// <param name="checkUserAllowedCultures">Indicates if the allowed cultures of the user should be checked</param>
        public static AuthorizationResultEnum IsAuthorizedPerTreeNode(this UserInfo user, TreeNode node, NodePermissionsEnum permission, string culture, bool checkUserAllowedCultures)
        {
            return DocumentSecurityHelper.IsAuthorizedPerTreeNode(node, permission, culture, checkUserAllowedCultures, user);
        }


        /// <summary>
        /// Checks whether the user is authorized to create new document.
        /// </summary>
        /// <param name="user">User to check</param>
        /// <param name="parentNodeId">Parent node id</param>
        /// <param name="documentClassName">Document class name</param>
        public static bool IsAuthorizedToCreateNewDocument(this UserInfo user, int parentNodeId, string documentClassName)
        {
            return DocumentSecurityHelper.IsAuthorizedToCreateNewDocument(parentNodeId, documentClassName, CultureHelper.GetPreferredCulture(), user);
        }


        /// <summary>
        /// Checks whether the user is authorized to create new document.
        /// </summary>
        /// <param name="user">User to check</param>
        /// <param name="parentNode">Parent node</param>
        /// <param name="documentClassName">Document class name</param>
        public static bool IsAuthorizedToCreateNewDocument(this UserInfo user, TreeNode parentNode, string documentClassName)
        {
            return DocumentSecurityHelper.IsAuthorizedToCreateNewDocument(parentNode, documentClassName, CultureHelper.GetPreferredCulture(), user);
        }


        /// <summary>
        /// Checks if the user is authorized for specified document (Check all CONTENT, CLASS and Document type permissions.)
        /// </summary>
        /// <param name="user">User to check</param>
        /// <param name="node">Document node</param>
        /// <param name="permission">Permission</param>
        /// <param name="checkUserAllowedCultures">Indicates if the allowed cultures of the user should be checked</param>
        /// <param name="culture">Culture to check</param>
        public static AuthorizationResultEnum IsAuthorizedPerDocument(this UserInfo user, TreeNode node, NodePermissionsEnum permission, bool checkUserAllowedCultures = true, string culture = null)
        {
            if (culture == null)
            {
                culture = CultureHelper.GetPreferredCulture();
            }

            return DocumentSecurityHelper.IsAuthorizedPerDocument(node, permission, checkUserAllowedCultures, culture, user);
        }


        /// <summary>
        /// Checks if the user is authorized for specified document (Check all CONTENT, CLASS and Document type permissions.
        /// </summary>
        /// <param name="user">User to check</param>
        /// <param name="nodeId">ID of node</param>
        /// <param name="className">Class name of the document</param>
        /// <param name="permission">Permission</param>
        public static AuthorizationResultEnum IsAuthorizedPerDocument(this UserInfo user, int nodeId, string className, NodePermissionsEnum permission)
        {
            return DocumentSecurityHelper.IsAuthorizedPerDocument(nodeId, className, permission, CultureHelper.GetPreferredCulture(), user);
        }


        /// <summary>
        /// Checks if the user is authorized for specified document (Check all CONTENT, CLASS and Document type permissions.
        /// </summary>
        /// <param name="user">User to check</param>
        /// <param name="node">Document node</param>
        /// <param name="permissions">Permissions to check</param>
        /// <param name="checkUserAllowedCultures">Indicates if the allowed cultures of the user should be checked</param>
        /// <param name="targetCultureCode">Culture code of the document which should be checked. If null or empty user preferred culture is used instead.</param>
        public static AuthorizationResultEnum IsAuthorizedPerDocument(this UserInfo user, TreeNode node, NodePermissionsEnum[] permissions, bool checkUserAllowedCultures = true, string targetCultureCode = null)
        {
            return DocumentSecurityHelper.IsAuthorizedPerDocument(node, permissions, checkUserAllowedCultures, String.IsNullOrEmpty(targetCultureCode) ? CultureHelper.GetPreferredCulture() : targetCultureCode, user);
        }
    }
}