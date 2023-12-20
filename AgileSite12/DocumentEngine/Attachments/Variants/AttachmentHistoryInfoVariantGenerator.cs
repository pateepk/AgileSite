using CMS;
using CMS.DataEngine;
using CMS.DocumentEngine;

[assembly: RegisterImplementation(typeof(IAttachmentVariantGenerator<AttachmentHistoryInfo>), typeof(AttachmentHistoryInfoVariantGenerator), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Represents an attachment variant generator for <see cref="AttachmentHistoryInfo"/>.
    /// </summary>
    internal class AttachmentHistoryInfoVariantGenerator : AttachmentVariantGenerator<AttachmentHistoryInfo>
    {
        protected override ObjectQuery<AttachmentHistoryInfo> GetAttachmentVariants(AttachmentHistoryInfo attachment)
        {
            return AttachmentHistoryInfoProvider
                .GetAttachmentHistories()
                .VariantsForAttachments(attachment.AttachmentHistoryID);
        }
    }
}
