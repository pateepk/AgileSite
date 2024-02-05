using System.Linq;

using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class providing DocumentTypeScopeClass management.
    /// </summary>
    public class DocumentTypeScopeClassInfoProvider : AbstractInfoProvider<DocumentTypeScopeClassInfo, DocumentTypeScopeClassInfoProvider>
    {
        #region "Public methods - Basic"
                
        /// <summary>
        /// Returns document type scope class with specified ID.
        /// </summary>
        /// <param name="classId">Document type scope class ID.</param>        
        public static DocumentTypeScopeClassInfo GetScopeClassInfo(int classId)
        {
            return ProviderObject.GetInfoById(classId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified document type scope class.
        /// </summary>
        /// <param name="classObj">Document type scope class to be set.</param>
        public static void SetScopeClassInfo(DocumentTypeScopeClassInfo classObj)
        {
            ProviderObject.SetInfo(classObj);
        }


        /// <summary>
        /// Deletes specified document type scope class.
        /// </summary>
        /// <param name="classObj">Document type scope class to be deleted.</param>
        public static void DeleteScopeClassInfo(DocumentTypeScopeClassInfo classObj)
        {
            ProviderObject.DeleteInfo(classObj);
        }


        /// <summary>
        /// Deletes document type scope class with specified ID.
        /// </summary>
        /// <param name="classId">Document type scope class ID.</param>
        public static void DeleteScopeClassInfo(int classId)
        {
            DocumentTypeScopeClassInfo classObj = GetScopeClassInfo(classId);
            DeleteScopeClassInfo(classObj);
        }


        /// <summary>
        /// Gets the query for all scope class bindings.
        /// </summary>
        public static ObjectQuery<DocumentTypeScopeClassInfo> GetScopeClasses()
        {
            return ProviderObject.GetObjectQuery();
        }

                
        /// <summary>
        /// Returns dataset of all document type scope classes matching the specified parameters.
        /// </summary>
        /// <param name="where">Where condition.</param>
        /// <param name="orderBy">Order by expression.</param>
        /// <param name="topN">Number of records to be selected.</param>        
        /// <param name="columns">Columns to be selected.</param>
        public static ObjectQuery<DocumentTypeScopeClassInfo> GetScopeClasses(string where, string orderBy = null, int topN = 0, string columns = null)
        {
            return GetScopeClasses().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns);
        }


        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns the DocumentTypeScopeClassInfo structure for the specified scope and class.
        /// </summary>
        /// <param name="scopeId">Scope identifier</param>        
        /// <param name="classId">Class identifier</param>
        public static DocumentTypeScopeClassInfo GetScopeClassInfo(int scopeId, int classId)
        {
            return ProviderObject.GetScopeClassInfoInternal(scopeId, classId);
        }


        /// <summary>
        /// Removes document type from scope.
        /// </summary>
        /// <param name="scopeId">Scope identifier</param>        
        /// <param name="classId">Class identifier</param>
        public static void RemoveClassFromScope(int scopeId, int classId)
        {
            ProviderObject.RemoveClassFromScopeInternal(scopeId, classId);
        }


        /// <summary>
        /// Adds document type to scope.
        /// </summary>
        /// <param name="scopeId">Scope identifier</param>        
        /// <param name="classId">Class identifier</param>
        public static void AddClassToScope(int scopeId, int classId)
        {
            ProviderObject.AddClassToScopeInternal(scopeId, classId);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns the DocumentTypeScopeClassInfo structure for the specified scope and class.
        /// </summary>
        /// <param name="scopeId">Scope identifier</param>        
        /// <param name="classId">Class identifier</param>
        protected virtual DocumentTypeScopeClassInfo GetScopeClassInfoInternal(int scopeId, int classId)
        {
            return GetObjectQuery().TopN(1)
                    .WhereEquals("ScopeID", scopeId)
                    .WhereEquals("ClassID", classId)
                    .FirstOrDefault();
        }


        /// <summary>
        /// Removes document type from scope.
        /// </summary>
        /// <param name="scopeId">Scope identifier</param>        
        /// <param name="classId">Class identifier</param>
        protected virtual void RemoveClassFromScopeInternal(int scopeId, int classId)
        {
            DocumentTypeScopeClassInfo bindingInfo = GetScopeClassInfo(scopeId, classId);
            if (bindingInfo != null)
            {
                bindingInfo.Delete();
            }
        }


        /// <summary>
        /// Adds document type to scope.
        /// </summary>
        /// <param name="scopeId">Scope identifier</param>        
        /// <param name="classId">Class identifier</param>
        protected virtual void AddClassToScopeInternal(int scopeId, int classId)
        {
            DocumentTypeScopeClassInfo bindingInfo = new DocumentTypeScopeClassInfo
            {
                ClassID = classId,
                ScopeID = scopeId
            };

            SetScopeClassInfo(bindingInfo);
        }

        #endregion
    }
}
