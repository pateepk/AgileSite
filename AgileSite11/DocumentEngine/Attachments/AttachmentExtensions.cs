using System;

using CMS.DataEngine;
using CMS.Core;
using CMS.ResponsiveImages;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Extension methods for <see cref="AttachmentInfo"/>.
    /// </summary>
    public static class AttachmentExtensions
    {
        private static IAttachmentVariantGenerator<AttachmentInfo> mAttachmentVariantGenerator;


        private static IAttachmentVariantGenerator<AttachmentInfo> AttachmentVariantGenerator
        {
            get
            {
                return mAttachmentVariantGenerator ?? (mAttachmentVariantGenerator = Service.Resolve<IAttachmentVariantGenerator<AttachmentInfo>>());
            }
            set
            {
                mAttachmentVariantGenerator = value;
            }
        }


        /// <summary>
        /// Returns true if the attachment is a variant of an original attachment.
        /// </summary>
        /// <param name="attachment">Attachment to check.</param>
        public static bool IsVariant(this IAttachment attachment)
        {
            return !String.IsNullOrEmpty(attachment.AttachmentVariantDefinitionIdentifier);
        }


        /// <summary>
        /// Filters attachments to get only ones which do not represent variants.
        /// </summary>
        /// <param name="query">Query for retrieving attachments.</param>
        public static ObjectQuery<AttachmentInfo> ExceptVariants(this ObjectQuery<AttachmentInfo> query)
        {
            return query.WhereNull("AttachmentVariantParentID");
        }


        /// <summary>
        /// Gets attachment variants for given set of parent attachments.
        /// </summary>
        /// <param name="query">Query for retrieving attachments.</param>
        /// <param name="parentAttachmentIds">Collection of parent attachment IDs.</param>
        internal static ObjectQuery<AttachmentInfo> VariantsForAttachments(this ObjectQuery<AttachmentInfo> query, params int[] parentAttachmentIds)
        {
            return query.WhereIn("AttachmentVariantParentID", parentAttachmentIds);
        }


        /// <summary>
        /// Returns <see cref="AttachmentInfo"/> that represents variant of the given attachment. The variant is specified by <paramref name="definitionIdentifier"/> parameter.
        /// </summary>
        /// <param name="attachment">Attachment info object.</param>
        /// <param name="definitionIdentifier">Definition identifier.</param>
        /// <exception cref="ArgumentNullException">Thrown when either of the parameters are null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="definitionIdentifier"/> is empty.</exception>
        public static AttachmentInfo GetVariant(this AttachmentInfo attachment, string definitionIdentifier)
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

            return AttachmentVariantGenerator.GetVariant(attachment, definitionIdentifier);
        }


        /// <summary>
        /// Generates single image variant for given attachment. The variant is specified by <paramref name="definitionIdentifier"/> parameter.
        /// </summary>
        /// <remarks>Deletes an existing variant if the given variant definition is no longer applicable on the attachment.</remarks>
        /// <param name="attachment">Attachment info object.</param>
        /// <param name="context">Variant processing context.</param>
        /// <param name="definitionIdentifier">Definition identifier.</param>
        /// <returns>Generated image variant from the main attachment or <c>null</c> if variant did not generate.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either of the parameters are null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="definitionIdentifier"/> is empty.</exception>
        public static AttachmentInfo GenerateVariant(this AttachmentInfo attachment, IVariantContext context, string definitionIdentifier)
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

            return AttachmentVariantGenerator.GenerateVariant(attachment, context, definitionIdentifier);
        }


        /// <summary>
        /// Generates all image variants for given attachment overwriting existing variants.
        /// </summary>
        /// <remarks>Only applicable variants will be generated. Variant definitions which are no more applicable to the attachment will be deleted.</remarks>
        /// <param name="attachment">Attachment info object.</param>
        /// <param name="context">Variant processing context.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="attachment"/> is null.</exception>
        public static void GenerateAllVariants(this AttachmentInfo attachment, IVariantContext context)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException("attachment");
            }

            AttachmentVariantGenerator.GenerateAllVariants(attachment, context);
        }


        /// <summary>
        /// Generates attachment image variants which were not yet generated.
        /// </summary>
        /// <param name="attachment">Attachment info object.</param>
        /// <param name="context">Context</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="attachment"/> is null.</exception>
        public static void GenerateMissingVariants(this AttachmentInfo attachment, IVariantContext context)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException("attachment");
            }

            AttachmentVariantGenerator.GenerateMissingVariants(attachment, context);
        }


        /// <summary>
        /// Sets the attachment variant generator.
        /// </summary>
        /// <param name="generator">Attachment variant generator instance.</param>
        internal static void SetAttachmentVariantGenerator(IAttachmentVariantGenerator<AttachmentInfo> generator)
        {
            AttachmentVariantGenerator = generator;
        }
    }
}
