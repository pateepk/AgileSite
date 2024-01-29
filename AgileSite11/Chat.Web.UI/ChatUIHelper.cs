using System;
using System.Text;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.PortalEngine.Web.UI;
using CMS.PortalEngine;
using CMS.Modules;
using CMS.UIControls;


namespace CMS.Chat.Web.UI
{
    /// <summary>
    /// Provides helper methods for chat UI.
    /// </summary>
    public class ChatUIHelper : AbstractHelper<ChatUIHelper>
    {
        /// <summary>
        /// Make webpart's envelope, which is used for styling and hiding.
        /// </summary>
        /// <param name="cssClass">String. Envelope css classes.</param>
        /// <param name="webpart">CMSAbstractWebpart. Reference to webpart object.</param>
        /// <param name="innerContainerTitle">String. Defines title of container (if webpart has no container defined).</param>
        /// <param name="innerContainerName">String. Defines name of container (if webpart has no container defined).</param>
        public static void MakeWebpartEnvelope(string cssClass, CMSAbstractWebPart webpart, string innerContainerTitle, string innerContainerName)
        {
            HelperObject.MakeWebpartEnvelopeInternal(cssClass, webpart, innerContainerTitle, innerContainerName);
        }


        /// <summary>
        /// Get webpart's loading div which is showed when data are loaded.
        /// </summary>
        /// <param name="cssClass">String. Loading div css classes.</param>
        /// <param name="loadingTextResStrName">String. Defines name of resource string which will be used to resolve loading text.</param>
        public static string GetWebpartLoadingDiv(string cssClass, string loadingTextResStrName)
        {
            return HelperObject.GetWebpartLoadingDivInternal(cssClass, loadingTextResStrName);
        }


        /// <summary>
        /// Gets webpart's transformation.
        /// </summary>
        /// <param name="transformationName">String. Name of transfromation which will be loaded.</param>
        /// <param name="transformationError">String. Reference to resource string which will be returned when error occured during loading transformation.</param>
        public static string GetWebpartTransformation(string transformationName, string transformationError)
        {
            return HelperObject.GetWebpartTransformationInternal(transformationName, transformationError);
        }


        /// <summary>
        /// Gets room ID from query or returns original room ID.
        /// </summary>
        /// <param name="roomID">Int. Original webparts room ID (will be returned if no query room id setted).</param>
        /// <param name="groupID">String. Webpart's group id.</param>
        public static int GetRoomIdFromQuery(int roomID, string groupID)
        {
            return HelperObject.GetRoomIdFromQueryInternal(roomID, groupID);
        }


        /// <summary>
        /// Returns chat room window url from global chat settings
        /// </summary>
        /// <param name="page">Page that asks for window URL. Administration window URL is returned when administration page is given, live site window URL otherwise.</param>
        public static string GetChatRoomWindowURL(Page page = null)
        {
            return HelperObject.GetChatRoomWindowURLInternal(page);
        }


        /// <summary>
        /// Gets string representation which can be used to display chat user in CMS Desk.
        /// 
        /// If user is not anonymous, returned string is a HTML code which contains anchor to open user's details in new window.
        /// 
        /// If user is anonymous, returned string will be simply his nickname.
        /// 
        /// Nickname will be always HTML encoded.
        /// </summary>
        /// <param name="control">Script for opening new window will be added to this control</param>
        /// <param name="chatUserID">ID of chat user which will be displayed</param>
        /// <param name="chatUserNickname">Nickname of chat user which will be displayed</param>
        /// <param name="isAnonymous">True if chat user is anonymous</param>
        /// <returns>String representation of this chat user</returns>
        public static string GetCMSDeskChatUserField(Control control, int chatUserID, string chatUserNickname, bool isAnonymous)
        {
            return HelperObject.GetCMSDeskChatUserFieldInternal(control, chatUserID, chatUserNickname, isAnonymous);
        }


        /// <summary>
        /// Gets string representation which can be used to display chat user in CMS Desk.
        /// 
        /// If user is not anonymous, returned string is a HTML code which contains anchor to open user's details in new window.
        /// 
        /// If user is anonymous, returned string will be simply his nickname.
        /// 
        /// Nickname will be always HTML encoded.
        /// </summary>
        /// <param name="control">Script for opening new window will be added to this control</param>
        /// <param name="chatUser">ChatUser whos nickname will be returned</param>
        /// <returns>String representation of this chat user</returns>
        public static string GetCMSDeskChatUserField(Control control, ChatUserInfo chatUser)
        {
            return GetCMSDeskChatUserField(control, chatUser.ChatUserID, chatUser.ChatUserNickname, chatUser.IsAnonymous);
        }


        /// <summary>
        /// Make webpart's envelope, which is used for styling and hiding.
        /// </summary>
        /// <param name="cssClass">String. Envelope css classes.</param>
        /// <param name="webpart">CMSAbstractWebpart. Reference to webpart object.</param>
        /// <param name="innerContainerTitle">String. Defines title of container (if webpart has no container defined).</param>
        /// <param name="innerContainerName">String. Defines name of container (if webpart has no container defined).</param>
        protected virtual void MakeWebpartEnvelopeInternal(string cssClass, CMSAbstractWebPart webpart, string innerContainerTitle, string innerContainerName)
        {
            if (webpart.Container != null)
            {
                webpart.Container = webpart.Container.Clone();
                webpart.Container.ContainerTextBefore = String.Format("<div id=\"envelope_{0}\" class=\"{1}\">{2}", webpart.ClientID, cssClass, webpart.Container.ContainerTextBefore);
                webpart.Container.ContainerTextAfter += "</div>";
            }
            else
            {
                WebPartContainerInfo container = WebPartContainerInfoProvider.GetWebPartContainerInfo(innerContainerName);
                if (container != null)
                {
                    webpart.ContentBefore = container.ContainerTextBefore.Replace("{%ContainerTitle%}", ResHelper.LocalizeString(innerContainerTitle)) + webpart.ContentBefore;
                    webpart.ContentAfter = webpart.ContentAfter + container.ContainerTextAfter;
                }

                webpart.ContentBefore = String.Format("<div id=\"envelope_{0}\" class=\"{1}\">{2}", webpart.ClientID, cssClass, webpart.ContentBefore);
                webpart.ContentAfter += "</div>";
            }
        }


        /// <summary>
        /// Get webpart's loading div which is showed when data are loaded.
        /// </summary>
        /// <param name="cssClass">String. Loading div css classes.</param>
        /// <param name="loadingTextResStrName">String. Defines name of resource string which will be used to resolve loading text.</param>
        protected virtual string GetWebpartLoadingDivInternal(string cssClass, string loadingTextResStrName)
        {
            return "<div class=\"" + cssClass + "\">" + ResHelper.GetString(loadingTextResStrName) + "</div>";
        }


        /// <summary>
        /// Gets webpart's transformation.
        /// </summary>
        /// <param name="transformationName">String. Name of transfromation which will be loaded.</param>
        /// <param name="transformationError">String. Reference to resource string which will be returned when error occured during loading transformation.</param>
        protected virtual string GetWebpartTransformationInternal(string transformationName, string transformationError)
        {
            TransformationInfo ti = TransformationInfoProvider.GetTransformation(transformationName);
            if (ti == null)
            {
                return ResHelper.GetString(transformationError);
            }
            var culture = LocalizationContext.CurrentCulture ?? DocumentContext.CurrentDocumentCulture ?? LocalizationContext.CurrentUICulture;
            return MacroResolver.Resolve(ti.TransformationCode, new MacroSettings()
            {
                Culture = culture?.CultureCode
            });
        }


        /// <summary>
        /// Gets room ID from query or returns original room ID.
        /// </summary>
        /// <param name="roomID">Int. Original webparts room ID (will be returned if no query room id setted).</param>
        /// <param name="groupID">String. Webpart's group id.</param>
        protected virtual int GetRoomIdFromQueryInternal(int roomID, string groupID)
        {
            int queryRoomID = QueryHelper.GetInteger("roomid", -1);
            string queryGroupID = QueryHelper.GetString("groupname", null);
            if ((queryRoomID > 0) && !(roomID > 0))
            {
                if ((queryGroupID == null) || (queryGroupID == groupID))
                {
                    return queryRoomID;
                }
            }
            return roomID;
        }


        /// <summary>
        /// Returns chat room window url from global chat settings
        /// </summary>
        /// <param name="page">Page that asks for window URL. Administration window URL is returned when administration page is given, live site window URL otherwise.</param>
        protected virtual string GetChatRoomWindowURLInternal(Page page)
        {
            if ((page != null) && IsChatAdministrationPage(page))
            {
                return ChatSettingsProvider.ChatAdministrationPopupWindowUrl;
            }

            return ChatSettingsProvider.ChatRoomPopupWindowUrl;
        }


        /// <summary>
        /// Gets string representation which can be used to display chat user in CMS Desk.
        /// 
        /// If user is not anonymous, returned string is a HTML code which contains anchor to open user's details in new window.
        /// 
        /// If user is anonymous, returned string will be simply his nickname.
        /// 
        /// Nickname will be always HTML encoded.
        /// </summary>
        /// <param name="control">Script for opening new window will be added to this control</param>
        /// <param name="chatUserID">ID of chat user which will be displayed</param>
        /// <param name="chatUserNickname">Nickname of chat user which will be displayed</param>
        /// <param name="isAnonymous">True if chat user is anonymous</param>
        /// <returns>String representation of this chat user</returns>
        protected virtual string GetCMSDeskChatUserFieldInternal(Control control, int chatUserID, string chatUserNickname, bool isAnonymous)
        {
            if (isAnonymous)
            {
                return HTMLHelper.HTMLEncode(chatUserNickname);
            }
            else
            {
                ScriptHelper.RegisterDialogScript(control.Page);

                string url = ApplicationUrlHelper.GetElementDialogUrl("CMS.Chat", "EditChatUser", chatUserID);
                string onclick = String.Format("modalDialog(\"{0}\",\"ChatUserWindow{1}\",800,350); return false;", url, chatUserID);
                string address = String.Format("<a href=\"{0}\" onclick=\"{1}\" target=\"_blank\">{2}</a>", url, HTMLHelper.EncodeForHtmlAttribute(onclick), HTMLHelper.HTMLEncode(chatUserNickname));

                return address;
            }
        }


        /// <summary>
        /// Checks whether the given page is administration page or live site page.
        /// </summary>
        /// <param name="page">Page to check.</param>
        /// <returns>True if page is administration page, false otherwise.</returns>
        internal static bool IsChatAdministrationPage(Page page)
        {
            return ((page is CMSChatPage) || (page is CMSModalPage));
        }
    }
}
