using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Community;

[assembly: RegisterObjectType(typeof(GroupMemberListInfo), "community.groupmemberlist")]

namespace CMS.Community
{
    internal class GroupMemberListInfo : AbstractInfo<GroupMemberListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "community.groupmemberlist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, GroupMemberInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty GroupMemberListInfo object.
        /// </summary>
        public GroupMemberListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new GroupMemberListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public GroupMemberListInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the default list of column names for this class
        /// </summary>
        protected override List<string> GetColumnNames()
        {
            return CombineColumnNames(
                "UserID",
                "UserName",
                "FirstName",
                "MiddleName",
                "LastName",
                "FullName",
                "Email",
                "PreferredCultureCode",
                "PreferredUICultureCode",
                "UserEnabled",
                "UserIsExternal",
                "UserPasswordFormat",
                "UserCreated",
                "LastLogon",
                "UserStartingAliasPath",
                "UserGUID",
                "UserLastModified",
                "UserLastLogonInfo",
                "UserIsHidden",
                "UserPrivilegeLevel",
                "UserVisibility",
                "UserIsDomain",
                "UserHasAllowedCultures",
                "UserSettingsID",
                "UserNickName",
                "UserPicture",
                "UserSignature",
                "UserURLReferrer",
                "UserCampaign",
                "UserMessagingNotificationEmail",
                "UserCustomData",
                "UserRegistrationInfo",
                "UserPreferences",
                "UserActivationDate",
                "UserActivatedByUserID",
                "UserTimeZoneID",
                "UserAvatarID",
                "UserBadgeID",
                "UserShowIntroductionTile",
                "UserActivityPoints",
                "UserForumPosts",
                "UserBlogComments",
                "UserGender",
                "UserDateOfBirth",
                "UserMessageBoardPosts",
                "UserSettingsUserGUID",
                "UserSettingsUserID",
                "WindowsLiveID",
                "UserBlogPosts",
                "UserWaitingForApproval",
                "UserDialogsConfiguration",
                "UserDescription",
                "UserUsedWebParts",
                "UserUsedWidgets",
                "UserFacebookID",
                "UserAuthenticationGUID",
                "UserSkype",
                "UserIM",
                "UserPhone",
                "UserPosition",
                "UserLinkedInID",
                "UserAccountLockReason",
                "MemberID",
                "MemberGUID",
                "MemberUserID",
                "MemberGroupID",
                "MemberJoined",
                "MemberApprovedWhen",
                "MemberRejectedWhen",
                "MemberApprovedByUserID",
                "MemberComment",
                "MemberInvitedByUserID",
                "MemberStatus",
                "SiteID",
                "AvatarID",
                "AvatarGUID",
                "AvatarName"
                );
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("community.groupmember", "selectallview");
        }

        #endregion
    }
}