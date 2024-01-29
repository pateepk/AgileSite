using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Core;
using CMS.ResponsiveImages;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Extension methods for <see cref="AttachmentHistoryInfo"/>.
    /// </summary>
    public static class AttachmentHistoryExtensions
    {
        private static IAttachmentVariantGenerator<AttachmentHistoryInfo> mAttachmentVariantGenerator;


        private static IAttachmentVariantGenerator<AttachmentHistoryInfo> AttachmentVariantGenerator
        {
            get
            {
                return mAttachmentVariantGenerator ?? (mAttachmentVariantGenerator = Service.Resolve<IAttachmentVariantGenerator<AttachmentHistoryInfo>>());
            }
            set
            {
                mAttachmentVariantGenerator = value;
            }
        }


        /// <summary>
        /// Creates new instance of <see cref="AttachmentInfo"/> from instance of <see cref="AttachmentHistoryInfo"/>.
        /// </summary>
        /// <remarks>The AttachmentVariantParentID property of <paramref name="attachmentVersion"/> is not converted to corresponding ID of published attachment.</remarks>
        /// <param name="attachmentVersion">Attachment history version.</param>
        /// <param name="loadBinaryData">Indicates if binary data should be loaded from version attachment in case not already available.</param>
        internal static AttachmentInfo ConvertToAttachment(this AttachmentHistoryInfo attachmentVersion, bool loadBinaryData = true)
        {
            var attachment = AttachmentInfo.New(attachmentVersion);

            if (loadBinaryData && (attachment.AttachmentBinary == null))
            {
                // Ensure binary data from AttachmentHistoryInfo in case it's empty.
                // This can happen when binary data are stored only on filesystem.
                // This is a workaround because ObjectQuery doesn't support loading binary data from filesystem yet.
                attachment.AttachmentBinary = attachmentVersion.AttachmentBinary;
            }

            return attachment;
        }


        /// <summary>
        /// Applies data from <see cref="DocumentAttachment"/> to <see cref="AttachmentHistoryInfo"/> instance.
        /// </summary>
        /// <remarks>The AttachmentVariantParentID property of <paramref name="attachment"/> is not converted to corresponding ID of attachment version history.</remarks>
        /// <param name="attachmentVersion">Attachment history version.</param>
        /// <param name="attachment">Attachment.</param>
        /// <param name="loadBinaryData">Indicates if binary data should be loaded from attachment in case not already available.</param>
        internal static void ApplyData(this AttachmentHistoryInfo attachmentVersion, DocumentAttachment attachment, bool loadBinaryData = true)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException("attachment");
            }

            if (loadBinaryData)
            {
                EnsureBinaryData(attachment);
            }

            // Copy values from source attachment
            attachment.ColumnNames.ForEach(c =>
            {
                // Copy only matching columns to attachment history
                if (!attachmentVersion.ContainsColumn(c))
                {
                    return;
                }

                var value = DataHelper.GetNull(attachment.GetValue(c));
                attachmentVersion.SetValue(c, value);
            });
        }


        private static void EnsureBinaryData(DocumentAttachment sourceAttachment)
        {
            if (sourceAttachment.AttachmentBinary != null)
            {
                return;
            }

            sourceAttachment.AttachmentBinary = AttachmentBinaryHelper.GetAttachmentBinary(sourceAttachment);
        }


        /// <summary>
        /// Gets attachment history variants for given set of parent attachment histories.
        /// </summary>
        /// <param name="query">Query for retrieving attachment histories.</param>
        /// <param name="parentAttachmentIds">Collection of parent attachment history IDs.</param>
        internal static ObjectQuery<AttachmentHistoryInfo> VariantsForAttachments(this ObjectQuery<AttachmentHistoryInfo> query, params int[] parentAttachmentIds)
        {
            return query.WhereIn("AttachmentVariantParentID", parentAttachmentIds);
        }


        /// <summary>
        /// Filters attachment histories to get only those which do not represent variants for given version history ID.
        /// </summary>
        /// <param name="query">Query for retrieving attachment histories.</param>
        /// <param name="versionHistoryId">Version history ID.</param>
        public static ObjectQuery<AttachmentHistoryInfo> InVersionExceptVariants(this ObjectQuery<AttachmentHistoryInfo> query, int versionHistoryId)
        {
            return InVersionsExceptVariants(query, versionHistoryId);
        }


        /// <summary>
        /// Filters attachment histories to get only those which do not represent variants for given version histories.
        /// </summary>
        /// <param name="query">Query for retrieving attachment histories.</param>
        /// <param name="versionHistoryIds">Version history IDs.</param>
        internal static ObjectQuery<AttachmentHistoryInfo> InVersionsExceptVariants(this ObjectQuery<AttachmentHistoryInfo> query, params int[] versionHistoryIds)
        {
            var historyIds = GetHistoryIDsForVersion(versionHistoryIds);

            return query.WhereIn("AttachmentHistoryID", historyIds)
                        .WhereNull("AttachmentVariantParentID");
        }


        /// <summary>
        /// Filters all attachment histories for given version history ID.
        /// </summary>
        /// <param name="query">Query for retrieving attachment histories.</param>
        /// <param name="versionHistoryId">Version history ID.</param>
        public static ObjectQuery<AttachmentHistoryInfo> AllInVersion(this ObjectQuery<AttachmentHistoryInfo> query, int versionHistoryId)
        {
            var historyIds = GetHistoryIDsForVersion(versionHistoryId);

            var where = new WhereCondition()
                .WhereIn("AttachmentHistoryID", historyIds)
                .Or()
                .WhereIn("AttachmentVariantParentID", historyIds);

            return query.Where(where);
        }


        /// <summary>
        /// Filters attachment histories to get only ones which do not represent variants.
        /// </summary>
        /// <param name="query">Query for retrieving attachment histories.</param>
        public static ObjectQuery<AttachmentHistoryInfo> ExceptVariants(this ObjectQuery<AttachmentHistoryInfo> query)
        {
            return query.WhereNull("AttachmentVariantParentID");
        }


        private static ObjectQuery<VersionAttachmentInfo> GetHistoryIDsForVersion(params int[] versionHistoryIds)
        {
            return VersionAttachmentInfoProvider.GetVersionAttachments()
                                                .Column("AttachmentHistoryID")
                                                .WhereIn("VersionHistoryID", versionHistoryIds);
        }


        /// <summary>
        /// Returns <see cref="AttachmentHistoryInfo"/> that represents variant of the given attachment. The variant is specified by <paramref name="definitionIdentifier"/> parameter.
        /// </summary>
        /// <param name="attachment">Attachment info object.</param>
        /// <param name="definitionIdentifier">Definition identifier.</param>
        /// <exception cref="ArgumentNullException">Thrown when either of the parameters are null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="definitionIdentifier"/> is empty.</exception>
        public static AttachmentHistoryInfo GetVariant(this AttachmentHistoryInfo attachment, string definitionIdentifier)
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
        /// <exception cref="ArgumentNullException">Thrown when either of the parameters are null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="definitionIdentifier"/> is empty.</exception>
        public static AttachmentHistoryInfo GenerateVariant(this AttachmentHistoryInfo attachment, IVariantContext context, string definitionIdentifier)
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
        public static void GenerateAllVariants(this AttachmentHistoryInfo attachment, IVariantContext context)
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
        /// <param name="context">Variant processing context.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="attachment"/> is null.</exception>
        public static void GenerateMissingVariants(this AttachmentHistoryInfo attachment, IVariantContext context)
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
        internal static void SetAttachmentVariantGenerator(IAttachmentVariantGenerator<AttachmentHistoryInfo> generator)
        {
            AttachmentVariantGenerator = generator;
        }
    }
}
