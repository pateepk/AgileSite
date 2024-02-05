using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;

namespace CMS.DocumentEngine.Internal
{
    /// <summary>
    /// Base class for copying document attachments.
    /// </summary>
    public abstract class DocumentAttachmentsCopierBase
    {
        #region "Properties"

        /// <summary>
        /// Target document.
        /// </summary>
        protected TreeNode TargetDocument
        {
            get;
            set;
        }


        /// <summary>
        /// Source document.
        /// </summary>
        protected TreeNode SourceDocument
        {
            get;
            set;
        }


        /// <summary>
        /// Collection of original attachment GUID as key and copied attachment GUID as value.
        /// </summary>
        protected internal IDictionary<Guid, Guid> AttachmentGUIDs
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates if variants should be copied.
        /// </summary>
        protected bool CopyVariants
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates instance of <see cref="DocumentAttachmentsCopierBase"/>.
        /// </summary>
        /// <param name="sourceDocument">Source document.</param>
        /// <param name="targetDocument">Target document.</param>
        protected DocumentAttachmentsCopierBase(TreeNode sourceDocument, TreeNode targetDocument)
        {
            if (sourceDocument == null)
            {
                throw new ArgumentNullException(nameof(sourceDocument));
            }

            if (targetDocument == null)
            {
                throw new ArgumentNullException(nameof(targetDocument));
            }

            SourceDocument = sourceDocument;
            TargetDocument = targetDocument;
            AttachmentGUIDs = new Dictionary<Guid, Guid>();
            CopyVariants = true;
        }


        /// <summary>
        /// Copies attachments.
        /// </summary>
        /// <returns>Indicates if any of the field attachments was copied and the document data should be updated.</returns>
        public bool Copy()
        {
            CopyGroupedAndUnsortedAttachments();

            return CopyFieldAttachments();
        }


        /// <summary>
        /// Copies field attachments.
        /// </summary>
        /// <returns>Indicates if any of the field attachments was copied and the document data should be updated.</returns>
        /// <remarks> Expects that <see cref="TargetDocument"/> is newly created culture 
        /// version of <see cref="SourceDocument"/>  and its values of field attachments are not cleared. </remarks>
        public bool CopyFieldAttachments()
        {
            if (!TargetDocument.IsCoupled)
            {
                return false;
            }

            var fields = GetAttachmentFields();
            var where = GetFieldWhere(fields);
            CopyAttachments(where);

            return UpdateDocumentFieldValues(fields);
        }


        /// <summary>
        /// Updates document fields with GUID of copied attachment.
        /// </summary>
        /// <param name="fields">List of attachment fields.</param>
        /// <returns>Indicates if at least one value of document field was updated.</returns>
        protected bool UpdateDocumentFieldValues(List<AttachmentField> fields)
        {
            bool update = false;

            fields.ForEach(f =>
            {
                if (!AttachmentGUIDs.ContainsKey(f.Value))
                {
                    return;
                }

                TargetDocument.SetValue(f.Name, AttachmentGUIDs[f.Value]);
                update = true;
            });

            return update;
        }


        /// <summary>
        /// Gets where condition for field attachments.
        /// </summary>
        /// <param name="fields">List of attachment fields.</param>
        protected WhereCondition GetFieldWhere(List<AttachmentField> fields)
        {
            return new WhereCondition().WhereIn("AttachmentGUID", fields.Select(f => f.Value).ToList());
        }


        /// <summary>
        /// Gets list of attachment fields.
        /// </summary>
        protected List<AttachmentField> GetAttachmentFields()
        {
            var form = FormHelper.GetFormInfo(TargetDocument.ClassName, false);
            return
                form.GetFields(FieldDataType.File)
                    .Select(field => CreateAttachmentField(field.Name))
                    .Where(field => (field != null) && (field.Value != Guid.Empty))
                    .ToList();
        }


        private AttachmentField CreateAttachmentField(string name)
        {
            var sourceFieldValue = ValidationHelper.GetGuid(SourceDocument.GetValue(name), Guid.Empty);
            var targetFieldValue = ValidationHelper.GetGuid(TargetDocument.GetValue(name), Guid.Empty);

            // If values in source and target do not match, the attachment should not be copied, because there is already some new different attachment
            if (sourceFieldValue != targetFieldValue)
            {
                return null;
            }

            return new AttachmentField
            {
                Name = name,
                Value = sourceFieldValue
            };
        }


        private void CopyGroupedAndUnsortedAttachments()
        {
            var where = GetGroupedAndUnsortedWhere();
            CopyAttachments(where);
        }


        private WhereCondition GetGroupedAndUnsortedWhere()
        {
            return
                new WhereCondition()
                    .Where(w => w
                        .WhereTrue("AttachmentIsUnsorted")
                        .Or()
                        .WhereNotNull("AttachmentGroupGUID")
                    );
        }


        private void CopyAttachments(WhereCondition where)
        {
            var processedAttachments = CopyAttachmentsExceptVariants(where);
            CopyAttachmentVariants(processedAttachments);
        }


        /// <summary>
        /// Copies attachments except variants.
        /// </summary>
        /// <param name="where">Where condition to limit copied attachments.</param>
        /// <returns>Dictionary of copied attachments where key is the original attachment ID.</returns>
        protected IDictionary<int, int> CopyAttachmentsExceptVariants(WhereCondition where)
        {
            var processedAttachments = new Dictionary<int, int>();
            var attachments = GetAttachmentsExceptVariants(where);

            foreach (var attachment in attachments)
            {
                var originalId = attachment.ID;

                EnsureCopiedAttachmentData(attachment);
                SaveAttachment(attachment);

                processedAttachments.Add(originalId, attachment.ID);
            }

            return processedAttachments;
        }


        private void CopyAttachmentVariants(IDictionary<int, int> processedAttachments)
        {
            if (!CopyVariants)
            {
                return;
            }

            var ids = processedAttachments.Keys;
            if (ids.Count == 0)
            {
                return;
            }

            var variants = GetVariants(ids);

            foreach (var variant in variants)
            {
                EnsureCopiedAttachmentData(variant);
                variant.AttachmentVariantParentID = processedAttachments[variant.AttachmentVariantParentID];

                SaveVariant(variant);
            }
        }


        private void EnsureCopiedAttachmentData(DocumentAttachment sourceAttachment)
        {
            EnsureBinaryData(sourceAttachment);

            var originalGuid = sourceAttachment.AttachmentGUID;

            sourceAttachment.ID = 0;
            sourceAttachment.AttachmentGUID = Guid.NewGuid();
            sourceAttachment.AttachmentDocumentID = TargetDocument.DocumentID;
            sourceAttachment.AttachmentSiteID = TargetDocument.NodeSiteID;

            AttachmentGUIDs.Add(originalGuid, sourceAttachment.AttachmentGUID);
        }


        /// <summary>
        /// Ensures binary data within the given attachment.
        /// </summary>
        /// <param name="sourceAttachment">Source attachment.</param>
        protected virtual void EnsureBinaryData(DocumentAttachment sourceAttachment)
        {
            if (sourceAttachment.AttachmentBinary != null)
            {
                return;
            }

            sourceAttachment.AttachmentBinary = AttachmentBinaryHelper.GetAttachmentBinary(sourceAttachment);
        }


        /// <summary>
        /// Saves the attachment variant as <see cref="AttachmentInfo"/> published attachment.
        /// </summary>
        /// <param name="attachment">Attachment to save.</param>
        protected void SaveVariantInternal(DocumentAttachment attachment)
        {
            var attachmentInfo = attachment.GetAttachmentInfo();
            attachmentInfo.Insert();
            attachment.Load(attachmentInfo);
        }


        /// <summary>
        /// Saves the attachment variant as <see cref="AttachmentHistoryInfo"/> attachment version.
        /// </summary>
        /// <param name="attachment">Attachment to save.</param>
        protected void SaveVariantHistoryInternal(DocumentAttachment attachment)
        {
            var history = new AttachmentHistoryInfo();
            history.ApplyData(attachment);
            // Clear applied GUID from existing attachment history
            history.AttachmentHistoryGUID = Guid.NewGuid();
            history.Insert();
            attachment.Load(history, TargetDocument.DocumentCheckedOutVersionHistoryID);
        }

        #endregion


        #region "Abstract methods"

        /// <summary>
        /// Gets attachments variants based on the given list of parent IDs.
        /// </summary>
        /// <param name="parentAttachmentIds">Parent attachment IDs.</param>
        protected abstract IEnumerable<DocumentAttachment> GetVariants(IEnumerable<int> parentAttachmentIds);


        /// <summary>
        /// Saves a copied attachment variant.
        /// </summary>
        /// <param name="variant">Attachment variant.</param>
        protected abstract void SaveVariant(DocumentAttachment variant);


        /// <summary>
        /// Gets the attachments except variants for the source document.
        /// </summary>
        /// <param name="where">Where condition.</param>
        protected abstract IEnumerable<DocumentAttachment> GetAttachmentsExceptVariants(IWhereCondition where);


        /// <summary>
        /// Saves a copied attachment.
        /// </summary>
        /// <param name="attachment">Attachment.</param>
        protected abstract void SaveAttachment(DocumentAttachment attachment);

        #endregion


        /// <summary>
        /// Attachment field.
        /// </summary>
        protected class AttachmentField
        {
            /// <summary>
            /// Name of the field.
            /// </summary>
            public string Name
            {
                get;
                set;
            }


            /// <summary>
            /// Value of the field.
            /// </summary>
            public Guid Value
            {
                get;
                set;
            }
        }
    }
}