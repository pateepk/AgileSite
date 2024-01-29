using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Forums;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

[assembly: RegisterObjectType(typeof(ForumGroupInfo), ForumGroupInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(ForumGroupInfo), ForumGroupInfo.OBJECT_TYPE_GROUP)]

namespace CMS.Forums
{
    /// <summary>
    /// ForumGroupInfo data container class.
    /// </summary>
    public class ForumGroupInfo : AbstractInfo<ForumGroupInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "forums.forumgroup";

        /// <summary>
        /// Object type for group
        /// </summary>
        public const string OBJECT_TYPE_GROUP = "forums.groupforumgroup";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ForumGroupInfoProvider), OBJECT_TYPE, "Forums.ForumGroup", "GroupID", "GroupLastModified", "GroupGUID", "GroupName", "GroupDisplayName", null, "GroupSiteID", "GroupGroupID", null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, SOCIALANDCOMMUNITY),
                }
            },
            TouchCacheDependencies = true,
            LogEvents = true,
            GroupIDColumn = "GroupGroupID",
            ModuleName = ModuleName.FORUMS,
            ImportExportSettings =
            {
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, SOCIALANDCOMMUNITY),
                },
            },
            OrderColumn = "GroupOrder",
            TypeCondition = new TypeCondition().WhereIsNull("GroupGroupID"),
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };


        /// <summary>
        /// Type information for group forum group.
        /// </summary>
        public static ObjectTypeInfo TYPEINFOGROUP = new ObjectTypeInfo(typeof(ForumGroupInfoProvider), OBJECT_TYPE_GROUP, "Forums.ForumGroup", "GroupID", "GroupLastModified", "GroupGUID", "GroupName", "GroupDisplayName", null, "GroupSiteID", "GroupGroupID", PredefinedObjectType.GROUP)
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
            ModuleName = ModuleName.FORUMS,
            GroupIDColumn = "GroupGroupID",
            OrderColumn = "GroupOrder",
            ImportExportSettings = { AllowSingleExport = false, LogExport = true },
            TypeCondition = new TypeCondition().WhereIsNotNull("GroupGroupID"),
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the max file size for forum attachment, 0 = unlimited.
        /// </summary>
        public int GroupAttachmentMaxFileSize
        {
            get
            {
                return GetIntegerValue("GroupAttachmentMaxFileSize", 0);
            }
            set
            {
                SetValue("GroupAttachmentMaxFileSize", value);
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the user has to enter e-mail when inserting new post.
        /// </summary>
        public virtual bool GroupRequireEmail
        {
            get
            {
                return GetBooleanValue("GroupRequireEmail", false);
            }
            set
            {
                SetValue("GroupRequireEmail", value);
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the user e-mail is displayed in Group post.
        /// </summary>
        public virtual bool GroupDisplayEmails
        {
            get
            {
                return GetBooleanValue("GroupDisplayEmails", false);
            }
            set
            {
                SetValue("GroupDisplayEmails", value);
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the group forums use CAPTCHA for new posts.
        /// </summary>
        public virtual bool GroupUseCAPTCHA
        {
            get
            {
                return GetBooleanValue("GroupUseCAPTCHA", false);
            }
            set
            {
                SetValue("GroupUseCAPTCHA", value);
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the group forums use HTML editor to edit posts.
        /// </summary>
        public virtual bool GroupHTMLEditor
        {
            get
            {
                return GetBooleanValue("GroupHTMLEditor", false);
            }
            set
            {
                SetValue("GroupHTMLEditor", value);
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the author of the post can edit his own posts.
        /// </summary>
        public bool GroupAuthorEdit
        {
            get
            {
                return GetBooleanValue("GroupAuthorEdit", false);
            }
            set
            {
                SetValue("GroupAuthorEdit", value);
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the author of the post can delete his own posts.
        /// </summary>
        public bool GroupAuthorDelete
        {
            get
            {
                return GetBooleanValue("GroupAuthorDelete", false);
            }
            set
            {
                SetValue("GroupAuthorDelete", value);
            }
        }


        /// <summary>
        /// Gets or sets the type of the forum.
        /// 0 = User can choose whether post is question 
        /// 1 = Discussion forum (default) 
        /// 2 = Answer forum
        /// </summary>
        public virtual int GroupType
        {
            get
            {
                return GetIntegerValue("GroupType", 1);
            }
            set
            {
                SetValue("GroupType", value);
            }
        }


        /// <summary>
        /// Gets or sets the limit of the votes to mark the post as an answer.
        /// </summary>
        public virtual int GroupIsAnswerLimit
        {
            get
            {
                return GetIntegerValue("GroupIsAnswerLimit", 5);
            }
            set
            {
                SetValue("GroupIsAnswerLimit", value);
            }
        }


        /// <summary>
        /// Gets or sets the maximal allowed side size of the image (larger images are resized to this size).
        /// </summary>
        public virtual int GroupImageMaxSideSize
        {
            get
            {
                return GetIntegerValue("GroupImageMaxSideSize", 400);
            }
            set
            {
                SetValue("GroupImageMaxSideSize", value);
            }
        }


        /// <summary>
        /// Returns inherited base URL from settings.
        /// </summary>
        private string InheritedBaseUrl
        {
            get
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(GroupSiteID);
                if (si != null)
                {
                    return ValidationHelper.GetString(SettingsKeyInfoProvider.GetValue(si.SiteName + ".CMSForumBaseUrl"), "");
                }
                return String.Empty;
            }
        }


        /// <summary>
        /// Forum group base URL.
        /// </summary>
        public virtual string GroupBaseUrl
        {
            get
            {
                return GetStringValue("GroupBaseUrl", InheritedBaseUrl);
            }
            set
            {
                SetValue("GroupBaseUrl", value);
            }
        }


        /// <summary>
        /// Returns inherited unsubscription URL from settings.
        /// </summary>
        private string InheritedUnsubscriptionUrl
        {
            get
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(GroupSiteID);
                if (si != null)
                {
                    return ValidationHelper.GetString(SettingsKeyInfoProvider.GetValue(si.SiteName + ".CMSForumUnsubscriptionUrl"), "");
                }
                return String.Empty;
            }
        }


        /// <summary>
        /// Forum group unsubscription URL.
        /// </summary>
        public virtual string GroupUnsubscriptionUrl
        {
            get
            {
                return GetStringValue("GroupUnsubscriptionUrl", InheritedUnsubscriptionUrl);
            }
            set
            {
                SetValue("GroupUnsubscriptionUrl", value);
            }
        }


        /// <summary>
        /// Group description.
        /// </summary>
        public virtual string GroupDescription
        {
            get
            {
                return GetStringValue("GroupDescription", "");
            }
            set
            {
                SetValue("GroupDescription", value);
            }
        }


        /// <summary>
        /// Group site ID.
        /// </summary>
        public virtual int GroupSiteID
        {
            get
            {
                return GetIntegerValue("GroupSiteID", 0);
            }
            set
            {
                SetValue("GroupSiteID", value);
            }
        }


        /// <summary>
        /// Group ordinal number.
        /// </summary>
        public virtual int GroupOrder
        {
            get
            {
                return GetIntegerValue("GroupOrder", 0);
            }
            set
            {
                SetValue("GroupOrder", value, 0);
            }
        }


        /// <summary>
        /// Group display name.
        /// </summary>
        public virtual string GroupDisplayName
        {
            get
            {
                return GetStringValue("GroupDisplayName", "");
            }
            set
            {
                SetValue("GroupDisplayName", value);
            }
        }


        /// <summary>
        /// Group name.
        /// </summary>
        public virtual string GroupName
        {
            get
            {
                return GetStringValue("GroupName", "");
            }
            set
            {
                SetValue("GroupName", value);
            }
        }


        /// <summary>
        /// Group ID.
        /// </summary>
        public virtual int GroupID
        {
            get
            {
                return GetIntegerValue("GroupID", 0);
            }
            set
            {
                SetValue("GroupID", value);
            }
        }


        /// <summary>
        /// Group GUID.
        /// </summary>
        public virtual Guid GroupGUID
        {
            get
            {
                return GetGuidValue("GroupGUID", Guid.Empty);
            }
            set
            {
                SetValue("GroupGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime GroupLastModified
        {
            get
            {
                return GetDateTimeValue("GroupLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("GroupLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Community group id.
        /// </summary>
        public virtual int GroupGroupID
        {
            get
            {
                return GetIntegerValue("GroupGroupID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("GroupGroupID", null);
                }
                else
                {
                    SetValue("GroupGroupID", value);
                }
            }
        }


        /// <summary>
        /// Log activity.
        /// </summary>
        public virtual bool GroupLogActivity
        {
            get
            {
                return GetBooleanValue("GroupLogActivity", false);
            }
            set
            {
                SetValue("GroupLogActivity", value);
            }
        }


        /// <summary>
        /// Returns inherited value indicating if the forum group should use double opt-in from settings.
        /// </summary>
        private bool InheritedEnableOptIn
        {
            get
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(GroupSiteID);
                if (si != null)
                {
                    return ValidationHelper.GetBoolean(SettingsKeyInfoProvider.GetBoolValue(si.SiteName + ".CMSForumEnableOptIn"), false);
                }
                return false;
            }
        }


        /// <summary>
        /// Gets or sets whether the forum group should use double opt-in.
        /// </summary>
        public virtual bool GroupEnableOptIn
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("GroupEnableOptIn"), InheritedEnableOptIn);
            }
            set
            {
                SetValue("GroupEnableOptIn", value);
            }
        }


        /// <summary>
        /// Returns inherited value indicating whether subscription confirmation should be sent after double opt-in e-mail from settings.
        /// </summary>
        private bool InheritedSendOptInConfirmation
        {
            get
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(GroupSiteID);
                if (si != null)
                {
                    return ValidationHelper.GetBoolean(SettingsKeyInfoProvider.GetBoolValue(si.SiteName + ".CMSForumEnableOptInConfirmation"), false);
                }
                return false;
            }
        }


        /// <summary>
        /// Gets or sets whether subscription confirmation should be sent after double opt-in e-mail.
        /// </summary>
        public virtual bool GroupSendOptInConfirmation
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("GroupSendOptInConfirmation"), InheritedSendOptInConfirmation);
            }
            set
            {
                SetValue("GroupSendOptInConfirmation", value);
            }
        }


        /// <summary>
        /// Returns inherited URL of the double opt-in page from settings.
        /// </summary>
        private string InheritedOptInApprovalURL
        {
            get
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(GroupSiteID);
                if (si != null)
                {
                    return ValidationHelper.GetString(SettingsKeyInfoProvider.GetValue(si.SiteName + ".CMSForumOptInApprovalPath"), "");
                }
                return String.Empty;
            }
        }


        /// <summary>
        /// Gets or sets the URL of the double opt-in page.
        /// </summary>
        public virtual string GroupOptInApprovalURL
        {
            get
            {
                return ValidationHelper.GetString(GetValue("GroupOptInApprovalURL"), InheritedOptInApprovalURL);
            }
            set
            {
                SetValue("GroupOptInApprovalURL", value);
            }
        }


        #region "Discussion properties"

        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert URL macros into the post text.
        /// </summary>
        public bool GroupEnableURL
        {
            get
            {
                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.InsertURL);
            }
            set
            {
                SetValue("GroupDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.InsertURL, value));
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert URL macros into the post text using the advanced dialog.
        /// </summary>
        public bool GroupEnableAdvancedURL
        {
            get
            {
                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.InsertAdvancedURL);
            }
            set
            {
                SetValue("GroupDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.InsertAdvancedURL, value));
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert Quote macros into the post text.
        /// </summary>
        public bool GroupEnableQuote
        {
            get
            {
                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.InsertQuote);
            }
            set
            {
                SetValue("GroupDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.InsertQuote, value));
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert Image macros into the post text.
        /// </summary>
        public bool GroupEnableImage
        {
            get
            {
                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.InsertImage);
            }
            set
            {
                SetValue("GroupDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.InsertImage, value));
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert Image macros into the post text using the advanced dialog.
        /// </summary>
        public bool GroupEnableAdvancedImage
        {
            get
            {
                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.InsertAdvancedImage);
            }
            set
            {
                SetValue("GroupDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.InsertAdvancedImage, value));
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert bold font macros into the post text.
        /// </summary>
        public bool GroupEnableFontBold
        {
            get
            {
                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.FontBold);
            }
            set
            {
                SetValue("GroupDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.FontBold, value));
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert italics font macros into the post text.
        /// </summary>
        public bool GroupEnableFontItalics
        {
            get
            {
                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.FontItalics);
            }
            set
            {
                SetValue("GroupDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.FontItalics, value));
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert underline font macros into the post text.
        /// </summary>
        public bool GroupEnableFontUnderline
        {
            get
            {
                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.FontUnderline);
            }
            set
            {
                SetValue("GroupDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.FontUnderline, value));
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert strike font macros into the post text.
        /// </summary>
        public bool GroupEnableFontStrike
        {
            get
            {
                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.FontStrike);
            }
            set
            {
                SetValue("GroupDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.FontStrike, value));
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert code snippet macros into the post text.
        /// </summary>
        public bool GroupEnableCodeSnippet
        {
            get
            {
                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.InsertCode);
            }
            set
            {
                SetValue("GroupDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.InsertCode, value));
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the forum users will be able to insert font color macros into the post text.
        /// </summary>
        public bool GroupEnableFontColor
        {
            get
            {
                return DiscussionMacroResolver.IsBBCodeEnabled(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.FontColor);
            }
            set
            {
                SetValue("GroupDiscussionActions", DiscussionMacroResolver.SetBBCode(ValidationHelper.GetInteger(GetValue("GroupDiscussionActions"), 0), DiscussionActionEnum.FontColor, value));
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
                if (GroupGroupID == 0)
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
            ForumGroupInfoProvider.DeleteForumGroupInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ForumGroupInfoProvider.SetForumGroupInfo(this);
        }


        /// <summary>
        /// Checks if the object has unique code name. Returns true if the object has unique code name.
        /// </summary>
        public override bool CheckUniqueCodeName()
        {
            return !CheckUnique || CheckUniqueValues(new string[] { "GroupName", "GroupSiteID", "GroupGroupID" });
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ForumGroupInfo object.
        /// </summary>
        public ForumGroupInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ForumGroupInfo object from the given DataRow.
        /// </summary>
        public ForumGroupInfo(DataRow dr)
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
                    if (UserInfoProvider.IsAuthorizedPerResource(ModuleName.GROUPS, "Read", siteName, (UserInfo)userInfo, false) || MembershipContext.AuthenticatedUser.IsGroupMember(GroupGroupID))
                    {
                        return true;
                    }

                    break;

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Destroy:
                case PermissionsEnum.Modify:
                    if (UserInfoProvider.IsAuthorizedPerResource(ModuleName.GROUPS, "Manage", siteName, (UserInfo)userInfo, false) || MembershipContext.AuthenticatedUser.IsGroupAdministrator(GroupGroupID))
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