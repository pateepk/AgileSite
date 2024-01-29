using System;
using System.Collections.Generic;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Helper class to check the document security
    /// </summary>
    public static class DocumentSecurityHelper
    {
        #region "Constants"

        /// <summary>
        /// Storage key used for grouping user's authorization per node.
        /// </summary>
        private const string IS_AUTHORIZED_PER_NODE_STORAGE_KEY = "IsAuthorizedPerNode";

        #endregion


        #region "Public methods"


        /// <summary>
        /// Determines whether current user is authorized to access Content module.
        /// </summary>
        /// <param name="siteName">Site name to check</param>
        /// <param name="user">User info</param>
        public static bool IsUserAuthorizedPerContent(String siteName, UserInfo user)
        {
            // Check 'Explore tree' module permission
            if (user.IsAuthorizedPerResource("CMS.Content", "ExploreTree", siteName))
            {
                return true;
            }

            string startingPath = (String.IsNullOrEmpty(user.UserStartingAliasPath) ? "/" : user.UserStartingAliasPath);
            TreeProvider tp = new TreeProvider(MembershipContext.AuthenticatedUser);
            TreeNode rootNode = tp.SelectSingleNode(siteName, startingPath, TreeProvider.ALL_CULTURES, false, null, false);

            // Check 'Explore tree' permission for root node
            if (rootNode != null)
            {
                return (user.IsAuthorizedPerTreeNode(rootNode, NodePermissionsEnum.ExploreTree) == AuthorizationResultEnum.Allowed);
            }

            return false;
        }


        /// <summary>
        /// Converts the given permission to the node permission
        /// </summary>
        /// <param name="permission">Permission to convert</param>
        public static NodePermissionsEnum GetNodePermissionEnum(PermissionsEnum permission)
        {
            switch (permission)
            {
                case PermissionsEnum.Create:
                    return NodePermissionsEnum.Create;

                case PermissionsEnum.Delete:
                    return NodePermissionsEnum.Delete;

                case PermissionsEnum.Destroy:
                    return NodePermissionsEnum.Destroy;

                case PermissionsEnum.Modify:
                    return NodePermissionsEnum.Modify;

                case PermissionsEnum.Read:
                    return NodePermissionsEnum.Read;

                default:
                    throw new Exception("[DocumentSecurityHelper.GetNodePermissionEnum]: Conversion of this permission to node permission is not supported.");
            }
        }


        /// <summary>
        /// Checks whether the user is authorized for the given document and permission, returns the authorization result.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="permission">Permission to check</param>
        /// <param name="culture">Culture of the document to be checked</param>
        /// <param name="checkUserAllowedCultures">Indicates if the allowed cultures of the user should be checked</param>
        /// <param name="userInfo">User to check</param>
        public static AuthorizationResultEnum IsAuthorizedPerTreeNode(TreeNode node, NodePermissionsEnum permission, string culture, bool checkUserAllowedCultures, UserInfo userInfo)
        {
            if (node == null)
            {
                return AuthorizationResultEnum.Insignificant;
            }

            // For virtual context, only read, browse tree permissions are allowed to check
            if (VirtualContext.IsUserInitialized && (permission != NodePermissionsEnum.Read) && (permission != NodePermissionsEnum.ExploreTree))
            {
                return AuthorizationResultEnum.Denied;
            }

            // Reserve the log item
            DataRow dr = SecurityDebug.StartSecurityOperation("IsAuthorizedPerTreeNode");

            AuthorizationResultEnum result = TreeSecurityProvider.IsAuthorizedPerNode(node, permission, userInfo, checkUserAllowedCultures, culture);

            // Log the operation
            if (dr != null)
            {
                SecurityDebug.FinishSecurityOperation(dr, userInfo.UserName, node.NodeID.ToString(), null, result, null);
            }

            return result;
        }


        /// <summary>
        /// Checks whether the user is authorized for the given document and permission, returns the authorization result. Doesn't check the allowed cultures of an user.
        /// </summary>
        /// <param name="nodeId">Document node id</param>
        /// <param name="permission">Permission to check</param>
        /// <param name="culture">Document culture code. If the culture is null the user allowed cultures check is not performed</param>
        /// <param name="userInfo">User to check</param>
        public static AuthorizationResultEnum IsAuthorizedPerTreeNode(int nodeId, NodePermissionsEnum permission, string culture, UserInfo userInfo)
        {
            // Reserve the log item
            DataRow dr = SecurityDebug.StartSecurityOperation("IsAuthorizedPerTreeNode");

            AuthorizationResultEnum result;

            // For virtual context, only read, browse tree permissions are allowed to check
            if (VirtualContext.IsUserInitialized && (permission != NodePermissionsEnum.Read) && (permission != NodePermissionsEnum.ExploreTree))
            {
                return AuthorizationResultEnum.Denied;
            }

            // If using custom handlers, regular authentication
            if (DocumentEvents.AuthorizeDocument.IsBound)
            {
                // Get the document
                TreeProvider treeProvider = new TreeProvider();
                TreeNode node = treeProvider.SelectSingleNode(nodeId);

                result = IsAuthorizedPerTreeNode(node, permission, culture, true, userInfo);
            }
            else
            {
                // Get from the request storage for duplicate results
                string resultKey = TreeSecurityProvider.GetAuthorizationResultKey(userInfo, nodeId, permission, culture);

                // Try to get from request
                if (RequestStockHelper.Contains(IS_AUTHORIZED_PER_NODE_STORAGE_KEY, resultKey, false))
                {
                    result = (AuthorizationResultEnum)RequestStockHelper.GetItem(IS_AUTHORIZED_PER_NODE_STORAGE_KEY, resultKey, false);
                }
                else
                {
                    // Global administrator is allowed for everything
                    if (userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                    {
                        return AuthorizationResultEnum.Allowed;
                    }

                    // Without custom handlers - fast authentication (only with tree data)
                    QueryDataParameters parameters = new QueryDataParameters();
                    parameters.Add("@ID", nodeId);

                    // Get the data
                    DataSet ds = ConnectionHelper.ExecuteQuery("cms.tree.select", parameters);

                    // Get node data row
                    DataRow drNode = null;
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        drNode = ds.Tables[0].Rows[0];
                    }

                    result = TreeSecurityProvider.IsAuthorizedPerNodeData(drNode, permission, userInfo, culture);

                    // Save the result and return
                    RequestStockHelper.AddToStorage(IS_AUTHORIZED_PER_NODE_STORAGE_KEY, resultKey, result, false);
                }
            }

            // Log the operation
            if (dr != null)
            {
                SecurityDebug.FinishSecurityOperation(dr, userInfo.UserName, nodeId.ToString(), null, result, null);
            }

            return result;
        }


        /// <summary>
        /// Checks whether the user is authorized for the given document and permission, returns the authorization result. Doesn't check the allowed cultures of an user.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="permission">Permission to check</param>
        /// <param name="culture">Document culture code. If the culture is null the user allowed cultures check is not performed</param>
        /// <param name="userInfo">User to check</param>
        public static AuthorizationResultEnum IsAuthorizedPerTreeNode(TreeNode node, NodePermissionsEnum permission, string culture, UserInfo userInfo)
        {
            if (node == null)
            {
                return AuthorizationResultEnum.Insignificant;
            }

            // Reserve the log item
            DataRow dr = SecurityDebug.StartSecurityOperation("IsAuthorizedPerTreeNode");

            AuthorizationResultEnum result;

            // For virtual context, only read, browse tree permissions are allowed to check
            if (VirtualContext.IsUserInitialized && (permission != NodePermissionsEnum.Read) && (permission != NodePermissionsEnum.ExploreTree))
            {
                return AuthorizationResultEnum.Denied;
            }

            // If using custom handlers, regular authentication
            if (DocumentEvents.AuthorizeDocument.IsBound)
            {
                result = IsAuthorizedPerTreeNode(node, permission, culture, true, userInfo);
            }
            else
            {
                // Get from the request storage for duplicate results
                string resultKey = TreeSecurityProvider.GetAuthorizationResultKey(userInfo, node.NodeID, permission, culture);

                // Try to get from request
                if (RequestStockHelper.Contains(IS_AUTHORIZED_PER_NODE_STORAGE_KEY, resultKey, false))
                {
                    result = (AuthorizationResultEnum)RequestStockHelper.GetItem(IS_AUTHORIZED_PER_NODE_STORAGE_KEY, resultKey, false);
                }
                else
                {
                    result = TreeSecurityProvider.IsAuthorizedPerNodeData(node, permission, userInfo, culture);

                    // Save the result and return
                    RequestStockHelper.AddToStorage(IS_AUTHORIZED_PER_NODE_STORAGE_KEY, resultKey, result, false);
                }
            }

            // Log the operation
            if (dr != null)
            {
                SecurityDebug.FinishSecurityOperation(dr, userInfo.UserName, node.NodeID.ToString(), null, result, null);
            }

            return result;
        }


        /// <summary>
        /// Checks if the user is authorized for specified document (Checks all CONTENT, Document type and ACL permissions). Can handle also empty instance of TreeNode.
        /// For empty instance are not evaluated ACL permissions and document type permissions.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="permission">Permission</param>
        /// <param name="checkUserAllowedCultures">Indicates if the allowed cultures of the user should be checked</param>
        /// <param name="culture">Culture to check</param>
        /// <param name="userInfo">User to check</param>
        /// <param name="siteName">Site name to check</param>
        internal static AuthorizationResultEnum IsAuthorizedPerDocument(TreeNode node, NodePermissionsEnum permission, bool checkUserAllowedCultures, string culture, UserInfo userInfo, string siteName)
        {
            if (node == null)
            {
                return AuthorizationResultEnum.Allowed;
            }

            // Perform check for document's culture (for existing documents)
            if (checkUserAllowedCultures && (permission != NodePermissionsEnum.Create))
            {
                culture = node.DocumentCulture;
            }

            // Reserve the log item
            DataRow dr = SecurityDebug.StartSecurityOperation("IsAuthorizedPerDocument");

            AuthorizationResultEnum result = AuthorizationResultEnum.Insignificant;

            // For virtual context, only read, browse tree permissions are allowed to check
            if (VirtualContext.IsUserInitialized && (permission != NodePermissionsEnum.Read) && (permission != NodePermissionsEnum.ExploreTree))
            {
                result = AuthorizationResultEnum.Denied;
            }
            else
            {
                string permissionName = TreeSecurityProvider.GetPermissionName(permission);

                // Global administrator is allowed for everything
                if (userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                {
                    result = AuthorizationResultEnum.Allowed;
                }
                else
                {
                    // Check content permissions - only positive
                    if (UserSecurityHelper.IsAuthorizedPerResource("CMS.Content", permissionName, siteName, userInfo))
                    {
                        result = AuthorizationResultEnum.Allowed;
                    }

                    if (!String.IsNullOrEmpty(node.NodeClassName))
                    {
                        // Check document type permissions - only positive
                        if (UserSecurityHelper.IsAuthorizedPerClassName(node.NodeClassName, permissionName, siteName, userInfo))
                        {
                            result = AuthorizationResultEnum.Allowed;
                        }
                    }

                    // Evaluate ACL permissions only for not empty TreeNode 
                    if (node.GetValue("NodeACLID", 0) > 0)
                    {
                        // Check document type permission
                        switch (IsAuthorizedPerTreeNode(node, permission, culture, checkUserAllowedCultures, userInfo))
                        {
                            // Explicitly allowed
                            case AuthorizationResultEnum.Allowed:
                                result = AuthorizationResultEnum.Allowed;
                                break;

                            // Explicitly denied
                            case AuthorizationResultEnum.Denied:
                            case AuthorizationResultEnum.Insignificant:
                                result = AuthorizationResultEnum.Denied;
                                break;
                        }
                    }
                }
            }

            // Log the operation
            if (dr != null)
            {
                SecurityDebug.FinishSecurityOperation(dr, userInfo.UserName, node.NodeID.ToString(), node.NodeClassName, result, null);
            }

            return result;
        }


        /// <summary>
        /// Checks if the user is authorized for specified document (Checks all CONTENT, Document type and ACL permissions).
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="permission">Permission</param>
        /// <param name="checkUserAllowedCultures">Indicates if the allowed cultures of the user should be checked</param>
        /// <param name="culture">Culture to check</param>
        /// <param name="userInfo">User to check</param>
        public static AuthorizationResultEnum IsAuthorizedPerDocument(TreeNode node, NodePermissionsEnum permission, bool checkUserAllowedCultures, string culture, UserInfo userInfo)
        {
            var siteName = (node != null) ? node.NodeSiteName : null;

            return IsAuthorizedPerDocument(node, permission, checkUserAllowedCultures, culture, userInfo, siteName);
        }


        /// <summary>
        /// Checks if the user is authorized for specified document (Checks all CONTENT, Document type and ACL permissions).
        /// </summary>
        /// <param name="nodeId">ID of node</param>
        /// <param name="className">Class name of the document</param>
        /// <param name="permission">Permission</param>
        /// <param name="culture">Culture to check</param>
        /// <param name="userInfo">User to check</param>
        public static AuthorizationResultEnum IsAuthorizedPerDocument(int nodeId, string className, NodePermissionsEnum permission, string culture, UserInfo userInfo)
        {
            // Reserve the log item
            DataRow dr = SecurityDebug.StartSecurityOperation("IsAuthorizedPerDocument");

            AuthorizationResultEnum result = AuthorizationResultEnum.Insignificant;

            // For virtual context, only read, browse tree permissions are allowed to check
            if (VirtualContext.IsUserInitialized && (permission != NodePermissionsEnum.Read) && (permission != NodePermissionsEnum.ExploreTree))
            {
                result = AuthorizationResultEnum.Denied;
            }
            else
            {
                // Global administrator is allowed for everything
                if (userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                {
                    result = AuthorizationResultEnum.Allowed;
                }
                else
                {
                    // Check document permissions
                    switch (IsAuthorizedPerTreeNode(nodeId, permission, culture, userInfo))
                    {
                        // Explicitly allowed
                        case AuthorizationResultEnum.Allowed:
                            result = AuthorizationResultEnum.Allowed;
                            break;

                        // Explicitly denied
                        case AuthorizationResultEnum.Denied:
                        case AuthorizationResultEnum.Insignificant:
                            result = AuthorizationResultEnum.Denied;
                            break;
                    }
                }
            }

            // Log the operation
            if (dr != null)
            {
                SecurityDebug.FinishSecurityOperation(dr, userInfo.UserName, nodeId.ToString(), className, result, null);
            }

            return result;
        }


        /// <summary>
        /// Checks if the user is authorized for specified document (Checks all CONTENT, Document type and ACL permissions).
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="permissions">Permissions to check</param>
        /// <param name="checkUserAllowedCultures">Indicates if the allowed cultures of the user should be checked</param>
        /// <param name="culture">Culture to check</param>
        /// <param name="userInfo">User to check</param>
        public static AuthorizationResultEnum IsAuthorizedPerDocument(TreeNode node, NodePermissionsEnum[] permissions, bool checkUserAllowedCultures, string culture, UserInfo userInfo)
        {
            // Reserve the log item
            DataRow dr = SecurityDebug.StartSecurityOperation("IsAuthorizedPerDocument");

            AuthorizationResultEnum result = AuthorizationResultEnum.Insignificant;

            string siteName = (node != null) ? node.NodeSiteName : SiteContext.CurrentSiteName;

            // Global administrator is allowed for everything
            if (userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                result = AuthorizationResultEnum.Allowed;
            }
            else
            {
                // Check all the permissions
                bool insignificant = false;
                foreach (NodePermissionsEnum permission in permissions)
                {
                    AuthorizationResultEnum documentResult = IsAuthorizedPerDocument(node, permission, checkUserAllowedCultures, culture, userInfo);
                    if (documentResult == AuthorizationResultEnum.Denied)
                    {
                        // Denied for some permission
                        result = AuthorizationResultEnum.Denied;
                        break;
                    }
                    else if (documentResult == AuthorizationResultEnum.Allowed)
                    {
                        // Allowed
                        result = AuthorizationResultEnum.Allowed;
                    }
                    else
                    {
                        insignificant = true;
                    }
                }

                // If one was insignificant, all result is insignificant (worst case scenario)
                if (insignificant)
                {
                    result = AuthorizationResultEnum.Insignificant;
                }
            }

            // Log the operation
            if (dr != null)
            {
                var nodeId = node != null ? node.NodeID : 0;
                var nodeClassName = node != null ? node.NodeClassName : null;
                SecurityDebug.FinishSecurityOperation(dr, userInfo.UserName, nodeId.ToString(), nodeClassName, result, siteName);
            }

            return result;
        }


        /// <summary>
        /// Checks whether the user is authorized to create new document.
        /// </summary>
        /// <param name="parentNode">Parent node</param>
        /// <param name="documentClassName">Document class name</param>
        /// <param name="culture">Culture</param>
        /// <param name="userInfo">User to check</param>
        public static bool IsAuthorizedToCreateNewDocument(TreeNode parentNode, string documentClassName, string culture, UserInfo userInfo)
        {
            // For virtual context, only read, browse tree permissions are allowed to check
            if (VirtualContext.IsUserInitialized)
            {
                return false;
            }

            var siteName = (parentNode != null) ? parentNode.NodeSiteName : SiteContext.CurrentSiteName;
            var authorizationResult = IsAuthorizedPerTreeNode(parentNode, NodePermissionsEnum.Create, culture, userInfo);
            switch (authorizationResult)
            {
                case AuthorizationResultEnum.Allowed:
                    return true;

                case AuthorizationResultEnum.Denied:
                    return false;

                default:
                    return (documentClassName == null) 
                        || UserSecurityHelper.IsAuthorizedPerResource("cms.content", "create", siteName, userInfo) 
                        || UserSecurityHelper.IsAuthorizedPerClassName(documentClassName, "create", siteName, userInfo);
            }
        }


        /// <summary>
        /// Checks whether the user is authorized to create new document.
        /// </summary>
        /// <param name="parentNodeId">Parent node id</param>
        /// <param name="documentClassName">Document class name</param>
        /// <param name="culture">Culture to check</param>
        /// <param name="userInfo">User to check</param>
        public static bool IsAuthorizedToCreateNewDocument(int parentNodeId, string documentClassName, string culture, UserInfo userInfo)
        {
            //// Get the document
            TreeProvider treeProvider = new TreeProvider();
            TreeNode parentNode = treeProvider.SelectSingleNode(parentNodeId);

            return IsAuthorizedToCreateNewDocument(parentNode, documentClassName, culture, userInfo);
        }


        /// <summary>
        /// Aggregates the given permissions into the value which represents permission flags within <see cref="AclItemInfo.Denied"/> or <see cref="AclItemInfo.Allowed"/>
        /// </summary>
        /// <param name="permissions">Permissions to include to flags</param>
        public static int GetNodePermissionFlags(params NodePermissionsEnum[] permissions)
        {
            if (permissions == null)
            {
                return AclItemInfoProvider.PERMISSIONS_UNDEFINED;
            }

            // Include all permissions to the resulting flags
            int flags = 0;
            foreach (var permission in permissions)
            {
                var flag = GetFlagFromPermission(permission);

                flags |= flag;
            }

            return flags;
        }


        /// <summary>
        /// Gets the specific flag value from the given permission
        /// </summary>
        /// <param name="permission">Permission</param>
        public static int GetFlagFromPermission(NodePermissionsEnum permission)
        {
            return Convert.ToInt32(Math.Pow(2, Convert.ToInt32(permission)));
        }


        /// <summary>
        /// Parses the given permission flags as stored in <see cref="AclItemInfo.Denied"/> or <see cref="AclItemInfo.Allowed"/> to a list of permissions.
        /// </summary>
        /// <param name="flags">Permissions to include to flags</param>
        public static IEnumerable<NodePermissionsEnum> GetNodePermissionsFromFlags(int flags)
        {
            // Include all permissions to the resulting flags
            var permissions = Enum.GetValues(typeof(NodePermissionsEnum));

            foreach (NodePermissionsEnum permission in permissions)
            {
                var flag = GetFlagFromPermission(permission);

                if ((flags & flag) > 0)
                {
                    yield return permission;
                }
            }
        }


        /// <summary>
        /// Checks if the user is authorized for specified document (Checks all CONTENT, Document type and ACL permissions). Can handle also empty instance of TreeNode.
        /// For empty instance are not evaluated ACL permissions and document type permissions.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="permission">Permission</param>
        /// <param name="userInfo">User to check</param>
        /// <param name="siteName">Site name to check</param>
        internal static bool IsAuthorizedPerDocument(TreeNode node, PermissionsEnum permission, IUserInfo userInfo, string siteName)
        {
            var user = (UserInfo)userInfo;
            var nodePermission = GetNodePermissionEnum(permission);
            var result = IsAuthorizedPerDocument(node, nodePermission, true, node.DocumentCulture, user, siteName);

            return result.ToBoolean();
        }

        #endregion
    }
}
