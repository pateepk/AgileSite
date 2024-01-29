using System;

using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;

namespace CMS.Relationships
{
    /// <summary>
    /// Provides access to information about relationship names.
    /// </summary>
    public class RelationshipNameInfoProvider : AbstractInfoProvider<RelationshipNameInfo, RelationshipNameInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public RelationshipNameInfoProvider()
            : base(RelationshipNameInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
					Name = true
				})
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static RelationshipNameInfo GetRelationshipNameInfoByGUID(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Sets the specified relationship name data from info object to DB.
        /// </summary>
        /// <param name="relationshipNameInfo">Relationship name data object</param>
        public static void SetRelationshipNameInfo(RelationshipNameInfo relationshipNameInfo)
        {
            ProviderObject.SetInfo(relationshipNameInfo);
        }


        /// <summary>
        /// Returns the RelationshipNameInfo structure for the specified relationship name ID.
        /// </summary>
        /// <param name="relationshipNameId">Relationship name ID to use for retrieving the resource data</param>
        public static RelationshipNameInfo GetRelationshipNameInfo(int relationshipNameId)
        {
            return ProviderObject.GetInfoById(relationshipNameId);
        }


        /// <summary>
        /// Returns the RelationshipNameInfo structure for the specified relationship name.
        /// </summary>
        /// <param name="relationshipName">Relationship name to use for retrieving the resource data</param>
        public static RelationshipNameInfo GetRelationshipNameInfo(string relationshipName)
        {
            return ProviderObject.GetInfoByCodeName(relationshipName);
        }


        /// <summary>
        /// Deletes relationship name specified by id.
        /// </summary>
        /// <param name="ri">RelationshipName object</param>
        public static void DeleteRelationshipName(RelationshipNameInfo ri)
        {
            ProviderObject.DeleteInfo(ri);
        }


        /// <summary>
        /// Deletes relationship name specified by id.
        /// </summary>
        /// <param name="relationshipNameId">ID of relationship name to delete</param>
        public static void DeleteRelationshipName(int relationshipNameId)
        {
            RelationshipNameInfo ri = GetRelationshipNameInfo(relationshipNameId);
            DeleteRelationshipName(ri);
        }


        /// <summary>
        /// Returns object query of relationship names on selected site specified by where condition.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="where">Where condition</param>
        public static ObjectQuery<RelationshipNameInfo> GetRelationshipNames(int siteId, string where = null)
        {
            var results = GetRelationshipNames(where);

            return (siteId >= 0) ? results.OnSite(siteId) : results;
        }


        /// <summary>
        /// Returns the query for all relationship names.
        /// </summary>        
        public static ObjectQuery<RelationshipNameInfo> GetRelationshipNames()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns object query of relationship names specified by where condition.
        /// </summary>
        /// <param name="where">Where condition to filter data</param>
        /// <param name="orderBy">Order by statement</param>
        /// <param name="topN">Specifies number of returned records</param>        
        /// <param name="columns">Data columns to return</param>
        public static ObjectQuery<RelationshipNameInfo> GetRelationshipNames(string where, string orderBy = null, int topN = -1, string columns = null)
        {
            return GetRelationshipNames().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Check whether relationship with given name already exists.
        /// </summary>
        /// <param name="name">Relationship name to check</param>
        public static Boolean RelationshipNameExists(string name)
        {
            return ProviderObject.RelationshipNameExistsInternal(name);
        }


        /// <summary>
        /// Returns where condition for selecting relationship names from the database.
        /// </summary>
        /// <param name="allowedForObjects">Indicates if relationship names for object should be retrieved</param>
        /// <param name="allowedForDocuments">Indicates if relationship names for documents should be retrieved</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="includeAdHoc">Indicates if Ad-hoc relationship names should be retrieved</param>
        public static WhereCondition GetRelationshipNamesWhereCondition(bool allowedForObjects, bool allowedForDocuments, int siteId, bool includeAdHoc)
        {
            var condition = new WhereCondition();

            // Set where condition
            if (!allowedForObjects)
            {
                condition.WhereNotContains("RelationshipAllowedObjects", ObjectHelper.GROUP_OBJECTS);
            }
            if (!allowedForDocuments)
            {
                condition.WhereNotContains("RelationshipAllowedObjects", ObjectHelper.GROUP_DOCUMENTS);
            }

            var typeCondition = new WhereCondition();

            // If site specified, restrict to relationships assigned to the site
            if (siteId > 0)
            {
                typeCondition.WhereIn("RelationshipNameID", new IDQuery<RelationshipNameSiteInfo>().WhereEquals("SiteID", siteId));
            }

            if (includeAdHoc)
            {
                // Include also ad-hoc relationship names
                typeCondition.Or().WhereEquals("RelationshipNameIsAdHoc", true);
            }

            condition.Where(typeCondition);
            return condition;
        }
        
        #endregion


        #region "Ad-hoc relationship name methods"

        /// <summary>
        /// Returns the ad-hoc relationship name object. Creates new if doesn't exist.
        /// </summary>
        /// <param name="classInfo">Data class info</param>
        /// <param name="field">Field for ad-hoc relationship name</param>
        public static RelationshipNameInfo EnsureAdHocRelationshipNameInfo(DataClassInfo classInfo, IField field)
        {
            if (classInfo == null)
            {
                throw new NullReferenceException("[RelationshipNameInfoProvider.EnsureAdHocRelationshipNameInfo]: Class name is not specified.");
            }

            var name = GetAdHocRelationshipNameCodeName(classInfo.ClassName, field);
            var relationshipNameInfo = GetRelationshipNameInfo(name);
            if (relationshipNameInfo != null)
            {
                // Relationship already exists
                return relationshipNameInfo;
            }

            relationshipNameInfo = new RelationshipNameInfo
            {
                RelationshipNameIsAdHoc = true,
                RelationshipName = name,
                RelationshipDisplayName = GetAdHocRelationshipNameDisplayName(classInfo.ClassDisplayName, field),
                RelationshipAllowedObjects = ObjectHelper.GROUP_DOCUMENTS
            };

            // Create new
            relationshipNameInfo.Insert();

            return relationshipNameInfo;
        }


        /// <summary>
        /// Gets code name for ad-hoc relationship name
        /// </summary>
        /// <param name="className">Class name of the document</param>
        /// <param name="field">Field for ad-hoc relationship name</param>
        public static string GetAdHocRelationshipNameCodeName(string className, IField field)
        {
            return String.Format("{0}_{1}", className, field.Guid);
        }


        /// <summary>
        /// Gets display name for ad-hoc relationship name
        /// </summary>
        /// <param name="classDisplayName">Class display name of the document</param>
        /// <param name="field">Field for ad-hoc relationship name</param>
        public static string GetAdHocRelationshipNameDisplayName(string classDisplayName, IField field)
        {
            var fieldName = !String.IsNullOrEmpty(field.Caption) ? ResHelper.LocalizeString(field.Caption) : field.Name;

            return String.Format("{0} ({1})", ResHelper.LocalizeString(classDisplayName), fieldName);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Check whether relationship with given name already exists.
        /// </summary>
        /// <param name="name">Relationship name to check</param>
        protected virtual bool RelationshipNameExistsInternal(string name)
        {
            return GetObjectQuery().WhereEquals("RelationshipName", name).HasResults();
        }

        #endregion
    }
}