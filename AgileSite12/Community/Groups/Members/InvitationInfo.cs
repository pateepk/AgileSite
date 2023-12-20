using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Helpers;
using CMS.Membership;
using CMS.DataEngine;
using CMS.Community;

[assembly: RegisterObjectType(typeof(InvitationInfo), InvitationInfo.OBJECT_TYPE)]

namespace CMS.Community
{
    /// <summary>
    /// InvitationInfo data container class.
    /// </summary>
    public class InvitationInfo : AbstractInfo<InvitationInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "community.invitation";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(InvitationInfoProvider), OBJECT_TYPE, "Community.Invitation", "InvitationID", "InvitationLastModified", "InvitationGUID", null, null, null, null, "InvitationGroupID", GroupInfo.OBJECT_TYPE)
                                              {
                                                  DependsOn = new List<ObjectDependency>() { new ObjectDependency("InvitedUserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.NotRequired), new ObjectDependency("InvitedByUserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.Required) },
                                                  ModuleName = "cms.groups",
                                                  GroupIDColumn = "InvitationGroupID",
                                                  TouchCacheDependencies = true,
                                                  ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, },
                                                  SynchronizationSettings =
                                                  {
                                                      IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                                                      LogSynchronization = SynchronizationTypeEnum.None
                                                  },
                                              };

        #endregion


        #region "Public properties"

        /// <summary>
        /// Invitation created.
        /// </summary>
        public virtual DateTime InvitationCreated
        {
            get
            {
                return GetDateTimeValue("InvitationCreated", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("InvitationCreated", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Invitation ID.
        /// </summary>
        public virtual int InvitationID
        {
            get
            {
                return GetIntegerValue("InvitationID", 0);
            }
            set
            {
                SetValue("InvitationID", value);
            }
        }


        /// <summary>
        /// Invitation group ID.
        /// </summary>
        public virtual int InvitationGroupID
        {
            get
            {
                return GetIntegerValue("InvitationGroupID", 0);
            }
            set
            {
                if (value > 0)
                {
                    SetValue("InvitationGroupID", value);
                }
                else
                {
                    SetValue("InvitationGroupID", null);
                }
            }
        }


        /// <summary>
        /// Invitation last modified.
        /// </summary>
        public virtual DateTime InvitationLastModified
        {
            get
            {
                return GetDateTimeValue("InvitationLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("InvitationLastModified", value);
            }
        }


        /// <summary>
        /// Invitation GUID.
        /// </summary>
        public virtual Guid InvitationGUID
        {
            get
            {
                return GetGuidValue("InvitationGUID", Guid.Empty);
            }
            set
            {
                SetValue("InvitationGUID", value);
            }
        }


        /// <summary>
        /// Invitation comment.
        /// </summary>
        public virtual string InvitationComment
        {
            get
            {
                return GetStringValue("InvitationComment", string.Empty);
            }
            set
            {
                SetValue("InvitationComment", value);
            }
        }


        /// <summary>
        /// Invited by user ID.
        /// </summary>
        public virtual int InvitedByUserID
        {
            get
            {
                return GetIntegerValue("InvitedByUserID", 0);
            }
            set
            {
                SetValue("InvitedByUserID", value);
            }
        }


        /// <summary>
        /// Invitation valid to.
        /// </summary>
        public virtual DateTime InvitationValidTo
        {
            get
            {
                return GetDateTimeValue("InvitationValidTo", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("InvitationValidTo", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Invited user ID.
        /// </summary>
        public virtual int InvitedUserID
        {
            get
            {
                return GetIntegerValue("InvitedUserID", 0);
            }
            set
            {
                if (value > 0)
                {
                    SetValue("InvitedUserID", value);
                }
                else
                {
                    SetValue("InvitedUserID", null);
                }
            }
        }


        /// <summary>
        /// Invitation user e-mail.
        /// </summary>
        public virtual string InvitationUserEmail
        {
            get
            {
                return GetStringValue("InvitationUserEmail", string.Empty);
            }
            set
            {
                SetValue("InvitationUserEmail", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            InvitationInfoProvider.DeleteInvitationInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            InvitationInfoProvider.SetInvitationInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty InvitationInfo object.
        /// </summary>
        public InvitationInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new InvitationInfo object from the given DataRow.
        /// </summary>
        public InvitationInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}