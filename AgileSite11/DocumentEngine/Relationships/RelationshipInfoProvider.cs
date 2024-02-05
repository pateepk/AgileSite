using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.Relationships;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class providing RelationshipInfo management.
    /// </summary>
    public class RelationshipInfoProvider : AbstractInfoProvider<RelationshipInfo, RelationshipInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the RelationshipInfo objects.
        /// </summary>
        public static ObjectQuery<RelationshipInfo> GetRelationships()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Gets relationship info object.
        /// </summary>
        /// <param name="leftNodeId">ID of left node</param>
        /// <param name="rightNodeId">ID of right node</param>
        /// <param name="relationshipNameId">ID of relationship name</param>
        public static RelationshipInfo GetRelationshipInfo(int leftNodeId, int rightNodeId, int relationshipNameId)
        {
            return ProviderObject.GetRelationshipInfoInternal(leftNodeId, rightNodeId, relationshipNameId);
        }


        /// <summary>
        /// Gets relationship info object.
        /// </summary>
        /// <param name="relationshipId">ID of relationship</param>
        public static RelationshipInfo GetRelationshipInfo(int relationshipId)
        {
            return ProviderObject.GetInfoById(relationshipId);
        }


        /// <summary>
        /// Sets relationship info object.
        /// </summary>
        /// <param name="relationship">Relationship info</param>
        public static void SetRelationshipInfo(RelationshipInfo relationship)
        {
            ProviderObject.SetInfo(relationship);
        }


        /// <summary>
        /// Removes the relationship between two nodes.
        /// </summary>
        /// <param name="relationshipInfo">Relationship object</param>
        public static void DeleteRelationshipInfo(RelationshipInfo relationshipInfo)
        {
            ProviderObject.DeleteInfo(relationshipInfo);
        }


        /// <summary>
        /// Removes the relationship between two nodes.
        /// </summary>
        /// <param name="relationshipId">Relationship identifier</param>
        public static void DeleteRelationshipInfo(int relationshipId)
        {
            var relationshipInfo = GetRelationshipInfo(relationshipId);
            DeleteRelationshipInfo(relationshipInfo);
        }


        #endregion


        #region "Public methods - Advanced"


        /// <summary>
        /// Clears the document relationship cache for the given relationship.
        /// </summary>
        /// <param name="relationship">Relationship</param>
        public static void ClearCache(RelationshipInfo relationship)
        {
            ProviderObject.ClearCacheInternal(relationship);
        }


        /// <summary>
        /// Creates the relationship for two nodes.
        /// </summary>
        /// <param name="leftNodeId">ID of left node</param>
        /// <param name="rightNodeId">ID of right node</param>
        /// <param name="relationshipNameId">ID of relationship name</param>
        public static void AddRelationship(int leftNodeId, int rightNodeId, int relationshipNameId)
        {
            if (RelationshipExists(leftNodeId, rightNodeId, relationshipNameId))
            {
                return;
            }

            // Create new relationship if doesn't exist
            var relationship = new RelationshipInfo
            {
                LeftNodeId = leftNodeId,
                RightNodeId = rightNodeId,
                RelationshipNameId = relationshipNameId
            };

            // Set up default relationship order for ad-hoc relationships -> new item is the last in the order
            var relationshipName = RelationshipNameInfoProvider.GetRelationshipNameInfo(relationshipNameId);
            if ((relationshipName != null) && relationshipName.RelationshipNameIsAdHoc)
            {
                relationship.RelationshipOrder = GetRelationshipCount(relationshipNameId, leftNodeId) + 1;
                relationship.RelationshipIsAdHoc = true;
            }

            // Set relationship info object
            SetRelationshipInfo(relationship);
        }


        /// <summary>
        /// Removes the relationship between two nodes.
        /// </summary>
        /// <param name="leftNodeId">ID of left node</param>
        /// <param name="rightNodeId">ID of right node</param>
        /// <param name="relationshipNameId">ID of relationship name</param>
        public static void RemoveRelationship(int leftNodeId, int rightNodeId, int relationshipNameId)
        {
            var relationship = GetRelationshipInfo(leftNodeId, rightNodeId, relationshipNameId);

            RemoveRelationship(relationship);
        }


        /// <summary>
        /// Removes the relationship between two nodes.
        /// </summary>
        /// <param name="relationshipId">ID of relationship</param>
        public static void RemoveRelationship(int relationshipId)
        {
            var relationship = GetRelationshipInfo(relationshipId);

            RemoveRelationship(relationship);
        }


        /// <summary>
        /// Removes given relationship. Initializes objects order in case of ad-hoc relationship.
        /// </summary>
        /// <param name="relationship">Relationship to be removed</param>
        private static void RemoveRelationship(RelationshipInfo relationship)
        {
            DeleteRelationshipInfo(relationship);

            // Ensure that value of order column is without gaps.
            if (relationship.RelationshipIsAdHoc)
            {
                relationship.Generalized.InitObjectsOrder();
            }
        }


        /// <summary>
        /// Check whether relationship between two nodes already exists.
        /// </summary>
        /// <param name="leftNodeId">ID of left node</param>
        /// <param name="rightNodeId">ID of right node</param>
        /// <param name="relationshipNameId">ID of relationship name</param>
        /// <returns>Returns true if the relationship between two nodes already exists</returns>
        public static bool RelationshipExists(int leftNodeId, int rightNodeId, int relationshipNameId)
        {
            return ProviderObject.RelationshipExistsInternal(leftNodeId, rightNodeId, relationshipNameId);
        }

        /// <summary>
        /// Returns the relationships DataSet for specified node.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="nodeLeft">If true, returns the relationships where node is located on the left side</param>
        /// <param name="nodeRight">If true, returns the relationships where node is located on the right side</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        public static DataSet GetRelationships(int nodeId, bool nodeLeft = true, bool nodeRight = true, string orderBy = null, int topN = 0, string columns = null)
        {
            return ProviderObject.GetRelationshipsInternal(nodeId, nodeLeft, nodeRight, orderBy, topN, columns);
        }

        /// <summary>
        /// Deletes the node relationships.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="nodeLeft">If true, relationships with node on the left side are deleted</param>
        /// <param name="nodeRight">If true, relationships with node on the right side are deleted</param>
        public static void DeleteRelationships(int nodeId, bool nodeLeft, bool nodeRight)
        {
            ProviderObject.DeleteRelationshipsInternal(nodeId, nodeLeft, nodeRight);
        }


        /// <summary>
        /// Apply relationship order data on multidocument query.
        /// </summary>
        /// <param name="query">Multi document query where relationship order data will be added.</param>
        /// <param name="leftNodeId">ID of node which is on the left side of the relationship.</param>
        /// <param name="relationshipNameId">ID of ad-hoc relationship name. Only relationships from ad-hoc relationship name can be ordered.</param>
        public static MultiDocumentQuery ApplyRelationshipOrderData(MultiDocumentQuery query, int leftNodeId, int relationshipNameId)
        {
            return ProviderObject.ApplyRelationshipOrderDataInternal(query, leftNodeId, relationshipNameId);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Gets relationship info object.
        /// </summary>
        /// <param name="leftNodeId">ID of left node</param>
        /// <param name="rightNodeId">ID of right node</param>
        /// <param name="relationshipNameId">ID of relationship name</param>
        protected virtual RelationshipInfo GetRelationshipInfoInternal(int leftNodeId, int rightNodeId, int relationshipNameId)
        {
            // Prepare the where condition
            var where = GetWhereCondition(leftNodeId, rightNodeId, relationshipNameId);

            // Get the data
            return GetObjectQuery().TopN(1).Where(where).FirstOrDefault();
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(RelationshipInfo info)
        {
            base.SetInfo(info);

            ClearCache(info);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(RelationshipInfo info)
        {
            base.DeleteInfo(info);

            ClearCache(info);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Clears the document relationship cache for the given relationship.
        /// </summary>
        /// <param name="relationship">Relationship</param>
        protected virtual void ClearCacheInternal(RelationshipInfo relationship)
        {
            string key = "nodeid|{0}|relationships";

            CacheHelper.TouchKeys(new[] {
                string.Format(key, relationship.LeftNodeId),
                string.Format(key, relationship.RightNodeId)
            });
        }


        /// <summary>
        /// Check whether relationship between two nodes already exists.
        /// </summary>
        /// <param name="leftNodeId">ID of left node</param>
        /// <param name="rightNodeId">ID of right node</param>
        /// <param name="relationshipNameId">ID of relationship name</param>
        /// <returns>Returns true if the relationship between two nodes already exists</returns>
        protected virtual bool RelationshipExistsInternal(int leftNodeId, int rightNodeId, int relationshipNameId)
        {
            var where = GetWhereCondition(leftNodeId, rightNodeId, relationshipNameId);

            return GetRelationships()
                .Where(where)
                .HasResults();
        }


        /// <summary>
        /// Returns the relationships DataSet for specified node.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="nodeLeft">If true, returns the relationships where node is located on the left side</param>
        /// <param name="nodeRight">If true, returns the relationships where node is located on the right side</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        protected virtual DataSet GetRelationshipsInternal(int nodeId, bool nodeLeft, bool nodeRight, string orderBy, int topN, string columns)
        {
            if (!nodeLeft && !nodeRight)
            {
                return null;
            }

            // Prepare the where condition
            WhereCondition where = GetRelationshipsWhereCondition(nodeId, nodeLeft, nodeRight);

            // Get the data
            return ConnectionHelper.ExecuteQuery("CMS.RelationShip.SelectRelationships", where.Parameters, where.WhereCondition, orderBy, topN, columns);
        }


        /// <summary>
        /// Deletes the node relationships.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="nodeLeft">If true, relationships with node on the left side are deleted</param>
        /// <param name="nodeRight">If true, relationships with node on the right side are deleted</param>
        protected virtual void DeleteRelationshipsInternal(int nodeId, bool nodeLeft, bool nodeRight)
        {
            if (!nodeLeft && !nodeRight)
            {
                return;
            }

            // Prepare the where condition
            var where = GetRelationshipsWhereCondition(nodeId, nodeLeft, nodeRight);

            // We need to define object type explicitly to delete all object types
            BulkDelete(where, new BulkDeleteSettings { ObjectType = RelationshipInfo.OBJECT_TYPE });
            BulkDelete(where, new BulkDeleteSettings { ObjectType = RelationshipInfo.OBJECT_TYPE_ADHOC });
        }


        /// <summary>
        /// Apply relationship order data on multidocument query.
        /// </summary>
        /// <param name="query">Multi document query where relationship order data will be added.</param>
        /// <param name="leftNodeId">ID of node which is on the left side of the relationship.</param>
        /// <param name="relationshipNameId">ID of ad-hoc relationship name. Only relationships from ad-hoc relationship name can be ordered.</param>
        protected virtual MultiDocumentQuery ApplyRelationshipOrderDataInternal(MultiDocumentQuery query, int leftNodeId, int relationshipNameId)
        {

            // Ensure correct order for ad-hoc relationships 
            query.Source(s => s.Join<RelationshipInfo>("NodeID", "RightNodeID",
                                                        JoinTypeEnum.Inner,
                                                        new WhereCondition()
                                                            .WhereEquals("RelationshipNameID", relationshipNameId)
                                                            .WhereEquals("LeftNodeID", leftNodeId)))
                 .ResultOrderBy("RelationshipOrder");

            return query;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Creates where condition for obtaining relationships containing the specified node.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="nodeLeft">If true, relationships with node on the left side are obtained</param>
        /// <param name="nodeRight">If true, relationships with node on the right side are obtained</param>
        /// <returns>Returns where condition for the specified node</returns>
        private WhereCondition GetRelationshipsWhereCondition(int nodeId, bool nodeLeft, bool nodeRight)
        {
            if (!nodeLeft && !nodeRight)
            {
                return null;
            }

            WhereCondition condition = new WhereCondition();

            if (nodeLeft)
            {
                condition.WhereEquals("LeftNodeID", nodeId);
            }

            if (nodeRight)
            {
                condition.Or().WhereEquals("RightNodeID", nodeId);
            }

            return condition;
        }


        /// <summary>
        /// Returns count of relationship with the same relationship name created under one node.
        /// </summary>
        /// <param name="relationshipNameID">ID of relationship name.</param>
        /// <param name="leftNodeID">ID of node which is on the left side of relationship.</param>
        private static int GetRelationshipCount(int relationshipNameID, int leftNodeID)
        {
            return GetRelationships().WhereEquals("RelationshipNameID", relationshipNameID).WhereEquals("LeftNodeID", leftNodeID).GetCount();
        }


        private static WhereCondition GetWhereCondition(int leftNodeId, int rightNodeId, int relationshipNameId)
        {
            var where = new WhereCondition().WhereEquals("RelationshipNameID", relationshipNameId)
                                            .WhereEquals("LeftNodeID", leftNodeId)
                                            .WhereEquals("RightNodeID", rightNodeId);
            return where;
        }

        #endregion
    }
}