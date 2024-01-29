using System;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.UIControls;


namespace CMS.Chat.Web.UI
{
    /// <summary>
    /// Provides helper methods for registration of chat scripts.
    /// </summary>
    public class ChatCssHelper : AbstractHelper<ChatCssHelper>
    {
        /// <summary>
        /// Registers CSS stylesheet. If page inherits CMSChatPage, CMSDesk stylesheet will be used  instead of livesite stylesheet.
        /// </summary>
        /// <param name="page">Page that the stylesheet will be registered to.</param>
        public static void RegisterStylesheet(Page page)
        {
            HelperObject.RegisterStylesheetInternal(page);
        }


        /// <summary>
        /// Registers CSS stylesheet. If cmsDeskStyle is true, CMSDesk stylesheet will be used instead of livesite stylesheet.
        /// The CMS desk stylesheet is bootstrap for support dialog, old css otherwise.
        /// </summary>
        /// <param name="page">Page that the stylesheet will be registered to.</param>
        /// <param name="cmsDeskStyle">Indicates if administration stylesheet will be used instead of live site stylesheet.</param>
        public static void RegisterStylesheet(Page page, bool cmsDeskStyle)
        {
            HelperObject.RegisterStylesheetInternal(page, cmsDeskStyle);
        }


        /// <summary>
        /// Registers CSS stylesheet. If page inherits CMSChatPage, CMSDesk stylesheet will be used  instead of livesite stylesheet.
        /// </summary>
        /// <param name="page">Page that the stylesheet will be registered to.</param>
        protected virtual void RegisterStylesheetInternal(Page page)
        {
            if (ChatUIHelper.IsChatAdministrationPage(page))
            {
                RegisterStylesheet(page, true);
            }
            else
            {
                RegisterStylesheet(page, false);
            }
        }


        /// <summary>
        /// Registers CSS stylesheet. If cmsDeskStyle is true, CMSDesk stylesheet will be used instead of livesite stylesheet.
        /// The CMS desk stylesheet is bootstrap for support dialog, old css otherwise.
        /// </summary>
        /// <param name="page">Page that the stylesheet will be registered to.</param>
        /// <param name="cmsDeskStyle">Indicates if administration stylesheet will be used instead of live site stylesheet.</param>
        protected virtual void RegisterStylesheetInternal(Page page, bool cmsDeskStyle)
        {
            if (cmsDeskStyle)
            {
                // Support window in Desk inherits from CMSModalPage and uses LESS CSS file linked differently.
                if (!(page is CMSModalPage))
                {
                    CssRegistration.RegisterCssLink(page, "~/App_Themes/Design/Chat/ChatCMSDesk.css");
                }
            }
            else
            {
                // Try to get path to "ChatLiveSite.css" in current or custom or default theme
                string themeUrl = CssLinkHelper.GetThemeCssUrl(PortalContext.CurrentSiteStylesheetName, "Chat/ChatLiveSite.css", false);
                if (String.IsNullOrEmpty(themeUrl))
                {
                    // Try to get path to "ChatLiveSite.css" in design theme
                    themeUrl = CssLinkHelper.GetThemeCssUrl("Design", "Chat/ChatLiveSite.css", true);
                }

                if (!String.IsNullOrEmpty(themeUrl))
                {
                    CssRegistration.RegisterCssLink(page, themeUrl);
                }
            }
        }
    }
}
