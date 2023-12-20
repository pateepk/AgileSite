using System.Web.UI;

using CMS.Helpers;

namespace CMS.Base.Web.UI.ActionsConfig
{
    /// <summary>
    /// Class for the automation next step action.
    /// </summary>
    public class NextStepAction : HeaderAction
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NextStepAction(Page page)
        {
            Text = ResHelper.GetString("general.nextstep", CultureCode);
            CommandName = ComponentEvents.AUTOMATION_MOVE_NEXT;
            EventName = ComponentEvents.AUTOMATION_MOVE_NEXT;
        }
    }
}