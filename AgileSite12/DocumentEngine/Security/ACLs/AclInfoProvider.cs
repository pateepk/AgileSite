using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;
using CMS.Membership;
using CMS.Search;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// ACL management provider.
    /// </summary>
    public class AclInfoProvider : AbstractInfoProvider<AclInfo, AclInfoProvider>
    {
        #region "Constants"

        // Separator of list of (ACL) IDs – i.e. ACLInheritedACLs
        private const char STRING_ID_LIST_SEPARATOR = ',';

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the AclInfo objects.
        /// </summary>
        public static ObjectQuery<AclInfo> GetAcls()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns AclInfo with specified ID.
        /// </summary>
        /// <param name="id">AclInfo ID.</param>
        public static AclInfo GetAclInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified AclInfo.
        /// </summary>
        /// <param name="infoObj">AclInfo to be set.</param>
        public static void SetAclInfo(AclInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified AclInfo.
        /// </summary>
        /// <param name="infoObj">AclInfo to be deleted.</param>
        public static void DeleteAclInfo(AclInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes AclInfo with specified ID.
        /// </summary>
        /// <param name="id">AclInfo ID.</param>
        public static void DeleteAclInfo(int id)
        {
            AclInfo infoObj = GetAclInfo(id);
            DeleteAclInfo(infoObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Restores inheritance from the parent node and combines parent permissions with original native permissions.
        /// </summary>
        /// <param name="node">Document node</param>
        public static void RestoreInheritance(TreeNode node)
        {
            ProviderObject.RestoreInheritanceInternal(node);
        }


        /// <summary>
        /// Removes permission inheritance of the selected node.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="copyParentPermissions">Indicates if parent permissions should be copied to the node</param>
        public static int BreakInheritance(TreeNode node, bool copyParentPermissions)
        {
            return ProviderObject.BreakInheritanceInternal(node, copyParentPermissions);
        }


        /// <summary>
        /// Returns ACLID of the given node. If the node doesn't have its own ACL, it creates a new one.
        /// </summary>
        /// <param name="node">Document node</param>
        public static int EnsureOwnAcl(TreeNode node)
        {
            return ProviderObject.EnsureOwnAclInternal(node);
        }


        /// <summary>
        /// Removes specified ACLID values from the ACLs of the specified node's child nodes.
        /// </summary>
        /// <param name="node">Parent document node</param>
        /// <param name="removeIDs">List of ACLID values to be removed separated with comma (,)</param>
        public static void RemoveAclIds(TreeNode node, string removeIDs)
        {
            ProviderObject.RemoveAclIdsInternal(node, removeIDs);
        }


        /// <summary>
        /// Changes the ACLID within the section defined by path and removes old ACLs and ACL items which are no longer valid.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="path">Section path where the ACL IDs will be changed</param>
        /// <param name="newAclId">New ACLID</param>
        public static void ChangeAclId(int siteId, string path, int newAclId)
        {
            ProviderObject.ChangeAclIdInternal(siteId, path, newAclId);
        }


        /// <summary>
        /// Changes ACL ID to the set of nodes identified by the given where condition.
        /// Condition must cover a set of nodes which don't have ACLs referenced from outside, e.g. a subsection of tree or a single leaf node.
        /// This method does not handle updating of the inherited ACLs in other ACLs
        /// </summary>
        /// <param name="nodesWhere">Where condition for <see cref="DocumentNodeDataInfo" /></param>
        internal static void RemoveACLsFromNodes(WhereCondition nodesWhere)
        {
            ProviderObject.ChangeAclIdInternal(nodesWhere, 0);
        }


        /// <summary>
        /// Returns a data row containing node data and ACL data.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="columns">Columns to be selected</param>
        public static DataRow GetNodeAndAcl(int nodeId, string columns = null)
        {
            return ProviderObject.GetNodeAndAclInternal(nodeId, columns);
        }


        /// <summary>
        /// Returns true if specified node inherits permissions.
        /// </summary>
        /// <param name="node">Node instance</param>
        public static bool DoesNodeInheritPermissions(TreeNode node)
        {
            return ProviderObject.DoesNodeInheritPermissionsInternal(node);
        }


        /// <summary>
        /// Copies document permissions including the parent document permissions to the target document.
        /// </summary>
        /// <param name="sourceNode">Source document</param>
        /// <param name="targetNode">Target document</param>
        /// <param name="preserveAclHierarchy">Indicates if target ACL should be same as source</param>
        public static void CopyAcl(TreeNode sourceNode, TreeNode targetNode, bool preserveAclHierarchy)
        {
            ProviderObject.CopyAclInternal(sourceNode, targetNode, preserveAclHierarchy);
        }


        /// <summary>
        /// Merges ACL items if given ACL is not already contained.
        /// </summary>
        /// <param name="inheritedACLs">ACLs separated with comma (,)</param>
        /// <param name="aclId">ACL</param>
        /// <returns>String with merged ACLs separated with comma (,)</returns>
        public static string MergeACLs(string inheritedACLs, int aclId)
        {
            string stringAclId = aclId.ToString();
            if (string.IsNullOrEmpty(inheritedACLs))
            {
                return stringAclId;
            }

            var aclIDs = SplitAclIdsList(inheritedACLs);
            aclIDs.Add(stringAclId);

            return JoinAclIdsToList(aclIDs);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Restores inheritance from the parent node and combines parent permissions with original native permissions.
        /// </summary>
        /// <param name="node">Document node</param>
        protected virtual void RestoreInheritanceInternal(TreeNode node)
        {
            // Process within transaction
            using (var tr = BeginTransaction())
            {
                // If current node is parent, do nothing
                if (node.NodeParentID == 0)
                {
                    return;
                }

                int currentNodeAclId = ValidationHelper.GetInteger(node.GetValue("NodeACLID"), 0);
                if (currentNodeAclId == 0)
                {
                    return;
                }

                // Get ACLIDs that should be added to the node
                DataRow drParentNodeAcl = GetNodeAndAcl(node.NodeParentID, "ACLInheritedACLs, ACLID");

                if (!DataHelper.DataSourceIsEmpty(drParentNodeAcl))
                {
                    string addAcls = DataHelper.GetStringValue(drParentNodeAcl, "ACLInheritedACLs").Trim();
                    int parentAcl = DataHelper.GetIntValue(drParentNodeAcl, "ACLID");

                    if (parentAcl != 0)
                    {
                        addAcls = MergeACLs(addAcls, parentAcl);
                    }

                    // Update the given node's ACL
                    AclInfo currentNodeACL = GetAclInfo(currentNodeAclId);
                    currentNodeACL.ACLInheritedACLs = addAcls;
                    SetAclInfo(currentNodeACL);

                    // Update child nodes of the given node - add inherited ACLs
                    PropagateAclId(node, currentNodeAclId, currentNodeAclId, addAcls);
                }
                // Parent doesn't have the ACL, delete the ACL of current node
                else
                {
                    // Do not log events
                    using (new CMSActionContext { LogEvents = false })
                    {
                        node.NodeACLID = 0;
                        node.Update(false);
                    }

                    // Clear ACLItems from the ACL of the node
                    AclItemInfoProvider.DeleteAclItems(currentNodeAclId);

                    // Delete the given node's ACL
                    AclInfo currentNodeAcl = GetAclInfo(currentNodeAclId);
                    DeleteAclInfo(currentNodeAcl);
                }

                // Commit transaction if necessary
                tr.Commit();
            }
        }


        /// <summary>
        /// Removes permission inheritance of the selected node.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="copyParentPermissions">Indicates if parent permissions should be copied to the node</param>
        protected virtual int BreakInheritanceInternal(TreeNode node, bool copyParentPermissions)
        {
            int aclId;

            // Process within transaction
            using (var tr = BeginTransaction())
            {
                DataSet dsAclItems = AclItemInfoProvider.GetACLItemsAndOperators(node.NodeID)
                                                            .OrderBy("ACLOwnerNodeID", "Operator")
                                                            .Columns("ACLOwnerNodeID", "Operator", "Allowed", "Denied");

                aclId = EnsureOwnAcl(node);
                var aclData = GetAclInfoByNodeId(node.NodeID);

                // Clear inheritance from the ACL
                AclInfo acl = GetAclInfo(aclId);
                acl.ACLInheritedACLs = "";
                SetAclInfo(acl);

                // Remove old ACLID values from the child nodes
                if (aclData != null)
                {
                    string originalNodeACL = aclData.ACLInheritedACLs;
                    RemoveAclIds(node, originalNodeACL);
                }

                // Create original ACL items
                int nodeId = node.NodeID;

                if (!DataHelper.DataSourceIsEmpty(dsAclItems))
                {
                    DataTable dtAclItems = dsAclItems.Tables[0];

                    int i = 0;
                    while (i < dtAclItems.Rows.Count)
                    {
                        // Only copy if parent permissions should be copied or the permissions come from the item itself
                        int ownerNodeId = ValidationHelper.GetInteger(dtAclItems.Rows[i]["ACLOwnerNodeID"], 0);
                        if (copyParentPermissions || (ownerNodeId == nodeId))
                        {
                            int allowed = 0;
                            int denied = 0;
                            string lastOperator;

                            // Sets all permissions
                            do
                            {
                                lastOperator = DataHelper.GetStringValue(dtAclItems.Rows[i], "Operator");
                                allowed = allowed | ((int)(dtAclItems.Rows[i]["Allowed"]));
                                denied = denied | ((int)(dtAclItems.Rows[i]["Denied"]));
                                i++;

                                // Continue to next item if operator is still same and we want also parent permissions or owner do not changed
                            } while ((i < dtAclItems.Rows.Count) && (dtAclItems.Rows[i]["Operator"].ToString() == lastOperator) && (copyParentPermissions || (ownerNodeId == ValidationHelper.GetInteger(dtAclItems.Rows[i]["ACLOwnerNodeID"], 0))));

                            var operatorId = ValidationHelper.GetInteger(lastOperator.Substring(1), 0);

                            if (lastOperator.StartsWithCSafe("U"))
                            {
                                AclItemInfoProvider.SetUserPermissions(node, allowed, denied, UserInfoProvider.GetUserInfo(operatorId));
                            }
                            else
                            {
                                AclItemInfoProvider.SetRolePermissions(node, allowed, denied, RoleInfoProvider.GetRoleInfo(operatorId));
                            }
                        }
                        else
                        {
                            i++;
                        }
                    }
                }

                // Commit transaction if necessary
                tr.Commit();
            }
            return aclId;
        }


        /// <summary>
        /// Returns ACLID of the given node. If the node doesn't have its own ACL, it creates a new one.
        /// </summary>
        /// <param name="node">Document node</param>
        protected virtual int EnsureOwnAclInternal(TreeNode node)
        {
            int newAclId = 0;
            int oldAclId = 0;

            var aclData = GetAclInfoByNodeId(node.NodeID);

            bool updateACLs = false;
            AclInfo newAcl = null;
            int nodeId = node.NodeID;

            // If there is no ACL, creates new one
            if (aclData == null)
            {
                newAcl = CreateNewAcl(node.NodeSiteID);

                updateACLs = true;
            }
            else
            {
                if (!node.NodeIsACLOwner)
                {
                    string inheritedACLs = MergeACLs(aclData.ACLInheritedACLs, aclData.ACLID);

                    // The node does not have its own ACL - create a new ACL
                    newAcl = CreateNewAcl(node.NodeSiteID);
                    newAcl.ACLInheritedACLs = inheritedACLs;

                    updateACLs = true;
                    oldAclId = aclData.ACLID;
                }
                else
                {
                    newAclId = aclData.ACLID;
                }
            }

            if (!updateACLs)
            {
                return newAclId;
            }

            SetAclInfo(newAcl);

            newAclId = newAcl.ACLID;

            // Update the node info
            UpdateAclId(newAclId, nodeId, true);

            PropagateAclId(node, oldAclId, newAclId, "");
            node.NodeACLID = newAclId;
            node.NodeIsACLOwner = true;

            return newAclId;
        }


        /// <summary>
        /// Prepares a new ACL object for the given node ID and site ID. Does not save the ACL object to the database
        /// </summary>
        /// <param name="siteId">Site ID</param>
        internal static AclInfo CreateNewAcl(int siteId)
        {
            var newAcl = new AclInfo
            {
                ACLInheritedACLs = "",
                ACLGUID = Guid.NewGuid(),
                ACLLastModified = DateTime.Now,
                ACLSiteID = siteId
            };

            return newAcl;
        }


        /// <summary>
        /// Splits given string list by <see cref="STRING_ID_LIST_SEPARATOR"/> and space character (removing empty entities).
        /// </summary>
        /// <param name="aclIdsList">Comma-separated list of ACL IDs.</param>
        internal static HashSet<string> SplitAclIdsList(string aclIdsList)
        {
            return new HashSet<string>(aclIdsList.Split(new[] { STRING_ID_LIST_SEPARATOR, ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }


        /// <summary>
        /// Joins provided collection of IDs (expectedly <see cref="string"/>s or <see cref="int"/>s) to a single comma-separated string list.
        /// </summary>
        /// <typeparam name="T">Type of IDs (expectedly <see cref="string"/>s or <see cref="int"/>s)</typeparam>
        /// <param name="ids">Collection of IDs to be joined into single string.</param>
        internal static string JoinAclIdsToList<T>(IEnumerable<T> ids)
        {
            return String.Join(STRING_ID_LIST_SEPARATOR.ToString(), ids);
        }


        /// <summary>
        /// Removes specified ACLID values from the ACLs of the specified node's child nodes.
        /// </summary>
        /// <param name="node">Parent document node</param>
        /// <param name="removeIDs">List of ACLID values to be removed separated with comma (,)</param>
        protected virtual void RemoveAclIdsInternal(TreeNode node, string removeIDs)
        {
            var nodes = DocumentNodeDataInfoProvider.GetDocumentNodes()
                .Distinct()
                .Column("NodeACLID")
                .WhereEquals("NodeSiteID", node.NodeSiteID)
                .Where(TreePathUtils.GetPathWhereCondition(node.NodeAliasPath, PathTypeEnum.Children));

            var acls = GetAcls()
                .WhereIn("ACLID", nodes)
                .WhereNotEmpty("ACLInheritedACLs");

            var idsToRemove = SplitAclIdsList(removeIDs);

            acls.ForEachObject(aclInfo =>
            {
                var inheritedAcls = SplitAclIdsList(aclInfo.ACLInheritedACLs);
                inheritedAcls.RemoveWhere(idsToRemove.Contains);
                var newInheritedAclsString = JoinAclIdsToList(inheritedAcls);

                // Check if new values are different than original
                if (!aclInfo.ACLInheritedACLs.EqualsCSafe(newInheritedAclsString))
                {
                    aclInfo.ACLInheritedACLs = newInheritedAclsString;
                    aclInfo.Update();
                }
            });
        }


        /// <summary>
        /// Changes the ACLID within the section defined by path and removes old ACLs and ACL items which are no longer valid.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="path">Section path where the ACL IDs will be changed</param>
        /// <param name="newAclId">New ACLID</param>
        protected void ChangeAclIdInternal(int siteId, string path, int newAclId)
        {
            path = TreePathUtils.EnsureSinglePath(path);

            var where =
                new WhereCondition()
                .WhereEquals("NodeSiteID", siteId)
                    .Where(TreePathUtils.GetPathWhereCondition(path, PathTypeEnum.Section));

            ChangeAclIdInternal(where, newAclId);
        }


        /// <summary>
        /// Changes ACL ID to the set of nodes identified by the given where condition
        /// </summary>
        /// <param name="nodesWhere">Where condition for <see cref="DocumentNodeDataInfo" /></param>
        /// <param name="newAclId">New ACL ID. If 0, sets the NodeACLID to NULL</param>
        protected virtual void ChangeAclIdInternal(WhereCondition nodesWhere, int newAclId)
        {
            // Get node's ACLs which are owned by the nodes in section
            var nodesQuery =
                DocumentNodeDataInfoProvider.GetDocumentNodes()
                    .Column("NodeACLID")
                    .Where(nodesWhere)
                    .WhereTrue("NodeIsACLOwner");

            // Get where condition to delete ACLs which are no longer valid
            // Materializes the node query to ACL ids, because ACL references will be removed before deletion of the ACLs, so nested query would not pick up the right data
            var aclWhere = new WhereCondition().WhereIn("ACLID", nodesQuery.GetListResult<int>());

            // Transaction is used to make sure that in case of error there are no detached ACLs without external reference from node
            // This is subject to change in case the transaction proves itself to be too long
            using (var transaction = BeginTransaction())
            {
                // Clear foreign keys pointing to the deleted ACLs
                DocumentNodeDataInfoProvider.BulkUpdateData(
                    nodesWhere,
                    new Dictionary<string, object> {
                        { "NodeACLID", (newAclId > 0) ? (object)newAclId : null },
                        { "NodeIsACLOwner", false }
                    }
                );

                // Delete ACLs
                DeleteAcls(aclWhere);

                transaction.Commit();
            }
        }


        /// <summary>
        /// Returns a data row containing node data and ACL data.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="columns">Columns to be selected</param>
        protected virtual DataRow GetNodeAndAclInternal(int nodeId, string columns)
        {
            var query = DocumentNodeDataInfoProvider.GetDocumentNodes()
                .Columns(columns)
                .Source(source => source.InnerJoin<AclInfo>("NodeACLID", "ACLID"))
                .WhereEquals("NodeID", nodeId);

            var result = query.Result;
            if (!DataHelper.DataSourceIsEmpty(result))
            {
                return result.Tables[0].Rows[0];
            }

            return null;
        }


        /// <summary>
        /// Returns true if specified node inherits permissions.
        /// </summary>
        /// <param name="node">Node to check</param>
        protected virtual bool DoesNodeInheritPermissionsInternal(TreeNode node)
        {
            var aclInfoData = GetAclInfoByNodeId(node.NodeID);
            if (aclInfoData == null)
            {
                return true;
            }

            // Node is not ACL owner or there are inherited ACLs
            return !node.NodeIsACLOwner || (aclInfoData.ACLInheritedACLs != "");
        }


        /// <summary>
        /// Copies document permissions including the parent document permissions to the target document.
        /// </summary>
        /// <param name="sourceNode">Source document</param>
        /// <param name="targetNode">Target document</param>
        /// <param name="preserveAclHierarchy">Indicates if target ACL should be same as source</param>
        protected virtual void CopyAclInternal(TreeNode sourceNode, TreeNode targetNode, bool preserveAclHierarchy)
        {
            // Get source ACL items
            DataSet dsSourceACLItems = AclItemInfoProvider.GetACLItemsAndOperators(sourceNode.NodeID)
                                                                .OrderBy("Operator")
                                                                .Columns("ACLOwnerNodeID, Operator, Allowed, Denied");

            DataRow[] aclItems = null;
            if (preserveAclHierarchy)
            {
                // Check if source node has own ACL
                if (sourceNode.NodeIsACLOwner)
                {
                    if (DoesNodeInheritPermissions(sourceNode))
                    {
                        EnsureOwnAcl(targetNode);
                        RestoreInheritance(targetNode);
                    }
                    else
                    {
                        BreakInheritance(targetNode, false);
                    }

                    // Filter only node own ACL items
                    if (!DataHelper.DataSourceIsEmpty(dsSourceACLItems))
                    {
                        aclItems = dsSourceACLItems.Tables[0].Select("ACLOwnerNodeID = " + sourceNode.NodeID);
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                BreakInheritance(targetNode, false);
                if (!DataHelper.DataSourceIsEmpty(dsSourceACLItems))
                {
                    aclItems = dsSourceACLItems.Tables[0].Select();
                }
            }

            // Check if some ACL items are available
            if (aclItems != null)
            {
                int i = 0;
                while (i < aclItems.Length)
                {
                    string operatorId;
                    int allowed = 0;
                    int denied = 0;

                    // Sets all permissions
                    do
                    {
                        // Take next acl item
                        DataRow dr = aclItems[i];
                        operatorId = ValidationHelper.GetString(dr["Operator"], "");

                        allowed = allowed | ValidationHelper.GetInteger(dr["Allowed"], 0);
                        denied = denied | ValidationHelper.GetInteger(dr["Denied"], 0);
                        i++;
                    } while ((i < aclItems.Length) && (aclItems[i]["Operator"].ToString() == operatorId));

                    if (operatorId.StartsWithCSafe("U"))
                    {
                        int userId = int.Parse(operatorId.Substring(1));
                        UserInfo ui = UserInfoProvider.GetUserInfo(userId);
                        AclItemInfoProvider.SetUserPermissions(targetNode, allowed, denied, ui);
                    }
                    else
                    {
                        RoleInfo ri = RoleInfoProvider.GetRoleInfo(int.Parse(operatorId.Substring(1)));
                        AclItemInfoProvider.SetRolePermissions(targetNode, allowed, denied, ri);
                    }
                }
            }
        }


        /// <summary>
        /// Bulk deletes ACLs and their items based on the given where condition
        /// </summary>
        /// <param name="where">Where condition</param>
        internal static void DeleteAcls(WhereCondition where)
        {
            // Delete child ACL items first.
            // This is subject to change when base implementation of DeleteData supports handling of dependencies.
            var itemWhere = new WhereCondition()
                .WhereIn(
                    "ACLID",
                    GetAcls()
                        .Column("ACLID")
                        .Where(where)
                );

            AclItemInfoProvider.DeleteAclItems(itemWhere);

            // Delete ACLs
            ProviderObject.BulkDelete(where);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Get ACL object for given node.
        /// </summary>
        /// <param name="nodeId">Node identifier</param>
        internal static AclInfo GetAclInfoByNodeId(int nodeId)
        {
            return GetAcls()
                    .TopN(1)
                    .WhereEquals("ACLID", DocumentNodeDataInfoProvider.GetDocumentNodes()
                                                    .Column("NodeACLID")
                                                    .WhereEquals("NodeID", nodeId))
                    .FirstOrDefault();
        }


        /// <summary>
        /// Updates ACLID for given node
        /// </summary>
        /// <param name="newAclId">New ACL ID</param>
        /// <param name="nodeId">Node ID</param>
        /// <param name="markAsOwner">Indicates if node should be marked as ACL owner</param>
        private void UpdateAclId(int newAclId, int nodeId, bool markAsOwner = false)
        {
            // Prepare the values to update
            var values = new Dictionary<string, object>
            {
                { "NodeACLID", (newAclId > 0) ? (object)newAclId : null }
            };

            if (markAsOwner)
            {
                values.Add("NodeIsACLOwner", true);
            }

            // Update nodes
            DocumentNodeDataInfoProvider.BulkUpdateData(
                new WhereCondition().WhereEquals("NodeID", nodeId),
                values
            );
        }


        /// <summary>
        /// Propagates given ACLID to all underlying nodes that inherit from the specified node.
        /// </summary>
        /// <param name="node">Parent document node</param>
        /// <param name="oldAclId">Original ACLID that will be replaced with new ACLID</param>
        /// <param name="aclId">New ACLID to be propagated</param>
        /// <param name="extraAclIds">Additional ACLID values that should be optionally added to the InheritedACLs property of the child ACLs</param>
        private void PropagateAclId(TreeNode node, int oldAclId, int aclId, string extraAclIds)
        {
            int i = 0;

            // Prepare columns to be selected and get child documents data
            const string columns = "NodeID, ACLInheritedACLs, NodeIsACLOwner, NodeACLID, NodeAliasPath, ACLID";

            var dsNodes = DocumentNodeDataInfoProvider.GetDocumentNodes()
                                                            .Column(columns)
                                                            .Source(source => source.Join<AclInfo>("NodeACLID", "ACLID"))
                                                            .WhereEquals("NodeSiteID", node.NodeSiteID)
                                                            .Where(TreePathUtils.GetPathWhereCondition(node.NodeAliasPath, PathTypeEnum.Children))
                                                            .OrderBy("NodeAliasPath")
                                                            .Result;

            while (i < dsNodes.Tables[0].Rows.Count)
            {
                var dr = dsNodes.Tables[0].Rows[i];
                var nodeId = ValidationHelper.GetInteger(dr["NodeID"], 0);
                var nodeOwnerAclId = ValidationHelper.GetInteger(dr["NodeACLID"], 0);
                var nodeIsAclOwner = ValidationHelper.GetBoolean(dr["NodeIsACLOwner"], false);
                var nodeAclId = ValidationHelper.GetInteger(dr["ACLID"], 0);
                var nodeAliasPath = ValidationHelper.GetString(dr["NodeAliasPath"], null);
                var inheritedACLs = ValidationHelper.GetString(dr["ACLInheritedACLs"], "").Trim();

                var nodeIsOwner = (nodeAclId == nodeOwnerAclId) && nodeIsAclOwner;

                if (string.IsNullOrEmpty(inheritedACLs) && nodeIsOwner)
                {
                    // Skip child alias paths
                    string skippedAliasPath = nodeAliasPath;
                    while ((i < dsNodes.Tables[0].Rows.Count) && (dsNodes.Tables[0].Rows[i]["NodeAliasPath"].ToString().StartsWithCSafe(skippedAliasPath)))
                    {
                        i += 1;
                    }
                }
                else
                {
                    if (nodeAclId != aclId)
                    {
                        // ACL is not the same as ACL of the currently modified node
                        if (nodeIsOwner)
                        {
                            // The node has its own ACL with inheritance from parents
                            AclInfo acl = GetAclInfo(nodeAclId);

                            // Add ACLID to the InheritedACLs value if not already contained
                            inheritedACLs = MergeACLs(acl.ACLInheritedACLs, aclId);

                            // Add optionally inherited ACLs of parent node
                            if (!string.IsNullOrEmpty(extraAclIds))
                            {
                                inheritedACLs = string.Format("{0},{1}", inheritedACLs, extraAclIds);
                            }

                            acl.ACLInheritedACLs = inheritedACLs;
                            SetAclInfo(acl);
                        }
                        else
                        {
                            if (nodeAclId == oldAclId)
                            {
                                // The node was using the original ACLID of the parent -> set it to the new ACLID
                                UpdateAclId(aclId, nodeId);
                            }
                        }
                    }
                    i += 1;
                }
            }

            // Log search task
            if (SearchIndexInfoProvider.SearchEnabled)
            {
                // Log partial rebuild task to propagate document change including child documents
                SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, TreeNode.OBJECT_TYPE, SearchFieldsConstants.PARTIAL_REBUILD, node.NodeSiteName + ";" + node.NodeAliasPath, node.DocumentID);
            }
        }

        #endregion
    }
}