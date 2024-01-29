using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Newsletters;

[assembly: RegisterObjectType(typeof(UnsubscriptionInfo), UnsubscriptionInfo.OBJECT_TYPE)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Container for unsubscription data.
    /// Contains email unsubscribed from newsletter.
    /// If false, having null in the newsletter ID column means unsubscribed from all newsletters.
    /// </summary>
    [Serializable]
    public class UnsubscriptionInfo : AbstractInfo<UnsubscriptionInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.NEWSLETTERUNSUBSCRIPTION;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(UnsubscriptionInfoProvider), OBJECT_TYPE, "newsletter.unsubscription", "UnsubscriptionID", null, "UnsubscriptionGUID", null, "UnsubscriptionEmail", null, null, null, null)
        {          
            DependsOn = new List<ObjectDependency>
            { 
                new ObjectDependency("UnsubscriptionNewsletterID", NewsletterInfo.OBJECT_TYPE),  
                new ObjectDependency("UnsubscriptionFromIssueID", IssueInfo.OBJECT_TYPE)
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None,
            },
            ModuleName = ModuleName.NEWSLETTER,
            Feature = FeatureEnum.Newsletters,
            SupportsGlobalObjects = true,
            ContainsMacros = false,
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.None,
                AllowSingleExport = false,
                IsExportable = false,
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Unsubscription ID.
        /// </summary>
        [DatabaseField]
        public virtual int UnsubscriptionID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("UnsubscriptionID"), 0);
            }
            set
            {
                SetValue("UnsubscriptionID", value);
            }
        }


        /// <summary>
        /// Unsubscribed email address. Must be lowercase.
        /// </summary>
        [DatabaseField]
        public virtual string UnsubscriptionEmail
        {
            get
            {
                return ValidationHelper.GetString(GetValue("UnsubscriptionEmail"), String.Empty);
            }
            set
            {
                SetValue("UnsubscriptionEmail", value);
            }
        }


        /// <summary>
        /// Time when unsubscription was created.
        /// </summary>
        [DatabaseField]
        public virtual DateTime UnsubscriptionCreated
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("UnsubscriptionCreated"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("UnsubscriptionCreated", value);
            }
        }


        /// <summary>
        /// Unsubscribed from specific newsletter.
        /// If null, unsubscribed from all newsletters.
        /// </summary>
        [DatabaseField]
        public virtual int? UnsubscriptionNewsletterID
        {
            get
            {
                var value = GetIntegerValue("UnsubscriptionNewsletterID", 0);
                return value == 0 ? null : (int?)value;
            }
            set
            {
                SetValue("UnsubscriptionNewsletterID", value);
            }
        }


        /// <summary>
        /// Issue id from where was unsubscription created.
        /// If null, unsubscriptions were not made from the issue.
        /// </summary>
        [DatabaseField]
        public virtual int? UnsubscriptionFromIssueID
        {
            get
            {
                var value = ValidationHelper.GetInteger(GetValue("UnsubscriptionFromIssueID"), 0);
                return value == 0 ? null : (int?)value;
            }
            set
            {
                SetValue("UnsubscriptionFromIssueID", value);
            }
        }


        /// <summary>
        /// Unsubscription GUID.
        /// For unique identification.
        /// </summary>
        [DatabaseField]
        public virtual Guid UnsubscriptionGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("UnsubscriptionGUID"), Guid.Empty);
            }
            set
            {
                SetValue("UnsubscriptionGUID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            UnsubscriptionInfoProvider.DeleteUnsubscriptionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            UnsubscriptionInfoProvider.SetUnsubscriptionInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public UnsubscriptionInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty UnsubscriptionInfo object.
        /// </summary>
        public UnsubscriptionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new UnsubscriptionInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public UnsubscriptionInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
        

        #region "Permissions"

        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            switch (permission)
            {
                case PermissionsEnum.Read:
                    return UserInfoProvider.IsAuthorizedPerResource(ModuleName.NEWSLETTER, "Read", siteName, (UserInfo)userInfo, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    return UserInfoProvider.IsAuthorizedPerResource(ModuleName.NEWSLETTER, "ManageSubscribers", siteName, (UserInfo)userInfo, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }
}