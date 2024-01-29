using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Base;
using CMS.Blogs;
using CMS.DocumentEngine.Internal;

[assembly: RegisterObjectType(typeof(BlogCommentInfo), BlogCommentInfo.OBJECT_TYPE)]

namespace CMS.Blogs
{
    /// <summary>
    /// BlogCommentInfo data container class.
    /// </summary>
    public class BlogCommentInfo : AbstractInfo<BlogCommentInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.BLOGCOMMENT;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(BlogCommentInfoProvider), OBJECT_TYPE, "Blog.Comment", "CommentID", null, "CommentGUID", null, "CommentText", null, null, "CommentPostDocumentID", DocumentCultureDataInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("CommentUserID", UserInfo.OBJECT_TYPE),
                new ObjectDependency("CommentApprovedByUserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.RequiredHasDefault)
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
            },
            ContainsMacros = false,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField("CommentInfo")
                }
            }
        };

        #endregion


        #region "Variables"

        private UserDataInfo mCommentInfo;

        #endregion


        #region "Properties"

        /// <summary>
        /// Post ip, and agent values.
        /// </summary>
        public virtual UserDataInfo CommentInfo
        {
            get
            {
                if (mCommentInfo == null)
                {
                    // Load the xml data
                    mCommentInfo = new UserDataInfo();
                    mCommentInfo.LoadData(ValidationHelper.GetString(GetValue("CommentInfo"), ""));
                }
                return mCommentInfo;
            }
        }


        /// <summary>
        /// Comment date and time.
        /// </summary>
        public virtual DateTime CommentDate
        {
            get
            {
                return GetDateTimeValue("CommentDate", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("CommentDate", value);
            }
        }


        /// <summary>
        /// Comment url.
        /// </summary>
        public virtual string CommentUrl
        {
            get
            {
                return GetStringValue("CommentUrl", "");
            }
            set
            {
                SetValue("CommentUrl", value);
            }
        }


        /// <summary>
        /// Comment text.
        /// </summary>
        public virtual string CommentText
        {
            get
            {
                return GetStringValue("CommentText", "");
            }
            set
            {
                SetValue("CommentText", value);
            }
        }


        /// <summary>
        /// Comment e-mail.
        /// </summary>
        public virtual string CommentEmail
        {
            get
            {
                return GetStringValue("CommentEmail", "");
            }
            set
            {
                SetValue("CommentEmail", value);
            }
        }


        /// <summary>
        /// Comment user ID.
        /// </summary>
        public virtual int CommentUserID
        {
            get
            {
                return GetIntegerValue("CommentUserID", 0);
            }
            set
            {
                SetValue("CommentUserID", value, value > 0);
            }
        }


        /// <summary>
        /// Comment is spam.
        /// </summary>
        public virtual bool CommentIsSpam
        {
            get
            {
                return GetBooleanValue("CommentIsSpam", false);
            }
            set
            {
                SetValue("CommentIsSpam", value);
            }
        }


        /// <summary>
        /// Comment is approved.
        /// </summary>
        public virtual bool CommentApproved
        {
            get
            {
                return GetBooleanValue("CommentApproved", false);
            }
            set
            {
                SetValue("CommentApproved", value);
            }
        }


        /// <summary>
        /// Comment approved by user ID.
        /// </summary>
        public virtual int CommentApprovedByUserID
        {
            get
            {
                return GetIntegerValue("CommentApprovedByUserID", 0);
            }
            set
            {
                SetValue("CommentApprovedByUserID", value, value > 0);
            }
        }


        /// <summary>
        /// Comment ID.
        /// </summary>
        public virtual int CommentID
        {
            get
            {
                return GetIntegerValue("CommentID", 0);
            }
            set
            {
                SetValue("CommentID", value);
            }
        }


        /// <summary>
        /// Comment GUID.
        /// </summary>
        public virtual Guid CommentGUID
        {
            get
            {
                return GetValue("CommentGUID", Guid.Empty);
            }
            set
            {
                SetValue("CommentGUID", value);
            }
        }


        /// <summary>
        /// Comment user name.
        /// </summary>
        public virtual string CommentUserName
        {
            get
            {
                return GetStringValue("CommentUserName", "");
            }
            set
            {
                SetValue("CommentUserName", value);
            }
        }


        /// <summary>
        /// Comment post document ID.
        /// </summary>
        public virtual int CommentPostDocumentID
        {
            get
            {
                return GetIntegerValue("CommentPostDocumentID", 0);
            }
            set
            {
                SetValue("CommentPostDocumentID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            BlogCommentInfoProvider.DeleteBlogCommentInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            BlogCommentInfoProvider.SetBlogCommentInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty BlogCommentInfo object.
        /// </summary>
        public BlogCommentInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new BlogCommentInfo object from the given DataRow.
        /// </summary>
        public BlogCommentInfo(DataRow dr)
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
                    {
                        TreeNode blogNode = BlogHelper.GetCommentParentBlog(CommentID, false);

                        return (blogNode != null) && BlogHelper.IsUserAuthorizedToManageComments(blogNode);
                    }

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }
}