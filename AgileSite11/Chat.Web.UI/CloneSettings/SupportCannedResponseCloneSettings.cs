using CMS.Chat;
using CMS.Chat.Web.UI;
using CMS.UIControls;

[assembly: RegisterCustomCloneSettingsControl(ChatSupportCannedResponseInfo.OBJECT_TYPE, typeof(SupportCannedResponseCloneSettings))]

namespace CMS.Chat.Web.UI
{
    internal class SupportCannedResponseCloneSettings : CloneSettingsControl
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
