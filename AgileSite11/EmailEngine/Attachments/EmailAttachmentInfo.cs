using System;
using System.Data;

using CMS;
using CMS.Helpers;
using CMS.DataEngine;
using CMS.EmailEngine;

[assembly: RegisterObjectType(typeof(EmailAttachmentInfo), EmailAttachmentInfo.OBJECT_TYPE)]

namespace CMS.EmailEngine
{
    /// <summary>
    /// EmailAttachmentInfo data container class.
    /// </summary>
    public class EmailAttachmentInfo : AbstractInfo<EmailAttachmentInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.emailattachment";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(EmailAttachmentInfoProvider), OBJECT_TYPE, "CMS.EmailAttachment", "AttachmentID", "AttachmentLastModified", "AttachmentGUID", "AttachmentName", null, "AttachmentBinary", "AttachmentSiteID", null, null)
        {
            TouchCacheDependencies = true,
            MimeTypeColumn = "AttachmentMimeType",
            ExtensionColumn = "AttachmentExtension",
            SizeColumn = "AttachmentSize",
            SupportsGlobalObjects = true
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Attachment ID.
        /// </summary>
        public virtual int AttachmentID
        {
            get
            {
                return GetIntegerValue("AttachmentID", 0);
            }
            set
            {
                SetValue("AttachmentID", value);
            }
        }


        /// <summary>
        /// Attachment name.
        /// </summary>
        public virtual string AttachmentName
        {
            get
            {
                return GetStringValue("AttachmentName", "");
            }
            set
            {
                SetValue("AttachmentName", value);
            }
        }


        /// <summary>
        /// Attachment extension.
        /// </summary>
        public virtual string AttachmentExtension
        {
            get
            {
                return GetStringValue("AttachmentExtension", "");
            }
            set
            {
                SetValue("AttachmentExtension", value);
            }
        }


        /// <summary>
        /// Attachment size.
        /// </summary>
        public virtual int AttachmentSize
        {
            get
            {
                return GetIntegerValue("AttachmentSize", 0);
            }
            set
            {
                SetValue("AttachmentSize", value);
            }
        }


        /// <summary>
        /// Attachment type.
        /// </summary>
        public virtual string AttachmentMimeType
        {
            get
            {
                return GetStringValue("AttachmentMimeType", "");
            }
            set
            {
                SetValue("AttachmentMimeType", value);
            }
        }


        /// <summary>
        /// Attachment binary data.
        /// </summary>
        public virtual byte[] AttachmentBinary
        {
            get
            {
                return ValidationHelper.GetBinary(GetValue("AttachmentBinary"), null);
            }
            set
            {
                SetValue("AttachmentBinary", value);
            }
        }


        /// <summary>
        /// Attachment ContentId.
        /// </summary>
        public virtual string AttachmentContentID
        {
            get
            {
                return GetStringValue("AttachmentContentID", "");
            }
            set
            {
                SetValue("AttachmentContentID", value);
            }
        }


        /// <summary>
        /// Attachment GUID.
        /// </summary>
        public virtual Guid AttachmentGUID
        {
            get
            {
                return GetGuidValue("AttachmentGUID", Guid.Empty);
            }
            set
            {
                SetValue("AttachmentGUID", value);
            }
        }


        /// <summary>
        /// Attachment last modified DateTime.
        /// </summary>
        public virtual DateTime AttachmentLastModified
        {
            get
            {
                return GetDateTimeValue("AttachmentLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("AttachmentLastModified", value);
            }
        }


        /// <summary>
        /// Attachment site ID.
        /// </summary>
        public virtual int AttachmentSiteID
        {
            get
            {
                return GetIntegerValue("AttachmentSiteID", 0);
            }
            set
            {
                SetValue("AttachmentSiteID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            EmailAttachmentInfoProvider.DeleteEmailAttachmentInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            EmailAttachmentInfoProvider.SetEmailAttachmentInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty EmailAttachmentInfo object.
        /// </summary>
        public EmailAttachmentInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new EmailAttachmentInfo object from the given DataRow.
        /// </summary>
        public EmailAttachmentInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}