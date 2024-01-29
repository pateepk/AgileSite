using CMS.Helpers;

namespace CMS.Base.Web.UI.ActionsConfig
{
    /// <summary>
    /// Class for the document check-out action.
    /// </summary>
    public class DocumentCheckOutAction : HeaderAction
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DocumentCheckOutAction()
        {
            Text = ResHelper.GetString("EditMenu.IconCheckOut", CultureCode);
            CommandName = DocumentComponentEvents.CHECKOUT;
            EventName = DocumentComponentEvents.CHECKOUT;
            Tooltip = ResHelper.GetString("EditMenu.CheckOut", CultureCode);
            ButtonStyle = ButtonStyle.Default;
        }
    }
}