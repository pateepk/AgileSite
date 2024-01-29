using System;
using System.Collections.Generic;
using System.Web;
using System.Data;

using CMS;
using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;
using CMS.EmailEngine;

[assembly: RegisterObjectType(typeof(EmailUserInfo), EmailUserInfo.OBJECT_TYPE)]

namespace CMS.EmailEngine
{
    /// <summary>
    /// EmailUserInfo data container class.
    /// </summary>
    public class EmailUserInfo : AbstractInfo<EmailUserInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.emailuser";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(EmailUserInfoProvider), OBJECT_TYPE, "CMS.EmailUser", null, null, null, null, null, null, null, "EmailID", EmailInfo.OBJECT_TYPE)
        {
            TouchCacheDependencies = true,
            AllowRestore = false,
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
        /// ID of the User.
        /// </summary>
        public virtual int UserID
        {
            get
            {
                return GetIntegerValue("UserID", 0);
            }
            set
            {
                SetValue("UserID", value);
            }
        }


        /// <summary>
        /// E-mail last send result.
        /// </summary>
        public virtual string LastSendResult
        {
            get
            {
                return GetStringValue("LastSendResult", "");
            }
            set
            {
                SetValue("LastSendResult", value);
            }
        }


        /// <summary>
        /// E-mail last send attempt.
        /// </summary>
        public virtual DateTime LastSendAttempt
        {
            get
            {
                return GetDateTimeValue("LastSendAttempt", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("LastSendAttempt", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// E-mail status - Created, Waiting, Sending, Archived.
        /// </summary>
        public virtual EmailStatusEnum Status
        {
            get
            {
                switch (ValidationHelper.GetInteger(GetValue("Status"), 1))
                {
                    case 0:
                        return EmailStatusEnum.Created;
                    case 1:
                        return EmailStatusEnum.Waiting;
                    case 2:
                        return EmailStatusEnum.Sending;
                    case 3:
                        return EmailStatusEnum.Archived;
                }
                return EmailStatusEnum.Waiting;
            }
            set
            {
                SetValue("Status", Convert.ToInt32(value));
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            EmailUserInfoProvider.DeleteEmailUserInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            EmailUserInfoProvider.SetEmailUserInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty EmailUserInfo object.
        /// </summary>
        public EmailUserInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new EmailUserInfo object from the given DataRow.
        /// </summary>
        public EmailUserInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}