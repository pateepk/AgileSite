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
using CMS.SocialMarketing;
using CMS.WebAnalytics;

[assembly: RegisterObjectType(typeof(LinkedInPostInfo), LinkedInPostInfo.OBJECT_TYPE)]
    
namespace CMS.SocialMarketing
{
    /// <summary>
    /// Represents a LinkedIn company share.
    /// </summary>
    [Serializable]
    public class LinkedInPostInfo : AbstractInfo<LinkedInPostInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "sm.linkedinpost";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(LinkedInPostInfoProvider), OBJECT_TYPE, "SM.LinkedInPost", "LinkedInPostID", "LinkedInPostLastModified", "LinkedInPostGUID", null, null, null, "LinkedInPostSiteID", "LinkedInPostLinkedInAccountID", LinkedInAccountInfo.OBJECT_TYPE)
        {
            TouchCacheDependencies = true,
            SupportsCloning = false,
            LogEvents = true,
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None,
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
            },
            ImportExportSettings =
            {
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
            },

            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("LinkedInPostCampaignID", CampaignInfo.OBJECT_TYPE, ObjectDependencyEnum.NotRequired),
            },
            Feature = FeatureEnum.SocialMarketing,
            ModuleName = ModuleName.SOCIALMARKETING,
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the comment to be published as a company share.
        /// </summary>
        [DatabaseField]
        public virtual string LinkedInPostComment
        {
            get
            {
                return ValidationHelper.GetString(GetValue("LinkedInPostComment"), String.Empty);
            }
            set
            {
                SetValue("LinkedInPostComment", value);
            }
        }


        /// <summary>
        /// Gets or sets the type of the URL shortener that will be used to shorten links in this LinkedIn post.
        /// </summary>
        [DatabaseField]
        public virtual URLShortenerTypeEnum LinkedInPostURLShortenerType
        {
            get
            {
                return (URLShortenerTypeEnum)ValidationHelper.GetInteger(GetValue("LinkedInPostURLShortenerType"), 0);
            }
            set
            {
                SetValue("LinkedInPostURLShortenerType", (int)value);
        }
        }


        /// <summary>
        /// Gets or sets the date and time when the LinkedIn post (company share) is schleduled for publishing on LinkedIn account (company profile).
        /// If not set or in the past, the post gets published immediately.
        /// </summary>
        [DatabaseField]
        public virtual DateTime? LinkedInPostScheduledPublishDateTime
        {
            get
            {
                DateTime? scheduledPublish = GetDateTimeValue("LinkedInPostScheduledPublishDateTime", DateTimeHelper.ZERO_TIME);
                if (scheduledPublish == DateTimeHelper.ZERO_TIME)
                {
                    scheduledPublish = null;
                }
                return scheduledPublish;
            }
            set
            {
                DateTime? scheduledPublish = value;
                if (scheduledPublish.HasValue && (scheduledPublish.Value == DateTimeHelper.ZERO_TIME))
                {
                    scheduledPublish = null;
                }
                SetValue("LinkedInPostScheduledPublishDateTime", scheduledPublish);
            }
        }


        /// <summary>
        /// Gets or sets the campaign ID the LinkedIn post belongs to.
        /// </summary>
        [DatabaseField]
        public virtual int? LinkedInPostCampaignID
        {
            get
            {
                return (int?)GetValue("LinkedInPostCampaignID");
            }
            set
            {
                SetValue("LinkedInPostCampaignID", value);
            }
        }


        /// <summary>
        /// Gets or sets the update key of the LinkedIn post (the LinkedIn API returns it after successful publishing).
        /// </summary>
        [DatabaseField]
        public virtual string LinkedInPostUpdateKey
        {
            get
            {
                return ValidationHelper.GetString(GetValue("LinkedInPostUpdateKey"), String.Empty);
            }
            set
            {
                SetValue("LinkedInPostUpdateKey", value, String.Empty);
            }
        }


        /// <summary>
        /// Indicates whether the post has to be posted to LinkedIn after the related document gets published.
        /// </summary>
        [DatabaseField]
        public virtual bool LinkedInPostPostAfterDocumentPublish
        {
            get
            {
                return GetBooleanValue("LinkedInPostPostAfterDocumentPublish", false);
            }
            set
            {
                SetValue("LinkedInPostPostAfterDocumentPublish", value);
            }
        }


        /// <summary>
        /// Gets or sets the document GUID the LinkedIn post belongs to.
        /// </summary>
        [DatabaseField]
        public virtual Guid? LinkedInPostDocumentGUID
        {
            get
            {
                Guid? documentGuid = GetGuidValue("LinkedInPostDocumentGUID", Guid.Empty);
                if (documentGuid == Guid.Empty)
                {
                    documentGuid = null;
                }
                return documentGuid;
            }
            set
            {
                SetValue("LinkedInPostDocumentGUID", value);
            }
        }


        /// <summary>
        /// Indicates whether the post was created by user using autopost form control or not.
        /// </summary>
        [DatabaseField]
        public virtual bool LinkedInPostIsCreatedByUser
        {
            get
            {
                return GetBooleanValue("LinkedInPostIsCreatedByUser", false);
            }
            set
            {
                SetValue("LinkedInPostIsCreatedByUser", value);
            }
        }


        /// <summary>
        /// Gets or sets the date and time when the LinkedIn post was published on LinkedIn account (company profile).
        /// </summary>
        [DatabaseField]
        public virtual DateTime? LinkedInPostPublishedDateTime
        {
            get
            {
                DateTime? published = GetDateTimeValue("LinkedInPostPublishedDateTime", DateTimeHelper.ZERO_TIME);
                if (published == DateTimeHelper.ZERO_TIME)
                {
                    published = null;
                }
                return published;
            }
            set
            {
                DateTime? published = value;
                if (published.HasValue && (published.Value == DateTimeHelper.ZERO_TIME))
                {
                    published = null;
                }
                SetValue("LinkedInPostPublishedDateTime", published);
            }
        }


        /// <summary>
        /// Gets or sets the HTTP status code returned while publishing the LinkedIn post.
        /// The HTTP status code on successful publishing is 201 (Created)
        /// </summary>
        [DatabaseField]
        public virtual int LinkedInPostHTTPStatusCode
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("LinkedInPostHTTPStatusCode"), 0);
            }
            set
            {
                SetValue("LinkedInPostHTTPStatusCode", value, 0);
            }
        }


        /// <summary>
        /// Gets or sets the error code the LinkedIn API returned in response to unsuccessful publishing in the response body.
        /// </summary>
        [DatabaseField]
        public virtual int LinkedInPostErrorCode
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("LinkedInPostErrorCode"), 0);
            }
            set
            {
                SetValue("LinkedInPostErrorCode", value, 0);
            }
        }


        /// <summary>
        /// Gets or sets the error message the LinkedIn API returned in response to unsuccessful publishing in the response body.
        /// </summary>
        [DatabaseField]
        public virtual string LinkedInPostErrorMessage
        {
            get
            {
                return ValidationHelper.GetString(GetValue("LinkedInPostErrorMessage"), String.Empty);
            }
            set
            {
                SetValue("LinkedInPostErrorMessage", value, String.Empty);
            }
        }

        #endregion


        #region "Insights properties"

        /// <summary>
        /// Gets or sets the LinkedIn post click count
        /// </summary>
        [DatabaseField]
        public virtual int LinkedInPostClickCount
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("LinkedInPostClickCount"), 0);
            }
            set
            {
                SetValue("LinkedInPostClickCount", value);
            }
        }


        /// <summary>
        /// Gets or sets the LinkedIn post comment count
        /// </summary>
        [DatabaseField]
        public virtual int LinkedInPostCommentCount
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("LinkedInPostCommentCount"), 0);
            }
            set
            {
                SetValue("LinkedInPostCommentCount", value);
            }
        }


        /// <summary>
        /// Gets or sets the LinkedIn post engagement
        /// </summary>
        [DatabaseField]
        public virtual double LinkedInPostEngagement
        {
            get
            {
                return ValidationHelper.GetDouble(GetValue("LinkedInPostEngagement"), 0);
            }
            set
            {
                SetValue("LinkedInPostEngagement", value);
            }
        }


        /// <summary>
        /// Gets or sets the LinkedIn post impression count
        /// </summary>
        [DatabaseField]
        public virtual int LinkedInPostImpressionCount
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("LinkedInPostImpressionCount"), 0);
            }
            set
            {
                SetValue("LinkedInPostImpressionCount", value);
            }
        }


        /// <summary>
        /// Gets or sets the LinkedIn post like count
        /// </summary>
        [DatabaseField]
        public virtual int LinkedInPostLikeCount
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("LinkedInPostLikeCount"), 0);
            }
            set
            {
                SetValue("LinkedInPostLikeCount", value);
            }
        }


        /// <summary>
        /// Gets or sets the LinkedIn post share count
        /// </summary>
        [DatabaseField]
        public virtual int LinkedInPostShareCount
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("LinkedInPostShareCount"), 0);
            }
            set
            {
                SetValue("LinkedInPostShareCount", value);
            }
        }


        /// <summary>
        /// Gets or sets the LinkedIn post insights last updated
        /// </summary>
        [DatabaseField]
        public virtual DateTime LinkedInPostInsightsLastUpdated
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("LinkedInPostInsightsLastUpdated"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("LinkedInPostInsightsLastUpdated", value, DateTimeHelper.ZERO_TIME);
            }
        }

        #endregion


        #region "Computed properties"

        /// <summary>
        /// Indicates whether the post is published or not.
        /// </summary>
        public virtual bool IsPublished
        {
            get
            {
                return !String.IsNullOrEmpty(LinkedInPostUpdateKey) && LinkedInPostPublishedDateTime.HasValue;
            }
        }


        /// <summary>
        /// Indicates whether the post is faulty (there was an error when publishing the post or post is scheduled and was not published at proper time) or not.
        /// </summary>
        public virtual bool IsFaulty
        {
            get
            {
                bool isDelayed = !IsPublished && LinkedInPostScheduledPublishDateTime.HasValue && (DateTime.Compare(LinkedInPostScheduledPublishDateTime.Value, DateTime.Now - LinkedInPostInfoProvider.POST_DELAY_TOLERANCE) <= 0);

                return (LinkedInPostErrorCode != 0) || (LinkedInPostHTTPStatusCode != 201 && LinkedInPostHTTPStatusCode != 0) || isDelayed;
            }
        }


        /// <summary>
        /// Indicates whether the post can be edited or not. Faulty or published posts cannot be edited.
        /// </summary>
        public virtual bool IsEditable
        {
            get
            {
                return !IsPublished && !IsFaulty;
            }
        }
        
        #endregion


        #region "System properties"

        /// <summary>
        /// Gets or sets the identifier of the LinkedIn post (company share).
        /// </summary>
        [DatabaseField]
        public virtual int LinkedInPostID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("LinkedInPostID"), 0);
            }
            set
            {
                SetValue("LinkedInPostID", value);
            }
        }


        /// <summary>
        /// Gets or sets the LinkedIn account (company profile) on which the post is to be published.
        /// </summary>
        [DatabaseField]
        public virtual int LinkedInPostLinkedInAccountID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("LinkedInPostLinkedInAccountID"), 0);
            }
            set
            {
                SetValue("LinkedInPostLinkedInAccountID", value);
            }
        }


        /// <summary>
        /// Gets or sets the timestamp of the LinkedIn post.
        /// </summary>
        [DatabaseField]
        public virtual DateTime LinkedInPostLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("LinkedInPostLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("LinkedInPostLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Gets or sets the globally unique identifier of the LinkedIn post.
        /// </summary>
        [DatabaseField]
        public virtual Guid LinkedInPostGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("LinkedInPostGUID"), Guid.Empty);
            }
            set
            {
                SetValue("LinkedInPostGUID", value);
            }
        }


        /// <summary>
        /// Gets or sets the site identifier of the LinkedIn post.
        /// </summary>
        [DatabaseField]
        public virtual int LinkedInPostSiteID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("LinkedInPostSiteID"), 0);
            }
            set
            {
                SetValue("LinkedInPostSiteID", value);
            }
        }


        /// <summary>
        /// Gets or sets the display name of the LinkedIn post.
        /// </summary>
        protected override string ObjectDisplayName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(LinkedInPostComment))
                {
                    return base.ObjectDisplayName;
                }
                return TextHelper.LimitLength(LinkedInPostComment, 50);
            }
            set
            {
                base.ObjectDisplayName = value;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        /// <exception cref="LinkedInPartialDeleteException">Thrown when the deleted post has already been published on LiknedIn. In such case the post is deleted locally only.</exception>
        protected override void DeleteObject()
        {
            LinkedInPostInfoProvider.DeleteLinkedInPostInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            LinkedInPostInfoProvider.SetLinkedInPostInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public LinkedInPostInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty LinkedInPostInfo object.
        /// </summary>
        public LinkedInPostInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new LinkedInPostInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public LinkedInPostInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Sets the field value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        public override bool SetValue(string columnName, object value)
        {
            if (columnName.Equals("LinkedInPostCampaignID", StringComparison.InvariantCultureIgnoreCase))
            {
                int? campaignId = (int?)value;
                if (campaignId.HasValue && campaignId <= 0)
                {
                    value = null;
                }
            }

            return base.SetValue(columnName, value);
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
            var user = (UserInfo)userInfo;

            switch (permission)
            {
                case PermissionsEnum.Read:
                    return UserInfoProvider.IsAuthorizedPerResource(ModuleName.SOCIALMARKETING, "Read", siteName, user, false);

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                case PermissionsEnum.Destroy:
                    return UserInfoProvider.IsAuthorizedPerResource(ModuleName.SOCIALMARKETING, "Modify", siteName, user, false) ||
                        UserInfoProvider.IsAuthorizedPerResource(ModuleName.SOCIALMARKETING, "ModifyPosts", siteName, user, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }
}