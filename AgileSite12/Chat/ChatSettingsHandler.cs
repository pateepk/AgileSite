using System;
using System.Web;

using CMS.Core;
using CMS.Helpers;

using Newtonsoft.Json;

namespace CMS.Chat
{
    /// <summary>
    /// ASP.NET handler for generating chat settings.
    /// </summary>
    public class ChatSettingsHandler : IHttpHandler
    {
        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements
        /// the System.Web.IHttpHandler interface.
        /// </summary>
        /// <param name="context">
        /// An System.Web.HttpContext object that provides references to the intrinsic
        /// server objects (for example, Request, Response, Session, and Server) used
        /// to service HTTP requests.
        /// </param>
        public void ProcessRequest(HttpContext context)
        {
            var cookieLevelProvider = Service.Resolve<ICurrentCookieLevelProvider>();

            int kickInterval = ChatSettingsProvider.KickLastingIntervalSetting / 60;
            string kickIntervalText = String.Format("{0} {1}", kickInterval, (kickInterval == 1) ? ResHelper.GetString("chat.minute") : ResHelper.GetString("chat.minutes"));

            string json = JsonConvert.SerializeObject(
                new
                {
                    AnonymsAllowedGlobally = ChatSettingsProvider.AreAnonymsAllowedGloballySetting,
                    KickLastingInterval = ChatSettingsProvider.KickLastingIntervalSetting,
                    KickMessage = ResHelper.GetString("chat.errormessage.kickedfromroom"),
                    MaximumMessageLength = ChatSettingsProvider.MaximumMessageLengthSetting,
                    FirstLoadMessagesCount = ChatSettingsProvider.FirstLoadMessagesCountSetting,
                    AnonymsNotAllowedGlobalyMsg = ResHelper.GetString("chat.settings.anonymsnotallowedmsg"),
                    UnknownErrorMsg = ResHelper.GetString("chat.settings.unknownerrormsg"),
                    StoppingChatErrorMsg = ResHelper.GetString("chat.settings.stoppingchaterrormsg"),
                    NotJoinedInRoomErrLink = ResHelper.GetString("chat.errormessage.notjoinedinaroomlink"),
                    GlobalPingInterval = ChatSettingsProvider.GlobalPingIntervalSetting,
                    RoomPingInterval = ChatSettingsProvider.RoomPingIntervalSetting,
                    UserLogoutTimeout = ChatSettingsProvider.UserLogoutTimeoutSetting * 1000,
                    GuestPrefix = ChatSettingsProvider.GuestPrefixSetting,
                    ChatSettingsProvider.WPPagingItems,
                    ChatSettingsProvider.WPGroupPagesBy,
                    WPFilterLimit = ChatSettingsProvider.WPShowFilterLimit,
                    RedirectURLLeave = ChatHelper.GetDocumentAbsoluteUrl(ChatSettingsProvider.RedirectURLLeaveSetting),
                    RedirectURLJoin = ChatHelper.GetDocumentAbsoluteUrl(ChatSettingsProvider.RedirectURLJoinSetting),
                    RedirectURLLogout = ChatHelper.GetDocumentAbsoluteUrl(ChatSettingsProvider.RedirectURLLogoutSetting),
                    Debug = false,
                    SupportMailDialogURL = URLHelper.GetAbsoluteUrl("~/CMSModules/Chat/CMSPages/OfflineSupportForm.aspx"),
                    SupportMailDialogSettings = "width=670,height=350,location=0,scrollbars=1",
                    ChatSettingsProvider.EnableSmileys,
                    SmileysPath = URLHelper.GetAbsoluteUrl("~/App_Themes/Design/Chat/Smiley/"),
                    PopupWindowURL = ChatSettingsProvider.ChatRoomPopupWindowUrl,
                    PopupWindowURLAdministration = ChatSettingsProvider.ChatAdministrationPopupWindowUrl,
                    PopupWindowConfirmation = ResHelper.GetString("chat.rooms.openpopup"),
                    NotificationKickInterval = kickIntervalText,
                    ChatSettingsProvider.ResolveURLEnabled,
                    CurrentCookieLevel = cookieLevelProvider.GetCurrentCookieLevel(),
                    PopupWindowErrorMsg = ResHelper.GetString("chat.settings.popupwindowerrormsg"),
                    IsLiveSite = true,
                    ChatServiceUrl = URLHelper.GetAbsoluteUrl("~/CMSModules/Chat/Services/ChatService.svc")
                },
                new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeHtml }
            );

            context.Response.ContentType = "application/javascript";
            context.Response.Write(String.Format("var ChatSettings = {0};" , json));
        }

        /// <summary>
        /// Gets a value indicating whether another request can use the System.Web.IHttpHandler instance.
        ///
        /// True if the System.Web.IHttpHandler instance is reusable; otherwise, false.
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }

}
