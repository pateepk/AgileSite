using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace CMS.Chat
{
    /// <summary>
    /// Usage of string value attribute in ChatMessageTypeEnum.
    /// </summary>
    public enum ChatMessageTypeStringValueUsageEnum
    {
        /// <summary>
        /// This string value is resolved by macro before sending to live site.
        /// </summary>
        LiveSiteMessage = 0,

        /// <summary>
        /// This string is used to display appropriate label for the message type at CMS Desk.
        /// </summary>
        CMSDeskDescription = 1,
    }


    /// <summary>
    /// Types of chat system messages.
    /// 
    /// IsResourceString is left to false in StringValueType.LiveSiteMessage, because it is resolved manually later.
    /// 
    /// [SystemMessage] attribute must be added to all system messages to distinguish them from non-system (posted by user) messages.
    /// </summary>
    public enum ChatMessageTypeEnum
    {
        /// <summary>
        /// 'Classic' message.
        /// 
        /// Non-system message doesn't have a default string representation.
        /// </summary>
        [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.CMSDeskDescription, "chat.system.cmsdesk.classicmessage", IsResourceString = true)]
        ClassicMessage = 0,

        /// <summary>
        /// 'Whisper' message.
        /// 
        /// Non-system message doesn't have a default string representation.
        /// </summary>
        [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.CMSDeskDescription, "chat.system.cmsdesk.whisper", IsResourceString = true)]
        Whisper = 1,

        /// <summary>
        /// Leave room.
        /// </summary>
        [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.CMSDeskDescription, "chat.system.cmsdesk.leaveroom", IsResourceString = true)]
        [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.LiveSiteMessage, "chat.system.userhasleftroom")]
        [SystemMessage]
        LeaveRoom = 2,

        /// <summary>
        /// Enter room.
        /// </summary>
        [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.CMSDeskDescription, "chat.system.cmsdesk.enterroom", IsResourceString = true)]
        [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.LiveSiteMessage, "chat.system.userhasenteredroom")]
        [SystemMessage]
        EnterRoom = 3,

        /// <summary>
        /// Nickname changed.
        /// </summary>
        [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.CMSDeskDescription, "chat.system.cmsdesk.changenickname", IsResourceString = true)]
        [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.LiveSiteMessage, "chat.system.userhaschangednickname")]
        [SystemMessage]
        ChangeNickname = 4,

        /// <summary>
        /// Kicked.
        /// </summary>
        [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.CMSDeskDescription, "chat.system.cmsdesk.kicked", IsResourceString = true)]
        [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.LiveSiteMessage, "chat.system.userhasbeenkickedby")]
        [SystemMessage]
        Kicked = 5,

        /// <summary>
        /// Invitation.
        /// </summary>
        [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.CMSDeskDescription, "chat.system.cmsdesk.userinvited", IsResourceString = true)]
        [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.LiveSiteMessage, "chat.system.userhasinviteduser")]
        [SystemMessage]
        UserInvited = 6,

        /// <summary>
        /// Support greeting.
        /// </summary>
        [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.CMSDeskDescription, "chat.system.cmsdesk.supportgreeting", IsResourceString = true)]
        [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.LiveSiteMessage, "chat.system.supportgreeting")]
        [SystemMessage]
        SupportGreeting = 7,

        /// <summary>
        /// LeaveRoomPermanently.
        /// </summary>
        [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.CMSDeskDescription, "chat.system.cmsdesk.permanentleaveroom", IsResourceString = true)]
        [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.LiveSiteMessage, "chat.system.userhasleftroompermanently")]
        [SystemMessage]
        LeaveRoomPermanently = 8,

        /// <summary>
        /// KickedPermanently.
        /// </summary>
        [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.CMSDeskDescription, "chat.system.cmsdesk.permanentkick", IsResourceString = true)]
        [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.LiveSiteMessage, "chat.system.userhasbeenpermanentlykickedby")]
        [SystemMessage]
        KickedPermanently = 9,

        /// <summary>
        /// Announcement.
        /// </summary>
        [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.CMSDeskDescription, "chat.system.cmsdesk.announcement", IsResourceString = true)]
        [SystemMessage]
        Announcement = 10,

        /// <summary>
        /// User declined chat request.
        /// </summary>
        [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.CMSDeskDescription, "chat.system.cmsdesk.chatrequestdeclined", IsResourceString = true)]
        [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.LiveSiteMessage, "chat.system.chatrequestdeclined")]
        [SystemMessage]
        ChatRequestDeclined = 11,
    }

    
    /// <summary>
    /// This attribute is used to distinguish system messages vs. classic (user submitted) messages.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class SystemMessageAttribute : Attribute
    {
    }


    /// <summary>
    /// ChatMessageTypeEnum extension methods.
    /// </summary>
    public static class ChatMessageTypeEnumExtensionMethods
    {
        /// <summary>
        /// Checks if this message type is system (message is system if it has SystemMessageAttribute Attribute).
        /// </summary>
        /// <param name="en">ChatMessageTypeEnum</param>
        /// <returns>True if this message type is system</returns>
        public static bool IsSystemMessage(this ChatMessageTypeEnum en)
        {
            // Get fieldinfo for this type
            FieldInfo fieldInfo = typeof(ChatMessageTypeEnum).GetField(en.ToString());

            // Return true if SystemMessageAttribute was found.
            return fieldInfo.GetCustomAttributes(typeof(SystemMessageAttribute), false).Count() >= 1;
        }
    }
}
