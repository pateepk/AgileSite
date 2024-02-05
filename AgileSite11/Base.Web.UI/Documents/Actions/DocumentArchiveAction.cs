using CMS.Helpers;

namespace CMS.Base.Web.UI.ActionsConfig
{
    /// <summary>
    /// Class for the document archive action.
    /// </summary>
    public class DocumentArchiveAction : HeaderAction
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DocumentArchiveAction()
        {
            Text = ResHelper.GetString("general.archive", CultureCode);
            CommandName = DocumentComponentEvents.ARCHIVE;
            EventName = DocumentComponentEvents.ARCHIVE;
            ButtonStyle = ButtonStyle.Default;
            Tooltip = ResHelper.GetString("EditMenu.Archive", CultureCode);
        }
    }
}