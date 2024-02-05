using System;
using System.Collections.Generic;
using System.Data;

using CMS.Base;
using CMS.CMSImportExport;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.PortalEngine;

namespace CMS.Blogs
{
    /// <summary>
    /// Handles special actions during the Blog import process.
    /// </summary>
    internal static class BlogImport
    {
        #region "Methods"

        /// <summary>
        /// Initializes import handlers
        /// </summary>
        public static void Init()
        {
            DocumentImportExportEvents.ImportDocuments.After += ImportDocuments_After;

            ImportExportEvents.GetImportData.After += GetImportData_After;

            SpecialActionsEvents.ProcessMainObject.After += ProcessMainObject_After;
            SpecialActionsEvents.ProcessMainObject.Before += ProcessMainObject_Before;
        }


        private static void ImportDocuments_After(object sender, DocumentsImportEventArgs e)
        {
            var settings = e.Settings;
            var th = e.TranslationHelper;
            var documentsIds = e.ImportedDocumentIDs;

            // Import blog comments and post subscriptions
            if (!ModuleEntryManager.IsModuleLoaded(ModuleName.BLOGS) || !LicenseHelper.CheckFeature(settings.CurrentUrl, FeatureEnum.Blogs))
            {
                return;
            }

            if (ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_BLOG_COMMENTS), true))
            {
                ImportProvider.ImportObjectType(settings, BlogCommentInfo.OBJECT_TYPE, false, th, ProcessObjectEnum.All, documentsIds);
            }

            // Blog post subscriptions
            ImportProvider.ImportObjectType(settings, BlogPostSubscriptionInfo.OBJECT_TYPE, false, th, ProcessObjectEnum.All, documentsIds);
        }


        /// <summary>
        /// Removes not supported fields for Trackbacks from CommentView web part
        /// </summary>
        private static void ProcessMainObject_Before(object sender, ImportEventArgs e)
        {
            if (e.Object.TypeInfo.ObjectType != WebPartInfo.OBJECT_TYPE)
            {
                return;
            }

            var mainObj = e.Object as WebPartInfo;
            if (mainObj == null)
            {
                return;
            }

            if (mainObj.WebPartName.EqualsCSafe("CommentView", StringComparison.InvariantCultureIgnoreCase))
            {
                var fields = new List<string>
                {
                    "DisplayTrackbacks",
                    "TrackbackURLSize"
                };

                mainObj.WebPartProperties = RemoveFieldsFromFormDefinition(mainObj.WebPartProperties, fields);
            }
        }

        
        /// <summary>
        /// Removes not supported fields from Blog and Blog post page types.
        /// </summary>
        private static void ProcessMainObject_After(object sender, ImportEventArgs e)
        {
            if (e.Object.TypeInfo.ObjectType != DocumentTypeInfo.OBJECT_TYPE_DOCUMENTTYPE)
            {
                return;
            }

            var mainObj = (DataClassInfo)e.Object;
            if (mainObj == null)
            {
                return;
            }

            if (!e.Settings.IsSelected(DocumentTypeInfo.OBJECT_TYPE_DOCUMENTTYPE, mainObj.ClassName, false))
            {
                // If not selected don't process this page type
                return;
            }

            // Update CMS.Blog page type if exists
            if (mainObj.ClassName.EqualsCSafe("cms.blog", StringComparison.InvariantCultureIgnoreCase))
            {
                DocumentTypeHelper.RemoveColumnsFromPageType(mainObj, "BlogEnableTrackbacks");
            }

            // Update CMS.BlogPost page type if exists
            if (mainObj.ClassName.EqualsCSafe("cms.blogpost", StringComparison.InvariantCultureIgnoreCase))
            {
                DocumentTypeHelper.RemoveColumnsFromPageType(mainObj, "BlogPostPingedUrls", "BlogPostNotPingedUrls");
            }
        }


        /// <summary>
        /// Removes old not supported setting keys and form controls for MetaWeblog API and Trackbacks
        /// </summary>
        private static void GetImportData_After(object sender, ImportGetDataEventArgs e)
        {
            var objectType = e.ObjectType;
            DataTable data = null;

            if ((objectType == FormUserControlInfo.OBJECT_TYPE) && ImportExportHelper.GetDataTable(e.Data, objectType, ref data))
            {
                // Set keys to remove
                var controlsToRemove = new List<string>
                {
                    "b9dfb6d3-5efb-4753-af41-e2e36e7d6732", // TrackbacksNotPingedUrls
                    "c761883c-30aa-431c-8b5b-8ed02806f796", // TrackbacksPingedUrls
                };

                DataHelper.DeleteRows(data, SqlHelper.GetWhereCondition<Guid>("UserControlGUID", controlsToRemove, false));
            }
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Removes fields from form definition and returns new definition
        /// </summary>
        /// <param name="formDefinition"></param>
        /// <param name="fields">List of fields to remove</param>
        private static string RemoveFieldsFromFormDefinition(string formDefinition, List<string> fields)
        {
            // Remove fields from form definition
            FormInfo formInfo = new FormInfo(formDefinition);

            formInfo.RemoveFields(t => fields.Contains(t.Name));

            return formInfo.GetXmlDefinition();
        }
        
        #endregion
    }
}