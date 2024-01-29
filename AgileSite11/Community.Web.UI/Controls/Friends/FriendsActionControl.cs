using System;
using System.Collections.Generic;
using System.Data;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.Messaging;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.Community.Web.UI
{
    /// <summary>
    /// Summary description for FriendsActionControl.
    /// </summary>
    public abstract class FriendsActionControl : CMSAdminEditControl
    {
        #region "Variables"

        /// <summary>
        /// Current user.
        /// </summary>
        protected CurrentUserInfo currentUser;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Indicates if automatic approvement should be performed.
        /// </summary>
        public bool AutomaticApprovment
        {
            get;
            set;
        }


        /// <summary>
        /// Gets and sets selectedFriends.
        /// </summary>
        public ICollection<int> SelectedFriends
        {
            get;
            set;
        }


        /// <summary>
        /// Gets and sets User ID.
        /// </summary>
        public int UserID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets and sets Requested User ID.
        /// </summary>
        public int RequestedUserID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets and sets whether to send mail.
        /// </summary>
        public bool SendMail
        {
            get;
            set;
        }


        /// <summary>
        /// Gets and sets whether to send message.
        /// </summary>
        public bool SendMessage
        {
            get;
            set;
        }


        /// <summary>
        /// Gets and sets action comment.
        /// </summary>
        public string Comment
        {
            get;
            set;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Performes selected action based on parameter.
        /// </summary>
        /// <param name="action">Action to perform</param>
        public string PerformAction(FriendsActionEnum action)
        {
            string errorMessage;

            try
            {
                errorMessage = ValidateAction();

                if (errorMessage == string.Empty)
                {
                    EmailTemplateInfo template = null;
                    bool sendNotification = SendMail || SendMessage;

                    if (sendNotification)
                    {
                        string templateName = "Friends.";

                        // 'Approve' e-mail template for automatic approvment
                        if (AutomaticApprovment && CanApprove() && (action == FriendsActionEnum.Request))
                        {
                            templateName += FriendsActionEnum.Approve;
                        }
                        else
                        {
                            templateName += action;
                        }

                        // Get e-mail template
                        template = EmailTemplateProvider.GetEmailTemplate(templateName, SiteContext.CurrentSiteName);

                        // Tempalte is not defined
                        if (template == null)
                        {
                            int siteId = SiteContext.CurrentSiteID;

                            EventLogProvider.LogEvent(EventType.ERROR, "Content", "MISSINGTEMPLATE", "Missing email template 'Friends." + action + "'.", null, MembershipContext.AuthenticatedUser.UserID, UserInfoProvider.GetUserName(), 0, null, RequestContext.UserHostAddress, siteId);
                        }
                    }

                    // Only one friendship is specified
                    if (RequestedUserID != 0)
                    {
                        FriendInfo fi = FriendInfoProvider.GetFriendInfo(UserID, RequestedUserID);
                        return PerformActionInternal(fi, action, sendNotification, template);
                    }
                    else if (SelectedFriends != null)
                    {
                        // Get all needed friends
                        DataSet friendships = FriendInfoProvider.GetFriends()
                            .WhereIn("FriendID", SelectedFriends);

                        if (!DataHelper.DataSourceIsEmpty(friendships))
                        {
                            // Perform action with each friendsip
                            foreach (DataRow friendship in friendships.Tables[0].Rows)
                            {
                                errorMessage = PerformActionInternal(new FriendInfo(friendship), action, sendNotification, template);
                                if (errorMessage != string.Empty)
                                {
                                    return errorMessage;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            return errorMessage;
        }


        /// <summary>
        /// Indicates if current user can automatically approve friendship.
        /// </summary>
        public bool CanApprove()
        {
            return currentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || (currentUser.IsAuthorizedPerResource("CMS.Friends", "Manage"));
        }


        /// <summary>
        /// Virtual method for get formatted user name.
        /// Override if you want to format sender and recipients usernames.
        /// </summary>
        public virtual string GetFormattedUserName(string userName, string fullName, string nickName)
        {
            throw new NotImplementedException("Override GetFormattedUserName function in child controls.");
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// OnLoad event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnLoad(EventArgs e)
        {
            currentUser = MembershipContext.AuthenticatedUser;
            ScriptHelper.RegisterWOpenerScript(Page);

            base.OnLoad(e);
        }


        private string PerformActionInternal(FriendInfo fi, FriendsActionEnum action, bool sendNotification, EmailTemplateInfo template)
        {
            if (fi == null)
            {
                if (action != FriendsActionEnum.Request)
                {
                    throw new Exception("[FriendsActionControl]: Friendship doesn't exist!");
                }
                fi = new FriendInfo();
            }

            // Validate action
            if (!CanPerformAction(action, fi))
            {
                return ResHelper.GetString("friends.cannotperform");
            }

            UserInfo recipient = null;

            fi.FriendComment = Comment;

            // Get recipient info
            switch (action)
            {
                case FriendsActionEnum.Request:
                    fi.FriendRequestedWhen = DateTime.Now;
                    fi.FriendRequestedUserID = RequestedUserID;
                    fi.FriendUserID = UserID;
                    fi.FriendRejectedBy = 0;
                    fi.FriendApprovedBy = 0;
                    fi.FriendStatus = FriendshipStatusEnum.Waiting;
                    if (sendNotification)
                    {
                        recipient = UserInfoProvider.GetFullUserInfo(RequestedUserID);
                    }

                    // Automatic approvement (check permission again)
                    if (AutomaticApprovment && CanApprove())
                    {
                        fi.FriendApprovedWhen = DateTime.Now;
                        fi.FriendApprovedBy = UserID;
                        fi.FriendStatus = FriendshipStatusEnum.Approved;
                    }
                    break;

                case FriendsActionEnum.Approve:
                    fi.FriendRejectedBy = 0;
                    fi.FriendApprovedWhen = DateTime.Now;
                    fi.FriendApprovedBy = UserID;
                    fi.FriendStatus = FriendshipStatusEnum.Approved;
                    if (sendNotification)
                    {
                        recipient = fi.FriendUserID == UserID ? UserInfoProvider.GetFullUserInfo(fi.FriendRequestedUserID) : UserInfoProvider.GetFullUserInfo(fi.FriendUserID);
                    }
                    break;

                case FriendsActionEnum.Reject:
                    fi.FriendRejectedBy = UserID;
                    fi.FriendRejectedWhen = DateTime.Now;
                    fi.FriendApprovedBy = 0;
                    fi.FriendStatus = FriendshipStatusEnum.Rejected;
                    if (sendNotification)
                    {
                        recipient = fi.FriendUserID == UserID ? UserInfoProvider.GetFullUserInfo(fi.FriendRequestedUserID) : UserInfoProvider.GetFullUserInfo(fi.FriendUserID);
                    }
                    break;
            }

            FriendInfoProvider.SetFriendInfo(fi, true);

            // Additionally send message or e-mail
            if (sendNotification && (template != null) && (recipient != null))
            {
                // Get sender info
                UserInfo sender = UserInfoProvider.GetFullUserInfo(UserID);

                string siteName = SiteContext.CurrentSiteName;
                string profileUrl = DocumentURLProvider.GetUrl(GroupMemberInfoProvider.GetMemberProfilePath(sender.UserName, siteName));
                string friendshipManagementUrl = SettingsKeyInfoProvider.GetValue(siteName + ".CMSFriendsManagementPath");

                if (string.IsNullOrEmpty(friendshipManagementUrl))
                {
                    // Get default friendship management path
                    friendshipManagementUrl = ResolveUrl("~/CMSModules/Friends/CMSPages/FriendshipManagement.aspx");
                }
                else
                {
                    friendshipManagementUrl = DocumentURLProvider.GetUrl(friendshipManagementUrl);
                }
                if (!string.IsNullOrEmpty(friendshipManagementUrl))
                {
                    friendshipManagementUrl = URLHelper.GetAbsoluteUrl(friendshipManagementUrl.Trim()) + "?friendguid=" + fi.FriendGUID;
                }
                if (!string.IsNullOrEmpty(profileUrl))
                {
                    profileUrl = URLHelper.GetAbsoluteUrl(profileUrl.Trim());
                }

                MacroResolver resolver = MacroContext.CurrentResolver.CreateChild();
                resolver.SetAnonymousSourceData(sender, recipient, fi);

                // Prioritized data sources
                resolver.SetNamedSourceData("Sender", sender);
                resolver.SetNamedSourceData("Recipient", recipient);
                resolver.SetNamedSourceData("Friendship", fi);

                // Not prioritized data sources
                resolver.SetNamedSourceData(new Dictionary<string, object>
                {
                    { "managementurl", friendshipManagementUrl },
                    { "profileurl", profileUrl },
                    { "formattedsendername", GetFormattedUserName(sender.UserName, sender.FullName, sender.UserNickName) },
                    { "formattedrecipientname", GetFormattedUserName(recipient.UserName, recipient.FullName, recipient.UserNickName) }
                }, isPrioritized: false);

                // Send message
                if (SendMessage)
                {
                    int recipientUserId = recipient.UserID;

                    // Set message info object
                    MessageInfo mi = new MessageInfo();
                    mi.MessageLastModified = DateTime.Now;
                    mi.MessageSent = DateTime.Now;
                    mi.MessageRecipientUserID = recipientUserId;
                    mi.MessageRecipientNickName = TextHelper.LimitLength(GetFormattedUserName(recipient.UserName, recipient.FullName, recipient.UserNickName), 200);
                    mi.MessageSenderUserID = UserID;
                    mi.MessageSenderNickName = TextHelper.LimitLength(GetFormattedUserName(sender.UserName, sender.FullName, sender.UserNickName), 200);
                    mi.MessageSubject = TextHelper.LimitLength(resolver.ResolveMacros(template.TemplateSubject), 200);
                    mi.MessageBody = resolver.ResolveMacros(template.TemplatePlainText);
                    // If sender is in ignore list don't display message in the recipient inbox
                    if (IgnoreListInfoProvider.IsInIgnoreList(recipientUserId, UserID))
                    {
                        mi.MessageRecipientDeleted = true;
                    }
                    MessageInfoProvider.SetMessageInfo(mi);
                }

                // Send e-mail
                if (SendMail && (recipient.Email != string.Empty) && (sender.Email != string.Empty))
                {
                    // Send e-mail
                    EmailMessage message = new EmailMessage();
                    message.Recipients = GetFormattedUserName(recipient.UserName, recipient.FullName, recipient.UserNickName) + " <" + recipient.Email + ">";
                    message.From = GetFormattedUserName(sender.UserName, sender.FullName, sender.UserNickName) + " <" + sender.Email + ">";

                    EmailSender.SendEmailWithTemplateText(siteName, message, template, resolver, false);
                }
            }
            return string.Empty;
        }


        private string ValidateAction()
        {
            if ((RequestedUserID == 0) && ((SelectedFriends == null) || (SelectedFriends.Count == 0)))
            {
                return ResHelper.GetString("friends.friendrequired");
            }
            if (UserID == 0)
            {
                return ResHelper.GetString("friends.friendrequired");
            }
            return UserID == RequestedUserID ? ResHelper.GetString("friends.cannotselectyourself") : string.Empty;
        }


        private bool CanPerformAction(FriendsActionEnum action, FriendInfo fi)
        {
            if (action == FriendsActionEnum.Request)
            {
                return true;
            }

            var currentUserCanApprove = CanApprove();
            var currentUserIsRequested = (fi.FriendRequestedUserID == currentUser.UserID);

            if (fi.FriendStatus == FriendshipStatusEnum.Waiting)
            {
                return (currentUserCanApprove || currentUserIsRequested);
            }

            var currentUserIsInFriendshipRequest = (fi.FriendUserID == currentUser.UserID) || currentUserIsRequested;

            switch (action)
            {
                case FriendsActionEnum.Approve:
                    return (currentUserCanApprove || ((fi.FriendRejectedBy == currentUser.UserID) && currentUserIsInFriendshipRequest));

                case FriendsActionEnum.Reject:
                    return (currentUserCanApprove || currentUserIsInFriendshipRequest);
            }

            return false;
        }

        #endregion
    }
}
