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

[assembly: RegisterObjectType(typeof(TwitterPostInfo), TwitterPostInfo.OBJECT_TYPE)]

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Represents a Twitter post.
    /// </summary>
    public class TwitterPostInfo : AbstractInfo<TwitterPostInfo>
    {

        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "sm.twitterpost";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(TwitterPostInfoProvider), OBJECT_TYPE, "SM.TwitterPost", "TwitterPostID", "TwitterPostLastModified", "TwitterPostGUID", null, null, null, "TwitterPostSiteID", "TwitterPostTwitterAccountID", TwitterAccountInfo.OBJECT_TYPE)
        {
            TouchCacheDependencies = true,
            SupportsCloning = false,
            LogEvents = true,
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("TwitterPostCampaignID", CampaignInfo.OBJECT_TYPE, ObjectDependencyEnum.NotRequired),
            },
            Feature = FeatureEnum.SocialMarketing
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the identifier of the Twitter account (channel) object.
        /// </summary>
        [DatabaseField]
        public virtual int TwitterPostTwitterAccountID
        {
            get
            {
                return GetIntegerValue("TwitterPostTwitterAccountID", 0);
            }
            set
            {
                SetValue("TwitterPostTwitterAccountID", value);
            }
        }


        /// <summary>
        /// Gets or sets the campaign ID the Twitter post belongs to.
        /// </summary>
        [DatabaseField]
        public virtual int? TwitterPostCampaignID
        {
            get
            {
                return (int?) GetValue("TwitterPostCampaignID");
            }
            set
            {
                SetValue("TwitterPostCampaignID", value);
            }
        }


        /// <summary>
        /// Indicates whether the post has to be posted to Twitter after the related document gets published.
        /// </summary>
        [DatabaseField]
        public virtual bool TwitterPostPostAfterDocumentPublish
        {
            get
            {
                return GetBooleanValue("TwitterPostPostAfterDocumentPublish", false);
            }
            set
            {
                SetValue("TwitterPostPostAfterDocumentPublish", value);
            }
        }


        /// <summary>
        /// Gets or sets the document GUID the Twitter post belongs to.  
        /// </summary>
        [DatabaseField]
        public virtual Guid? TwitterPostDocumentGUID
        {
            get
            {
                Guid? documentGuid = GetGuidValue("TwitterPostDocumentGUID", Guid.Empty);
                if (documentGuid == Guid.Empty)
                {
                    documentGuid = null;
                }
                return documentGuid;
            }
            set
            {
                SetValue("TwitterPostDocumentGUID", value);
            }
        }


        /// <summary>
        /// Indicates whether the tweet was created by user using autopost form control or not.
        /// </summary>
        [DatabaseField]
        public virtual bool TwitterPostIsCreatedByUser
        {
            get
            {
                return GetBooleanValue("TwitterPostIsCreatedByUser", false);
            }
            set
            {
                SetValue("TwitterPostIsCreatedByUser", value);
            }
        }
        

        /// <summary>
        /// Gets or sets the text content of the Twitter post.
        /// </summary>
        [DatabaseField]
        public virtual string TwitterPostText
        {
            get
            {
                return GetStringValue("TwitterPostText", String.Empty);
            }
            set
            {
                SetValue("TwitterPostText", value);
            }
        }


        /// <summary>
        /// Gets or sets the type of the URL shortener that will be used to shorten links in this Twitter post.
        /// </summary>
        [DatabaseField]
        public virtual URLShortenerTypeEnum TwitterPostURLShortenerType
        {
            get
            {
                return (URLShortenerTypeEnum)GetIntegerValue("TwitterPostURLShortenerType", 0);
            }
            set
            {
                SetValue("TwitterPostURLShortenerType", (int)value);
            }
        }


        /// <summary>
        /// Gets or sets the Twitter ID of the Twitter post (tweet).
        /// </summary>
        [DatabaseField]
        public virtual string TwitterPostExternalID
        {
            get
            {
                return GetStringValue("TwitterPostExternalID", String.Empty);
            }
            set
            {
                SetValue("TwitterPostExternalID", value, String.Empty);
            }
        }


        /// <summary>
        /// Gets or sets the date and time when the Twitter post was published on Twitter channel.
        /// </summary>
        [DatabaseField]
        public virtual DateTime? TwitterPostPublishedDateTime
        {
            get
            {
                DateTime? published = GetDateTimeValue("TwitterPostPublishedDateTime", DateTimeHelper.ZERO_TIME);
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
                SetValue("TwitterPostPublishedDateTime", published);
            }
        }


        /// <summary>
        /// Gets or sets the date and time when the Twitter post is scheduled for publishing on Twitter channel.
        /// </summary>
        [DatabaseField]
        public virtual DateTime? TwitterPostScheduledPublishDateTime
        {
            get
            {
                DateTime? scheduledPublish = GetDateTimeValue("TwitterPostScheduledPublishDateTime", DateTimeHelper.ZERO_TIME);
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
                SetValue("TwitterPostScheduledPublishDateTime", scheduledPublish);
            }
        }


        /// <summary>
        /// Gets or sets the code of the error that occurred while publishing the Twitter post.
        /// Positive numbers are Twitter's error codes, negative numbers are our custom error codes.
        /// <see cref="TwitterPostInfoProvider.ERROR_CODE_UNKNOWN_ERROR"/> Unexpected error that was not returned by Twitter.
        /// <see cref="TwitterPostInfoProvider.ERROR_CODE_INVALID_ACCOUNT"/> Account doesn't exist or it is not valid.
        /// <see cref="TwitterPostInfoProvider.ERROR_CODE_INVALID_APPLICATION"/> Application doesn't exist or it is not valid.
        /// <see cref="TwitterPostInfoProvider.ERROR_CODE_DOCUMENT_NOT_EXIST"/> Related document doesn't exist.
        /// </summary>
        [DatabaseField]
        public virtual int? TwitterPostErrorCode
        {
            get
            {
                int? code = GetIntegerValue("TwitterPostErrorCode", int.MinValue);
                if (code == int.MinValue)
                {
                    code = null;
                }

                return code;
            }
            set
            {
                SetValue("TwitterPostErrorCode", value);
            }
        }


        /// <summary>
        /// Gets or sets the number that indicates approximately how many times this tweet has been favored by Twitter users.
        /// </summary>
        /// <remarks>
        /// Favorites were renamed to likes in twitter application.
        /// </remarks>
        [DatabaseField]
        public virtual int TwitterPostFavorites
        {
            get
            {
                return GetIntegerValue("TwitterPostFavorites", 0);
            }
            set
            {
                SetValue("TwitterPostFavorites", value);
            }
        }


        /// <summary>
        /// Gets or sets the number of times this tweet has been retweeted. 
        /// </summary>
        [DatabaseField]
        public virtual int TwitterPostRetweets
        {
            get
            {
                return GetIntegerValue("TwitterPostRetweets", 0);
            }
            set
            {
                SetValue("TwitterPostRetweets", value);
            }
        }


        /// <summary>
        /// Gets or sets the date and time when the tweet insights were last updated.
        /// </summary>
        [DatabaseField]
        public virtual DateTime TwitterPostInsightsUpdateDateTime
        {
            get
            {
                return GetDateTimeValue("TwitterPostInsightsUpdateDateTime", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("TwitterPostInsightsUpdateDateTime", value);
            }
        }

        #endregion


        #region "System properties"

        /// <summary>
        /// Gets or sets the globally unique identifier of the Twitter post.
        /// </summary>
        [DatabaseField]
        public virtual Guid TwitterPostGUID
        {
            get
            {
                return GetGuidValue("TwitterPostGUID", Guid.Empty);
            }
            set
            {         
                SetValue("TwitterPostGUID", value);
            }
        }


        /// <summary>
        /// Gets or sets the site identifier of the Twitter post.
        /// </summary>
        [DatabaseField]
        public virtual int TwitterPostSiteID
        {
            get
            {
                return GetIntegerValue("TwitterPostSiteID", 0);
            }
            set
            {         
                SetValue("TwitterPostSiteID", value);
            }
        }


        /// <summary>
        /// Gets or sets the timestamp of the Twitter post.
        /// </summary>
        [DatabaseField]
        public virtual DateTime TwitterPostLastModified
        {
            get
            {
                return GetDateTimeValue("TwitterPostLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {         
                SetValue("TwitterPostLastModified", value);
            }
        }


        /// <summary>
        /// Gets or sets the identifier of the Twitter post.
        /// </summary>
        [DatabaseField]
        public virtual int TwitterPostID
        {
            get
            {
                return GetIntegerValue("TwitterPostID", 0);
            }
            set
            {         
                SetValue("TwitterPostID", value);
            }
        }


        /// <summary>
        /// Gets or sets the display name of the Twitter post.
        /// </summary>
        protected override string ObjectDisplayName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(TwitterPostText))
                {
                    return base.ObjectDisplayName;
                }
                return TextHelper.LimitLength(TwitterPostText, 50);
            }
            set
            {
                base.ObjectDisplayName = value;
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
                return !String.IsNullOrEmpty(TwitterPostExternalID) && TwitterPostPublishedDateTime.HasValue;
            }
        }


        /// <summary>
        /// Indicates whether the post is faulty (there was an error when publishing the post or post is scheduled and was not published at proper time) or not.
        /// </summary>
        public virtual bool IsFaulty
        {
            get
            {
                bool isDelayed = !IsPublished && TwitterPostScheduledPublishDateTime.HasValue && (DateTime.Compare(TwitterPostScheduledPublishDateTime.Value, DateTime.Now - TwitterPostInfoProvider.POST_DELAY_TOLERANCE) <= 0);
                return TwitterPostErrorCode.HasValue || isDelayed;
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


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes this object.
        /// </summary>
        protected override void DeleteObject()
        {
            TwitterPostInfoProvider.DeleteTwitterPostInfo(this);
        }


        /// <summary>
        /// Updates this object.
        /// </summary>
        protected override void SetObject()
        {
            TwitterPostInfoProvider.SetTwitterPostInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the TwitterPostInfo class.
        /// </summary>
        public TwitterPostInfo() : base(TYPEINFO)
        {

        }


        /// <summary>
        /// Initializes a new instance of the TwitterPostInfo class with the specified data.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public TwitterPostInfo(DataRow dr) : base(TYPEINFO, dr)
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
            if (columnName.Equals("TwitterPostCampaignID", StringComparison.InvariantCultureIgnoreCase))
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