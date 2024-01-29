using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.WebAnalytics;
using CMS.SocialMarketing;

[assembly: RegisterObjectType(typeof(FacebookPostInfo), FacebookPostInfo.OBJECT_TYPE)]

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Represents a Facebook post.
    /// </summary>
    public class FacebookPostInfo : AbstractInfo<FacebookPostInfo>
    {

        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "sm.facebookpost";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(FacebookPostInfoProvider), OBJECT_TYPE, "SM.FacebookPost", "FacebookPostID", "FacebookPostLastModified", "FacebookPostGUID", null, null, null, "FacebookPostSiteID", "FacebookPostFacebookAccountID", FacebookAccountInfo.OBJECT_TYPE)
        {
            TouchCacheDependencies = true,
            SupportsCloning = false,
            LogEvents = true,
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None,
            },
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None },

            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("FacebookPostCampaignID", CampaignInfo.OBJECT_TYPE, ObjectDependencyEnum.NotRequired),
            },
            Feature = FeatureEnum.SocialMarketing
        };

        #endregion


        #region "Post properties"

        /// <summary>
        /// Gets or sets the identifier of the Facebook account (page) object.
        /// </summary>
        [DatabaseField]
        public virtual int FacebookPostFacebookAccountID
        {
            get
            {
                return GetIntegerValue("FacebookPostFacebookAccountID", 0);
            }
            set
            {
                SetValue("FacebookPostFacebookAccountID", value);
            }
        }


        /// <summary>
        /// Gets or sets the campaign ID the Facebook post belongs to.
        /// </summary>
        [DatabaseField]
        public virtual int? FacebookPostCampaignID
        {
            get
            {
                return (int?) GetValue("FacebookPostCampaignID");
            }
            set
            {
                SetValue("FacebookPostCampaignID", value);
            }
        }


        /// <summary>
        /// Indicates whether the post has to be posted to Facebook after the related document gets published.
        /// </summary>
        [DatabaseField]
        public virtual bool FacebookPostPostAfterDocumentPublish
        {
            get
            {
                return GetBooleanValue("FacebookPostPostAfterDocumentPublish", false);
            }
            set
            {
                SetValue("FacebookPostPostAfterDocumentPublish", value);
            }
        }


        /// <summary>
        /// Gets or sets the document GUID the Facebook post belongs to.
        /// </summary>
        [DatabaseField]
        public virtual Guid? FacebookPostDocumentGUID
        {
            get
            {
                Guid? documentGuid = GetGuidValue("FacebookPostDocumentGUID", Guid.Empty);
                if (documentGuid == Guid.Empty)
                {
                    documentGuid = null;
                }
                return documentGuid;
            }
            set
            {
                SetValue("FacebookPostDocumentGUID", value);
            }
        }


        /// <summary>
        /// Indicates whether the post was created by user using autopost form control or not.
        /// </summary>
        [DatabaseField]
        public virtual bool FacebookPostIsCreatedByUser
        {
            get
            {
                return GetBooleanValue("FacebookPostIsCreatedByUser", false);
            }
            set
            {
                SetValue("FacebookPostIsCreatedByUser", value);
            }
        }


        /// <summary>
        /// Gets or sets the text content of the Facebook post.
        /// </summary>
        [DatabaseField]
        public virtual string FacebookPostText
        {
            get
            {
                return GetStringValue("FacebookPostText", String.Empty);
            }
            set
            {
                SetValue("FacebookPostText", value);
            }
        }


        /// <summary>
        /// Gets or sets the Facebook ID of the Facebook post.
        /// </summary>
        [DatabaseField]
        public virtual string FacebookPostExternalID
        {
            get
            {
                return GetStringValue("FacebookPostExternalID", String.Empty);
            }
            set
            {
                SetValue("FacebookPostExternalID", value);
            }
        }


        /// <summary>
        /// Gets or sets the date and time when the Facebook post was published on Facebook page.
        /// </summary>
        [DatabaseField]
        public virtual DateTime? FacebookPostPublishedDateTime
        {
            get
            {
                DateTime? published = GetDateTimeValue("FacebookPostPublishedDateTime", DateTimeHelper.ZERO_TIME);
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
                SetValue("FacebookPostPublishedDateTime", published);
            }
        }


        /// <summary>
        /// Gets or sets the date and time when the Facebook post is scheduled for publishing on Facebook page.
        /// </summary>
        [DatabaseField]
        public virtual DateTime? FacebookPostScheduledPublishDateTime
        {
            get
            {
                DateTime? scheduledPublish = GetDateTimeValue("FacebookPostScheduledPublishDateTime", DateTimeHelper.ZERO_TIME);
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
                SetValue("FacebookPostScheduledPublishDateTime", scheduledPublish);
            }
        }


        /// <summary>
        /// Gets or sets the code of the error that occurred while publishing the Facebook post.
        /// Positive numbers are Facebook's error codes, negative numbers are our custom error codes.
        /// <see cref="FacebookPostInfoProvider.ERROR_CODE_UNKNOWN_ERROR"/> Unknown error.
        /// <see cref="FacebookPostInfoProvider.ERROR_CODE_INVALID_ACCOUNT"/> Account doesn't exist or it is not valid.
        /// <see cref="FacebookPostInfoProvider.ERROR_CODE_DOCUMENT_NOT_EXIST"/> Related document doesn't exist.
        /// </summary>
        [DatabaseField]
        public virtual int? FacebookPostErrorCode
        {
            get
            {
                int? code = GetIntegerValue("FacebookPostErrorCode", int.MinValue);
                if (code == int.MinValue)
                {
                    code = null;
                }

                return code;
            }
            set
            {
                SetValue("FacebookPostErrorCode", value);
            }
        }


        /// <summary>
        /// Gets or sets the subcode of the error that occurred while publishing the Facebook post.
        /// </summary>
        [DatabaseField]
        public virtual int? FacebookPostErrorSubcode
        {
            get
            {
                int? code = GetIntegerValue("FacebookPostErrorSubcode", int.MinValue);
                if (code == int.MinValue)
                {
                    code = null;
                }

                return code;
            }
            set
            {
                SetValue("FacebookPostErrorSubcode", value);
            }
        }


        /// <summary>
        /// Gets or sets the type of the URL shortener that will be used to shorten links in this Facebook post.
        /// </summary>
        [DatabaseField]
        public virtual URLShortenerTypeEnum FacebookPostURLShortenerType
        {
            get
            {
                return (URLShortenerTypeEnum)GetIntegerValue("FacebookPostURLShortenerType", 0);
            }
            set
            {
                SetValue("FacebookPostURLShortenerType", (int)value);
            }
        }

        #endregion


        #region "Insight properties"

        /// <summary>
        /// Gets or sets the number of people this post reached.
        /// </summary>
        [DatabaseField]
        public virtual int FacebookPostInsightPeopleReached
        {
            get
            {
                return GetIntegerValue("FacebookPostInsightPeopleReached", 0);
            }
            set
            {
                SetValue("FacebookPostInsightPeopleReached", value);
            }
        }


        /// <summary>
        /// Gets or sets the number of likes this post has from the page.
        /// </summary>
        [DatabaseField]
        public virtual int FacebookPostInsightLikesFromPage
        {
            get
            {
                return GetIntegerValue("FacebookPostInsightLikesFromPage", 0);
            }
            set
            {
                SetValue("FacebookPostInsightLikesFromPage", value);
            }
        }


        /// <summary>
        /// Gets or sets the number of comments this post has from the page.
        /// </summary>
        [DatabaseField]
        public virtual int FacebookPostInsightCommentsFromPage
        {
            get
            {
                return GetIntegerValue("FacebookPostInsightCommentsFromPage", 0);
            }
            set
            {
                SetValue("FacebookPostInsightCommentsFromPage", value);
            }
        }


        /// <summary>
        /// Gets or sets the number of shares this post has from the page.
        /// </summary>
        [DatabaseField]
        public virtual int FacebookPostInsightSharesFromPage
        {
            get
            {
                return GetIntegerValue("FacebookPostInsightSharesFromPage", 0);
            }
            set
            {
                SetValue("FacebookPostInsightSharesFromPage", value);
            }
        }


        /// <summary>
        /// Gets or sets the number of likes this post has from both the page and all its shares.
        /// </summary>
        [DatabaseField]
        public virtual int FacebookPostInsightLikesTotal
        {
            get
            {
                return GetIntegerValue("FacebookPostInsightLikesTotal", 0);
            }
            set
            {
                SetValue("FacebookPostInsightLikesTotal", value);
            }
        }


        /// <summary>
        /// Gets or sets the number of comments this post has from both the page and all its shares.
        /// </summary>
        [DatabaseField]
        public virtual int FacebookPostInsightCommentsTotal
        {
            get
            {
                return GetIntegerValue("FacebookPostInsightCommentsTotal", 0);
            }
            set
            {
                SetValue("FacebookPostInsightCommentsTotal", value);
            }
        }


        /// <summary>
        /// Gets or sets the number of negative actions that leaded to hide the post.
        /// </summary>
        [DatabaseField]
        public virtual int FacebookPostInsightNegativeHidePost
        {
            get
            {
                return GetIntegerValue("FacebookPostInsightNegativeHidePost", 0);
            }
            set
            {
                SetValue("FacebookPostInsightNegativeHidePost", value);
            }
        }


        /// <summary>
        /// Gets or sets the number of negative actions that leaded to hide the post.
        /// </summary>
        [DatabaseField]
        public virtual int FacebookPostInsightNegativeHideAllPosts
        {
            get
            {
                return GetIntegerValue("FacebookPostInsightNegativeHideAllPosts", 0);
            }
            set
            {
                SetValue("FacebookPostInsightNegativeHideAllPosts", value);
            }
        }


        /// <summary>
        /// Gets or sets the number of negative actions that leaded to report the post as spam.
        /// </summary>
        [DatabaseField]
        public virtual int FacebookPostInsightNegativeReportSpam
        {
            get
            {
                return GetIntegerValue("FacebookPostInsightNegativeReportSpam", 0);
            }
            set
            {
                SetValue("FacebookPostInsightNegativeReportSpam", value);
            }
        }


        /// <summary>
        /// Gets or sets the number of negative actions that leaded to unlike the page.
        /// </summary>
        [DatabaseField]
        public virtual int FacebookPostInsightNegativeUnlikePage
        {
            get
            {
                return GetIntegerValue("FacebookPostInsightNegativeUnlikePage", 0);
            }
            set
            {
                SetValue("FacebookPostInsightNegativeUnlikePage", value);
            }
        }


        /// <summary>
        /// Gets or sets date and time when post insights were successfully retrieved.
        /// </summary>
        [DatabaseField]
        public virtual DateTime FacebookPostInsightsLastUpdated
        {
            get
            {
                return GetDateTimeValue("FacebookPostInsightsLastUpdated", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("FacebookPostInsightsLastUpdated", value);
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
                return !String.IsNullOrEmpty(FacebookPostExternalID) && FacebookPostPublishedDateTime.HasValue;
            }
        }


        /// <summary>
        /// Indicates whether the post is faulty (there was an error when publishing the post or post is scheduled and was not published at proper time) or not.
        /// </summary>
        public virtual bool IsFaulty
        {
            get
            {
                bool isDelayed = !IsPublished && FacebookPostScheduledPublishDateTime.HasValue && (DateTime.Compare(FacebookPostScheduledPublishDateTime.Value, DateTime.Now - FacebookPostInfoProvider.POST_DELAY_TOLERANCE) <= 0);
                return FacebookPostErrorCode.HasValue || isDelayed;
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
        /// Gets or sets the globally unique identifier of the Facebook post.
        /// </summary>
        [DatabaseField]
        public virtual Guid FacebookPostGUID
        {
            get
            {
                return GetGuidValue("FacebookPostGUID", Guid.Empty);
            }
            set
            {         
                SetValue("FacebookPostGUID", value);
            }
        }


        /// <summary>
        /// Gets or sets the site identifier of the Facebook post.
        /// </summary>
        [DatabaseField]
        public virtual int FacebookPostSiteID
        {
            get
            {
                return GetIntegerValue("FacebookPostSiteID", 0);
            }
            set
            {         
                SetValue("FacebookPostSiteID", value);
            }
        }


        /// <summary>
        /// Gets or sets the timestamp of the Facebook post.
        /// </summary>
        [DatabaseField]
        public virtual DateTime FacebookPostLastModified
        {
            get
            {
                return GetDateTimeValue("FacebookPostLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {         
                SetValue("FacebookPostLastModified", value);
            }
        }


        /// <summary>
        /// Gets or sets the identifier of the Facebook post.
        /// </summary>
        [DatabaseField]
        public virtual int FacebookPostID
        {
            get
            {
                return GetIntegerValue("FacebookPostID", 0);
            }
            set
            {         
                SetValue("FacebookPostID", value);
            }
        }


        /// <summary>
        /// Gets or sets the display name of the Facebook post.
        /// </summary>
        protected override string ObjectDisplayName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(FacebookPostText))
                {
                    return base.ObjectDisplayName;
                }
                return TextHelper.LimitLength(FacebookPostText, 50);
            }
            set
            {
                base.ObjectDisplayName = value;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes this object.
        /// </summary>
        protected override void DeleteObject()
        {
            FacebookPostInfoProvider.DeleteFacebookPostInfo(this);
        }


        /// <summary>
        /// Updates this object.
        /// </summary>
        protected override void SetObject()
        {
            FacebookPostInfoProvider.SetFacebookPostInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the FacebookPostInfo class.
        /// </summary>
        public FacebookPostInfo() : base(TYPEINFO)
        {

        }


        /// <summary>
        /// Initializes a new instance of the FacebookPostInfo class with the specified data.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public FacebookPostInfo(DataRow dr) : base(TYPEINFO, dr)
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
            if (columnName.Equals("FacebookPostCampaignID", StringComparison.InvariantCultureIgnoreCase))
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
                    return UserInfoProvider.IsAuthorizedPerResource(ModuleName.SOCIALMARKETING, "Read", siteName, user, exceptionOnFailure);

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