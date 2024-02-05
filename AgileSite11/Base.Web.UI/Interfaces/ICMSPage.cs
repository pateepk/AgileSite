using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI.ActionsConfig;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Interface for the CMS pages.
    /// </summary>
    public interface ICMSPage
    {
        /// <summary>
        /// Local header actions
        /// </summary>
        HeaderActions HeaderActions
        {
            get;
            set;
        }


        /// <summary>
        /// Local page messages placeholder
        /// </summary>
        MessagesPlaceHolder MessagesPlaceHolder
        {
            get;
            set;
        }


        /// <summary>
        /// Container control for the page managers.
        /// </summary>
        Control ManagersContainer
        {
            get;
            set;
        }


        /// <summary>
        /// Container control for the context menus.
        /// </summary>
        Control ContextMenuContainer
        {
            get;
            set;
        }


        /// <summary>
        /// Container control for the page footer.
        /// </summary>
        Control FooterContainer
        {
            get;
            set;
        }


        /// <summary>
        /// Container control for the log controls.
        /// </summary>
        Control LogsContainer
        {
            get;
            set;
        }


        /// <summary>
        /// Script manager control.
        /// </summary>
        ScriptManager ScriptManagerControl
        {
            get;
            set;
        }


        /// <summary>
        /// Document manager control.
        /// </summary>
        ICMSDocumentManager DocumentManager
        {
            get;
        }


        /// <summary>
        /// Ensures the script manager on the page.
        /// </summary>
        ScriptManager EnsureScriptManager();


        /// <summary>
        /// Shows the specified message, optionally with a tooltip text and description.
        /// </summary>
        /// <param name="type">Message type</param>
        /// <param name="text">Message text</param>
        /// <param name="description">Additional description</param>
        /// <param name="tooltipText">Tooltip text</param>
        /// <param name="persistent">Indicates if the message is persistent</param>
        void ShowMessage(MessageTypeEnum type, string text, string description, string tooltipText, bool persistent);


        /// <summary>
        /// Adds text to existing message on the page.
        /// </summary>
        /// <param name="type">Message type</param>
        /// <param name="text">Information message</param>
        /// <param name="separator">Separator</param>
        void AddMessage(MessageTypeEnum type, string text, string separator);
    }
}