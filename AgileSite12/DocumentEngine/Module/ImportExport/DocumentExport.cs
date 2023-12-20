using System;
using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.SiteProvider;
using CMS.Synchronization;
using CMS.Taxonomy;
using CMS.WorkflowEngine;

using TaskInfo = CMS.Scheduler.TaskInfo;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Handles special actions during the Documents export process.
    /// </summary>
    public static class DocumentExport
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        internal static void Init()
        {
            ImportExportEvents.GetExportData.Before += GetExportData_Before;
            ImportExportEvents.ExportObjects.Before += ExportObjects_Before;
            ImportExportEvents.SingleExportSelection.Execute += SingleExportSelection_Execute;
        }


        private static void SingleExportSelection_Execute(object sender, SingleExportSelectionEventArgs e)
        {
            bool siteObject = (e.InfoObject.Generalized.ObjectSiteID > 0);
            if (siteObject)
            {
                e.Settings.SetObjectsProcessType(ProcessObjectEnum.None, TreeNode.OBJECT_TYPE, true);
            }
        }


        private static void ExportObjects_Before(object sender, ExportEventArgs e)
        {
            if (e.ObjectType == TreeNode.OBJECT_TYPE)
            {
                var settings = e.Settings;

                // Get all data for documents if site is exported
                if (settings.GetObjectsProcessType(TreeNode.OBJECT_TYPE, true) != ProcessObjectEnum.None)
                {
                    ExportDocumentsData(settings, e.TranslationHelper);
                }

                // Cancel further processing
                e.Cancel();
            }
        }


        private static void GetExportData_Before(object sender, ExportGetDataEventArgs e)
        {
            if (e.ObjectType == TreeNode.OBJECT_TYPE)
            {
                // Get documents
                e.Data = GetDocuments(e.Settings);

                // Cancel further processing
                e.Cancel();
            }
        }


        /// <summary>
        /// Exports the documents.
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="th">Translation table</param>
        internal static void ExportDocumentsData(SiteExportSettings settings, TranslationHelper th)
        {
            // Export process canceled
            if (settings.ProcessCanceled)
            {
                ExportProvider.ExportCanceled();
            }

            try
            {
                // Handle the event
                using (var h = DocumentImportExportEvents.ExportDocuments.StartEvent(settings, th))
                {
                    if (h.CanContinue())
                    {
                        // Export documents data
                        DataSet dsDocuments = ExportProvider.GetExportData(settings, TreeNode.OBJECT_TYPE, true, true, false, th);
                        if (!DataHelper.DataSourceIsEmpty(dsDocuments))
                        {
                            // Register translation records
                            RegisterTranslationRecords(dsDocuments, TreeNode.OBJECT_TYPE, th, settings.SiteName, null);

                            // Save documents
                            ExportProvider.SaveObjects(settings, dsDocuments, TreeNode.OBJECT_TYPE, true);

                            // ACLs and ACL items
                            WhereCondition where;

                            if (ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_DOC_ACLS), true))
                            {
                                where = ImportExportHelper.GetDocumentsDataWhereCondition(AclInfo.OBJECT_TYPE, dsDocuments, "ACLID", "NodeACLID");
                                ExportRelatedDocumentsData(settings, AclInfo.OBJECT_TYPE, th, false, where);
                            }

                            // Attachments
                            where = ImportExportHelper.GetDocumentsDataWhereCondition(AttachmentInfo.OBJECT_TYPE, dsDocuments, "AttachmentDocumentID", "DocumentID");
                            ExportRelatedDocumentsData(settings, AttachmentInfo.OBJECT_TYPE, th, false, where);

                            // Relationships
                            if (ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_DOC_RELATIONSHIPS), true))
                            {
                                var leftWhere = ImportExportHelper.GetDocumentsDataWhereCondition(RelationshipInfo.OBJECT_TYPE, dsDocuments, "LeftNodeID", "NodeID");
                                var rightWhere = ImportExportHelper.GetDocumentsDataWhereCondition(RelationshipInfo.OBJECT_TYPE, dsDocuments, "RightNodeID", "NodeID");

                                where = new WhereCondition().Where(leftWhere).Or(rightWhere);

                                ExportRelatedDocumentsData(settings, RelationshipInfo.OBJECT_TYPE, th, false, where);
                                ExportRelatedDocumentsData(settings, RelationshipInfo.OBJECT_TYPE_ADHOC, th, false, where);
                            }

                            // Export history
                            if (ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_DOC_HISTORY), true))
                            {
                                // Get versions histories
                                where = ImportExportHelper.GetDocumentsDataWhereCondition(VersionHistoryInfo.OBJECT_TYPE, dsDocuments, "DocumentID", "DocumentID");
                                DataSet versionHistoryDS = ExportRelatedDocumentsData(settings, VersionHistoryInfo.OBJECT_TYPE, th, false, where);

                                // Get attachment histories
                                where = ImportExportHelper.GetDocumentsDataWhereCondition(AttachmentHistoryInfo.OBJECT_TYPE, dsDocuments, "AttachmentDocumentID", "DocumentID");
                                ExportRelatedDocumentsData(settings, AttachmentHistoryInfo.OBJECT_TYPE, th, false, where);

                                // Get workflow histories
                                // Prepare the condition for selection
                                var versionIDs = DataHelper.GetIntegerValues(versionHistoryDS.Tables["CMS_VersionHistory"], "VersionHistoryID");

                                where = new WhereCondition().WhereIn("VersionHistoryID", versionIDs);

                                ExportRelatedDocumentsData(settings, WorkflowHistoryInfo.OBJECT_TYPE, th, false, where);
                            }

                            // Document scheduled tasks
                            where = ImportExportHelper.GetDocumentsDataWhereCondition(TaskInfo.OBJECT_TYPE_OBJECTTASK, dsDocuments, "TaskObjectID", "DocumentID");
                            where.WhereEquals("TaskObjectType", TreeNode.OBJECT_TYPE);

                            ExportRelatedDocumentsData(settings, TaskInfo.OBJECT_TYPE_OBJECTTASK, th, true, where);

                            // Document categories
                            where = ImportExportHelper.GetDocumentsDataWhereCondition(DocumentCategoryInfo.OBJECT_TYPE, dsDocuments, "DocumentID", "DocumentID");
                            ExportRelatedDocumentsData(settings, DocumentCategoryInfo.OBJECT_TYPE, th, false, where);

                            // Document aliases
                            where = ImportExportHelper.GetDocumentsDataWhereCondition(DocumentAliasInfo.OBJECT_TYPE, dsDocuments, "AliasNodeID", "NodeID");
                            ExportRelatedDocumentsData(settings, DocumentAliasInfo.OBJECT_TYPE, th, false, where);

                            // Alternative urls
                            where = ImportExportHelper.GetDocumentsDataWhereCondition(AlternativeUrlInfo.OBJECT_TYPE, dsDocuments, "AlternativeUrlDocumentID", "DocumentID");
                            ExportRelatedDocumentsData(settings, AlternativeUrlInfo.OBJECT_TYPE, th, false, where);
                        }

                        h.EventArguments.Data = dsDocuments;
                    }

                    h.FinishEvent();
                }
            }
            catch (ProcessCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Log exception
                ExportProvider.LogProgressError(settings, settings.GetAPIString("ExportSite.ErrorExportingDocuments", "Error exporting pages"), ex);
                throw;
            }
        }


        /// <summary>
        /// Exports related document data for given object type.
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="objectType">Type of the object</param>
        /// <param name="th">Translation helper</param>
        /// <param name="siteObjects">Indicates if the object type is site dependent</param>
        /// <param name="where">Where condition</param>
        public static DataSet ExportRelatedDocumentsData(SiteExportSettings settings, string objectType, TranslationHelper th, bool siteObjects, WhereCondition where)
        {
            DataSet ds = null;

            var e = new ExportEventArgs
            {
                Settings = settings,
                TranslationHelper = th,
                ObjectType = objectType,
                SiteObjects = siteObjects
            };

            // Handle the event
            using (var h = ImportExportEvents.ExportObjects.StartEvent(e))
            {
                if (h.CanContinue())
                {
                    // Log process
                    ExportProvider.LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ExportSite.Exporting", "Exporting {0}"), ImportExportHelper.GetObjectTypeName(objectType, settings)));

                    // Save the settings
                    settings.SavePersistentLog();

                    // Get data
                    ds = ExportProvider.GetExportData(settings, where, objectType, true, false, th);

                    // Register translation record
                    RegisterTranslationRecords(ds, objectType, th, settings.SiteName, settings.ExcludedNames);

                    // Save data
                    ExportProvider.SaveObjects(settings, ds, objectType, siteObjects);

                    // Copy files
                    ExportProvider.CopyFiles(settings, ds, objectType);

                    h.EventArguments.Data = ds;
                }

                h.FinishEvent();
            }

            return ds;
        }


        /// <summary>
        /// Ensure translation records registration.
        /// </summary>
        /// <param name="ds">Source dataset</param>
        /// <param name="objectType">Type of the object</param>
        /// <param name="th">Translation helper</param>
        /// <param name="siteName">Site name</param>
        /// <param name="excludedNames">Excluded object names</param>
        private static void RegisterTranslationRecords(DataSet ds, string objectType, TranslationHelper th, string siteName, string[] excludedNames)
        {
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // Add translation records
                DataTable table = ds.Tables[0];

                switch (objectType)
                {
                    // Records for object versions
                    case ObjectVersionHistoryInfo.OBJECT_TYPE:
                        th.RegisterRecords(table, UserInfo.OBJECT_TYPE, "VersionModifiedByUserID", null, excludedNames);
                        th.RegisterRecords(table, UserInfo.OBJECT_TYPE, "VersionDeletedByUserID", null, excludedNames);
                        break;

                    // Records for documents
                    case TreeNode.OBJECT_TYPE:
                        foreach (DataTable dt in ds.Tables)
                        {
                            th.RegisterRecords(dt, DataClassInfo.OBJECT_TYPE, "NodeClassID", null, excludedNames);
                            th.RegisterRecords(dt, AclInfo.OBJECT_TYPE, "NodeACLID", null, excludedNames);
                            th.RegisterRecords(dt, UserInfo.OBJECT_TYPE, "NodeOwner", null, excludedNames);
                            th.RegisterRecords(dt, PredefinedObjectType.GROUP, "NodeGroupID", siteName, excludedNames);

                            th.RegisterRecords(dt, UserInfo.OBJECT_TYPE, "DocumentModifiedByUserID", null, excludedNames);
                            th.RegisterRecords(dt, UserInfo.OBJECT_TYPE, "DocumentCreatedByUserID", null, excludedNames);
                            th.RegisterRecords(dt, UserInfo.OBJECT_TYPE, "DocumentCheckedOutByUserID", null, excludedNames);
                            th.RegisterRecords(dt, WorkflowStepInfo.OBJECT_TYPE, "DocumentWorkflowStepID", null, excludedNames);

                            th.RegisterRecords(dt, PageTemplateInfo.OBJECT_TYPE, "DocumentPageTemplateID", null, excludedNames);
                            th.RegisterRecords(dt, PageTemplateInfo.OBJECT_TYPE, "DocumentPageTemplateID", siteName, excludedNames);

                            th.RegisterRecords(dt, PageTemplateInfo.OBJECT_TYPE, "NodeTemplateID", null, excludedNames);
                            th.RegisterRecords(dt, PageTemplateInfo.OBJECT_TYPE, "NodeTemplateID", siteName, excludedNames);

                            th.RegisterRecords(dt, TagGroupInfo.OBJECT_TYPE, "DocumentTagGroupID", siteName, excludedNames);

                            // Raise event to register custom translations
                            if (ColumnsTranslationEvents.RegisterRecords.IsBound)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    ColumnsTranslationEvents.RegisterRecords.StartEvent(th, objectType, new DataRowContainer(row));
                                }
                            }
                        }
                        break;

                    // Records for ACLs
                    case AclInfo.OBJECT_TYPE:
                        // Register records for users and roles
                        if (ds.Tables.Contains("CMS_ACLItem"))
                        {
                            DataTable dt = ds.Tables["CMS_ACLItem"];
                            th.RegisterRecords(dt, UserInfo.OBJECT_TYPE, "UserID", null, excludedNames);
                            th.RegisterRecords(dt, RoleInfo.OBJECT_TYPE, "RoleID", siteName, excludedNames);
                        }
                        break;

                    // Records for attachments
                    case AttachmentInfo.OBJECT_TYPE:
                        break;

                    // Records for version histories
                    case VersionHistoryInfo.OBJECT_TYPE:
                        th.RegisterRecords(table, UserInfo.OBJECT_TYPE, "ModifiedByUserID", null, excludedNames);
                        th.RegisterRecords(table, UserInfo.OBJECT_TYPE, "VersionDeletedByUserID", null, excludedNames);
                        th.RegisterRecords(table, DataClassInfo.OBJECT_TYPE, "VersionClassID", null, excludedNames);
                        th.RegisterRecords(table, WorkflowInfo.OBJECT_TYPE, "VersionWorkflowID", null, excludedNames);
                        th.RegisterRecords(table, WorkflowStepInfo.OBJECT_TYPE, "VersionWorkflowStepID", null, excludedNames);
                        break;

                    // Records for attachment histories
                    case AttachmentHistoryInfo.OBJECT_TYPE:
                        break;

                    // Records for workflow histories
                    case WorkflowHistoryInfo.OBJECT_TYPE:
                        th.RegisterRecords(table, WorkflowStepInfo.OBJECT_TYPE, "StepID", null, excludedNames);
                        th.RegisterRecords(table, WorkflowStepInfo.OBJECT_TYPE, "TargetStepID", null, excludedNames);
                        th.RegisterRecords(table, UserInfo.OBJECT_TYPE, "ApprovedByUserID", null, excludedNames);
                        th.RegisterRecords(table, TreeNode.OBJECT_TYPE, "HistoryObjectID", null, excludedNames);
                        th.RegisterRecords(table, WorkflowInfo.OBJECT_TYPE, "HistoryWorkflowID", null, excludedNames);
                        break;

                    // Records for categories
                    case DocumentCategoryInfo.OBJECT_TYPE:
                        th.RegisterRecords(table, CategoryInfo.OBJECT_TYPE, "CategoryID", null, excludedNames);
                        break;

                    // Records for scheduled tasks
                    case TaskInfo.OBJECT_TYPE_OBJECTTASK:
                        th.RegisterRecords(table, UserInfo.OBJECT_TYPE, "TaskUserID", null, excludedNames);
                        break;
                }

                // Raise event to register custom translations
                if ((objectType != TreeNode.OBJECT_TYPE) && ColumnsTranslationEvents.RegisterRecords.IsBound)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        ColumnsTranslationEvents.RegisterRecords.StartEvent(th, objectType, new DataRowContainer(row));
                    }
                }
            }
        }


        /// <summary>
        /// Gets the documents data.
        /// </summary>
        /// <param name="settings">Export settings</param>
        private static DataSet GetDocuments(SiteExportSettings settings)
        {
            // Get class names allowed for specified site
            string classNames = "";

            var dsDocTypes = SiteInfoProvider.GetDocumentTypeClassPerSite(settings.SiteId);

            if (!DataHelper.DataSourceIsEmpty(dsDocTypes))
            {
                foreach (DataRow dr in dsDocTypes.Tables[0].Rows)
                {
                    classNames += dr["ClassName"] + ";";
                }
            }

            // Get the documents
            var tree = new TreeProvider(settings.CurrentUser)
            {
                AllowAsyncActions = false,
            };

            var parameters = new NodeSelectionParameters
            {
                SiteName = settings.SiteName,
                CultureCode = TreeProvider.ALL_CULTURES,
                CombineWithDefaultCulture = false,
                ClassNames = classNames,
                OrderBy = "NodeLevel ASC, NodeID",
                SelectOnlyPublished = false,
                CheckLicense = false // Do not check the license in export
            };

            return tree.SelectNodes(parameters);
        }

        #endregion
    }
}