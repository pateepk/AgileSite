using System.ComponentModel;
using System;
using System.Reflection;

using CMS.Helpers;

namespace CMS.Chat
{
    /// <summary>
    /// ChatService response codes.
    /// </summary>
    public enum ChatResponseStatusEnum
    {
        /// <summary>
        /// Ok
        /// </summary>
        [StringValue("general.ok", IsResourceString = true)]
        OK = 0,

        /// <summary>
        /// NotLoggedIn
        /// </summary>
        [StringValue("chat.errormessage.notloggedin", IsResourceString = true)]
        NotLoggedIn = 1,

        /// <summary>
        /// AccessDenied
        /// </summary>
        [StringValue("chat.errormessage.accessdenied", IsResourceString = true)]
        AccessDenied = 2,

        /// <summary>
        /// MessageCanNotBeEmpty
        /// </summary>
        [StringValue("chat.errormessage.messagecannotbeempty", IsResourceString = true)]
        MessageCanNotBeEmpty = 3,

        /// <summary>
        /// NotJoinedInARoom
        /// </summary>
        [StringValue("chat.errormessage.notjoinedinaroom", IsResourceString = true)]
        NotJoinedInARoom = 4,

        /// <summary>
        /// UnknownError
        /// </summary>
        [StringValue("chat.errormessage.unknownerror", IsResourceString = true)]
        UnknownError = 5,

        /// <summary>
        /// ChatUserNotFound
        /// </summary>
        [StringValue("chat.errormessage.chatusernotfound", IsResourceString = true)]
        ChatUserNotFound = 6,

        /// <summary>
        /// WrongSecondUser
        /// </summary>
        [StringValue("chat.errormessage.wrongseconduser", IsResourceString = true)]
        WrongSecondUser = 7,

        /// <summary>
        /// NicknameNotAvailable
        /// </summary>
        [StringValue("chat.errormessage.nicknamenotavailable", IsResourceString = true)]
        NicknameNotAvailable = 9,

        /// <summary>
        /// SupportIsNotOnline
        /// </summary>
        [StringValue("chat.errormessage.supportisnotonline", IsResourceString = true)]
        SupportIsNotOnline = 10,

        /// <summary>
        /// RoomNotFound
        /// </summary>
        [StringValue("chat.errormessage.roomnotfound", IsResourceString = true)]
        RoomNotFound = 11,

        /// <summary>
        /// WrongPassword
        /// </summary>
        [StringValue("chat.errormessage.wrongpassword", IsResourceString = true)]
        WrongPassword = 12,

        /// <summary>
        /// AnonymsDisallowed
        /// </summary>
        [StringValue("chat.errormessage.anonymsdisallowed", IsResourceString = true)]
        AnonymsDisallowed = 13,

        /// <summary>
        /// BadWordsValidationFailed
        /// </summary>
        [StringValue("chat.errormessage.badwordsvalidationfailed", IsResourceString = true)]
        BadWordsValidationFailed = 14,

        /// <summary>
        /// BadRequest
        /// </summary>
        [StringValue("chat.errormessage.badrequest", IsResourceString = true)]
        BadRequest = 15,

        /// <summary>
        /// BannedIP
        /// </summary>
        [StringValue("general.bannedip", IsResourceString = true)]
        BannedIP = 16,

        /// <summary>
        /// KickedFromRoom
        /// </summary>
        [StringValue("chat.errormessage.kickedfromroom", IsResourceString = true)]
        KickedFromRoom = 17,

        /// <summary>
        /// AnonymsDisallowedGlobally
        /// </summary>
        [StringValue("chat.errormessage.anonymsdisallowedglobally", IsResourceString = true)]
        AnonymsDisallowedGlobally = 18,

        /// <summary>
        /// Flooding
        /// </summary>
        [StringValue("chat.errormessage.flooding", IsResourceString = true)]
        Flooding = 19,

        /// <summary>
        /// MessageTooLong
        /// </summary>
        [StringValue("chat.errormessage.messagetoolong", IsResourceString = true)]
        MessageTooLong = 20,

        /// <summary>
        /// CanNotKickAdmin
        /// </summary>
        [StringValue("chat.errormessage.cannotkickadmin", IsResourceString = true)]
        CanNotKickAdmin = 22,


        /// <summary>
        /// RoomNameCantBeEmpty
        /// </summary>
        [StringValue("chat.errormessage.roomnamecantbeempty", IsResourceString = true)]
        RoomNameCantBeEmpty = 23,


        /// <summary>
        /// RoomNameTooLong
        /// </summary>
        [StringValue("chat.errormessage.roomnametoolong", IsResourceString = true)]
        RoomNameTooLong = 24,


        /// <summary>
        /// RoomPasswordTooLong
        /// </summary>
        [StringValue("chat.errormessage.roompasswordtoolong", IsResourceString = true)]
        RoomPasswordTooLong = 25,


        /// <summary>
        /// NicknameCantBeEmpty
        /// </summary>
        [StringValue("chat.errormessage.nicknamecantbeempty", IsResourceString = true)]
        NicknameCantBeEmpty = 26,


        /// <summary>
        /// NicknameTooLong
        /// </summary>
        [StringValue("chat.errormessage.nicknametoolong", IsResourceString = true)]
        NicknameTooLong = 27,


        /// <summary>
        /// NicknameCantBeginWith
        /// </summary>
        [StringValue("chat.errormessage.nicknamecantbeginwith", IsResourceString = true)]
        NicknameCantBeginWith = 28,


        /// <summary>
        /// InitiatedChatRequestAlreadyAccepted
        /// </summary>
        [StringValue("chat.errormessage.initiatedchatrequestalreadyaccepted", IsResourceString = true)]
        InitiatedChatRequestAlreadyAccepted = 29,


        /// <summary>
        /// RoomAlreadyTaken
        /// </summary>
        [StringValue("chat.errormessage.roomalreadytaken", IsResourceString = true)]
        RoomAlreadyTaken = 30,


        /// <summary>
        /// InvitationAlreadyAnswered
        /// </summary>
        [StringValue("chat.errormessage.invitationalreadyanswered", IsResourceString = true)]
        InvitationAlreadyAnswered = 31,
    }
}