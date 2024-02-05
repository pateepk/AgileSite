using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Helpers;
using CMS.DataEngine;
using CMS.Newsletters;

[assembly: RegisterObjectType(typeof(EmailQueueItemInfo), EmailQueueItemInfo.OBJECT_TYPE)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Class representing newsletter queue item.
    /// </summary>
    public class EmailQueueItemInfo : AbstractInfo<EmailQueueItemInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.EMAILQUEUEITEM;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(EmailQueueItemInfoProvider), OBJECT_TYPE, "Newsletter.Emails", "EmailID", null, "EmailGUID", null, null, null, "EmailSiteID", null, null)
            {

                DependsOn = new List<ObjectDependency>
                { 
                    new ObjectDependency("EmailNewsletterIssueID", IssueInfo.OBJECT_TYPE, ObjectDependencyEnum.Required), 
                    new ObjectDependency("EmailSubscriberID", SubscriberInfo.OBJECT_TYPE, ObjectDependencyEnum.Required)
                },
                LogEvents = false,
                TouchCacheDependencies = true,
                SupportsVersioning = false,
                ImportExportSettings = { LogExport = false },
                MacroCollectionName = "EmailQueueItem"
            };

        #endregion


        #region "Properties"

        /// <summary>
        /// EmailID.
        /// </summary>
        public virtual int EmailID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("EmailID"), 0);
            }
            set
            {
                SetValue("EmailID", value);
            }
        }


        /// <summary>
        /// EmailNewsletterIssueID.
        /// </summary>
        public virtual int EmailNewsletterIssueID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("EmailNewsletterIssueID"), 0);
            }
            set
            {
                SetValue("EmailNewsletterIssueID", value);
            }
        }


        /// <summary>
        /// EmailSubscriberID.
        /// </summary>
        public virtual int EmailSubscriberID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("EmailSubscriberID"), 0);
            }
            set
            {
                SetValue("EmailSubscriberID", value);
            }
        }


        /// <summary>
        /// E-mail site ID.
        /// </summary>
        public virtual int EmailSiteID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("EmailSiteID"), 0);
            }
            set
            {
                SetValue("EmailSiteID", value);
            }
        }


        /// <summary>
        /// EmailLastSendResult.
        /// </summary>
        public virtual string EmailLastSendResult
        {
            get
            {
                return ValidationHelper.GetString(GetValue("EmailLastSendResult"), "");
            }
            set
            {
                SetValue("EmailLastSendResult", value);
            }
        }


        /// <summary>
        /// EmailLastSendAttempt.
        /// </summary>
        public virtual DateTime EmailLastSendAttempt
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("EmailLastSendAttempt"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("EmailLastSendAttempt", value, value != DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// EmailSending.
        /// </summary>
        public virtual bool EmailSending
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("EmailSending"), false);
            }
            set
            {
                SetValue("EmailSending", value);
            }
        }


        /// <summary>
        /// E-mail GUID.
        /// </summary>
        public virtual Guid EmailGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("EmailGUID"), Guid.Empty);
            }
            set
            {
                SetValue("EmailGUID", value, value != Guid.Empty);
            }
        }


        /// <summary>
        /// EmailContactID.
        /// </summary>
        public virtual int EmailContactID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("EmailContactID"), 0);
            }
            set
            {
                SetValue("EmailContactID", value, value > 0);
            }
        }


        /// <summary>
        /// EmailAddress.
        /// </summary>
        public virtual string EmailAddress
        {
            get
            {
                return ValidationHelper.GetString(GetValue("EmailAddress"), string.Empty);
            }
            set
            {
                SetValue("EmailAddress", value, !string.IsNullOrEmpty(value));
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            EmailQueueItemInfoProvider.DeleteEmailQueueItem(EmailID);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            EmailQueueItemInfoProvider.SetEmailQueueItem(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty EmailQueueItem object.
        /// </summary>
        public EmailQueueItemInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new EmailQueueItem object from the given DataRow.
        /// </summary>
        public EmailQueueItemInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}