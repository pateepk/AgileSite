using CMS.Helpers;

namespace CMS.Base.Web.UI.ActionsConfig
{
    /// <summary>
    /// Class for the document check-in action.
    /// </summary>
    public class DocumentCheckInAction : HeaderAction
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DocumentCheckInAction()
        {
            Text = ResHelper.GetString("EditMenu.IconCheckIn", CultureCode);
            CommandName = DocumentComponentEvents.CHECKIN;
            EventName = DocumentComponentEvents.CHECKIN;
            Tooltip = ResHelper.GetString("EditMenu.CheckIn", CultureCode);
            ButtonStyle = ButtonStyle.Default;
        }
    }
}