using CMS.Helpers;

namespace CMS.Base.Web.UI.ActionsConfig
{
    /// <summary>
    /// Class for the document undo check-out action.
    /// </summary>
    public class DocumentUndoCheckOutAction : HeaderAction
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DocumentUndoCheckOutAction()
        {
            Text = ResHelper.GetString("EditMenu.IconUndoCheckout", CultureCode);
            CommandName = DocumentComponentEvents.UNDO_CHECKOUT;
            EventName = DocumentComponentEvents.UNDO_CHECKOUT;
            Tooltip = ResHelper.GetString("EditMenu.UndoCheckout", CultureCode);
            ButtonStyle = ButtonStyle.Default;
        }
    }
}