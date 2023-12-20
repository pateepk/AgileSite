using System.Data;

using CMS.CMSImportExport;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;

namespace CMS.Blogs
{
    /// <summary>
    /// Handles special actions during the Blog export process.
    /// </summary>
    internal static class BlogExport
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        internal static void Init()
        {
            DocumentImportExportEvents.ExportDocuments.After += ExportDocuments_After;
            ImportExportEvents.ExportObjects.After += ExportObjects_After;
        }


        private static void ExportObjects_After(object sender, ExportEventArgs e)
        {
            var objectType = e.ObjectType;
            if (objectType == BlogCommentInfo.OBJECT_TYPE)
            {
                // Register translation record
                RegisterTranslationRecords(e.Data, e.TranslationHelper, e.Settings.ExcludedNames);
            }
        }


        /// <summary>
        /// Ensure translation records registration.
        /// </summary>
        /// <param name="data">Source dataset</param>
        /// <param name="th">Translation helper</param>
        /// <param name="excludedNames">Excluded object names</param>
        private static void RegisterTranslationRecords(DataSet data, TranslationHelper th, string[] excludedNames)
        {
            if (DataHelper.DataSourceIsEmpty(data))
            {
                return;
            }

            // Add translation records
            DataTable table = data.Tables[0];
            th.RegisterRecords(table, UserInfo.OBJECT_TYPE, "CommentUserID", null, excludedNames);
            th.RegisterRecords(table, UserInfo.OBJECT_TYPE, "CommentApprovedByUserID", null, excludedNames);
        }


        private static void ExportDocuments_After(object sender, DocumentsExportEventArgs e)
        {
            var settings = e.Settings;
            var data = e.Data;
            var th = e.TranslationHelper;

            if (DataHelper.DataSourceIsEmpty(data))
            {
                return;
            }

            // No data to be exported since the module is not loaded
            if (!ModuleEntryManager.IsModuleLoaded(ModuleName.BLOGS))
            {
                return;
            }

            WhereCondition where;

            // Blog comments 
            if (ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_BLOG_COMMENTS), true))
            {
                where = ImportExportHelper.GetDocumentsDataWhereCondition(BlogCommentInfo.OBJECT_TYPE, data, "CommentPostDocumentID", "DocumentID");
                DocumentExport.ExportRelatedDocumentsData(settings, BlogCommentInfo.OBJECT_TYPE, th, false, where);
            }

            // Blog post subscriptions
            where = ImportExportHelper.GetDocumentsDataWhereCondition(BlogPostSubscriptionInfo.OBJECT_TYPE, data, "SubscriptionPostDocumentID", "DocumentID");

            DocumentExport.ExportRelatedDocumentsData(settings, BlogPostSubscriptionInfo.OBJECT_TYPE, th, false, where);
        }

        #endregion
    }
}