using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Forums;
using CMS.Helpers;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(ForumSubscriptionInfo), ForumSubscriptionInfo.OBJECT_TYPE)]

namespace CMS.Forums
{
    /// <summary>
    /// ForumSubscriptionInfo data container class.
    /// </summary>
    public class ForumSubscriptionInfo : AbstractInfo<ForumSubscriptionInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "forums.forumsubscription";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ForumSubscriptionInfoProvider), OBJECT_TYPE, "Forums.ForumSubscription", "SubscriptionID", "SubscriptionLastModified", "SubscriptionGUID", null, null, null, null, "SubscriptionForumID", ForumInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("SubscriptionUserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                new ObjectDependency("SubscriptionPostID", ForumPostInfo.OBJECT_TYPE, ObjectDependencyEnum.Required)
            },
            ModuleName = ModuleName.FORUMS,
            RegisterAsChildToObjectTypes = new List<string>
            {
                ForumInfo.OBJECT_TYPE,
                ForumInfo.OBJECT_TYPE_GROUP
            },
            TouchCacheDependencies = true,
            ImportExportSettings =
            {
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                IsExportable = true,
                IsAutomaticallySelected = true
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None,
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None
            },
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Subscription email.
        /// </summary>
        public virtual string SubscriptionEmail
        {
            get
            {
                return GetStringValue("SubscriptionEmail", "");
            }
            set
            {
                SetValue("SubscriptionEmail", value);
            }
        }


        /// <summary>
        /// Subscription post ID.
        /// </summary>
        public virtual int SubscriptionPostID
        {
            get
            {
                return GetIntegerValue("SubscriptionPostID", 0);
            }
            set
            {
                if (ValidationHelper.GetInteger(value, 0) == 0)
                {
                    SetValue("SubscriptionPostID", value);
                }
                else
                {
                    SetValue("SubscriptionPostID", value);
                }
            }
        }


        /// <summary>
        /// Subscription GUID.
        /// </summary>
        public virtual Guid SubscriptionGUID
        {
            get
            {
                return GetGuidValue("SubscriptionGUID", Guid.Empty);
            }
            set
            {
                SetValue("SubscriptionGUID", value);
            }
        }


        /// <summary>
        /// Subscription ID.
        /// </summary>
        public virtual int SubscriptionID
        {
            get
            {
                return GetIntegerValue("SubscriptionID", 0);
            }
            set
            {
                SetValue("SubscriptionID", value);
            }
        }


        /// <summary>
        /// Forum ID.
        /// </summary>
        public virtual int SubscriptionForumID
        {
            get
            {
                return GetIntegerValue("SubscriptionForumID", 0);
            }
            set
            {
                SetValue("SubscriptionForumID", value);
            }
        }


        /// <summary>
        /// User ID of subscription.
        /// </summary>
        public virtual int SubscriptionUserID
        {
            get
            {
                return GetIntegerValue("SubscriptionUserID", 0);
            }
            set
            {
                SetValue("SubscriptionUserID", value, 0);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime SubscriptionLastModified
        {
            get
            {
                return GetDateTimeValue("SubscriptionLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SubscriptionLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Indicates if subscription approved.
        /// </summary>
        public virtual bool SubscriptionApproved
        {
            get
            {
                return GetBooleanValue("SubscriptionApproved", true);
            }
            set
            {
                SetValue("SubscriptionApproved", value);
            }
        }


        /// <summary>
        /// Subscription approval hash code.
        /// </summary>
        public virtual string SubscriptionApprovalHash
        {
            get
            {
                return GetStringValue("SubscriptionApprovalHash", null);
            }
            set
            {
                SetValue("SubscriptionApprovalHash", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ForumSubscriptionInfoProvider.DeleteForumSubscriptionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ForumSubscriptionInfoProvider.SetForumSubscriptionInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ForumSubscriptionInfo object.
        /// </summary>
        public ForumSubscriptionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ForumSubscriptionInfo object from the given DataRow.
        /// </summary>
        public ForumSubscriptionInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}