using System;

using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.MessageBoards.Web.UI
{
    /// <summary>
    /// Base page for CMS Message boards - Boards pages.
    /// </summary>
    [Security(Resource = "CMS.MessageBoards", UIElements = "Boards")]
    public abstract class CMSMessageBoardBoardsPage : CMSMessageBoardPage
    {
        /// <summary>
        /// Checks whether supplied siteId corresponds to current site ID and sets EditedObject to null if not.
        /// </summary>
        protected void CheckMessageBoardSiteID(int boardSiteId)
        {
            if (boardSiteId != SiteContext.CurrentSiteID)
            {
                EditedObject = null;
            }
        }
    }
}