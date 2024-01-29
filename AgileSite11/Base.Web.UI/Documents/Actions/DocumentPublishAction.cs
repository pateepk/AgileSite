using CMS.Helpers;

namespace CMS.Base.Web.UI.ActionsConfig
{
    /// <summary>
    /// Class for the direct document publish action.
    /// </summary>
    public class DocumentPublishAction : HeaderAction
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DocumentPublishAction()
        {
            Text = ResHelper.GetString("EditMenu.IconApprovePublish", CultureCode);
            CommandName = DocumentComponentEvents.PUBLISH;
            EventName = DocumentComponentEvents.PUBLISH;
            Tooltip = ResHelper.GetString("EditMenu.ApprovePublish", CultureCode);
            ButtonStyle = ButtonStyle.Default;
        }
    }
}