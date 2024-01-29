using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.ResponsiveImages;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Represents a base generator implementation capable of generating variants for attachments.
    /// </summary>
    /// <typeparam name="TAttachment">Type for which the generator handles variants.</typeparam>
    internal abstract class AttachmentVariantGenerator<TAttachment> : IAttachmentVariantGenerator<TAttachment>
        where TAttachment : BaseInfo, IAttachment
    {
        /// <summary>
        /// Returns all variants for attachment.
        /// </summary>
        /// <param name="attachment">Attachment object.</param>
        protected abstract ObjectQuery<TAttachment> GetAttachmentVariants(TAttachment attachment);


        /// <summary>
        /// Returns an attachment variant.
        /// </summary>
        /// <param name="attachment">Attachment for which the variant should be returned.</param>
        /// <param name="definitionIdentifier">Definition identifier.</param>
        /// <returns>Returns an attachment variant or <c>null</c> if variant does not exist yet.</returns>
        public TAttachment GetVariant(TAttachment attachment, string definitionIdentifier)
        {
            return GetAttachmentVariants(attachment)
                .WhereEquals("AttachmentVariantDefinitionIdentifier", definitionIdentifier)
                .FirstObject;
        }


        /// <summary>
        /// Generates attachment variant.
        /// </summary>
        /// <param name="attachment">Attachment for which the variant should be generated.</param>
        /// <param name="context">Variant processing context.</param>
        /// <param name="definitionIdentifier">Definition identifier.</param>
        /// <returns>Generated image variant for the attachment or <c>null</c> if variant did not generate.</returns>
        public TAttachment GenerateVariant(TAttachment attachment, IVariantContext context, string definitionIdentifier)
        {
            var existingVariant = GetVariant(attachment, definitionIdentifier);

            TAttachment attachmentVariant = null;
            try
            {
                attachmentVariant = PrepareVariant(attachment, context, definitionIdentifier, existingVariant);
                if (attachmentVariant != null)
                {
                    attachmentVariant.Generalized.SetObject();
                }
                else if (existingVariant != null)
                {
                    existingVariant.Delete();
                }
            }
            catch (ImageFilterException exception)
            {
                EventLogProvider.LogException("ResponsiveImages", "GENERATEVARIANT", exception, additionalMessage: "Failed to generate variant for '" + definitionIdentifier + "' definition.");
            }

            return attachmentVariant;
        }


        /// <summary>
        /// Generates all attachment variants.
        /// </summary>
        /// <param name="attachment">Attachment for which the variants should be generated.</param>
        /// <param name="context">Variant processing context.</param>
        public void GenerateAllVariants(TAttachment attachment, IVariantContext context)
        {
            var existingVariants = GetAttachmentVariants(attachment).TypedResult.ToList();
            var preparedVariants = PrepareAllVariants(attachment, context, existingVariants);

            foreach (var variant in preparedVariants)
            {
                variant.Generalized.SetObject();
            }

            var preparedDefinitionIdentifiers = preparedVariants.Select(v => v.AttachmentVariantDefinitionIdentifier);

            // Delete variants which already exist in DB but their variant definition is not registered anymore or variants whose generation failed.
            var variantsToBeDeleted = existingVariants.Where(
                variant => !preparedDefinitionIdentifiers
                        .Contains(variant.AttachmentVariantDefinitionIdentifier)
            );

            foreach (var variant in variantsToBeDeleted)
            {
                variant.Delete();
            }
        }


        /// <summary>
        /// Generates all missing attachment variants.
        /// </summary>
        /// <param name="attachment">Attachment for which the missing  variants should be generated.</param>
        /// <param name="context">Variant processing context.</param>
        public void GenerateMissingVariants(TAttachment attachment, IVariantContext context)
        {
            var variantIdentifiers = GetAttachmentVariants(attachment)
                .Column("AttachmentVariantDefinitionIdentifier")
                .GetListResult<string>();
            var preparedVariants = PrepareMissingVariants(attachment, context, variantIdentifiers);

            foreach (var variant in preparedVariants)
            {
                variant.Generalized.SetObject();
            }
        }


        /// <summary>
        /// Creates a new variant attachment from the main attachment into which the generated attachment variant binary data will be saved.
        /// </summary>
        /// <param name="attachment">Main attachment that contains the data from which the variant should generate.</param>
        /// <param name="definitionIdentifier">Definition identifier.</param>
        protected TAttachment CreateVariantFromMainAttachment(TAttachment attachment, string definitionIdentifier)
        {
            TAttachment attachmentVariant = (TAttachment)attachment.CloneObject(true);
            attachmentVariant.AttachmentVariantParentID = attachment.Generalized.ObjectID;
            ResetVariantProperties(attachmentVariant);
            attachmentVariant.AttachmentName = definitionIdentifier;
            attachmentVariant.AttachmentVariantDefinitionIdentifier = definitionIdentifier;

            return attachmentVariant;
        }


        /// <summary>
        /// Returns a collection of missing attachment variants to be saved to database.
        /// </summary>
        /// <param name="mainAttachment">Main Attachment.</param>
        /// <param name="context">Variant processing context.</param>
        /// <param name="existingVariantDefinitionIdentifiers">Collection of existing variant definition identifiers.</param>
        protected IEnumerable<TAttachment> PrepareMissingVariants(TAttachment mainAttachment, IVariantContext context, IEnumerable<string> existingVariantDefinitionIdentifiers)
        {
            var allRegisteredVariantDefinitions = ImageVariantDefinitionManager.GetDefinitions();
            var variantDefinitionIdentifiers = existingVariantDefinitionIdentifiers.ToHashSet();
            var missingVariants = allRegisteredVariantDefinitions.Where(definition => !variantDefinitionIdentifiers.Contains(definition.Identifier));
            var preparedVariants = new List<TAttachment>();

            foreach (var definition in missingVariants)
            {
                try
                {
                    var attachmentVariant = PrepareVariant(mainAttachment, context, definition.Identifier, null);
                    if (attachmentVariant != null)
                    {
                        preparedVariants.Add(attachmentVariant);
                    }
                }
                catch (ImageFilterException exception)
                {
                    EventLogProvider.LogException("ResponsiveImages", "GENERATEMISSINGVARIANTS", exception, additionalMessage: "Failed to generate missing variant for '" + definition.Identifier + "' definition.");
                }
            }

            return preparedVariants;
        }


        /// <summary>
        /// Prepares variant to be saved to database.
        /// </summary>
        /// <param name="mainAttachment">Main Attachment.</param>
        /// <param name="context">Variant processing context.</param>
        /// <param name="definitionIdentifier">Definition identifier.</param>
        /// <param name="existingVariant">Attachment on which the variant is created.</param>
        /// <exception cref="ImageFilterException">Thrown when an error occurs during a filter application.</exception>
        protected TAttachment PrepareVariant(TAttachment mainAttachment, IVariantContext context, string definitionIdentifier, TAttachment existingVariant)
        {
            var definition = ImageVariantDefinitionManager.GetDefinition(definitionIdentifier);
            if (definition == null)
            {
                return null;
            }

            var metadata = CreateImageMetadata(mainAttachment);
            if (!definition.IsApplicable(metadata, context))
            {
                return null;
            }

            var container = definition.GenerateVariant(CreateImageContainer(mainAttachment, metadata), context);

            // Definition was not applicable
            if (container == null)
            {
                return null;
            }

            // Prefer modification of the existing attachment variant
            TAttachment attachmentVariant = existingVariant;
            if (attachmentVariant == null)
            {
                attachmentVariant = CreateVariantFromMainAttachment(mainAttachment, definitionIdentifier);
            }

            CopyDataFromImageContainerToAttachment(container, attachmentVariant);
            return attachmentVariant;
        }


        /// <summary>
        /// Returns a collection of all attachment variants to be saved to database.
        /// </summary>
        /// <param name="mainAttachment">Main Attachment.</param>
        /// <param name="context">Variant processing context.</param>
        /// <param name="existingVariants">Collection of all existing variants.</param>
        protected IList<TAttachment> PrepareAllVariants(TAttachment mainAttachment, IVariantContext context, ICollection<TAttachment> existingVariants)
        {
            var definitions = ImageVariantDefinitionManager.GetDefinitions();
            var preparedVariants = new List<TAttachment>();

            foreach (var definition in definitions)
            {
                TAttachment attachmentVariant = existingVariants.FirstOrDefault(variant => variant.AttachmentVariantDefinitionIdentifier == definition.Identifier);
                try
                {
                    attachmentVariant = PrepareVariant(mainAttachment, context, definition.Identifier, attachmentVariant);
                    if (attachmentVariant != null)
                    {
                        preparedVariants.Add(attachmentVariant);
                    }
                }
                catch (ImageFilterException exception)
                {
                    EventLogProvider.LogException("ResponsiveImages", "GENERATEVARIANT", exception, additionalMessage: "Failed to generate variant for '" + definition.Identifier + "' definition.");
                }
            }

            return preparedVariants;
        }


        /// <summary>
        /// Reset properties of <paramref name="attachment"/> which are not relevant for Attachment variant.
        /// </summary>
        /// <param name="attachment"></param>
        protected void ResetVariantProperties(TAttachment attachment)
        {
            attachment.AttachmentOrder = 0;
            attachment.AttachmentHash = null;
            attachment.AttachmentTitle = null;
            attachment.AttachmentDescription = null;
            attachment.AttachmentCustomData.Clear();
            attachment.AttachmentSearchContent.Clear();
            attachment.AttachmentGUID = Guid.NewGuid();
        }


        /// <summary>
        /// Creates <see cref="ImageContainer"/> instance expanded with data from <see cref="AttachmentInfo"/> object.
        /// </summary>
        /// <param name="attachment">Attachment object.</param>
        /// <param name="metadata">Attachment metadata.</param>
        private ImageContainer CreateImageContainer(TAttachment attachment, ImageMetadata metadata)
        {
            return new ImageContainer(attachment.Generalized.EnsureBinaryData(), metadata);
        }


        /// <summary>
        /// Creates <see cref="ImageMetadata"/> instance expanded with data from <see cref="AttachmentInfo"/> object.
        /// </summary>
        /// <param name="attachment">Attachment object.</param>
        private ImageMetadata CreateImageMetadata(TAttachment attachment)
        {
            return new ImageMetadata(attachment.AttachmentImageWidth, attachment.AttachmentImageHeight, attachment.AttachmentMimeType, attachment.AttachmentExtension);
        }


        /// <summary>
        /// Copies data from <see cref="ImageContainer"/> object to <see cref="IAttachment"/> object specified by <paramref name="attachment"/> parameter.
        /// </summary>
        /// <param name="container">Image container object.</param>
        /// <param name="attachment">Attachment object whose data is copied.</param>
        private void CopyDataFromImageContainerToAttachment(ImageContainer container, TAttachment attachment)
        {
            var metadata = container.Metadata;
            attachment.AttachmentImageHeight = metadata.Height;
            attachment.AttachmentImageWidth = metadata.Width;
            attachment.AttachmentMimeType = metadata.MimeType;

            using (var stream = container.OpenReadStream())
            {
                attachment.AttachmentBinary = BinaryData.GetByteArrayFromStream(stream);
            }

            attachment.AttachmentSize = attachment.AttachmentBinary.Length;
            attachment.AttachmentExtension = metadata.Extension;
        }
    }
}
