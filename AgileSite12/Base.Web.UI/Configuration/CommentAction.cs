using CMS.Helpers;

namespace CMS.Base.Web.UI.ActionsConfig
{
    /// <summary>
    /// Class for the comment action.
    /// </summary>
    public class CommentAction : HeaderAction
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="actionName">Action name</param>
        public CommentAction(string actionName)
        {
            Text = ResHelper.GetString("EditMenu.IconComment" + actionName, CultureCode);
        }
    }
}