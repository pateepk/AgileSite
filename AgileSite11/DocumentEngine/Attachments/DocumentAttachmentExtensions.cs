using CMS.ResponsiveImages;
using System;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Extension methods for attachment.
    /// </summary>
    public static class DocumentAttachmentExtensions
    {
        /// <summary>
        /// Returns <see cref="DocumentAttachment"/> that represents variant of the given attachment. The variant is specified by <paramref name="definitionIdentifier"/> parameter.
        /// </summary>
        /// <param name="attachment">Document attachment.</param>
        /// <param name="definitionIdentifier">Definition identifier.</param>
        /// <exception cref="ArgumentNullException">Thrown when either of the parameters are null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="definitionIdentifier"/> is empty.</exception>
        public static DocumentAttachment GetVariant(this DocumentAttachment attachment, string definitionIdentifier)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException("attachment");
            }
            if (definitionIdentifier == null)
            {
                throw new ArgumentNullException("definitionIdentifier");
            }
            if (String.IsNullOrWhiteSpace(definitionIdentifier))
            {
                throw new ArgumentException("Definition identifier cannot be empty.", "definitionIdentifier");
            }

            var historyInfo = attachment.WrappedAttachment as AttachmentHistoryInfo;
            if (historyInfo != null)
            {
                return (DocumentAttachment)historyInfo.GetVariant(definitionIdentifier);
            }

            var atInfo = attachment.WrappedAttachment as AttachmentInfo;
            return (DocumentAttachment)atInfo.GetVariant(definitionIdentifier);
        }


        /// <summary>
        /// Generates single image variant for given attachment. The variant is specified by <paramref name="definitionIdentifier"/> parameter.
        /// </summary>
        /// <remarks>Deletes an existing variant if the given variant definition is no longer applicable on the attachment.</remarks>
        /// <param name="attachment">Document attachment.</param>
        /// <param name="context">Variant processing context.</param>
        /// <param name="definitionIdentifier">Definition identifier.</param>
        /// <exception cref="ArgumentNullException">Thrown when either of the parameters are null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="definitionIdentifier"/> is empty.</exception>
        public static DocumentAttachment GenerateVariant(this DocumentAttachment attachment, IVariantContext context, string definitionIdentifier)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException("attachment");
            }
            if (definitionIdentifier == null)
            {
                throw new ArgumentNullException("definitionIdentifier");
            }
            if (String.IsNullOrWhiteSpace(definitionIdentifier))
            {
                throw new ArgumentException("Definition identifier cannot be empty.", "definitionIdentifier");
            }

            var historyInfo = attachment.WrappedAttachment as AttachmentHistoryInfo;
            if (historyInfo != null)
            {
                return (DocumentAttachment)historyInfo.GenerateVariant(context, definitionIdentifier);
            }

            var atInfo = attachment.WrappedAttachment as AttachmentInfo;
            return (DocumentAttachment)atInfo.GenerateVariant(context, definitionIdentifier);
        }


        /// <summary>
        /// Generates all image variants for given attachment overwriting existing variants.
        /// </summary>
        /// <remarks>Only applicable variants will be generated. Variant definitions which are no more applicable to the attachment will be deleted.</remarks>
        /// <param name="attachment">Document attachment.</param>
        /// <param name="context">Variant processing context.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="attachment"/> is null.</exception>
        public static void GenerateAllVariants(this DocumentAttachment attachment, IVariantContext context)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException("attachment");
            }

            var historyInfo = attachment.WrappedAttachment as AttachmentHistoryInfo;
            if (historyInfo != null)
            {
                historyInfo.GenerateAllVariants(context);
                return;
            }

            var atInfo = attachment.WrappedAttachment as AttachmentInfo;
            atInfo.GenerateAllVariants(context);
        }


        /// <summary>
        /// Generates attachment image variants which were not yet generated.
        /// </summary>
        /// <param name="attachment">Document attachment.</param>
        /// <param name="context">Variant processing context.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="attachment"/> is null.</exception>
        public static void GenerateMissingVariants(this DocumentAttachment attachment, IVariantContext context)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException("attachment");
            }

            var historyInfo = attachment.WrappedAttachment as AttachmentHistoryInfo;
            if (historyInfo != null)
            {
                historyInfo.GenerateMissingVariants(context);
                return;
            }

            var atInfo = attachment.WrappedAttachment as AttachmentInfo;
            atInfo.GenerateMissingVariants(context);
        }


        /// <summary>
        /// Loads wrapped attachment from a given source.
        /// </summary>
        /// <param name="attachment">Document attachment.</param>
        /// <param name="sourceAttachment">Attachment info to load.</param>
        internal static void Load(this DocumentAttachment attachment, AttachmentInfo sourceAttachment)
        {
            attachment.AttachmentVersionHistoryID = 0;
            attachment.WrappedAttachment = sourceAttachment;
        }


        /// <summary>
        /// Loads wrapped attachment from a given source.
        /// </summary>
        /// <param name="attachment">Document attachment.</param>
        /// <param name="sourceAttachment">Attachment history info to load.</param>
        /// <param name="versionHistoryId">Version history ID in context of which the attachment history is loaded.</param>
        internal static void Load(this DocumentAttachment attachment, AttachmentHistoryInfo sourceAttachment, int versionHistoryId)
        {
            attachment.AttachmentVersionHistoryID = versionHistoryId;
            attachment.WrappedAttachment = sourceAttachment;
        }
    }
}
