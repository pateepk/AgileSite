using System;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.DataEngine.CollectionExtensions;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;
using CMS.Membership;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class providing AclItemInfo management.
    /// </summary>
    public class AclItemInfoProvider : AbstractInfoProvider<AclItemInfo, AclItemInfoProvider>
    {
        #region "Constants"

        /// <summary>
        /// Indicates that permission won't be changed.
        /// </summary>
        public const int PERMISSIONS_UNDEFINED = -1;


        /// <summary>
        /// Storage key used for grouping stored ACL items.
        /// </summary>
        private const string ACL_ITEMS_STORAGE_KEY = "ACLItems";

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the AclItemInfo objects.
        /// </summary>
        public static ObjectQuery<AclItemInfo> GetAclItems()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns AclItemInfo with specified ID.
        /// </summary>
        /// <param name="id">ACL item identifier</param>
        public static AclItemInfo GetAclItemInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified AclItemInfo.
        /// </summary>
        /// <param name="infoObj">AclItemInfo to be set.</param>
        public static void SetAclItemInfo(AclItemInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified AclItemInfo.
        /// </summary>
        /// <param name="infoObj">AclItemInfo to be deleted.</param>
        public static void DeleteAclItemInfo(AclItemInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes AclItemInfo with specified ID.
        /// </summary>
        /// <param name="id">AclItemInfo ID.</param>
        public static void DeleteAclItemInfo(int id)
        {
            AclItemInfo infoObj = GetAclItemInfo(id);
            DeleteAclItemInfo(infoObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns DataSet containing all ACL items related to the specified node.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="includeInherited">If true, includes the inherited ACLs to the list</param>
        public static DataQuery GetACLItemsAndOperators(int nodeId, bool includeInherited = true)
        {
            return ProviderObject.GetACLItemsAndOperatorsInternal(nodeId, includeInherited);
        }


        /// <summary>
        /// Returns DataSet containing all ACL items related to the specified ACL and user.
        /// </summary>
        /// <param name="aclId">ACLID value</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="userInfo">UserInfo instance</param>
        public static DataSet GetAclItems(int aclId, int siteId, UserInfo userInfo)
        {
            return ProviderObject.GetAclItemsInternal(aclId, siteId, userInfo);
        }


        /// <summary>
        /// Returns DataSet containing ACL items related to the specified node and relevant for given user account (and its roles).
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="userInfo">UserInfo instance</param>
        public static DataSet GetAclItems(TreeNode node, UserInfo userInfo)
        {
            // Get ACLID
            int aclId = node.GetValue("NodeACLID", 0);

            return ProviderObject.GetAclItemsInternal(aclId, node.NodeSiteID, userInfo);
        }


        /// <summary>
        /// Returns DataSet of roles with allowed particular node permission.
        /// </summary>
        /// <param name="aclId">ACLID</param>
        /// <param name="permission">Permission to check</param>
        /// <param name="columns">Columns to be selected</param>
        public static DataSet GetAllowedRoles(int aclId, NodePermissionsEnum permission, string columns)
        {
            return ProviderObject.GetAllowedItemsInternal(RoleInfo.OBJECT_TYPE, aclId, permission, columns);
        }


        /// <summary>
        /// Returns DataSet of roles with allowed particular node permission.
        /// </summary>
        /// <param name="aclId">ACLID</param>
        /// <param name="permission">Permission to check</param>
        /// <param name="columns">Columns to be selected</param>
        public static DataSet GetAllowedUsers(int aclId, NodePermissionsEnum permission, string columns)
        {
            return ProviderObject.GetAllowedItemsInternal(UserInfo.OBJECT_TYPE, aclId, permission, columns);
        }


        /// <summary>
        /// Sets specified Allowed and Denied permissions to the specified user for given node.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="allowed">Allowed permissions</param>
        /// <param name="denied">Denied permissions</param>
        /// <param name="userInfo">UserInfo instance</param>
        public static void SetUserPermissions(TreeNode node, int allowed, int denied, UserInfo userInfo)
        {
            ProviderObject.SetOperatorPermissionsInternal(node, allowed, denied, userInfo, null);
        }


        /// <summary>
        /// Sets specified Allowed and Denied permissions to the specified role for given node.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="allowed">Allowed permissions</param>
        /// <param name="denied">Denied permissions</param>
        ///<param name="roleInfo">RoleInfo instance</param>
        public static void SetRolePermissions(TreeNode node, int allowed, int denied, RoleInfo roleInfo)
        {
            ProviderObject.SetOperatorPermissionsInternal(node, allowed, denied, null, roleInfo);
        }


        /// <summary>
        /// Invalidates security settings for given document and user.
        /// </summary>
        /// <param name="node">Tree node</param>
        /// <param name="userId">User identifier</param>
        public static void ClearRequestCache(TreeNode node, int userId)
        {
            ProviderObject.ClearRequestCacheInternal(node, userId);
        }


        /// <summary>
        /// Removes specified user from the ACL of the given node if the user has a native ACL Item for that node.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="userInfo">UserInfo instance</param>
        public static void RemoveUser(int nodeId, UserInfo userInfo)
        {
            ProviderObject.RemoveOperatorInternal(nodeId, userInfo, null);
        }


        /// <summary>
        /// Removes specified role from the ACL of the given node if the role has a native ACL Item for that node.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="roleInfo">RoleInfo instance</param>
        public static void RemoveRole(int nodeId, RoleInfo roleInfo)
        {
            ProviderObject.RemoveOperatorInternal(nodeId, null, roleInfo);
        }


        /// <summary>
        /// Clears ACL items.
        /// </summary>
        /// <param name="aclId">ACL ID</param>
        public static void DeleteAclItems(int aclId)
        {
            ProviderObject.DeleteAclItemsInternal(aclId);
        }


        /// <summary>
        /// Deletes the data in the database based on the given where condition.
        /// </summary>
        /// <param name="where">Where condition to filter data</param>
        internal static void DeleteAclItems(IWhereCondition where)
        {
            ProviderObject.BulkDelete(where);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns DataSet containing all ACL items related to the specified node.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="includeInherited">If true, includes the inherited ACLs to the list</param>
        protected virtual DataQuery GetACLItemsAndOperatorsInternal(int nodeId, bool includeInherited)
        {
            // Get ACLID and Inherited ACLs
            var aclInfoData = AclInfoProvider.GetAclInfoByNodeId(nodeId);
            if (aclInfoData == null)
            {
                return null;
            }

            var q = new DataQuery()
                .From("View_CMS_ACLItem_ItemsAndOperators")
                .Where(GetAclItemsWhereCondition(aclInfoData.ACLID, aclInfoData.ACLInheritedACLs, includeInherited));

            return q;
        }


        /// <summary>
        /// Returns DataSet containing all ACL items related to the specified ACL and user.
        /// </summary>
        /// <param name="aclId">ACLID value</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="userInfo">UserInfo instance</param>
        protected virtual DataSet GetAclItemsInternal(int aclId, int siteId, UserInfo userInfo)
        {
            DataSet ds = null;

            // Get ACL Items from request cache
            string key = GetACLItemsKey(userInfo.UserID, aclId, siteId);
            if (RequestStockHelper.Contains(ACL_ITEMS_STORAGE_KEY, key))
            {
                return (DataSet)RequestStockHelper.GetItem(ACL_ITEMS_STORAGE_KEY, key);
            }

            // Get ACL
            AclInfo aclObj = AclInfoProvider.GetAclInfo(aclId);
            if (aclObj != null)
            {
                string aclIDs = AclInfoProvider.MergeACLs(aclObj.ACLInheritedACLs, aclId);
                ds = GetAclItems(aclIDs, siteId, userInfo);
                RequestStockHelper.AddToStorage(ACL_ITEMS_STORAGE_KEY, key, ds);
            }

            return ds;
        }


        /// <summary>
        /// Returns DataSet of specified objects with allowed particular node permission.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="aclId">ACL ID</param>
        /// <param name="permission">Node permission</param>
        /// <param name="columns">Columns to be selected</param>
        protected virtual DataSet GetAllowedItemsInternal(string objectType, int aclId, NodePermissionsEnum permission, string columns)
        {
            int permValue = 1 << Convert.ToInt32(permission);

            var where = GetAllowedItemsWhereCondition(objectType, aclId, permValue);

            // Call the query
            return ConnectionHelper.ExecuteQuery("cms.aclitem.getalloweditems", where.Parameters, where.WhereCondition, null, 0, columns);
        }


        /// <summary>
        /// Sets specified Allowed and Denied permissions to the specified user or role for given node.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="allowed">Allowed permissions</param>
        /// <param name="denied">Denied permissions</param>
        /// <param name="userInfo">UserInfo instance. If you specify roleInfo, this value must be null</param>
        /// <param name="roleInfo">RoleInfo instance. If you specify userInfo, this value must be null</param>
        protected virtual void SetOperatorPermissionsInternal(TreeNode node, int allowed, int denied, UserInfo userInfo, RoleInfo roleInfo)
        {
            if ((userInfo == null) && (roleInfo == null))
            {
                throw new NullReferenceException("[AclItemInfoProvider.SetOperatorPermissionsInternal]: To set operator permissions either user or role needs to be specified.");
            }

            // Process within transaction
            using (var tr = BeginTransaction())
            {
                // Get ACLID
                int aclId = AclInfoProvider.EnsureOwnAcl(node);

                // Set allowed and denied values
                int allowedVal = (allowed == PERMISSIONS_UNDEFINED) ? 0 : allowed;
                int deniedVal = (denied == PERMISSIONS_UNDEFINED) ? 0 : denied;

                // Get ACL items
                var operatorACLItems = (userInfo != null)
                                                ? GetUserPermissions(aclId, userInfo.UserID)
                                                : GetRolePermissions(aclId, roleInfo.RoleID);

                var operatorACLItem = operatorACLItems.TopN(1).FirstOrDefault();
                if (operatorACLItem != null)
                {
                    // Check if allowed value should persist
                    if (allowed == PERMISSIONS_UNDEFINED)
                    {
                        allowedVal |= operatorACLItem.Allowed;
                    }

                    // Check if denied value should persist
                    if (denied == PERMISSIONS_UNDEFINED)
                    {
                        deniedVal |= operatorACLItem.Denied;
                    }

                    operatorACLItem.Allowed = allowedVal;
                    operatorACLItem.Denied = deniedVal;
                    operatorACLItem.LastModified = DateTime.Now;
                    operatorACLItem.LastModifiedByUserID = MembershipContext.AuthenticatedUser.UserID;

                    operatorACLItem.Update();
                }
                else
                {
                    // Create a new ACLItem
                    AclItemInfo aclItem = new AclItemInfo
                    {
                        ACLID = aclId,
                        UserID = (userInfo != null) ? userInfo.UserID : 0,
                        RoleID = (userInfo != null) ? 0 : roleInfo.RoleID,
                        Allowed = allowedVal,
                        Denied = deniedVal,
                        LastModified = DateTime.Now,
                        LastModifiedByUserID = MembershipContext.AuthenticatedUser.UserID
                    };

                    SetAclItemInfo(aclItem);
                }
                // Commit transaction if necessary
                tr.Commit();
            }
        }


        /// <summary>
        /// Invalidates security settings for given document and user.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="userId">User ID</param>
        protected virtual void ClearRequestCacheInternal(TreeNode node, int userId)
        {
            if (node == null)
            {
                return;
            }

            // Get node ACL ID
            int aclId = node.GetValue("NodeACLID", 0);

            // Get request stock key
            string key = GetACLItemsKey(userId, aclId, node.NodeSiteID);

            // Remove cached security settings from request
            if (RequestStockHelper.Contains(ACL_ITEMS_STORAGE_KEY, key))
            {
                RequestStockHelper.Remove(ACL_ITEMS_STORAGE_KEY, key);
            }
        }


        /// <summary>
        /// Removes specified user or role (operator) from the ACL of the given node if the operator has a native ACL Item for that node.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="userInfo">UserInfo instance. If roleInfo is specified, this value must be null</param>
        /// <param name="roleInfo">RoleInfo instance. If userInfo is specified, this value must be null</param>
        protected virtual void RemoveOperatorInternal(int nodeId, UserInfo userInfo, RoleInfo roleInfo)
        {
            if ((userInfo == null) && (roleInfo == null))
            {
                throw new NullReferenceException("[AclItemInfoProvider.RemoveOperatorInternal]: To remove operator permissions either user or role needs to be specified.");
            }

            // Get native ACL items
            var dsAclItems = userInfo != null
                ? GetOperatorNativeAclItems(nodeId, userInfo, null, "ACLItemID")
                : GetOperatorNativeAclItems(nodeId, null, roleInfo, "ACLItemID");

            if (dsAclItems == null)
            {
                var operatorString = (userInfo != null) ? string.Format("'{0}' user", userInfo.GetFormattedUserName(false)) : string.Format("'{0}' role", roleInfo.RoleDisplayName);
                throw new Exception(string.Format("[AclItemInfoProvider.RemoveOperatorInternal]: The {0} cannot be removed from the ACL since it's inherited from the parent node.", operatorString));
            }

            var where = AclItemInfo.TYPEINFO.CreateWhereCondition().WhereIn("ACLItemID", dsAclItems.GetListResult<int>());

            // Delete the ACL items
            ProviderObject.BulkDelete(where);
        }


        /// <summary>
        /// Clears ACL items.
        /// </summary>
        /// <param name="aclId">ACL ID</param>
        protected virtual void DeleteAclItemsInternal(int aclId)
        {
            var where = new WhereCondition().WhereEquals("ACLID", aclId);

            // Clear ACLItems from the ACL of the node
            ProviderObject.BulkDelete(where);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns DataSet containing ACL items related to the specified ACLs.
        /// </summary>
        /// <param name="aclIDs">List of ACL IDs separated by ','</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="userInfo">UserInfo instance</param>
        private DataSet GetAclItems(string aclIDs, int siteId, UserInfo userInfo)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@UserID", userInfo.UserID);
            parameters.Add("@SiteID", siteId);
            parameters.Add("@Date", DateTime.Now);

            // Get user generic roles
            var genericRolesWhere = UserInfoProvider.GetGenericRoles(userInfo);
            parameters.AddMacro("##GENERICROLES##", genericRolesWhere);

            return ConnectionHelper.ExecuteQuery("cms.aclitem.selectaclitemsbyaclidanduserid", parameters, " ACLID IN (" + SqlHelper.EscapeQuotes(aclIDs) + ") ");
        }


        /// <summary>
        /// Returns key for retrieving ACL items from request.
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="aclId">ACL identifier</param>
        /// <param name="siteId">Site ID</param>
        /// <returns>Key to retrieve ACL items from request</returns>
        internal static string GetACLItemsKey(int userId, int aclId, int siteId)
        {
            return String.Format("{0}|{1}|{2}", userId, aclId, siteId);
        }


        /// <summary>
        /// Returns DataSet containing ACL Items owned by given node. If node is not ACL owner returns null.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="userInfo">UserInfo instance. If roleInfo is specified, this value must be null</param>
        /// <param name="roleInfo">RoleInfo instance. If userInfo is specified, this value must be null</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        internal ObjectQuery<AclItemInfo> GetOperatorNativeAclItems(int nodeId, UserInfo userInfo, RoleInfo roleInfo, string columns)
        {
            var nodeData = DocumentNodeDataInfoProvider.GetDocumentNodeDataInfo(nodeId);

            // The node has to be the ACL owner
            if (!nodeData.NodeIsACLOwner)
            {
                return null;
            }

            // Get ACL items
            return (userInfo != null)
                ? GetUserPermissions(nodeData.NodeACLID, userInfo.UserID, columns)
                : GetRolePermissions(nodeData.NodeACLID, roleInfo.RoleID, columns);
        }


        /// <summary>
        /// Builds where condition for obtaining ACL items.
        /// </summary>
        /// <param name="aclId">ACL ID</param>
        /// <param name="inheritedAcls">Inherited ACLs</param>
        /// <param name="includeInherited">If true, includes the inherited ACLs to the list</param>
        /// <returns>Returns where condition</returns>
        internal static WhereCondition GetAclItemsWhereCondition(int aclId, string inheritedAcls, bool includeInherited)
        {
            var where = AclInfo.TYPEINFO.CreateWhereCondition();

            if (!includeInherited)
            {
                return where.WhereEquals("ACLID", aclId);
            }

            var inheritedAclIds = inheritedAcls.Split(',').Select(t => ValidationHelper.GetInteger(t, 0)).ToHashSetCollection();
            inheritedAclIds.Add(aclId);

            where.Or().WhereIn("ACLID", inheritedAclIds);

            return where;
        }


        /// <summary>
        /// Returns where condition for allowed items
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="aclId">ACL ID</param>
        /// <param name="permValue">Permission integer representation</param>
        private WhereCondition GetAllowedItemsWhereCondition(string objectType, int aclId, int permValue)
        {
            WhereCondition where = new WhereCondition();

            // Set query name according to object type
            switch (objectType)
            {
                case UserInfo.OBJECT_TYPE:
                    where.WhereNotNull("UserID");
                    break;

                case RoleInfo.OBJECT_TYPE:
                    where.WhereNotNull("RoleID");
                    break;

                case null:
                    break;

                default:
                    throw new Exception("[AclItemInfoProvider.GetAllowedItems]: " + String.Format(ResHelper.GetAPIString("acliteminfoprovider.getalloweditems.objecttypenotsupported", "Object type '{0}' is not supported"), objectType));
            }

            return where.Where(String.Format("((Allowed & {0}) >= {0})", permValue)).WhereEquals(TypeInfo.ParentIDColumn, aclId);
        }


        /// <summary>
        /// Returns ObjectQuery to retrieve user's permissions for given ACL
        /// </summary>
        /// <param name="ACLID">ACL ID</param>
        /// <param name="userID">User ID</param>
        /// <param name="columns">Columns to select</param>
        internal static ObjectQuery<AclItemInfo> GetUserPermissions(int ACLID, int userID, string columns = null)
        {
            return GetACLItemsByACLID(ACLID, columns)
                    .WhereEquals("UserID", userID);
        }


        /// <summary>
        /// Returns ObjectQuery to retrieve role's permissions for given ACL
        /// </summary>
        /// <param name="ACLID">ACL ID</param>
        /// <param name="roleID">Role ID</param>
        /// <param name="columns">Columns to select</param>
        internal static ObjectQuery<AclItemInfo> GetRolePermissions(int ACLID, int roleID, string columns = null)
        {
            return GetACLItemsByACLID(ACLID, columns)
                    .WhereEquals("RoleID", roleID);
        }


        /// <summary>
        /// Returns ObjectQuery to retrieve permissions for given ACL
        /// </summary>
        /// <param name="ACLID">ACL ID</param>
        /// <param name="columns">Columns to select</param>
        private static ObjectQuery<AclItemInfo> GetACLItemsByACLID(int ACLID, string columns)
        {
            return GetAclItems()
                       .Columns(columns)
                       .WhereEquals("ACLID", ACLID);
        }

        #endregion
    }
}