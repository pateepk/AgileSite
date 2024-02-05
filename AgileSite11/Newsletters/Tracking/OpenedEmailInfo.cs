using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Newsletters;

[assembly: RegisterObjectType(typeof(OpenedEmailInfo), OpenedEmailInfo.OBJECT_TYPE)]

namespace CMS.Newsletters
{
    /// <summary>
    /// OpenedEmail data container class.
    /// </summary>
    public class OpenedEmailInfo : AbstractInfo<OpenedEmailInfo>
    {
        #region "Constants"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "newsletter.openedemail";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(OpenedEmailInfoProvider), OBJECT_TYPE, "Newsletter.OpenedEmail", "OpenedEmailID", null, "OpenedEmailGuid", null, null, null, null, "OpenedEmailIssueID", IssueInfo.OBJECT_TYPE)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None,
            },
            DeleteObjectWithAPI = true,
            LogEvents = false,
            RegisterAsChildToObjectTypes = new List<string> { IssueInfo.OBJECT_TYPE, IssueInfo.OBJECT_TYPE_VARIANT },
            ContainsMacros = false,
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// OpenedEmail ID.
        /// </summary>
        public virtual int OpenedEmailID
        {
            get
            {
                return GetIntegerValue("OpenedEmailID", 0);
            }
            set
            {
                SetValue("OpenedEmailID", value);
            }
        }


        /// <summary>
        /// OpenedEmail Email.
        /// </summary>
        public virtual string OpenedEmailEmail
        {
            get
            {
                return GetStringValue("OpenedEmailEmail", String.Empty);
            }
            set
            {
                SetValue("OpenedEmailEmail", value);
            }
        }


        /// <summary>
        /// OpenedEmail GUID
        /// </summary>
        public virtual Guid OpenedEmailGuid
        {
            get
            {
                return GetGuidValue("OpenedEmailGuid", Guid.Empty);
            }
            set
            {
                SetValue("OpenedEmailGuid", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Gets or sets the timestamp when the issue of the newsletter was opened by subscriber.
        /// </summary>
        /// <value>
        /// Timestamp or CMS.Helpers.DateTimeHelper.ZERO_TIME if the issue was not read by the specified subscriber yet.
        /// </value>
        public virtual DateTime OpenedEmailTime
        {
            get
            {
                return GetDateTimeValue("OpenedEmailTime", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("OpenedEmailTime", value);
            }
        }


        /// <summary>
        /// Gets or sets ID of the newsletter's issue.
        /// </summary>
        /// <value>
        /// Newsletter issue's ID, or 0 if not found.
        /// </value>        
        public virtual int OpenedEmailIssueID
        {
            get
            {
                return GetIntegerValue("OpenedEmailIssueID", 0);
            }
            set
            {
                SetValue("OpenedEmailIssueID", value);
            }
        }

        #endregion


        #region "Contructors"

        /// <summary>
        /// Creates a new OpenedEmailInfo object.
        /// </summary>
        public OpenedEmailInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new OpenedEmailInfo object from the specified DataRow.
        /// </summary>
        /// <param name="openedEmailRow">Raw values from DB table that represent this object</param>
        public OpenedEmailInfo(DataRow openedEmailRow)
            : base(TYPEINFO, openedEmailRow)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Deletes this OpenedEmailInfo object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            OpenedEmailInfoProvider.DeleteOpenedEmailInfo(this);
        }


        /// <summary>
        /// Updates this OpenedEmailInfo object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            OpenedEmailInfoProvider.SetOpenedEmailInfo(this);
        }

        #endregion
    }
}