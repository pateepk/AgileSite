using System;
using System.Text;

using CMS.Helpers;
using CMS.SiteProvider;
using CMS.DataEngine;

namespace CMS.Chat
{
    /// <summary>
    /// Provides values for chat settings.
    /// </summary>
    public static class ChatSettingsProvider
    {
        /// <summary>
        /// If false, anonymou are allowed to have duplicate nicknames. If true, all nicknames must be uniques.
        /// </summary>
        public static bool ForceAnonymUniqueNicknamesSetting
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSChatForceAnonymUniqueNicknames");
            }
        }


        /// <summary>
        /// Gets timeout needed to automatically logout chat user. Value is taken from settings.
        /// </summary>
        public static int UserLogoutTimeoutSetting
        {
            get
            {
                return SettingsKeyInfoProvider.GetIntValue(SiteContext.CurrentSiteName + ".CMSChatUserLogoutTimeout");
            }
        }


        /// <summary>
        /// Gets timeout needed to automatically logout user from support chat. Value is taken from settings.
        /// </summary>
        public static int SupportLogoutTimeoutSetting
        {
            get
            {
                return SettingsKeyInfoProvider.GetIntValue(SiteContext.CurrentSiteName + ".CMSChatSupportEngineerLogoutTimeout");
            }
        }


        /// <summary>
        /// Gets number of seconds needed to realease room taken by support engineer. Value is taken from settings.
        /// </summary>
        public static int SupportRoomReleaseTimeoutSetting
        {
            get
            {
                return SettingsKeyInfoProvider.GetIntValue(SiteContext.CurrentSiteName + ".CMSChatSupportTakenRoomReleaseTimeout");
            }
        }


        /// <summary>
        /// Gets prefix of guest user. Value is taken from settings.
        /// </summary>
        public static string GuestPrefixSetting
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSChatGuestPrefix");
            }
        }


        /// <summary>
        /// Gets redirect URL after join room. Value is taken from settings.
        /// </summary>
        public static string RedirectURLJoinSetting
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSChatRedirectURLJoin");
            }
        }



        /// <summary>
        /// Gets redirect URL after leave room. Value is taken from settings.
        /// </summary>
        public static string RedirectURLLeaveSetting
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSChatRedirectURLLeave");
            }
        }


        /// <summary>
        /// Gets redirect URL after logout chat. Value is taken from settings.
        /// </summary>
        public static string RedirectURLLogoutSetting
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSChatRedirectURLLogout");
            }
        }


        /// <summary>
        /// Gets redirect URL after login chat. Value is taken from settings.
        /// </summary>
        public static string RedirectURLLoginSetting
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSChatRedirectURLLogin");
            }
        }


        /// <summary>
        /// Gets true, if anonymous users are allowed in chat.
        /// </summary>
        public static bool AreAnonymsAllowedGloballySetting
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSChatAllowAnonymGlobally");
            }
        }


        /// <summary>
        /// Post message flooding protection interval. In seconds.
        /// </summary>
        public static double PostMessageFloodProtectionIntervalSetting
        {
            get
            {
                return SettingsKeyInfoProvider.GetDoubleValue(SiteContext.CurrentSiteName + ".CMSChatFloodProtectionPostMessage");
            }
        }


        /// <summary>
        /// Create room flooding protection interval. In seconds.
        /// </summary>
        public static double CreateRoomFloodProtectionIntervalSetting
        {
            get
            {
                return SettingsKeyInfoProvider.GetDoubleValue(SiteContext.CurrentSiteName + ".CMSChatFloodProtectionCreateRoom");
            }
        }


        /// <summary>
        /// Join room flooding protection interval. In seconds.
        /// </summary>
        public static double JoinRoomFloodProtectionIntervalSetting
        {
            get
            {
                return SettingsKeyInfoProvider.GetDoubleValue(SiteContext.CurrentSiteName + ".CMSChatFloodProtectionJoinRoom");
            }
        }


        /// <summary>
        /// Change nickname flooding protection interval. In seconds.
        /// </summary>
        public static double ChangeNicknameFloodProtectionIntervalSetting
        {
            get
            {
                return SettingsKeyInfoProvider.GetDoubleValue(SiteContext.CurrentSiteName + ".CMSChatFloodProtectionChangeNickname");
            }
        }


        /// <summary>
        /// Kick lasting interval.
        /// </summary>
        public static int KickLastingIntervalSetting
        {
            get
            {
                return Math.Max(0, SettingsKeyInfoProvider.GetIntValue(SiteContext.CurrentSiteName + ".CMSChatKickLastingTime"));
            }
        }


        /// <summary>
        /// Kick lasting interval.
        /// </summary>
        public static int MaximumMessageLengthSetting
        {
            get
            {
                int val = SettingsKeyInfoProvider.GetIntValue(SiteContext.CurrentSiteName + ".CMSChatMaximumMessageLength");
                return val > 0 ? val : int.MaxValue;
            }
        }


        /// <summary>
        /// Gets true if flooding protection is enabled.
        /// </summary>
        public static bool IsFloodingProtectionEnabledSetting
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSChatEnableFloodProtection");
            }
        }


        /// <summary>
        /// Count of messages loaded on first load.
        /// </summary>
        public static int FirstLoadMessagesCountSetting
        {
            get
            {
                int val = SettingsKeyInfoProvider.GetIntValue(SiteContext.CurrentSiteName + ".CMSChatFirstLoadMessagesCount");
                return val >= 0 ? val : int.MaxValue;
            }
        }



        /// <summary>
        /// Global ping (method Ping() on service) interval.
        /// </summary>
        public static int GlobalPingIntervalSetting
        {
            get
            {
                return SettingsKeyInfoProvider.GetIntValue(SiteContext.CurrentSiteName + ".CMSChatGlobalPingInterval");
            }
        }


        /// <summary>
        /// Room ping interval.
        /// </summary>
        public static int RoomPingIntervalSetting
        {
            get
            {
                return SettingsKeyInfoProvider.GetIntValue(SiteContext.CurrentSiteName + ".CMSChatRoomPingInterval");
            }
        }


        /// <summary>
        /// Items per page default setting.
        /// </summary>
        public static int WPPagingItems
        {
            get
            {
                return SettingsKeyInfoProvider.GetIntValue(SiteContext.CurrentSiteName + ".CMSChatWPDefaultPagingItems");
            }
        }


        /// <summary>
        /// Number of pages in one group default setting.
        /// </summary>
        public static int WPGroupPagesBy
        {
            get
            {
                return SettingsKeyInfoProvider.GetIntValue(SiteContext.CurrentSiteName + ".CMSChatWPDefaultGroupPagesBy");
            }
        }


        /// <summary>
        /// Show filter limit default setting.
        /// </summary>
        public static int WPShowFilterLimit
        {
            get
            {
                return SettingsKeyInfoProvider.GetIntValue(SiteContext.CurrentSiteName + ".CMSChatWPDefaultShowFilterLimit");
            }
        }


        /// <summary>
        /// Search mode - response max users default limit.
        /// </summary>
        public static int WPSearchModeMaxUsers
        {
            get
            {
                return SettingsKeyInfoProvider.GetIntValue(SiteContext.CurrentSiteName + ".CMSChatWPDefaultSearchOnlineMaxUsers");
            }
        }


        /// <summary>
        /// Items per page limit for webparts in invite mode.
        /// </summary>
        public static int WPInviteModePagingItems
        {
            get
            {
                return SettingsKeyInfoProvider.GetIntValue(SiteContext.CurrentSiteName + ".CMSChatWPDefaultInviteModePagingItems");
            }
        }


        /// <summary>
        /// Items per page limit for webparts in invite mode.
        /// </summary>
        public static bool WPInviteModeSearchMode
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSChatWPDefaultInviteModeSearchMode");
            }
        }


        /// <summary>
        /// Enabled BB Code globally setting.
        /// </summary>
        public static bool EnableBBCodeSetting
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSChatEnableBBCode");
            }
        }


        /// <summary>
        /// Enabled URL resolving in messages setting.
        /// </summary>
        public static bool ResolveURLEnabled
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSChatResolveURLEnabled");
            }
        }


        /// <summary>
        /// Enabled smileys globally setting.
        /// </summary>
        public static bool EnableSmileys
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSChatEnableSmileys");
            }
        }


        /// <summary>
        /// Enabled sound in live chat setting.
        /// </summary>
        public static bool EnableSoundLiveChat
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSChatEnableSoundLiveChat");
            }
        }


        /// <summary>
        /// Enabled sound in support chat setting.
        /// </summary>
        public static bool EnableSoundSupportChat
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSChatEnableSoundSupportChat");
            }
        }


        /// <summary>
        /// Days needed to delete old chat records.
        /// </summary>
        public static int DaysNeededToDeleteRecordsSetting
        {
            get
            {
                return SettingsKeyInfoProvider.GetIntValue(SiteContext.CurrentSiteName + ".CMSChatDaysNeededToDeleteRecods");
            }
        }


        /// <summary>
        /// Gets default messages transformation name. Value is taken from settings.
        /// </summary>
        public static string TransformationRoomMessages
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSChatRoomMessageTrans");
            }
        }


        /// <summary>
        /// Gets default room users transformation name. Value is taken from settings.
        /// </summary>
        public static string TransformationRoomUsers
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSChatRoomUserTrans");
            }
        }


        /// <summary>
        /// Gets default room name transformation name. Value is taken from settings.
        /// </summary>
        public static string TransformationRoomName
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSChatRoomNameTrans");
            }
        }


        /// <summary>
        /// Gets default rooms transformation name. Value is taken from settings.
        /// </summary>
        public static string TransformationRooms
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSChatRoomTrans");
            }
        }


        /// <summary>
        /// Gets default notification transformation name. Value is taken from settings.
        /// </summary>
        public static string TransformationNotifications
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSChatNotificationTrans");
            }
        }


        /// <summary>
        /// Gets default online users transformation name. Value is taken from settings.
        /// </summary>
        public static string TransformationOnlineUsers
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSChatOnlineUserTrans");
            }
        }


        /// <summary>
        /// Gets default errors transformation name. Value is taken from settings.
        /// </summary>
        public static string TransformationErrors
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSChatErrorTrans");
            }
        }


        /// <summary>
        /// Gets default errors delete all transformation name. Value is taken from settings.
        /// </summary>
        public static string TransformationErrorsDeleteAll
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSChatErrorDeleteAllTrans");
            }
        }


        /// <summary>
        /// Gets default support request transformation. Value is taken from settings.
        /// </summary>
        public static string TransformationSupportRequest
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSChatSupportRequestTrans");
            }
        }


        /// <summary>
        /// Gets default initiated chat transformation. Value is taken from settings.
        /// </summary>
        public static string TransformationInitiatedChat
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSChatInitiatedChatTrans");
            }
        }

        
        /// <summary>
        /// Gets true if support chat is enabled.
        /// </summary>
        public static bool IsSupportEnabledSetting
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSChatSupportEnabled");
            }
        }


        /// <summary>
        /// Gets the email address where support messages are sent to.
        /// </summary>
        public static string SupportMessageSendToSetting
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSChatSendSupportMessagesTo");
            }
        }

       
        /// <summary>
        /// Gets address of support chat popup window opened from Desk.
        /// </summary>
        public static string ChatAdministrationPopupWindowUrl
        {
            get
            {
                return URLHelper.GetAbsoluteUrl("~/CMSModules/Chat/Pages/ChatSupportWindow/ChatRoomWindow.aspx");
            }
        }


        /// <summary>
        /// Returns chat room window url from global chat settings
        /// </summary>
        public static string ChatRoomPopupWindowUrl
        {
            get
            {
                return URLHelper.GetAbsoluteUrl(SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSChatRoomPopupWindow"));
            }
        }

        /// <summary>
        /// Gets address of offline support form. This is a form which is used to send support requests to mail.
        /// </summary>
        public static string SupportMailDialogURL
        {
            get
            {
                return URLHelper.GetAbsoluteUrl("~/CMSModules/Chat/CMSPages/OfflineSupportForm.aspx");
            }
        }


        /// <summary>
        /// Gets true if sending support requests to mail is enabled and addresses (from and to) are valid emails.
        /// 
        /// Only lenght of those addresses are checked because validity is handled by Settings validation regex in Site manager.
        /// </summary>
        public static bool IsSupportMailEnabledAndValid
        {
            get
            {
                return (SupportMessageSendToSetting.Length > 0);
            }
        }
    }
}
