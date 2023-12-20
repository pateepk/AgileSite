using System;
using System.Linq;
using System.Text;

using CMS.DataEngine;

namespace CMS.Chat.Web.UI
{
    /// <summary>
    /// Base page for Chat pages where room is edited (room -> General, room -> Chat Users, messages, view)
    /// </summary>
    public abstract class CMSChatRoomPage : CMSChatPage
    {
        /// <summary>
        /// Edited room. This property has to be set by child page.
        /// Property can't be of type ChatRoomInfo, because this project can't reference Chat project (circular references and separability).
        /// </summary>
        protected BaseInfo EditedChatRoom
        {
            get
            {
                return (BaseInfo)UIContext.EditedObjectParent;
            }
        }


        /// <summary>
        /// Edited room site id
        /// </summary>
        protected int? EditedRoomSiteID
        {
            get
            {
                int siteID = EditedChatRoom.Generalized.ObjectSiteID ;

                return siteID > 0 ? (int?)siteID : null;
            }
        }


        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            if (EditedChatRoom == null)
            {
                RedirectToInformation(GetString("chat.error.internal"));
            }

            // Global
            CheckReadPermission(EditedRoomSiteID);

            // Nobody can edit whisper rooms (the same as ChatRoomInfo's IsWhisperRoom)
            if (EditedChatRoom.GetBooleanValue("ChatRoomIsOneToOne", true) && !EditedChatRoom.GetBooleanValue("ChatRoomIsSupport", false))
            {
                RedirectToAccessDenied(GetString("chat.error.canteditwhisperroom"));
            }

            base.OnInit(e);
        }


        /// <summary>
        /// Checks if user has modify permission based on site ID of room stored in EditedChatRoom.
        /// </summary>
        /// <returns>True if user has edit permissions; otherwise false</returns>
        public bool HasUserModifyPermission()
        {
            return HasUserModifyPermission(EditedRoomSiteID);
        }
    }
}
