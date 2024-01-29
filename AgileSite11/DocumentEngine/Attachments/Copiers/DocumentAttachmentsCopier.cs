using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.DocumentEngine.Internal;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides methods to copy document attachments for a published data.
    /// </summary>
    internal class DocumentAttachmentsCopier : DocumentAttachmentsCopierBase
    {
        /// <summary>
        /// Creates instance of <see cref="DocumentAttachmentsCopier"/>.
        /// </summary>
        /// <param name="sourceDocument">Source document.</param>
        /// <param name="targetDocument">Target document.</param>
        /// <remarks>Copies attachments from source document to target document.</remarks>
        public DocumentAttachmentsCopier(TreeNode sourceDocument, TreeNode targetDocument)
            : base(sourceDocument, targetDocument)
        {            
        }


        /// <summary>
        /// Saves a copied main attachment.
        /// </summary>
        /// <param name="attachment">Main attachment.</param>
        protected override void SaveAttachment(DocumentAttachment attachment)
        {
            DocumentHelper.SaveAttachment(attachment, TargetDocument);
        }


        /// <summary>
        /// Gets the main attachments for the source document.
        /// </summary>
        /// <param name="where">Where condition.</param>
        protected override IEnumerable<DocumentAttachment> GetAttachmentsExceptVariants(IWhereCondition where)
        {
            return 
                AttachmentInfoProvider.GetAttachments(SourceDocument.DocumentID, false)
                    .ExceptVariants()
                    .Where(where)
                    .Select(att => new DocumentAttachment(att));
        }


        /// <summary>
        /// Gets attachments variants based on the given list of parent IDs.
        /// </summary>
        /// <param name="parentAttachmentIds">Parent attachment IDs.</param>
        protected override IEnumerable<DocumentAttachment> GetVariants(IEnumerable<int> parentAttachmentIds)
        {
            return 
                AttachmentInfoProvider.GetAttachments(SourceDocument.DocumentID, false)
                    .VariantsForAttachments(parentAttachmentIds.ToArray())
                    .Select(att => new DocumentAttachment(att));
        }


        /// <summary>
        /// Saves a copied attachment variant.
        /// </summary>
        /// <param name="variant">Attachment variant.</param>
        protected override void SaveVariant(DocumentAttachment variant)
        {
            SaveVariantInternal(variant);
        }
    }
}