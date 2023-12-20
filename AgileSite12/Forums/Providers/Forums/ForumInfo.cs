using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Forums;
using CMS.Helpers;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(ForumInfo), ForumInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(ForumInfo), ForumInfo.OBJECT_TYPE_GROUP)]

namespace CMS.Forums
{
    /// <summary>
    /// ForumInfo data container class.
    /// </summary>
    public class ForumInfo : AbstractInfo<ForumInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.FORUM;

        /// <summary>
        /// Object type for group
        /// </summary>
        public const string OBJECT_TYPE_GROUP = PredefinedObjectType.GROUPFORUM;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ForumInfoProvider), OBJECT_TYPE, "Forums.Forum", "ForumID", "ForumLastModified", "ForumGUID", "ForumName", "ForumDisplayName", null, "ForumSiteID", "ForumGroupID", ForumGroupInfo.OBJECT_TYPE)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, SOCIALANDCOMMUNITY),
                },
                ExcludedStagingColumns = new List<string>
                {
                    "ForumPosts",
                    "ForumPostsAbsolute",
                    "ForumThreads",
                    "ForumThreadsAbsolute"
                }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("ForumDocumentID", PredefinedObjectType.DOCUMENTLOCALIZATION),
            },
            GroupIDColumn = "ForumCommunityGroupID",
            ModuleName = ModuleName.FORUMS,
            Feature = FeatureEnum.Forums,
            ImportExportSettings =
            {
                AllowSingleExport = true,
                IsExportable = true,
                LogExport = true,
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, SOCIALANDCOMMUNITY),
                },
            },
            OrderColumn = "ForumOrder",
            SupportsInvalidation = true,
            TypeCondition = new TypeCondition().WhereIsNull("ForumCommunityGroupID"),
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
              ExcludedFieldNames =
              {
                  "ForumThreads",
                  "ForumPosts",
                  "ForumLastPostTime",
                  "ForumLastPostUserName",
                  "ForumThreadsAbsolute",
                  "ForumPostsAbsolute",
                  "ForumLastPostTimeAbsolute",
                  "ForumLastPostUserNameAbsolute"
              }
            }
        };


        /// <summary>
        /// Type information for group forum.
        /// </summary>
        public static ObjectTypeInfo TYPEINFOGROUP = new ObjectTypeInfo(typeof(ForumInfoProvider), OBJECT_TYPE_GROUP, "Forums.Forum", "ForumID", "ForumLastModified", "ForumGUID", "ForumName", "ForumDisplayName", null, "ForumSiteID", "ForumGroupID", ForumGroupInfo.OBJECT_TYPE_GROUP)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, SOCIALANDCOMMUNITY),
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            TouchCacheDependencies = true,
            LogEvents = true,
            OriginalTypeInfo = TYPEINFO,
            GroupIDColumn = "ForumCommunityGroupID",
            ModuleName = ModuleName.FORUMS,
            OrderColumn = "ForumOrder",
            ImportExportSettings =
            {
                AllowSingleExport = false,
                LogExport = true
            },
            SupportsInvalidation = true,

            TypeCondition = new TypeCondition().WhereIsNotNull("ForumCommunityGroupID"),
        };

        #endregion


        #region "Private properties"

        /// <summary>
        /// Gets ForumGroupInfo for this forum.
        /// </summary>
        private ForumGroupInfo CurrentForumGroupInfo
        {
            get
            {
                return ForumGroupInfoProvider.GetForumGroupInfo(ForumGroupID);
            }
        }

        #endregion


        #region "Public Properties"

        private ContainerCustomData mForumSettings = null;

        /// <summary>
        /// Gets or sets the max file size for forum attachment, 0 = unlimited.
        /// </summary>
        public int ForumAttachmentMaxFileSize
        {
            get
            {
                object val = GetValue("ForumAttachmentMaxFileSize");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupAttachmentMaxFileSize;
                    }
                }

                return ValidationHelper.GetInteger(val, 0);
            }
            set
            {
                if (value < 0)
                {
                    SetValue("ForumAttachmentMaxFileSize", null);
                }
                else
                {
                    SetValue("ForumAttachmentMaxFileSize", value);
                }
            }
        }


        /// <summary>
        /// Forum settings.
        /// </summary>
        public ContainerCustomData ForumSettings
        {
            get
            {
                if (mForumSettings == null)
                {
                    mForumSettings = new ContainerCustomData(this, "ForumSettings");
                }
                return mForumSettings;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the author of the post can edit his own posts.
        /// </summary>
        public bool ForumAuthorEdit
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumAuthorEdit");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupAuthorEdit;
                    }
                }

                return ValidationHelper.GetBoolean(val, false);
            }
            set
            {
                SetValue("ForumAuthorEdit", value);
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the author of the post can delete his own posts.
        /// </summary>
        public bool ForumAuthorDelete
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumAuthorDelete");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupAuthorDelete;
                    }
                }

                return ValidationHelper.GetBoolean(val, false);
            }
            set
            {
                SetValue("ForumAuthorDelete", value);
            }
        }


        /// <summary>
        /// Gets or sets the type of the forum.
        /// 0 = User can choose whether post is question 
        /// 1 = Discussion forum (default) 
        /// 2 = Answer forum
        /// </summary>
        public virtual int ForumType
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumType");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupType;
                    }
                }

                return ValidationHelper.GetInteger(val, 1);
            }
            set
            {
                SetValue("ForumType", value);
            }
        }


        /// <summary>
        /// Gets or sets the limit of the votes to mark the post as an answer.
        /// </summary>
        public virtual int ForumIsAnswerLimit
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumIsAnswerLimit");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupIsAnswerLimit;
                    }
                }

                return ValidationHelper.GetInteger(val, 5);
            }
            set
            {
                SetValue("ForumIsAnswerLimit", value);
            }
        }


        /// <summary>
        /// Gets or sets the maximal allowed side size of the image (larger images are resized to this size).
        /// </summary>
        public virtual int ForumImageMaxSideSize
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumImageMaxSideSize");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupImageMaxSideSize;
                    }
                }

                return ValidationHelper.GetInteger(val, 400);
            }
            set
            {
                SetValue("ForumImageMaxSideSize", value);
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether forum is locked.
        /// </summary>
        public virtual bool ForumIsLocked
        {
            get
            {
                return GetBooleanValue("ForumIsLocked", false);
            }
            set
            {
                SetValue("ForumIsLocked", value);
            }
        }


        /// <summary>
        /// Forum use CAPTCHA for new post.
        /// </summary>
        public virtual bool ForumUseCAPTCHA
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumUseCAPTCHA");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupUseCAPTCHA;
                    }
                }

                return ValidationHelper.GetBoolean(val, false);
            }
            set
            {
                SetValue("ForumUseCAPTCHA", value);
            }
        }


        /// <summary>
        /// Forum use HTML editor to edit posts.
        /// </summary>
        public virtual bool ForumHTMLEditor
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumHTMLEditor");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupHTMLEditor;
                    }
                }

                return ValidationHelper.GetBoolean(val, false);
            }
            set
            {
                SetValue("ForumHTMLEditor", value);
            }
        }


        /// <summary>
        /// Forum allow user to change the name.
        /// </summary>
        public virtual bool ForumAllowChangeName
        {
            get
            {
                return GetBooleanValue("ForumAllowChangeName", false);
            }
            set
            {
                SetValue("ForumAllowChangeName", value);
            }
        }


        /// <summary>
        /// Indicates whether forum is open (users can add posts).
        /// </summary>
        public virtual bool ForumOpen
        {
            get
            {
                return GetBooleanValue("ForumOpen", false);
            }
            set
            {
                SetValue("ForumOpen", value);
            }
        }


        /// <summary>
        /// Forum base URL.
        /// </summary>
        public string ForumBaseUrl
        {
            get
            {
                ForumGroupInfo fgi = ForumGroupInfoProvider.GetForumGroupInfo(ForumGroupID);
                if (fgi != null)
                {
                    return GetStringValue("ForumBaseUrl", fgi.GroupBaseUrl);
                }
                else
                {
                    return GetStringValue("ForumBaseUrl", "");
                }
            }
            set
            {
                SetValue("ForumBaseUrl", value);
            }
        }


        /// <summary>
        /// Forum base URL.
        /// </summary>
        public string ForumUnsubscriptionUrl
        {
            get
            {
                ForumGroupInfo fgi = ForumGroupInfoProvider.GetForumGroupInfo(ForumGroupID);
                if (fgi != null)
                {
                    return GetStringValue("ForumUnsubscriptionUrl", fgi.GroupUnsubscriptionUrl);
                }
                else
                {
                    return GetStringValue("ForumUnsubscriptionUrl", "");
                }
            }
            set
            {
                SetValue("ForumUnsubscriptionUrl", value);
            }
        }


        /// <summary>
        /// Forum document ID.
        /// </summary>
        public virtual int ForumDocumentID
        {
            get
            {
                return GetIntegerValue("ForumDocumentID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("ForumDocumentID", null);
                }
                else
                {
                    SetValue("ForumDocumentID", value);
                }
            }
        }


        /// <summary>
        /// Forum site ID.
        /// </summary>
        public virtual int ForumSiteID
        {
            get
            {
                return GetIntegerValue("ForumSiteID", 0);
            }
            set
            {
                SetValue("ForumSiteID", value);
            }
        }


        /// <summary>
        /// Indicates whether emails of users in posts are displayed.
        /// </summary>
        public virtual bool ForumDisplayEmails
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumDisplayEmails");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupDisplayEmails;
                    }
                }

                return ValidationHelper.GetBoolean(val, false);
            }
            set
            {
                SetValue("ForumDisplayEmails", value);
            }
        }


        /// <summary>
        /// Forum description.
        /// </summary>
        public virtual string ForumDescription
        {
            get
            {
                return GetStringValue("ForumDescription", "");
            }
            set
            {
                SetValue("ForumDescription", value);
            }
        }


        /// <summary>
        /// Last post user name.
        /// </summary>
        public virtual string ForumLastPostUserName
        {
            get
            {
                return GetStringValue("ForumLastPostUserName", "");
            }
            set
            {
                SetValue("ForumLastPostUserName", value);
            }
        }


        /// <summary>
        /// Number of posts.
        /// </summary>
        public virtual int ForumPosts
        {
            get
            {
                return GetIntegerValue("ForumPosts", 0);
            }
            set
            {
                SetValue("ForumPosts", value);
            }
        }


        /// <summary>
        /// Indicates whether forum requires email.
        /// </summary>
        public virtual bool ForumRequireEmail
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumRequireEmail");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupRequireEmail;
                    }
                }

                return ValidationHelper.GetBoolean(val, false);
            }
            set
            {
                SetValue("ForumRequireEmail", value);
            }
        }


        /// <summary>
        /// Display name.
        /// </summary>
        public virtual string ForumDisplayName
        {
            get
            {
                return GetStringValue("ForumDisplayName", "");
            }
            set
            {
                SetValue("ForumDisplayName", value);
            }
        }


        /// <summary>
        /// Forum name (code name).
        /// </summary>
        public virtual string ForumName
        {
            get
            {
                return GetStringValue("ForumName", "");
            }
            set
            {
                SetValue("ForumName", value);
            }
        }


        /// <summary>
        /// Forum ordinal number.
        /// </summary>
        public virtual int ForumOrder
        {
            get
            {
                return GetIntegerValue("ForumOrder", 0);
            }
            set
            {
                if (Convert.ToInt32(value) == 0)
                {
                    SetValue("ForumOrder", DBNull.Value);
                }
                else
                {
                    SetValue("ForumOrder", value);
                }
            }
        }


        /// <summary>
        /// Number of threads.
        /// </summary>
        public virtual int ForumThreads
        {
            get
            {
                return GetIntegerValue("ForumThreads", 0);
            }
            set
            {
                SetValue("ForumThreads", value);
            }
        }


        /// <summary>
        /// Forum group ID.
        /// </summary>
        public virtual int ForumGroupID
        {
            get
            {
                return GetIntegerValue("ForumGroupID", 0);
            }
            set
            {
                SetValue("ForumGroupID", value);
            }
        }


        /// <summary>
        /// Forum access bit array.
        /// </summary>
        public virtual int ForumAccess
        {
            get
            {
                return GetIntegerValue("ForumAccess", 040000);
            }
            set
            {
                SetValue("ForumAccess", value);
            }
        }


        /// <summary>
        /// Indicates whether the access to forum is allowed.
        /// </summary>
        public virtual SecurityAccessEnum AllowAccess
        {
            get
            {
                return SecurityHelper.GetSecurityAccessEnum(ForumAccess, 6);
            }
            set
            {
                ForumAccess = SecurityHelper.SetSecurityAccessEnum(ForumAccess, value, 6);
            }
        }


        /// <summary>
        /// Indicates whether the files could be attached to the forum post.
        /// </summary>
        public virtual SecurityAccessEnum AllowAttachFiles
        {
            get
            {
                return SecurityHelper.GetSecurityAccessEnum(ForumAccess, 5);
            }
            set
            {
                ForumAccess = SecurityHelper.SetSecurityAccessEnum(ForumAccess, value, 5);
            }
        }


        /// <summary>
        /// Indicates whether the subscribing to the forum post is allowed.
        /// </summary>
        public virtual SecurityAccessEnum AllowSubscribe
        {
            get
            {
                return SecurityHelper.GetSecurityAccessEnum(ForumAccess, 4);
            }
            set
            {
                ForumAccess = SecurityHelper.SetSecurityAccessEnum(ForumAccess, value, 4);
            }
        }


        /// <summary>
        /// Indicates whether the quoting of the forum post is allowed.
        /// </summary>
        public virtual SecurityAccessEnum AllowMarkAsAnswer
        {
            get
            {
                return SecurityHelper.GetSecurityAccessEnum(ForumAccess, 3);
            }
            set
            {
                ForumAccess = SecurityHelper.SetSecurityAccessEnum(ForumAccess, value, 3);
            }
        }


        /// <summary>
        /// Indicates whether the replies are allowed for the forum posts.
        /// </summary>
        public virtual SecurityAccessEnum AllowReply
        {
            get
            {
                return SecurityHelper.GetSecurityAccessEnum(ForumAccess, 2);
            }
            set
            {
                ForumAccess = SecurityHelper.SetSecurityAccessEnum(ForumAccess, value, 2);
            }
        }


        /// <summary>
        /// Indicates whether the posts are allowed for the forum.
        /// </summary>
        public virtual SecurityAccessEnum AllowPost
        {
            get
            {
                return SecurityHelper.GetSecurityAccessEnum(ForumAccess, 1);
            }
            set
            {
                ForumAccess = SecurityHelper.SetSecurityAccessEnum(ForumAccess, value, 1);
            }
        }


        /// <summary>
        /// Forum ID.
        /// </summary>
        public virtual int ForumID
        {
            get
            {
                return GetIntegerValue("ForumID", 0);
            }
            set
            {
                SetValue("ForumID", value);
            }
        }


        /// <summary>
        /// Last post time.
        /// </summary>
        public virtual DateTime ForumLastPostTime
        {
            get
            {
                return GetDateTimeValue("ForumLastPostTime", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ForumLastPostTime", value);
            }
        }


        /// <summary>
        /// Indicates whether forum is moderated.
        /// </summary>
        public virtual bool ForumModerated
        {
            get
            {
                return GetBooleanValue("ForumModerated", false);
            }
            set
            {
                SetValue("ForumModerated", value);
            }
        }


        /// <summary>
        /// Forum GUID.
        /// </summary>
        public virtual Guid ForumGUID
        {
            get
            {
                return GetGuidValue("ForumGUID", Guid.Empty);
            }
            set
            {
                SetValue("ForumGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime ForumLastModified
        {
            get
            {
                return GetDateTimeValue("ForumLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ForumLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Number of all threads.
        /// </summary>
        public virtual int ForumThreadsAbsolute
        {
            get
            {
                return GetIntegerValue("ForumThreadsAbsolute", 0);
            }
            set
            {
                SetValue("ForumThreadsAbsolute", value);
            }
        }


        /// <summary>
        /// Number of all posts.
        /// </summary>
        public virtual int ForumPostsAbsolute
        {
            get
            {
                return GetIntegerValue("ForumPostsAbsolute", 0);
            }
            set
            {
                SetValue("ForumPostsAbsolute", value);
            }
        }


        /// <summary>
        /// User name of last post.
        /// </summary>
        public virtual string ForumLastPostUserNameAbsolute
        {
            get
            {
                return GetStringValue("ForumLastPostUserNameAbsolute", null);
            }
            set
            {
                SetValue("ForumLastPostUserNameAbsolute", value);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime ForumLastPostTimeAbsolute
        {
            get
            {
                return GetDateTimeValue("ForumLastPostTimeAbsolute", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ForumLastPostTimeAbsolute", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Indicates whether forum actions are logged as on-line marketing activities.
        /// </summary>
        public virtual bool ForumLogActivity
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumLogActivity");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupLogActivity;
                    }
                }

                return ValidationHelper.GetBoolean(GetValue("ForumLogActivity"), false);
            }
            set
            {
                SetValue("ForumLogActivity", value);
            }
        }


        /// <summary>
        /// Forum community group ID
        /// </summary>
        public virtual int ForumCommunityGroupID
        {
            get
            {
                return GetIntegerValue("ForumCommunityGroupID", 0);
            }
            set
            {
                SetValue("ForumCommunityGroupID", value, 0);
            }
        }


        /// <summary>
        /// Gets or sets whether the forum should use double opt-in.
        /// </summary>
        public virtual bool ForumEnableOptIn
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumEnableOptIn");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupEnableOptIn;
                    }
                }

                return ValidationHelper.GetBoolean(GetValue("ForumEnableOptIn"), false);
            }
            set
            {
                SetValue("ForumEnableOptIn", value);
            }
        }


        /// <summary>
        /// Gets or sets whether subscription confirmation should be sent after double opt-in e-mail.
        /// </summary>
        public virtual bool ForumSendOptInConfirmation
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumSendOptInConfirmation");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupSendOptInConfirmation;
                    }
                }

                return ValidationHelper.GetBoolean(GetValue("ForumSendOptInConfirmation"), false);
            }
            set
            {
                SetValue("ForumSendOptInConfirmation", value);
            }
        }


        /// <summary>
        /// Gets or sets the URL of the double opt-in page.
        /// </summary>
        public virtual string ForumOptInApprovalURL
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumOptInApprovalURL");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupOptInApprovalURL;
                    }
                }

                return ValidationHelper.GetString(GetValue("ForumOptInApprovalURL"), string.Empty);
            }
            set
            {
                SetValue("ForumOptInApprovalURL", value);
            }
        }


        #region "Discussion properties"

        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert URL macros into the post text.
        /// </summary>
        public bool ForumEnableURL
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumDiscussionActions");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupEnableURL;
                    }
                }

                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.InsertURL);
            }
            set
            {
                SetValue("ForumDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.InsertURL, value));
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert URL macros into the post text using the advanced dialogs.
        /// </summary>
        public bool ForumEnableAdvancedURL
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumDiscussionActions");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupEnableAdvancedURL;
                    }
                }

                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.InsertAdvancedURL);
            }
            set
            {
                SetValue("ForumDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.InsertAdvancedURL, value));
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert Quote macros into the post text.
        /// </summary>
        public bool ForumEnableQuote
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumDiscussionActions");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupEnableQuote;
                    }
                }

                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.InsertQuote);
            }
            set
            {
                SetValue("ForumDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.InsertQuote, value));
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert Image macros into the post text.
        /// </summary>
        public bool ForumEnableImage
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumDiscussionActions");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupEnableImage;
                    }
                }

                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.InsertImage);
            }
            set
            {
                SetValue("ForumDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.InsertImage, value));
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert Image macros into the post text using the advanced dialogs.
        /// </summary>
        public bool ForumEnableAdvancedImage
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumDiscussionActions");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupEnableAdvancedImage;
                    }
                }

                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.InsertAdvancedImage);
            }
            set
            {
                SetValue("ForumDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.InsertAdvancedImage, value));
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert Image macros into the post text.
        /// </summary>
        public bool ForumEnableCodeSnippet
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumDiscussionActions");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupEnableCodeSnippet;
                    }
                }

                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.InsertCode);
            }
            set
            {
                SetValue("ForumDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.InsertCode, value));
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert Image macros into the post text.
        /// </summary>
        public bool ForumEnableFontBold
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumDiscussionActions");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupEnableFontBold;
                    }
                }

                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.FontBold);
            }
            set
            {
                SetValue("ForumDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.FontBold, value));
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert Image macros into the post text.
        /// </summary>
        public bool ForumEnableFontItalics
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumDiscussionActions");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupEnableFontItalics;
                    }
                }

                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.FontItalics);
            }
            set
            {
                SetValue("ForumDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.FontItalics, value));
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert Image macros into the post text.
        /// </summary>
        public bool ForumEnableFontUnderline
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumDiscussionActions");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupEnableFontUnderline;
                    }
                }

                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.FontUnderline);
            }
            set
            {
                SetValue("ForumDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.FontUnderline, value));
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert font strike macros into the post text.
        /// </summary>
        public bool ForumEnableFontStrike
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumDiscussionActions");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupEnableFontStrike;
                    }
                }

                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.FontStrike);
            }
            set
            {
                SetValue("ForumDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.FontStrike, value));
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert font color macros into the post text.
        /// </summary>
        public bool ForumEnableFontColor
        {
            get
            {
                // If Forum doesn't have this value inherit ForumGroup setting
                object val = GetValue("ForumDiscussionActions");
                if (val == null)
                {
                    if (CurrentForumGroupInfo != null)
                    {
                        return CurrentForumGroupInfo.GroupEnableFontColor;
                    }
                }

                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.FontColor);
            }
            set
            {
                SetValue("ForumDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("ForumDiscussionActions"), 0), DiscussionActionEnum.FontColor, value));
            }
        }

        #endregion


        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                if (ForumCommunityGroupID == 0)
                {
                    return TYPEINFO;
                }
                else
                {
                    return TYPEINFOGROUP;
                }
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ForumInfoProvider.DeleteForumInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ForumInfoProvider.SetForumInfo(this);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Registers the properties of this object
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty("Posts", m => m.Children[ForumPostInfo.OBJECT_TYPE]);
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            bool resetPostInfo = true;

            // This binding has to be cloned in the postprocessing phase (it's triple binding)
            settings.ExcludedBindingTypes.Add(ForumRoleInfo.OBJECT_TYPE);

            // Preserve CommunityGroupID
            ForumCommunityGroupID = originalObject.GetIntegerValue("ForumCommunityGroupID", 0);

            // Exclude Forum posts from children, these have to be handleled in a spacial manner (hierarchical object)
            if (settings.ExcludedChildTypes == null)
            {
                settings.ExcludedChildTypes = new List<string>();
            }
            settings.ExcludedChildTypes.Add(ForumPostInfo.OBJECT_TYPE);

            Hashtable p = settings.CustomParameters;
            if (p != null)
            {
                // Exclude forum posts entirely if required
                if (ValidationHelper.GetBoolean(p[PredefinedObjectType.FORUM + ".posts"], false))
                {
                    resetPostInfo = false;
                }
            }

            if (resetPostInfo)
            {
                // No posts can be cloned, reset info about posts
                ForumPosts = 0;
                ForumPostsAbsolute = 0;
                ForumThreads = 0;
                ForumThreadsAbsolute = 0;

                ForumLastPostUserName = null;
                ForumLastPostUserNameAbsolute = null;

                SetValue("ForumLastPostTime", null);
                SetValue("ForumLastPostTimeAbsolute", null);

                Insert();
            }
            else
            {
                Insert();

                // Posts should be cloned, clone threads and they will make sure about cloning their descendants
                DataSet ds = ForumPostInfoProvider.GetForumPosts().WhereEquals("PostForumID", originalObject.Generalized.ObjectID).WhereEquals("PostLevel", 0);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    int originalParentId = settings.ParentID;
                    settings.ParentID = this.ForumID;

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        ForumPostInfo post = new ForumPostInfo(dr);
                        post.Generalized.InsertAsClone(settings, result);
                    }

                    settings.ParentID = originalParentId;
                }
            }
        }


        /// <summary>
        /// Clones forum role and forum moderator bindings
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Result of the cloning - messages in this object will be altered by processing this method</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsClonePostprocessing(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            if (settings.IncludeBindings)
            {
                // Clone project role permissions
                DataSet ds = ForumRoleInfoProvider.GetRelationships("ForumID = " + originalObject.Generalized.ObjectID, null, -1, null);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    int originalParentId = settings.ParentID;
                    settings.ParentID = ForumID;

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        ForumRoleInfo binding = new ForumRoleInfo(dr);
                        int newRoleId = settings.Translations.GetNewID(RoleInfo.OBJECT_TYPE_GROUP, binding.RoleID, null, 0, null, null, null);
                        if (newRoleId > 0)
                        {
                            binding.RoleID = newRoleId;
                        }
                        binding.Generalized.InsertAsClone(settings, result);
                    }

                    settings.ParentID = originalParentId;
                }
            }
        }


        /// <summary>
        /// Checks the object license. Returns true if the licensing conditions for this object were matched
        /// </summary>
        /// <param name="action">Object action</param>
        /// <param name="domainName">Domain name, if not set, uses current domain</param>
        protected sealed override bool CheckLicense(ObjectActionEnum action, string domainName)
        {
            return ForumInfoProvider.CheckLicense(action, domainName);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ForumInfo object.
        /// </summary>
        public ForumInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ForumInfo object from the given DataRow.
        /// </summary>
        public ForumInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Overridden methods"

        /// <summary>
        /// Custom permissions check. Chat user can be read with Read or GlobalRead permission
        /// and modified with Modify or GlobalModify permission.
        /// </summary>
        /// <param name="permission">Permission to perform this operation will be checked</param>
        /// <param name="siteName">Permissions on this site will be checked</param>
        /// <param name="userInfo">Permissions of this user will be checked</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        /// <returns>True if user is allowed to perform specified operation on the this object; otherwise false</returns>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            if (!TypeInfo.ObjectType.EqualsCSafe(OBJECT_TYPE_GROUP, true))
            {
                // Non-group permission check is as usual
                return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }

            switch (permission)
            {
                case PermissionsEnum.Read:
                    if (UserInfoProvider.IsAuthorizedPerResource(ModuleName.GROUPS, "Read", siteName, (UserInfo)userInfo, false) || MembershipContext.AuthenticatedUser.IsGroupMember(ForumCommunityGroupID))
                    {
                        return true;
                    }

                    break;

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Destroy:
                case PermissionsEnum.Modify:
                    if (UserInfoProvider.IsAuthorizedPerResource(ModuleName.GROUPS, "Manage", siteName, (UserInfo)userInfo, false) || MembershipContext.AuthenticatedUser.IsGroupAdministrator(ForumCommunityGroupID))
                    {
                        return true;
                    }

                    break;
            }

            return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
        }

        #endregion
    }
}