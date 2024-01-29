using System;

namespace CMS.MessageBoards.Web.UI
{
    /// <summary>
    /// References to the method called after message was saved.
    /// </summary>
    /// <param name="message">Board message object</param>
    public delegate void OnAfterMessageSavedEventHandler(BoardMessageInfo message);


    /// <summary>
    /// References to the method called mefore message is saved.
    /// </summary>
    public delegate void OnBeforeMessageSavedEventHandler();
}