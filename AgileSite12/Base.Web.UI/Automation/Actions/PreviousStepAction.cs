using System.Web.UI;

using CMS.Helpers;

namespace CMS.Base.Web.UI.ActionsConfig
{
    /// <summary>
    /// Class for the automation previous action.
    /// </summary>
    public class PreviousStepAction : HeaderAction
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PreviousStepAction(Page page)
        {
            Text = ResHelper.GetString("general.previousstep", CultureCode);
            CommandName = ComponentEvents.AUTOMATION_MOVE_PREVIOUS;
            EventName = ComponentEvents.AUTOMATION_MOVE_PREVIOUS;
        }
    }
}