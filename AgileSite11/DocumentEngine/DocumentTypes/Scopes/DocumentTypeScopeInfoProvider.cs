using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class providing ScopeInfo management.
    /// </summary>
    public class DocumentTypeScopeInfoProvider : AbstractInfoProvider<DocumentTypeScopeInfo, DocumentTypeScopeInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns document type scope with specified ID.
        /// </summary>
        /// <param name="scopeId">Document type scope ID.</param>        
        public static DocumentTypeScopeInfo GetScopeInfo(int scopeId)
        {
            return ProviderObject.GetInfoById(scopeId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified document type scope.
        /// </summary>
        /// <param name="scopeObj">Document type scope to be set.</param>
        public static void SetScopeInfo(DocumentTypeScopeInfo scopeObj)
        {
            ProviderObject.SetInfo(scopeObj);
        }


        /// <summary>
        /// Deletes specified document type scope.
        /// </summary>
        /// <param name="scopeObj">Document type scope to be deleted.</param>
        public static void DeleteScopeInfo(DocumentTypeScopeInfo scopeObj)
        {
            ProviderObject.DeleteInfo(scopeObj);
        }


        /// <summary>
        /// Deletes document type scope with specified ID.
        /// </summary>
        /// <param name="scopeId">Document type scope ID.</param>
        public static void DeleteScopeInfo(int scopeId)
        {
            DocumentTypeScopeInfo scopeObj = GetScopeInfo(scopeId);
            DeleteScopeInfo(scopeObj);
        }


        /// <summary>
        /// Gets the query for all scopes.
        /// </summary>
        public static ObjectQuery<DocumentTypeScopeInfo> GetScopes()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns dataset of all document type scopes matching the specified parameters.
        /// </summary>
        /// <param name="where">Where condition.</param>
        /// <param name="orderBy">Order by expression.</param>
        /// <param name="topN">Number of records to be selected.</param>        
        /// <param name="columns">Columns to be selected.</param>
        public static ObjectQuery<DocumentTypeScopeInfo> GetScopes(string where, string orderBy = null, int topN = 0, string columns = null)
        {
            return GetScopes().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns dataset of all document type scopes for specified document type matching the specified parameters.
        /// </summary>       
        /// <param name="documentTypeId">Document type identifier</param>
        /// <param name="siteId">Site ID.</param>        		
        public static ObjectQuery<DocumentTypeScopeInfo> GetScopesForDocumentType(int documentTypeId, int siteId)
        {
            var whereCondition = new WhereCondition()
                .WhereIn("ScopeID", new IDQuery(DocumentTypeScopeClassInfo.OBJECT_TYPE, "ScopeID")
                                    .WhereEquals("ClassID", documentTypeId)
                                    .WhereIn("ClassID", new IDQuery(ClassSiteInfo.OBJECT_TYPE, "ClassID")
                                                        .Where("SiteID = ScopeSiteID")));

            return GetScopes().Where(whereCondition).OnSite(siteId);
        }


        /// <summary>
        /// Returns the best fitting document type scope for specified node alias path.
        /// </summary>
        /// <param name="node">Node under which is created a new document</param>
        public static DocumentTypeScopeInfo GetScopeInfo(TreeNode node)
        {
            return ProviderObject.GetScopeInfoInternal(node);
        }


        /// <summary>
        /// Returns the where condition to filter document types allowed in scope.
        /// </summary>
        /// <param name="node">Node under which is created a new document</param>
        public static WhereCondition GetScopeClassWhereCondition(TreeNode node)
        {
            return GetScopeClassWhereCondition(GetScopeInfo(node));
        }


        /// <summary>
        /// Returns the where condition to filter document types allowed in scope.
        /// </summary>
        /// <param name="scope">Document type scope</param>
        public static WhereCondition GetScopeClassWhereCondition(DocumentTypeScopeInfo scope)
        {
            return ProviderObject.GetScopeClassWhereConditionInternal(scope);
        }


        /// <summary>
        /// Returns true if document type is allowed in given alias path.
        /// </summary>
        /// <param name="node">Node under which is created a new document</param>
        /// <param name="classId">Created document type identifier</param>
        public static bool IsDocumentTypeAllowed(TreeNode node, int classId)
        {
            return ProviderObject.IsDocumentTypeAllowedInternal(node, classId);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns the best fitting document type scope for specified node alias path.
        /// </summary>
        /// <param name="node">Node under which is created a new document</param>
        protected virtual DocumentTypeScopeInfo GetScopeInfoInternal(TreeNode node)
        {
            if (node == null)
            {
                return null;
            }

            string aliasPath = TreePathUtils.EnsureSingleNodePath(node.NodeAliasPath);
            string safeAliasPath = String.Format("N'{0}'", SqlHelper.EscapeQuotes(aliasPath));

            WhereCondition condition = new WhereCondition()
                .Where(new WhereCondition()
                       .Where(new WhereCondition()
                              .WhereEquals("ScopePath", '/')
                              .WhereEquals("ScopeIncludeChildren", 1))
                       .Or()
                       .Where(safeAliasPath + " = ScopePath")
                       .Or()
                       .Where(new WhereCondition()
                              .WhereEquals("ScopeIncludeChildren", 1)
                              .Where(safeAliasPath + " LIKE ScopePath + '/%'")))
                .WhereEquals("ScopeSiteID", node.NodeSiteID);

            var scopes = GetScopes().Where(condition).OrderByDescending("ScopePath", "ScopeMacroCondition");

            if (scopes.Count > 0)
            {
                MacroResolver resolver = null;

                foreach (var scope in scopes)
                {
                    bool match = true;

                    if (!String.IsNullOrEmpty(scope.ScopeMacroCondition))
                    {
                        if (resolver == null)
                        {
                            resolver = GetScopeResolver(node);
                        }

                        // Resolve macro condition
                        match = ValidationHelper.GetBoolean(resolver.ResolveMacros(scope.ScopeMacroCondition), false);
                    }

                    if (match)
                    {
                        // Return the first scope that satisfies all conditions
                        return scope;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Returns the where condition to filter document types allowed in the best fitting scope for given node alias path.
        /// </summary>
        /// <param name="scope">Document type scope</param>
        protected virtual WhereCondition GetScopeClassWhereConditionInternal(DocumentTypeScopeInfo scope)
        {
            var condition = new WhereCondition();

            if ((scope == null) || scope.ScopeAllowAllTypes)
            {
                return condition;
            }
            
            condition.WhereIn("ClassID", new IDQuery<DocumentTypeScopeClassInfo>("ClassID")
                                                        .WhereEquals("ScopeID", scope.ScopeID));

            return condition;
        }


        /// <summary>
        /// Returns true if document type is allowed in given alias path.
        /// </summary>
        /// <param name="node">Node under which is created a new document</param>
        /// <param name="classId">Created document type identifier</param>
        protected virtual bool IsDocumentTypeAllowedInternal(TreeNode node, int classId)
        {
            DocumentTypeScopeInfo scope = GetScopeInfo(node);

            if ((scope != null) && !scope.ScopeAllowAllTypes)
            {
                WhereCondition condition = new WhereCondition()
                    .WhereEquals("ScopeID", scope.ScopeID)
                    .WhereEquals("ClassID", classId);

                var scopes = DocumentTypeScopeClassInfoProvider.GetScopeClasses().TopN(1).Column("ClassID").Where(condition);

                return (scopes.Count > 0);
            }

            return true;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Initializes macro resolver.
        /// </summary>
        /// <param name="node">Node under which is created a new document</param>
        private static MacroResolver GetScopeResolver(TreeNode node)
        {
            MacroResolver resolver = MacroResolver.GetInstance();
            resolver.SetNamedSourceData("CurrentUser", node.TreeProvider.UserInfo);
            resolver.SetNamedSourceData("Document", node);
            return resolver;
        }

        #endregion
    }
}
