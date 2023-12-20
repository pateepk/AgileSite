using CMS.DataEngine;
using CMS.ResponsiveImages;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Represents a generator capable of generating variants for attachments.
    /// </summary>
    /// <typeparam name="TAttachment">Type for which the generator handles variants.</typeparam>
    internal interface IAttachmentVariantGenerator<TAttachment> 
        where TAttachment : BaseInfo, IAttachment
    {
        /// <summary>
        /// Returns an attachment variant.
        /// </summary>
        /// <param name="attachment">Attachment for which the variant should be returned.</param>
        /// <param name="definitionIdentifier">Definition identifier.</param>
        /// <returns>Returns an attachment variant or <c>null</c> if variant does not exist yet.</returns>
        TAttachment GetVariant(TAttachment attachment, string definitionIdentifier);


        /// <summary>
        /// Generates attachment variant.
        /// </summary>
        /// <param name="attachment">Attachment for which the variant should be generated.</param>
        /// <param name="context">Variant processing context.</param>
        /// <param name="definitionIdentifier">Definition identifier.</param>
        /// <returns>Generated image variant for the attachment or <c>null</c> if variant did not generate.</returns>
        TAttachment GenerateVariant(TAttachment attachment, IVariantContext context, string definitionIdentifier);


        /// <summary>
        /// Generates all attachment variants.
        /// </summary>
        /// <param name="attachment">Attachment for which the variants should be generated.</param>
        /// <param name="context">Variant processing context.</param>
        void GenerateAllVariants(TAttachment attachment, IVariantContext context);


        /// <summary>
        /// Generates all missing attachment variants.
        /// </summary>
        /// <param name="attachment">Attachment for which the missing  variants should be generated.</param>
        /// <param name="context">Variant processing context.</param>
        void GenerateMissingVariants(TAttachment attachment, IVariantContext context);
    }
}
