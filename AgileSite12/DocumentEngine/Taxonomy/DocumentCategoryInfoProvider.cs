using System.Linq;

using CMS.DataEngine;
using CMS.Search;
using CMS.Base;
using CMS.Taxonomy;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class providing DocumentCategoryInfo management.
    /// </summary>
    public class DocumentCategoryInfoProvider : AbstractInfoProvider<DocumentCategoryInfo, DocumentCategoryInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the DocumentCategoryInfo structure for the specified documentCategory.
        /// </summary>
        /// <param name="categoryId">CategoryID</param>
        /// <param name="documentId">DocumentID</param>
        public static DocumentCategoryInfo GetDocumentCategoryInfo(int documentId, int categoryId)
        {
            return ProviderObject.GetDocumentCategoryInfoInternal(documentId, categoryId);
        }


        /// <summary>
        /// Returns a query for all the DocumentCategoryInfo objects.
        /// </summary>
        public static ObjectQuery<DocumentCategoryInfo> GetDocumentCategories()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns all categories the document is a member of.
        /// </summary>
        /// <param name="documentId">ID of the document categories are related to.</param>
        public static ObjectQuery<CategoryInfo> GetDocumentCategories(int documentId)
        {
            var documentCategories = GetDocumentCategories()
                .Column("CategoryID")
                .WhereEquals("DocumentID", documentId);

            return CategoryInfoProvider.GetCategories()
                .WhereIn("CategoryID", documentCategories);
        }

        /// <summary>
        /// Sets (updates or inserts) specified documentCategory.
        /// </summary>
        /// <param name="documentCategory">DocumentCategory to set</param>
        public static void SetDocumentCategoryInfo(DocumentCategoryInfo documentCategory)
        {
            ProviderObject.SetInfo(documentCategory);
        }


        /// <summary>
        /// Deletes specified documentCategory.
        /// </summary>
        /// <param name="categoryId">CategoryID</param>
        /// <param name="documentId">DocumentID</param>
        public static void DeleteDocumentCategoryInfo(int documentId, int categoryId)
        {
            DocumentCategoryInfo infoObj = GetDocumentCategoryInfo(documentId, categoryId);
            DeleteDocumentCategoryInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified documentCategory.
        /// </summary>
        /// <param name="infoObj">DocumentCategory object</param>
        public static void DeleteDocumentCategoryInfo(DocumentCategoryInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Adds document (post) to the specified category.
        /// </summary>
        /// <param name="documentId">ID of document</param>
        /// <param name="categoryId">ID of category</param>
        public static void AddDocumentToCategory(int documentId, int categoryId)
        {
            // Create new binding
            DocumentCategoryInfo infoObj = new DocumentCategoryInfo();
            infoObj.CategoryID = categoryId;
            infoObj.DocumentID = documentId;

            // Save to the database
            SetDocumentCategoryInfo(infoObj);
        }


        /// <summary>
        /// Removes document from category.
        /// </summary>
        /// <param name="documentId">ID of document</param>
        /// <param name="categoryId">ID of category</param>
        public static void RemoveDocumentFromCategory(int documentId, int categoryId)
        {
            DeleteDocumentCategoryInfo(documentId, categoryId);
        }


        /// <summary>
        /// Removes all categories from given document.
        /// </summary>
        /// <param name="documentId">ID of document</param>        
        /// <param name="logSynchronization">Indicates if staging tasks should be logged</param>
        public static void RemoveDocumentFromCategories(int documentId, bool logSynchronization = true)
        {
            ProviderObject.RemoveDocumentFromCategoriesInternal(documentId, logSynchronization);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns the DocumentCategoryInfo structure for the specified documentCategory.
        /// </summary>
        /// <param name="categoryId">CategoryID</param>
        /// <param name="documentId">DocumentID</param>
        protected virtual DocumentCategoryInfo GetDocumentCategoryInfoInternal(int documentId, int categoryId)
        {
            WhereCondition condition = new WhereCondition().WhereEquals("CategoryID", categoryId)
                                                           .WhereEquals("DocumentID", documentId);

            return GetObjectQuery().TopN(1).Where(condition).FirstOrDefault();
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(DocumentCategoryInfo info)
        {
            // Save to DB
            base.SetInfo(info);

            // Set category count
            SetCategoryCount(info.CategoryID);

            // Create search task for appropriate document if categories are enabled in smart search
            if (SearchIndexInfoProvider.SearchEnabled && CMSActionContext.CurrentCreateSearchTask)
            {
                var provider = new TreeProvider();
                var node = provider.SelectSingleDocument(info.DocumentID);
                if ((node != null) && DocumentHelper.IsSearchTaskCreationAllowed(node))
                {
                    SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, node.GetSearchID(), node.DocumentID);
                }
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(DocumentCategoryInfo info)
        {
            if (info == null)
            {
                return;
            }

            // Delete the object
            base.DeleteInfo(info);

            // Set category count
            SetCategoryCount(info.CategoryID);

            // Create search task for appropriate document if categories are enabled in smart search
            if (SearchIndexInfoProvider.SearchEnabled && CMSActionContext.CurrentCreateSearchTask)
            {
                var provider = new TreeProvider();
                var node = provider.SelectSingleDocument(info.DocumentID);
                if ((node != null) && DocumentHelper.IsSearchTaskCreationAllowed(node))
                {
                    SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, node.GetSearchID(), node.DocumentID);
                }
            }
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Removes all categories from given document.
        /// </summary>
        /// <param name="documentId">ID of document</param>        
        /// <param name="logSynchronization">Indicates if staging tasks should be logged</param>
        protected virtual void RemoveDocumentFromCategoriesInternal(int documentId, bool logSynchronization)
        {
            // Get document categories
            var categories = GetDocumentCategories().WhereEquals("DocumentID", documentId);
            if (categories.Count > 0)
            {
                using (new CMSActionContext { LogSynchronization = logSynchronization })
                {
                    // For each document category
                    foreach (DocumentCategoryInfo category in categories)
                    {
                        // Delete document category info
                        DeleteDocumentCategoryInfo(category);
                    }
                }
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Sets the number of documents associated to given category.
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        private static void SetCategoryCount(int categoryId)
        {
            // Set correct document count
            CategoryInfo ci = CategoryInfoProvider.GetCategoryInfo(categoryId);
            if (ci != null)
            {
                ci.CategoryCount = CategoryInfoProvider.GetCategoryCount(categoryId);

                using (CMSActionContext context = new CMSActionContext())
                {
                    context.DisableAll();
                    CategoryInfoProvider.SetCategoryInfo(ci);
                }
            }
        }

        #endregion
    }
}