using CMS.Helpers;

namespace CMS.Base.Web.UI.ActionsConfig
{
    /// <summary>
    /// Class for the save action.
    /// </summary>
    public class SaveAction : HeaderAction
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SaveAction()
        {
            Text = ResHelper.GetString("General.Save", CultureCode);
            CommandName = ComponentEvents.SAVE;
            EventName = ComponentEvents.SAVE;
            RegisterShortcutScript = true;
        }
    }
}