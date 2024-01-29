using System;
using System.Data;

using CMS.DataEngine;
using CMS.DataEngine.Internal;
using CMS.Base;
using CMS.Helpers;
using CMS.Search;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Represents a document attachment regardless whether it is a current version or not.
    /// </summary>
    public sealed class DocumentAttachment : AbstractWrapperInfo<DocumentAttachment>, IAttachment
    {
        #region "Wrapper properties"

        /// <summary>
        /// Wrapped info object
        /// </summary>
        protected override IInfo WrappedInfo
        {
            get
            {
                return WrappedAttachment;
            }
            set
            {
                WrappedAttachment = (IAttachment)value;
            }
        }


        /// <summary>
        /// Wrapped attachment
        /// </summary>
        internal IAttachment WrappedAttachment
        {
            get;
            set;
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the file name of the attachment or attachment version.
        /// </summary>
        public string AttachmentName
        {
            get
            {
                return WrappedAttachment.AttachmentName;
            }
            set
            {
                WrappedAttachment.AttachmentName = value;
            }
        }


        /// <summary>
        /// Gets the file name extension of the attachment or attachment version.
        /// </summary>
        public string AttachmentExtension
        {
            get
            {
                return WrappedAttachment.AttachmentExtension;
            }
            set
            {
                WrappedAttachment.AttachmentExtension = value;
            }
        }


        /// <summary>
        /// Gets the size, in bytes, of the attachment or attachment version.
        /// </summary>
        public int AttachmentSize
        {
            get
            {
                return WrappedAttachment.AttachmentSize;
            }
            set
            {
                WrappedAttachment.AttachmentSize = value;
            }
        }


        /// <summary>
        /// Gets the MIME type of the attachment or attachment version.
        /// </summary>
        public string AttachmentMimeType
        {
            get
            {
                return WrappedAttachment.AttachmentMimeType;
            }
            set
            {
                WrappedAttachment.AttachmentMimeType = value;
            }
        }


        /// <summary>
        /// Gets the binary content of the attachment or attachment version.
        /// </summary>
        public byte[] AttachmentBinary
        {
            get
            {
                return WrappedAttachment.AttachmentBinary;
            }
            set
            {
                WrappedAttachment.AttachmentBinary = value;
            }
        }


        /// <summary>
        /// Gets the width, in pixels, of the attachment or attachment version.
        /// </summary>
        public int AttachmentImageWidth
        {
            get
            {
                return WrappedAttachment.AttachmentImageWidth;
            }
            set
            {
                WrappedAttachment.AttachmentImageWidth = value;
            }
        }


        /// <summary>
        /// Gets the height, in pixels, of the attachment or attachment version.
        /// </summary>
        public int AttachmentImageHeight
        {
            get
            {
                return WrappedAttachment.AttachmentImageHeight;
            }
            set
            {
                WrappedAttachment.AttachmentImageHeight = value;
            }
        }


        /// <summary>
        /// Gets the globally unique identifier of the attachment or attachment version.
        /// </summary>
        public Guid AttachmentGUID
        {
            get
            {
                return WrappedAttachment.AttachmentGUID;
            }
            set
            {
                WrappedAttachment.AttachmentGUID = value;
            }
        }


        /// <summary>
        /// Gets the date and time when the attachment or attachment version was last modified.
        /// </summary>
        public DateTime AttachmentLastModified
        {
            get
            {
                 return WrappedAttachment.AttachmentLastModified;
            }
            set
            {
                 WrappedAttachment.AttachmentLastModified = value;
            }
        }


        /// <summary>
        /// Stores the attachment order.
        /// </summary>
        public int AttachmentOrder
        {
            get
            {
                return WrappedAttachment.AttachmentOrder;
            }
            set
            {
                WrappedAttachment.AttachmentOrder = value;
            }
        }


        /// <summary>
        /// Attachment custom data.
        /// </summary>
        public ContainerCustomData AttachmentCustomData
        {
            get
            {
                return WrappedAttachment.AttachmentCustomData;
            }
        }


        /// <summary>
        /// Extracted content from attachment binary used for search indexing.
        /// </summary>
        public XmlData AttachmentSearchContent
        {
            get
            {
                return WrappedAttachment.AttachmentSearchContent;
            }
        }
        

        /// <summary>
        /// Gets the string representation of the attachment or attachment version hash.
        /// </summary>
        public string AttachmentHash
        {
            get
            {
                return WrappedAttachment.AttachmentHash;
            }
            set
            {
                WrappedAttachment.AttachmentHash = value;
            }
        }


        /// <summary>
        /// Gets the title of the attachment or attachment version.
        /// </summary>
        public string AttachmentTitle
        {
            get
            {
                return WrappedAttachment.AttachmentTitle;
            }
            set
            {
                WrappedAttachment.AttachmentTitle = value;
            }
        }


        /// <summary>
        /// Gets the description of the attachment or attachment version.
        /// </summary>
        public string AttachmentDescription
        {
            get
            {
                return WrappedAttachment.AttachmentDescription;
            }
            set
            {
                WrappedAttachment.AttachmentDescription = value;
            }
        }


        /// <summary>
        /// Attachment site ID.
        /// </summary>
        public int AttachmentSiteID
        {
            get
            {
                return WrappedAttachment.AttachmentSiteID;
            }
            set
            {
                WrappedAttachment.AttachmentSiteID = value;
            }
        }


        /// <summary>
        /// Related document Document ID.
        /// </summary>
        public int AttachmentDocumentID
        {
            get
            {
                return WrappedAttachment.AttachmentDocumentID;
            }
            set
            {
                WrappedAttachment.AttachmentDocumentID = value;
            }
        }


        /// <summary>
        /// Text identification of used variant definition.
        /// </summary>
        public string AttachmentVariantDefinitionIdentifier
        {
            get
            {
                return WrappedAttachment.AttachmentVariantDefinitionIdentifier;
            }
            set
            {
                WrappedAttachment.AttachmentVariantDefinitionIdentifier = value;
            }
        }


        /// <summary>
        /// Attachment variant parent ID.
        /// </summary>
        /// <remarks>
        /// If attachment is history version, points to attachment history (AttachmentHistoryInfo), otherwise points to publishes attachment (AttachmentInfo)
        /// </remarks>
        public int AttachmentVariantParentID
        {
            get
            {
                var atInfo = WrappedAttachment as AttachmentInfo;
                if (atInfo != null)
                {
                    return atInfo.AttachmentVariantParentID;
                }

                var historyInfo = WrappedAttachment as AttachmentHistoryInfo;
                if (historyInfo != null)
                {
                    return historyInfo.AttachmentVariantParentID;
                }

                return 0;
            }
            set
            {
                var atInfo = WrappedAttachment as AttachmentInfo;
                if (atInfo != null)
                {
                    atInfo.AttachmentVariantParentID = value;
                    return;
                }

                var historyInfo = WrappedAttachment as AttachmentHistoryInfo;
                if (historyInfo != null)
                {
                    historyInfo.AttachmentVariantParentID = value;
                    return;
                }
            }
        }


        /// <summary>
        /// Indicates that the attachment is temporary and it is bound to the specific instance of form with this GUID.
        /// </summary>
        /// <remarks>
        /// Temporary attachments are not versioned, therefore this property always empty guid in case of attachment version.
        /// </remarks>
        public Guid AttachmentFormGUID
        {
            get
            {
                var atInfo = WrappedAttachment as AttachmentInfo;
                if (atInfo != null)
                {
                    return atInfo.AttachmentFormGUID;
                }

                return Guid.Empty;
            }
            set
            {
                var atInfo = WrappedAttachment as AttachmentInfo;
                if (atInfo != null)
                {
                    atInfo.AttachmentFormGUID = value;
                }
            }
        }


        /// <summary>
        /// Holds the GUID of the document field (group) under which the grouped attachment belongs.
        /// </summary>
        public Guid AttachmentGroupGUID
        {
            get
            {
                return WrappedAttachment.AttachmentGroupGUID;
            }
            set
            {
                WrappedAttachment.AttachmentGroupGUID = value;
            }
        }


        /// <summary>
        /// Indicates if attachment is unsorted attachment.
        /// </summary>
        public bool AttachmentIsUnsorted
        {
            get
            {
                return WrappedAttachment.AttachmentIsUnsorted;
            }
            set
            {
                WrappedAttachment.AttachmentIsUnsorted = value;
            }
        }


        /// <summary>
        /// Attachment ID.
        /// </summary>
        /// <remarks>
        /// If attachment is history version, returns the history ID, if the attachment is published attachment, returns attachment ID
        /// </remarks>
        public int ID
        {
            get
            {
                var atInfo = WrappedAttachment as AttachmentInfo;
                if (atInfo != null)
                {
                    return atInfo.AttachmentID;
                }

                var historyInfo = WrappedAttachment as AttachmentHistoryInfo;
                if (historyInfo != null)
                {
                    return historyInfo.AttachmentHistoryID;
                }

                return 0;
            }
            set
            {
                var atInfo = WrappedAttachment as AttachmentInfo;
                if (atInfo != null)
                {
                    atInfo.AttachmentID = value;
                    return;
                }

                var historyInfo = WrappedAttachment as AttachmentHistoryInfo;
                if (historyInfo != null)
                {
                    historyInfo.AttachmentHistoryID = value;
                    return;
                }
            }
        }


        /// <summary>
        /// Gets the identifier of the published attachment.
        /// </summary>
        /// <remarks>
        /// The identifier of the attachment that is versioned is 0.
        /// </remarks>
        public int AttachmentID
        {
            get
            {
                var atInfo = WrappedAttachment as AttachmentInfo;
                if (atInfo != null)
                {
                    return atInfo.AttachmentID;
                }

                return 0;
            }
        }



        /// <summary>
        /// Gets the identifier of the attachment version.
        /// </summary>
        /// <remarks>
        /// The identifier of the attachment that is not versioned is 0.
        /// </remarks>
        public int AttachmentHistoryID
        {
            get
            {
                var atInfo = WrappedAttachment as AttachmentHistoryInfo;
                if (atInfo != null)
                {
                    return atInfo.AttachmentHistoryID;
                }

                return 0;
            }
        }
                

        /// <summary>
        /// Attachment version history ID for latest version attachments
        /// </summary>
        public int AttachmentVersionHistoryID
        {
            get;
            set;
        }

        
        /// <summary>
        /// Attachment URL - for attachment information transfer purposes, not saved to the database.
        /// </summary>
        public string AttachmentUrl
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentAttachment"/> class
        /// </summary>
        public DocumentAttachment()
        {
            WrappedInfo = new AttachmentInfo();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentAttachment"/> class with the specified attachment or attachment version.
        /// </summary>
        /// <param name="attachment">The attachment or attachment version.</param>
        /// <exception cref="ArgumentNullException"><paramref name="attachment"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="attachment"/> is not an attachment or attachment version.</exception>
        public DocumentAttachment(IAttachment attachment)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException("attachment");
            }

            WrappedAttachment = attachment;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentAttachment"/> class with the specified attachment or attachment version data.
        /// </summary>
        /// <param name="dr">Data row</param>
        public DocumentAttachment(DataRow dr)
        {
            if ((dr != null))
            {
                // Temporary attachment comes always from published attachments (AttachmentInfo), skip to it in that case
                var formGuid = DataHelper.GetGuidValue(dr, "AttachmentFormGUID");
                if ((formGuid == Guid.Empty) && dr.Table.Columns.Contains("AttachmentHistoryID"))
                {
                    // History is made only when history ID is provided
                    WrappedAttachment = new AttachmentHistoryInfo(dr);
                    return;
                }
            }

            WrappedAttachment = new AttachmentInfo(dr);
        }


        /// <summary>
        /// Loads wrapped info data from the given data source.
        /// </summary>
        /// <param name="data">Source data to load.</param>
        protected override void LoadWrappedData(IDataContainer data)
        {
            if ((data == null) || !data.ContainsColumn("AttachmentHistoryID"))
            {
                var atInfo = new AttachmentInfo();
                if (data != null)
                {
                    atInfo.InitFromDataContainer(data);
                }

                WrappedInfo = atInfo;
            }
            else
            {
                var atInfo = new AttachmentHistoryInfo();
                atInfo.InitFromDataContainer(data);

                WrappedInfo = atInfo;
            }
        }


        /// <summary>
        /// Creates a new object from the given DataRow
        /// </summary>
        /// <param name="settings">Data settings</param>
        protected override BaseInfo NewObject(LoadDataSettings settings)
        {
            var attachment = new DocumentAttachment();
            attachment.LoadData(settings);

            return attachment;
        }
        

        /// <summary>
        /// Gets the attachment info object with the attachment data
        /// </summary>
        internal AttachmentInfo GetAttachmentInfo()
        {
            var historyInfo = WrappedAttachment as AttachmentHistoryInfo;
            if (historyInfo != null)
            {
                return historyInfo.ConvertToAttachment();
            }

            return WrappedAttachment as AttachmentInfo;
        }


        /// <summary>
        /// Returns the search content of the attachment (including names, title, description and extracted content of the attachment binary data) which should be included to the content field of the SearchDocument.
        /// Caches the extracted value to the AttachmentSearchContent field if called for the first time (calls AttachmentInfoProvider.SetAttachmentInfo(this, false) in this case to save to value to the DB).
        /// </summary>
        /// <param name="context">Extration context passed to the text extractors</param>
        /// <param name="searchFields">Search fields collection that can be extended by extractors (DOES NOT modify the content field, this is dealt with separately - as a return value of this method)</param>
        public string EnsureSearchContent(ExtractionContext context, ISearchFields searchFields)
        {
            var atInfo = WrappedAttachment as AttachmentInfo;
            if (atInfo != null)
            {
                return atInfo.EnsureSearchContent(context, searchFields);
            }

            throw new NotImplementedException("This operation is not implemented for attachment history info.");
        }

        #endregion


        #region "Operators"

        /// <summary>
        /// Implicit operator to convert AttachmentInfo to Attachment container
        /// </summary>
        /// <param name="attachment">Attachment</param>
        public static explicit operator DocumentAttachment(AttachmentInfo attachment)
        {
            if (attachment == null)
            {
                return null;
            }

            return new DocumentAttachment(attachment);
        }


        /// <summary>
        /// Implicit operator to convert AttachmentHistoryInfo to Attachment container
        /// </summary>
        /// <param name="attachmentHistory">Attachment</param>
        public static explicit operator DocumentAttachment(AttachmentHistoryInfo attachmentHistory)
        {
            if (attachmentHistory == null)
            {
                return null;
            }

            return new DocumentAttachment(attachmentHistory);
        }

        #endregion
    }
}
