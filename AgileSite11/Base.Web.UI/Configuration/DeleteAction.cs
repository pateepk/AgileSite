using CMS.Helpers;

namespace CMS.Base.Web.UI.ActionsConfig
{
    /// <summary>
    /// Class for the delete action.
    /// </summary>
    public class DeleteAction : HeaderAction
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DeleteAction()
        {
            Text = ResHelper.GetString("general.delete", CultureCode);
            CommandName = ComponentEvents.DELETE;
            EventName = ComponentEvents.DELETE;
            Tooltip = ResHelper.GetString("EditMenu.Delete", CultureCode);
            ButtonStyle = ButtonStyle.Default;
        }
    }
}