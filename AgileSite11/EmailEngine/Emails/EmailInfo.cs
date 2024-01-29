using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.EmailEngine;

[assembly: RegisterObjectType(typeof(EmailInfo), EmailInfo.OBJECT_TYPE)]

namespace CMS.EmailEngine
{
    /// <summary>
    /// EmailInfo data container class.
    /// </summary>
    public class EmailInfo : AbstractInfo<EmailInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.email";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(EmailInfoProvider), OBJECT_TYPE, "CMS.Email", "EmailID", "EmailLastModified", "EmailGUID", null, "EmailSubject", null, "EmailSiteID", null, null)
        {
            SupportsCloning = false,
            ContainsMacros = false,
            AllowRestore = false,
            SupportsGlobalObjects = true,
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None,
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
            },
            ImportExportSettings =
            {
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
            },
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// E-mail ID.
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
        /// E-mail from.
        /// </summary>
        public virtual string EmailFrom
        {
            get
            {
                return GetStringValue("EmailFrom", String.Empty);
            }
            set
            {
                SetValue("EmailFrom", value);
            }
        }


        /// <summary>
        /// E-mail to.
        /// </summary>
        public virtual string EmailTo
        {
            get
            {
                return GetStringValue("EmailTo", String.Empty);
            }
            set
            {
                SetValue("EmailTo", value);
            }
        }


        /// <summary>
        /// E-mail reply to.
        /// </summary>
        public virtual string EmailReplyTo
        {
            get
            {
                return GetStringValue("EmailReplyTo", String.Empty);
            }
            set
            {
                SetValue("EmailReplyTo", value);
            }
        }


        /// <summary>
        /// E-mail Cc.
        /// </summary>
        public virtual string EmailCc
        {
            get
            {
                return GetStringValue("EmailCc", String.Empty);
            }
            set
            {
                SetValue("EmailCc", value);
            }
        }


        /// <summary>
        /// E-mail Bcc.
        /// </summary>
        public virtual string EmailBcc
        {
            get
            {
                return GetStringValue("EmailBcc", String.Empty);
            }
            set
            {
                SetValue("EmailBcc", value);
            }
        }


        /// <summary>
        /// E-mail subject.
        /// </summary>
        public virtual string EmailSubject
        {
            get
            {
                return GetStringValue("EmailSubject", String.Empty);
            }
            set
            {
                SetValue("EmailSubject", value);
            }
        }


        /// <summary>
        /// E-mail body.
        /// </summary>
        public virtual string EmailBody
        {
            get
            {
                return GetStringValue("EmailBody", String.Empty);
            }
            set
            {
                SetValue("EmailBody", value);
            }
        }


        /// <summary>
        /// E-mail plain text body.
        /// </summary>
        public virtual string EmailPlainTextBody
        {
            get
            {
                return GetStringValue("EmailPlainTextBody", String.Empty);
            }
            set
            {
                SetValue("EmailPlainTextBody", value);
            }
        }


        /// <summary>
        /// E-mail format - Html, PlainText, Both, Default (default).
        /// </summary>
        public virtual EmailFormatEnum EmailFormat
        {
            get
            {
                return (EmailFormatEnum)ValidationHelper.GetInteger(GetValue("EmailFormat"), 3);
            }
            set
            {
                SetValue("EmailFormat", (int)value);
            }
        }


        /// <summary>
        /// E-mail priority - Low, Normal (default), High.
        /// </summary>
        public virtual EmailPriorityEnum EmailPriority
        {
            get
            {
                return (EmailPriorityEnum)ValidationHelper.GetInteger(GetValue("EmailPriority"), 1);
            }
            set
            {
                SetValue("EmailPriority", (int)value);
            }
        }


        /// <summary>
        /// E-mail site ID.
        /// </summary>
        public virtual int EmailSiteID
        {
            get
            {
                return GetIntegerValue("EmailSiteID", 0);
            }
            set
            {
                SetValue("EmailSiteID", value);
            }
        }


        /// <summary>
        /// E-mail last send result.
        /// </summary>
        public virtual string EmailLastSendResult
        {
            get
            {
                return GetStringValue("EmailLastSendResult", String.Empty);
            }
            set
            {
                SetValue("EmailLastSendResult", value, !String.IsNullOrEmpty(value));
            }
        }


        /// <summary>
        /// E-mail last send attempt.
        /// </summary>
        public virtual DateTime EmailLastSendAttempt
        {
            get
            {
                return GetDateTimeValue("EmailLastSendAttempt", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("EmailLastSendAttempt", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// E-mail status - Created, Waiting, Sending, Archived.
        /// </summary>
        public virtual EmailStatusEnum EmailStatus
        {
            get
            {
                return (EmailStatusEnum)ValidationHelper.GetInteger(GetValue("EmailStatus"), 1);
            }
            set
            {
                SetValue("EmailStatus", (int)value);
            }
        }


        /// <summary>
        /// Indicates e-mail as mass e-mail if true.
        /// </summary>
        public virtual bool EmailIsMass
        {
            get
            {
                return GetBooleanValue("EmailIsMass", false);
            }
            set
            {
                SetValue("EmailIsMass", value);
            }
        }


        /// <summary>
        /// E-mail GUID.
        /// </summary>
        public virtual Guid EmailGUID
        {
            get
            {
                return GetGuidValue("EmailGUID", Guid.Empty);
            }
            set
            {
                SetValue("EmailGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// E-mail last modified.
        /// </summary>
        public virtual DateTime EmailLastModified
        {
            get
            {
                return GetDateTimeValue("EmailLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("EmailLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Timestamp of object creation. Is set in <see cref="EmailInfoProvider"/> when creating a new object.
        /// </summary>
        public virtual DateTime EmailCreated
        {
            get
            {
                return GetDateTimeValue("EmailCreated", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("EmailCreated", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Gets or sets additional email headers to append to email message.
        /// </summary>
        public virtual string EmailHeaders
        {
            get
            {
                return GetStringValue("EmailHeaders", String.Empty);
            }
            set
            {
                SetValue("EmailHeaders", value, !String.IsNullOrEmpty(value));
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            EmailInfoProvider.DeleteEmailInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            EmailInfoProvider.SetEmailInfo(this);
        }


        /// <summary>
        /// Removes object dependencies. First tries to execute removedependencies query, if not found, automatic process is executed.
        /// </summary>
        /// <param name="deleteAll">If false, only required dependencies are deleted, dependencies with default value are replaced with default value and nullable values are replaced with null</param>
        /// <param name="clearHashtables">If true, hashtables of all objecttypes which were potentionally modified are cleared</param>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            // Do not remove dependencies automatically, they are handled by the stored procedure within explicit delete query
            //base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty EmailInfo object.
        /// </summary>
        public EmailInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new EmailInfo object from the given DataRow.
        /// </summary>
        public EmailInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Creates a new EmailInfo object from the specified EmailMessage.
        /// </summary>
        /// <param name="message">Email message to copy data from</param>
        /// <exception cref="ArgumentNullException">message cannot be null</exception>
        public EmailInfo(EmailMessage message)
            : base(TYPEINFO)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            EmailGUID = Guid.NewGuid();
            EmailLastModified = DateTime.Now;
            EmailBcc = message.BccRecipients;
            EmailBody = message.Body;
            EmailPlainTextBody = message.PlainTextBody;
            EmailCc = message.CcRecipients;
            EmailFormat = message.EmailFormat;
            EmailFrom = message.From;
            EmailTo = message.Recipients;
            EmailReplyTo = message.ReplyTo;
            EmailSubject = message.Subject ?? String.Empty;
            EmailStatus = EmailStatusEnum.Created;
            EmailPriority = message.Priority;
            EmailSiteID = 0;
            EmailIsMass = false;
            EmailHeaders = EmailMessage.GetHeaderFields(message.Headers);
        }

        #endregion
    }
}