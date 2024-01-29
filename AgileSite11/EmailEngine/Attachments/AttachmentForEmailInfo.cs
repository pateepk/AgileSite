using System;
using System.Collections.Generic;
using System.Web;
using System.Data;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.EmailEngine;

[assembly: RegisterObjectType(typeof(AttachmentForEmailInfo), AttachmentForEmailInfo.OBJECT_TYPE)]

namespace CMS.EmailEngine
{
    /// <summary>
    /// AttachmentForEmailInfo data container class.
    /// </summary>
    public class AttachmentForEmailInfo : AbstractInfo<AttachmentForEmailInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.attachmentforemail";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AttachmentForEmailInfoProvider), OBJECT_TYPE, "CMS.AttachmentForEmail", null, null, null, null, null, null, null, "EmailID", EmailInfo.OBJECT_TYPE)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>() { new ObjectDependency("AttachmentID", EmailAttachmentInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding) },
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// ID of the Email.
        /// </summary>
        public virtual int EmailID
        {
            get
            {
                return GetIntegerValue("EmailID", 0);
            }
            set
            {
                SetValue("EmailID", value);
            }
        }


        /// <summary>
        /// ID of the Attachment.
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

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            AttachmentForEmailInfoProvider.DeleteAttachmentForEmailInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            AttachmentForEmailInfoProvider.SetAttachmentForEmailInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty AttachmentForEmailInfo object.
        /// </summary>
        public AttachmentForEmailInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new AttachmentForEmailInfo object from the given DataRow.
        /// </summary>
        public AttachmentForEmailInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}