using System;
using System.Text;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.UIControls;


namespace CMS.Chat.Web.UI
{
    /// <summary>
    /// Provides helper methods for registration of chat scripts.
    /// </summary>
    public class ChatScriptHelper : AbstractHelper<ChatScriptHelper>
    {
        /// <summary>
        /// Registers AJAX proxy which can be used to call ChatService.svc from javascript.
        /// </summary>
        /// <param name="page">AbstractCMSPage. Script tag will be inserted to ScriptManager on this page.</param>
        public static void RegisterChatAJAXProxy(AbstractCMSPage page)
        {
            HelperObject.RegisterChatAJAXProxyInternal(page);
        }


        /// <summary>
        /// Registers AJAX proxy which can be used to call ChatSupportService.svc from javascript.
        /// </summary>
        /// <param name="page">AbstractCMSPage. Script tag will be inserted to ScriptManager on this page.</param>
        public static void RegisterChatSupportAJAXProxy(AbstractCMSPage page)
        {
            HelperObject.RegisterChatSupportAJAXProxyInternal(page);
        }


        /// <summary>
        /// Registers ChatSupportManager and support scripts.
        /// </summary>
        /// <param name="page">AbstractCMSPage. Script tag will be inserted to ScriptManager on this page.</param>
        public static void RegisterChatSupportManager(Page page)
        {
            HelperObject.RegisterChatSupportManagerInternal(page);
        }


        /// <summary>
        /// Registers Chat sound manager used for playing sound notifications.
        /// </summary>
        /// <param name="page">AbstractCMSPage. Script tag will be inserted to ScriptManager on this page.</param>
        public static void RegisterChatNotificationManager(Page page)
        {
            HelperObject.RegisterChatNotificationManagerInternal(page);
        }


        /// <summary>
        /// Registers ChatManager.js and support scripts.
        /// </summary>
        /// <param name="page">AbstractCMSPage. Script tag will be inserted to ScriptManager on this page.</param>
        public static void RegisterChatManager(Page page)
        {
            HelperObject.RegisterChatManagerInternal(page);
        }


        /// <summary>
        /// Registers AJAX proxy which can be used to call ChatService.svc from javascript.
        /// </summary>
        /// <param name="page">AbstractCMSPage. Script tag will be inserted to ScriptManager on this page.</param>
        protected virtual void RegisterChatAJAXProxyInternal(AbstractCMSPage page)
        {
            RegisterAJAXProxy(page, "~/CMSModules/Chat/Services/ChatService.svc");

            ScriptHelper.FixSSLForWCFServices(page, "chat.IChatService");
        }


        /// <summary>
        /// Registers AJAX proxy which can be used to call ChatSupportService.svc from javascript.
        /// </summary>
        /// <param name="page">AbstractCMSPage. Script tag will be inserted to ScriptManager on this page.</param>
        protected virtual void RegisterChatSupportAJAXProxyInternal(AbstractCMSPage page)
        {
            RegisterAJAXProxy(page, "~/CMSModules/Chat/Services/ChatSupportService.svc");

            ScriptHelper.FixSSLForWCFServices(page, "chat.IChatSupportService");
        }


        /// <summary>
        /// Registers ChatSupportManager and support scripts.
        /// </summary>
        /// <param name="page">AbstractCMSPage. Script tag will be inserted to ScriptManager on this page.</param>
        protected virtual void RegisterChatSupportManagerInternal(Page page)
        {
            AbstractCMSPage cmsPage = page as AbstractCMSPage;
            if (cmsPage != null)
            {
                RegisterChatSupportAJAXProxy(cmsPage);
            }

            ScriptHelper.RegisterJQuery(page);
            ScriptHelper.RegisterScriptFile(page, "~/CMSModules/Chat/Controls/SupportChatManager.js");
            string initScript = String.Format(@"
ChatSupportManager.init({{
    popupWindowUrl: ""{0}"",
    resPopupWindowError: ""{1}""
}});",
                ChatSettingsProvider.ChatAdministrationPopupWindowUrl + "?isSupport=1&windowroomid={RoomIDParam}",
                ResHelper.GetString("chat.settings.popupwindowerrormsg"));
            ScriptHelper.RegisterStartupScript(page, typeof(Page), "ChatSupportManagerInit", initScript, true);
        }


        /// <summary>
        /// Registers Chat sound manager used for playing sound notifications.
        /// </summary>
        /// <param name="page">AbstractCMSPage. Script tag will be inserted to ScriptManager on this page.</param>
        protected virtual void RegisterChatNotificationManagerInternal(Page page)
        {
            ScriptHelper.RegisterJQuery(page);
            ScriptHelper.RegisterScriptFile(page, "~/CMSModules/Chat/CMSPages/Scripts/soundmanager2-nodebug-jsmin.js");
            string settingsScript = String.Format("window.ChatNotificationManagerSettings = {{ SwfFolder: '{0}' }};", UrlResolver.ResolveUrl("~/CMSModules/Chat/CMSPages/Scripts/"));
            ScriptHelper.RegisterClientScriptBlock(page, typeof(Page), "ChatNotificationManagerSettings", settingsScript, true);
            ScriptHelper.RegisterScriptFile(page, "~/CMSModules/Chat/CMSPages/Scripts/ChatNotificationManager.js");
        }


        /// <summary>
        /// Registers ChatManager.js and support scripts.
        /// </summary>
        /// <param name="page">AbstractCMSPage. Script tag will be inserted to ScriptManager on this page.</param>
        protected virtual void RegisterChatManagerInternal(Page page)
        {
            ScriptHelper.RegisterJQuery(page);
            ScriptHelper.RegisterJQueryCookie(page);

            ScriptHelper.RegisterScriptFile(page, "jquery/jquery-tools.js");

            // It is generated file
            ScriptHelper.RegisterScriptFile(page, "~/CMSModules/Chat/CMSPages/ChatSettings.ashx");
            ScriptHelper.RegisterScriptFile(page, "~/CMSModules/Chat/CMSPages/Scripts/ChatDialogs.js");
            ScriptHelper.RegisterScriptFile(page, "~/CMSModules/Chat/CMSPages/Scripts/ChatManager.js");
            string settingsScript = String.Format("window.ChatSettings.IsLiveSite = {0};", ChatUIHelper.IsChatAdministrationPage(page) ? "false" : "true");
            ScriptHelper.RegisterClientScriptBlock(page, typeof(Page), "ChatSettings", settingsScript, true);
        }


        /// <summary>
        /// Registers service reference to the Script Manager of the specified page.
        /// </summary>
        /// <param name="page">Page with ScriptManager.</param>
        /// <param name="serviceURL">URL to the serive</param>
        private static void RegisterAJAXProxy(AbstractCMSPage page, string serviceURL)
        {
            page.EnsureScriptManager();
            if (page.ScriptManagerControl != null)
            {
                page.ScriptManagerControl.Services.Add(new ServiceReference(serviceURL));
            }
        }
    }
}
