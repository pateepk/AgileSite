using CMS;
using CMS.DataEngine;
using CMS.DocumentEngine;

[assembly: RegisterImplementation(typeof(IAttachmentVariantGenerator<AttachmentInfo>), typeof(AttachmentInfoVariantGenerator), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Represents an attachment variant generator for <see cref="AttachmentInfo"/>.
    /// </summary>
    internal class AttachmentInfoVariantGenerator : AttachmentVariantGenerator<AttachmentInfo>
    {
        protected override ObjectQuery<AttachmentInfo> GetAttachmentVariants(AttachmentInfo attachment)
        {
            return AttachmentInfoProvider.GetAttachments()
                                         .VariantsForAttachments(attachment.AttachmentID);
        }
    }
}
