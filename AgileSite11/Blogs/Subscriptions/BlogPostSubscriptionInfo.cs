using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Base;
using CMS.Blogs;
using CMS.DocumentEngine.Internal;

[assembly: RegisterObjectType(typeof(BlogPostSubscriptionInfo), BlogPostSubscriptionInfo.OBJECT_TYPE)]

namespace CMS.Blogs
{
    /// <summary>
    /// BlogPostSubscriptionInfo data container class.
    /// </summary>
    public class BlogPostSubscriptionInfo : AbstractInfo<BlogPostSubscriptionInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "blog.postsubscription";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(BlogPostSubscriptionInfoProvider), OBJECT_TYPE, "Blog.PostSubscription", "SubscriptionID", "SubscriptionLastModified", "SubscriptionGUID", null, null, null, null, "SubscriptionPostDocumentID", DocumentCultureDataInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("SubscriptionUserID", UserInfo.OBJECT_TYPE)
            },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = false,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            ModuleName = ModuleName.BLOGS,
            ImportExportSettings =
            {
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                LogExport = false
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Subscription post document ID.
        /// </summary>
        public virtual int SubscriptionPostDocumentID
        {
            get
            {
                return GetIntegerValue("SubscriptionPostDocumentID", 0);
            }
            set
            {
                SetValue("SubscriptionPostDocumentID", value);
            }
        }


        /// <summary>
        /// Subscription e-mail.
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
        /// Subscription last modified.
        /// </summary>
        public virtual DateTime SubscriptionLastModified
        {
            get
            {
                return GetDateTimeValue("SubscriptionLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SubscriptionLastModified", value);
            }
        }


        /// <summary>
        /// Subsription user ID.
        /// </summary>
        public virtual int SubscriptionUserID
        {
            get
            {
                return GetIntegerValue("SubscriptionUserID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("SubscriptionUserID", null);
                }
                else
                {
                    SetValue("SubscriptionUserID", value);
                }
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
            BlogPostSubscriptionInfoProvider.DeleteBlogPostSubscriptionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            BlogPostSubscriptionInfoProvider.SetBlogPostSubscriptionInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty BlogPostSubscriptionInfo object.
        /// </summary>
        public BlogPostSubscriptionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new BlogPostSubscriptionInfo object from the given DataRow.
        /// </summary>
        public BlogPostSubscriptionInfo(DataRow dr)
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
                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                case PermissionsEnum.Destroy:
                    return UserInfoProvider.IsAuthorizedPerResource("cms.blog", "Manage", siteName, (UserInfo)userInfo, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }
}