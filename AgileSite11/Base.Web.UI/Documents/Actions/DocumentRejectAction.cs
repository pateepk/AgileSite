using CMS.Helpers;

namespace CMS.Base.Web.UI.ActionsConfig
{
    /// <summary>
    /// Class for the reject action.
    /// </summary>
    public class DocumentRejectAction : HeaderAction
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DocumentRejectAction()
        {
            Text = ResHelper.GetString("general.reject", CultureCode);
            CommandName = DocumentComponentEvents.REJECT;
            EventName = DocumentComponentEvents.REJECT;
            Tooltip = ResHelper.GetString("EditMenu.Reject", CultureCode);
            ButtonStyle = ButtonStyle.Default;
        }
    }
}