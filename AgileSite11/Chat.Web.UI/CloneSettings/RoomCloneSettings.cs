using CMS.Chat;
using CMS.Chat.Web.UI;
using CMS.UIControls;

[assembly: RegisterCustomCloneSettingsControl(ChatRoomInfo.OBJECT_TYPE, typeof(RoomCloneSettings))]

namespace CMS.Chat.Web.UI
{
    internal class RoomCloneSettings : CloneSettingsControl
    {
        public override string CloseScript
        {
            get { return "wopener.RefreshUsingPostBack(); CloseDialog();"; }
        }


        public override bool DisplayControl
        {
            get { return false; }
        }

    }
}
