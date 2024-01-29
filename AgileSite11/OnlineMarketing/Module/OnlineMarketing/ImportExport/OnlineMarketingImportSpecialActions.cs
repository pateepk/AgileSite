using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.PortalEngine;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Handles special actions during the import process.
    /// </summary>
    internal static class OnlineMarketingImportSpecialActions
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            ImportExportEvents.ImportChildType.Before += ImportChildType_Before;
            SpecialActionsEvents.ImportEnsureAutomaticSelection.Execute += EnsureAutomaticSelection_Execute;
            SpecialActionsEvents.GetObjectTypeFolder.Execute += GetObjectTypeFolder_Execute;
            DocumentImportExportEvents.ExportDocuments.After += ExportDocuments_After;
            ImportExportEvents.ExportObjects.After += ExportObjects_After;
            DocumentImportExportEvents.ImportDocuments.After += ImportMVT;
            DocumentImportExportEvents.ImportDocuments.After += ImportPersonalizationVariants;
        }


        private static void ImportMVT(object sender, DocumentsImportEventArgs e)
        {
            var settings = e.Settings;
            var th = e.TranslationHelper;
            var documentsIds = e.ImportedDocumentIDs;

            if (LicenseHelper.CheckFeature(settings.CurrentUrl, FeatureEnum.MVTesting))
            {
                // Import MVT variants
                ImportProvider.ImportObjectType(settings, MVTVariantInfo.OBJECT_TYPE_DOCUMENTVARIANT, false, th, ProcessObjectEnum.All, documentsIds);
                // Import MVT combinations
                ImportProvider.ImportObjectType(settings, MVTCombinationInfo.OBJECT_TYPE_DOCUMENTCOMBINATION, false, th, ProcessObjectEnum.All, documentsIds);
            }
        }


        private static void ImportPersonalizationVariants(object sender, DocumentsImportEventArgs e)
        {
            var settings = e.Settings;
            var th = e.TranslationHelper;
            var documentsIds = e.ImportedDocumentIDs;

            // Import content personalization variants
            if (LicenseHelper.CheckFeature(settings.CurrentUrl, FeatureEnum.ContentPersonalization))
            {
                ImportProvider.ImportObjectType(settings, ContentPersonalizationVariantInfo.OBJECT_TYPE_DOCUMENTVARIANT, false, th, ProcessObjectEnum.All, documentsIds);
            }
        }


        private static void ExportObjects_After(object sender, ExportEventArgs e)
        {
            // Register translation record
            RegisterTranslationRecords(e.Data, e.ObjectType, e.TranslationHelper, e.Settings.ExcludedNames);
        }


        /// <summary>
        /// Ensure translation records registration.
        /// </summary>
        /// <param name="data">Source dataset</param>
        /// <param name="objectType">Type of the object</param>
        /// <param name="th">Translation helper</param>
        /// <param name="excludedNames">Excluded object names</param>
        private static void RegisterTranslationRecords(DataSet data, string objectType, TranslationHelper th, string[] excludedNames)
        {
            if (DataHelper.DataSourceIsEmpty(data))
            {
                return;
            }

            // Add translation records
            switch (objectType)
            {
                // Records for content personalization variants
                case ContentPersonalizationVariantInfo.OBJECT_TYPE:
                    th.RegisterRecords(data.Tables[0], PageTemplateInfo.OBJECT_TYPE, "VariantPageTemplateID", null, excludedNames);
                    break;

                // Records for document content personalization variants
                case ContentPersonalizationVariantInfo.OBJECT_TYPE_DOCUMENTVARIANT:
                    th.RegisterRecords(data.Tables[0], TreeNode.OBJECT_TYPE, "VariantDocumentID", null, excludedNames);
                    break;

                // Records for MVT variants
                case MVTVariantInfo.OBJECT_TYPE:
                    th.RegisterRecords(data.Tables[0], PageTemplateInfo.OBJECT_TYPE, "MVTVariantPageTemplateID", null, excludedNames);
                    break;

                // Records for document MVT variants
                case MVTVariantInfo.OBJECT_TYPE_DOCUMENTVARIANT:
                    th.RegisterRecords(data.Tables[0], TreeNode.OBJECT_TYPE, "MVTVariantDocumentID", null, excludedNames);
                    break;

                // Records for MVT combinations
                case MVTCombinationInfo.OBJECT_TYPE:
                    th.RegisterRecords(data.Tables[0], PageTemplateInfo.OBJECT_TYPE, "MVTCombinationPageTemplateID", null, excludedNames);
                    break;

                // Records for document MVT combinations
                case MVTCombinationInfo.OBJECT_TYPE_DOCUMENTCOMBINATION:
                    th.RegisterRecords(data.Tables[0], TreeNode.OBJECT_TYPE, "MVTCombinationDocumentID", null, excludedNames);
                    break;
            }
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

            // Content personalization variants
            var where = ImportExportHelper.GetDocumentsDataWhereCondition(ContentPersonalizationVariantInfo.OBJECT_TYPE_DOCUMENTVARIANT, data, "VariantDocumentID", "DocumentID");
            DocumentExport.ExportRelatedDocumentsData(settings, ContentPersonalizationVariantInfo.OBJECT_TYPE_DOCUMENTVARIANT, th, false, where);

            // MVT variants and combinations
            where = ImportExportHelper.GetDocumentsDataWhereCondition(MVTVariantInfo.OBJECT_TYPE_DOCUMENTVARIANT, data, "MVTVariantDocumentID", "DocumentID");
            DocumentExport.ExportRelatedDocumentsData(settings, MVTVariantInfo.OBJECT_TYPE_DOCUMENTVARIANT, th, false, where);

            where = ImportExportHelper.GetDocumentsDataWhereCondition(MVTCombinationInfo.OBJECT_TYPE_DOCUMENTCOMBINATION, data, "MVTCombinationDocumentID", "DocumentID");
            DocumentExport.ExportRelatedDocumentsData(settings, MVTCombinationInfo.OBJECT_TYPE_DOCUMENTCOMBINATION, th, false, where);
        }


        private static void GetObjectTypeFolder_Execute(object sender, GetObjectTypeFolderArgs e)
        {
            if (IsDocumentObjectType(e.ObjectType))
            {
                e.Folder = ImportExportHelper.DOCUMENTS_FOLDER;
            }
        }


        /// <summary>
        /// Returns true if object type is from documents data.
        /// </summary>
        /// <param name="objectType">Type of the object</param>
        private static bool IsDocumentObjectType(string objectType)
        {
            switch (objectType)
            {
                case ContentPersonalizationVariantInfo.OBJECT_TYPE_DOCUMENTVARIANT:
                case MVTVariantInfo.OBJECT_TYPE_DOCUMENTVARIANT:
                case MVTCombinationInfo.OBJECT_TYPE_DOCUMENTCOMBINATION:
                    return true;
            }

            return false;
        }


        private static void EnsureAutomaticSelection_Execute(object sender, ImportLoadSelectionArgs e)
        {
            var objectType = e.ObjectType;
            if ((objectType == MVTCombinationInfo.OBJECT_TYPE) || (objectType == MVTVariantInfo.OBJECT_TYPE))
            {
                var settings = e.Settings;
                e.ProcessDependency = !ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_PAGETEMPLATE_VARIANTS), false);
            }
        }


        private static void ImportChildType_Before(object sender, ImportObjectTypeArgs e)
        {
            var settings = e.Settings;
            var infoObj = e.ParentObject;
            var childType = e.ObjectType;

            // ### Special cases - Import the web part/zone/widget variants (MVT/Content personalization) only when the page template variant checkbox is checked
            if ((infoObj.TypeInfo.ObjectType == PageTemplateInfo.OBJECT_TYPE)
                && ((childType == MVTVariantInfo.OBJECT_TYPE)
                    || (childType == MVTCombinationInfo.OBJECT_TYPE)
                    || (childType == ContentPersonalizationVariantInfo.OBJECT_TYPE))
                && !ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_PAGETEMPLATE_VARIANTS), true))
            {
                e.Cancel();
            }
        }

        #endregion
    }
}