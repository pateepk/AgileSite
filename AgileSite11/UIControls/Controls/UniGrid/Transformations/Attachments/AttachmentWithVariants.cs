using System.Collections.Generic;

using CMS.DocumentEngine;
using CMS.Base;

namespace CMS.UIControls
{
    /// <summary>
    /// Container for attachment with variants
    /// </summary>
    [RegisterAllProperties]
    public class AttachmentWithVariants : AbstractHierarchicalObject<AttachmentWithVariants>
    {
        /// <summary>
        /// Main attachment
        /// </summary>
        public DocumentAttachment Attachment
        {
            get;
            private set;
        }


        /// <summary>
        /// Attachment variants
        /// </summary>
        public IEnumerable<DocumentAttachment> Variants
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
        /// Constructor
        /// </summary>
        /// <param name="versionHistoryId">Version history ID</param>
        /// <param name="attachment">Main attachment</param>
        /// <param name="variants">Attachment variants</param>
        public AttachmentWithVariants(int versionHistoryId, DocumentAttachment attachment, IEnumerable<DocumentAttachment> variants)
        {
            VersionHistoryID = versionHistoryId;
            Attachment = attachment;
            Variants = variants;
        }
    }
}
