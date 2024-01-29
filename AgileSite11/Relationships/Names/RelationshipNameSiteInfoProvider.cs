using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.Relationships
{
    /// <summary>
    /// Class providing RelationshipNameSiteInfo management.
    /// </summary>
    public class RelationshipNameSiteInfoProvider : AbstractInfoProvider<RelationshipNameSiteInfo, RelationshipNameSiteInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the RelationshipNameSiteInfo structure for the specified relationshipNameSite.
        /// </summary>
        /// <param name="relationshipNameId">RelationshipNameID</param>
        /// <param name="siteId">SiteID</param>
        public static RelationshipNameSiteInfo GetRelationshipNameSiteInfo(int relationshipNameId, int siteId)
        {
            return ProviderObject.GetRelationshipNameSiteInfoInternal(relationshipNameId, siteId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified relationshipNameSite.
        /// </summary>
        /// <param name="relationshipNameSite">RelationshipNameSite to set</param>
        public static void SetRelationshipNameSiteInfo(RelationshipNameSiteInfo relationshipNameSite)
        {
            ProviderObject.SetInfo(relationshipNameSite);
        }


        /// <summary>
        /// Deletes specified relationshipNameSite.
        /// </summary>
        /// <param name="infoObj">RelationshipNameSite object</param>
        public static void DeleteRelationshipNameSiteInfo(RelationshipNameSiteInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Returns the query for all relationship name site bindings.
        /// </summary>        
        public static ObjectQuery<RelationshipNameSiteInfo> GetRelationshipNameSites()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the bindings between relationship names and sites.
        /// </summary>
        /// <param name="where">Where condition to filter data</param>
        /// <param name="orderBy">Order by statement</param>
        /// <param name="topN">Specifies number of returned records</param>        
        /// <param name="columns">Data columns to return</param>
        public static ObjectQuery<RelationshipNameSiteInfo> GetRelationshipNameSites(string where, string orderBy = null, int topN = 0, string columns = null)
        {
            return GetRelationshipNameSites().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Deletes specified relationshipNameSite.
        /// </summary>
        /// <param name="relationshipNameId">RelationshipNameID</param>
        /// <param name="siteId">SiteID</param>
        public static void RemoveRelationshipNameFromSite(int relationshipNameId, int siteId)
        {
            RelationshipNameSiteInfo infoObj = GetRelationshipNameSiteInfo(relationshipNameId, siteId);
            DeleteRelationshipNameSiteInfo(infoObj);
        }


        /// <summary>
        /// Adds the relationship name specified by id to the site.
        /// </summary>
        /// <param name="relationshipNameId">ID of relationship name to add</param>
        /// <param name="siteId">ID of site</param>
        public static void AddRelationshipNameToSite(int relationshipNameId, int siteId)
        {
            // Create new binding
            RelationshipNameSiteInfo infoObj = new RelationshipNameSiteInfo();
            infoObj.RelationshipNameID = relationshipNameId;
            infoObj.SiteID = siteId;

            // Save to the database
            SetRelationshipNameSiteInfo(infoObj);
        }


        /// <summary>
        /// Returns true if the relationship is assigned to the given site.
        /// </summary>
        /// <param name="relationshipNameId">Relationship name ID</param>
        /// <param name="siteId">Site ID</param>
        public static bool IsRelationShipOnSite(int relationshipNameId, int siteId)
        {
            return (GetRelationshipNameSiteInfo(relationshipNameId, siteId) != null);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns the RelationshipNameSiteInfo structure for the specified relationshipNameSite.
        /// </summary>
        /// <param name="relationshipNameId">RelationshipNameID</param>
        /// <param name="siteId">SiteID</param>
        protected virtual RelationshipNameSiteInfo GetRelationshipNameSiteInfoInternal(int relationshipNameId, int siteId)
        {
            var condition = new WhereCondition()
                .WhereEquals("RelationshipNameID", relationshipNameId)
                .WhereEquals("SiteID", siteId);

            return GetObjectQuery().TopN(1).Where(condition).FirstOrDefault();
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(RelationshipNameSiteInfo info)
        {
            if (info != null)
            {
                // Check IDs
                if ((info.RelationshipNameID <= 0) || (info.SiteID <= 0))
                {
                    throw new ArgumentException("Object IDs not set.");
                }

                base.SetInfo(info);
            }
            else
            {
                throw new ArgumentNullException(nameof(info));
            }
        }

        #endregion
    }
}