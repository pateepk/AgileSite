using CMS.Helpers;

namespace CMS.Base.Web.UI.ActionsConfig
{
    /// <summary>
    /// Class for the customize action.
    /// </summary>
    public class CustomizeAction : HeaderAction
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CustomizeAction()
        {
            Text = ResHelper.GetString("General.Customize", CultureCode);
            CommandName = ComponentEvents.CUSTOMIZE;
            EventName = ComponentEvents.CUSTOMIZE;
            Index = -3;
            OnClientClick = "if (!confirm(" + ScriptHelper.GetLocalizedString("customization.question") + ")) return false;";
        }
    }
}
