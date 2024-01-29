using CMS.Helpers;

namespace CMS.Base.Web.UI.ActionsConfig
{
    /// <summary>
    /// Class for the document approve action.
    /// </summary>
    public class DocumentApproveAction : HeaderAction
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DocumentApproveAction()
        {
            Text = ResHelper.GetString("general.approve", CultureCode);
            CommandName = DocumentComponentEvents.APPROVE;
            EventName = DocumentComponentEvents.APPROVE;
            Tooltip = ResHelper.GetString("EditMenu.Approve", CultureCode);
            ButtonStyle = ButtonStyle.Default;
        }
    }
}