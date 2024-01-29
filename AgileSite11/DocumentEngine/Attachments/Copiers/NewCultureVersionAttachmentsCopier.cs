using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;

namespace CMS.DocumentEngine.Internal
{
    /// <summary>
    /// Provides methods to copy latest version of document attachments to the target document during creation of a new culture version.
    /// </summary>
    public class NewCultureVersionAttachmentsCopier : DocumentAttachmentsCopierBase
    {
        private bool GenerateVariants
        {
            get;
            set;
        }


        /// <summary>
        /// Creates instance of <see cref="NewCultureVersionAttachmentsCopier"/>.
        /// </summary>
        /// <param name="sourceDocument">Source document.</param>
        /// <param name="targetDocument">Target document.</param>
        /// <param name="generateVariants">Indicates if variants should be generated, not copied.</param>
        /// <remarks>Copies attachments from source document to target document.</remarks>
        public NewCultureVersionAttachmentsCopier(TreeNode sourceDocument, TreeNode targetDocument, bool generateVariants = false)
            : base(sourceDocument, targetDocument)
        {
            GenerateVariants = generateVariants;
            // Do not copy variants if generated
            CopyVariants = !GenerateVariants;
        }


        /// <summary>
        /// Gets the attachments except varinats for the source document.
        /// </summary>
        /// <param name="where">Where condition.</param>
        protected override IEnumerable<DocumentAttachment> GetAttachmentsExceptVariants(IWhereCondition where)
        {
            var data =
                DocumentHelper.GetAttachments(SourceDocument, false)
                    .ApplySettings(
                        settings => settings.Where(where)
                    );

            return data.Tables[0].AsEnumerable().Select(row => new DocumentAttachment(row));
        }


        /// <summary>
        /// Saves a copied attachment.
        /// </summary>
        /// <param name="attachment">Attachment.</param>
        protected override void SaveAttachment(DocumentAttachment attachment)
        {
            // Save the attachment version
            var versionId = TargetDocument.DocumentCheckedOutVersionHistoryID;
            if (versionId > 0)
            {
                // Save attachment within document version
                TargetDocument.VersionManager.SaveAttachmentVersion(attachment, TargetDocument.DocumentCheckedOutVersionHistoryID);
            }
            else
            {
                // Save attachment within published version
                DocumentHelper.SaveAttachment(attachment, TargetDocument);
            }

            if (GenerateVariants)
            {
                var context = new AttachmentVariantContext(TargetDocument);
                attachment.GenerateAllVariants(context);
            }
        }


        /// <summary>
        /// Saves a copied attachment variant.
        /// </summary>
        /// <param name="variant">Attachment variant.</param>
        protected override void SaveVariant(DocumentAttachment variant)
        {
            // Copy the attachment version
            var versionId = TargetDocument.DocumentCheckedOutVersionHistoryID;
            if (versionId > 0)
            {
                // Save variant as history info
                SaveVariantHistoryInternal(variant);
            }
            else
            {
                // Save variant as published attachment
                SaveVariantInternal(variant);
            }
        }


        /// <summary>
        /// Gets attachments variants based on the given list of parent IDs.
        /// </summary>
        /// <param name="parentAttachmentIds">Parent attachment IDs.</param>
        protected override IEnumerable<DocumentAttachment> GetVariants(IEnumerable<int> parentAttachmentIds)
        {
            IEnumerable<DocumentAttachment> query;
            if (SourceDocument.DocumentCheckedOutVersionHistoryID > 0)
            {
                query =
                    AttachmentHistoryInfoProvider.GetAttachmentHistories()
                        .BinaryData(false)
                        .VariantsForAttachments(parentAttachmentIds.ToArray())
                        .Select(att => new DocumentAttachment(att));
            }
            else
            {
                query =
                    AttachmentInfoProvider.GetAttachments()
                        .BinaryData(false)
                        .VariantsForAttachments(parentAttachmentIds.ToArray())
                        .Select(att => new DocumentAttachment(att));
            }

            return query;
        }


        /// <summary>
        /// Ensures binary data within the given attachment.
        /// </summary>
        /// <param name="sourceAttachment">Source attachment.</param>
        protected override void EnsureBinaryData(DocumentAttachment sourceAttachment)
        {
            var latest = SourceDocument.DocumentCheckedOutVersionHistoryID > 0;
            if (!latest)
            {
                base.EnsureBinaryData(sourceAttachment);
            }
            else
            {
                sourceAttachment.Generalized.EnsureBinaryData();
            }
        }
    }
}