using System.Collections.Generic;
using System.Linq;
using System.Data;
using System;

using CMS.Helpers;
using CMS.DataEngine;
using CMS.Base;
using CMS.DocumentEngine;
using CMS.Base.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Data provider for object transformation which retrieves attachments together with their variants
    /// </summary>
    public class AttachmentsWithVariantsTransformationDataProvider : ObjectTransformationDataProvider
    {
        /// <summary>
        /// Document node
        /// </summary>
        public TreeNode Node
        {
            get;
            private set;
        }


        /// <summary>
        /// Version history ID
        /// </summary>
        public int VersionHistoryID
        {
            get;
            private set;
        }


        /// <summary>
        /// Form GUID, when set, temporary attachments with this GUID are also retrieved
        /// </summary>
        public Guid FormGUID
        {
            get;
            private set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Parent document of the attachments</param>
        /// <param name="documentManager">Parent document manager</param>
        /// <param name="formGuid">Parent form GUID</param>
        public AttachmentsWithVariantsTransformationDataProvider(TreeNode node, ICMSDocumentManager documentManager, Guid formGuid)
        {
            FormGUID = Guid.Empty;

            if (documentManager != null)
            {
                // When new culture version is created based on existing version, use the existing, because some attachments may be loaded from it, and other attachments are temporary
                if ((documentManager.Mode == FormModeEnum.InsertNewCultureVersion) && (documentManager.SourceNode != null))
                {
                    node = documentManager.SourceNode;
                }

                // Get temporary attachments by GUID only in case of new / new culture version
                if (documentManager.Mode != FormModeEnum.Update)
                {
                    FormGUID = formGuid;
                }
            }

            DefaultDataHandler = GetAttachmentsWithVariantsByIds;
            Node = node;

            // Remember version history ID at the initialization phase to make sure the input IDs are consistent with retrieved data (node could move within workflow)
            VersionHistoryID = node.DocumentCheckedOutVersionHistoryID;
        }


        /// <summary>
        /// Gets the objects together with their children
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="ids">IDs</param>
        private IGeneralIndexable<int, IDataContainer> GetAttachmentsWithVariantsByIds(string objectType, IEnumerable<int> ids)
        {
            // Get the attachments
            var attachments = GetAllAttachments();
            var typeInfo = GetTypeInfo();

            var where = GetIDsWhere(ids, typeInfo);

            attachments.ApplySettings(s => s.Where(where));

            return GetResult(attachments, typeInfo);
        }


        private IDataQuery GetAllAttachments()
        {
            if (VersionHistoryID > 0)
            {
                return GetVersionedAttachments();
            }

            return GetPublishedAttachments();
        }


        private IDataQuery GetVersionedAttachments()
        {
            var commonColumns = AttachmentInfo.TYPEINFO.ClassStructureInfo.ColumnNames.Intersect(AttachmentHistoryInfo.TYPEINFO.ClassStructureInfo.ColumnNames).ToList();

            var versionAttachments =
                AttachmentHistoryInfoProvider.GetAttachmentHistories()
                    .Columns(commonColumns)
                    .AddColumn("AttachmentHistoryID")
                    .AddColumn(new QueryColumn("NULL").As("AttachmentFormGUID"))
                    .BinaryData(false)
                    .AllInVersion(VersionHistoryID);

            if (FormGUID != Guid.Empty)
            {
                // In case of new culture version do not include variants to keep new UI consistent
                versionAttachments.InVersionExceptVariants(VersionHistoryID);

                var temporaryAttachments =
                    AttachmentInfoProvider.GetAttachments()
                        .Columns(commonColumns)
                        .AddColumn(new QueryColumn("AttachmentID").As("AttachmentHistoryID"))
                        .AddColumn("AttachmentFormGUID")
                        .BinaryData(false)
                        .WhereEquals("AttachmentFormGUID", FormGUID);

                return DataQuery.Combine(new IDataQuery[] { versionAttachments, temporaryAttachments }, new[] { SqlOperator.UNION_ALL });
            }

            // In case of editing include variants
            return versionAttachments.AllInVersion(VersionHistoryID);
        }
        

        private IDataQuery GetPublishedAttachments()
        {
            var publishedAttachments =
                            AttachmentInfoProvider.GetAttachments()
                                .BinaryData(false)
                                .Where(GetPublishedAttachmentsWhereCondition());

            if (FormGUID != Guid.Empty)
            {
                // Do not include variants in case of new culture version to keep new UI consistent
                publishedAttachments.ExceptVariants();
            }

            return publishedAttachments;
        }


        private WhereCondition GetPublishedAttachmentsWhereCondition()
        {
            var documentId = Node.DocumentID;

            var where = new WhereCondition();
            if (FormGUID != Guid.Empty)
            {
                // Get temporary attachments
                where.WhereEquals("AttachmentFormGUID", FormGUID);

                // And attachments from current (specific) document
                if (documentId > 0)
                {
                    where.Or().WhereEquals("AttachmentDocumentID", Node.DocumentID);
                }
            }
            else
            {
                if (documentId > 0)
                {
                    // Only specific document attachments
                    where.WhereEquals("AttachmentDocumentID", Node.DocumentID);
                }
                else
                {
                    // Neither specific document attachments, nor temporary
                    where.NoResults();
                }
            }

            return where;
        }


        private ObjectTypeInfo GetTypeInfo()
        {
            return (VersionHistoryID <= 0) ? AttachmentInfo.TYPEINFO_VARIANT : AttachmentHistoryInfo.TYPEINFO_VARIANT;
        }


        private IGeneralIndexable<int, IDataContainer> GetResult(IDataQuery attachments, ObjectTypeInfo typeInfo)
        {
            var attachmentsList = attachments.Result.Tables[0].Rows.Cast<DataRow>();
            var groupedAttachments = attachmentsList.GroupBy(att => ValidationHelper.GetInteger(att[typeInfo.ParentIDColumn], 0)).ToDictionary(group => group.Key);

            var result = new SafeDictionary<int, IDataContainer>();

            // Group attachments with their variants
            var mains = groupedAttachments[0];

            foreach (var main in mains)
            {
                var id = ValidationHelper.GetInteger(main[typeInfo.IDColumn], 0);
                var withVariants = CreateAttachmentWithVariants(main, id, groupedAttachments);

                result[id] = withVariants;
            }

            return result;
        }


        private AttachmentWithVariants CreateAttachmentWithVariants(DataRow main, int id, Dictionary<int, IGrouping<int, DataRow>> groupedAttachments)
        {
            IGrouping<int, DataRow> variants;
            groupedAttachments.TryGetValue(id, out variants);

            var variantsList = CreateVariantsList(variants);

            var withVariants =
                new AttachmentWithVariants(
                    VersionHistoryID,
                    new DocumentAttachment(main) { AttachmentVersionHistoryID = VersionHistoryID },
                    variantsList
                );

            return withVariants;
        }


        private static List<DocumentAttachment> CreateVariantsList(IGrouping<int, DataRow> variants)
        {
            return 
                (variants ?? Enumerable.Empty<DataRow>())
                    .Select(
                        att => new DocumentAttachment(att)
                    )
                    .OrderBy(att => att.AttachmentVariantDefinitionIdentifier, StringComparer.InvariantCultureIgnoreCase)
                    .ToList();
        }


        private static WhereCondition GetIDsWhere(IEnumerable<int> ids, ObjectTypeInfo typeInfo)
        {
            var idsList = ids.ToList();

            return 
                new WhereCondition()
                    .WhereIn(typeInfo.IDColumn, idsList)
                    .Or()
                    .WhereIn(typeInfo.ParentIDColumn, idsList);
        }
    }
}
