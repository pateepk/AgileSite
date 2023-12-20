using System;

using CMS.UIControls;

namespace CMS.MessageBoards.Web.UI
{
    /// <summary>
    /// Base page for CMS Message boards - Messages pages.
    /// </summary>
    [Security(Resource = "CMS.MessageBoards", UIElements = "Messages")]
    public abstract class CMSMessageBoardMessagesPage : CMSMessageBoardPage
    {
    }
}