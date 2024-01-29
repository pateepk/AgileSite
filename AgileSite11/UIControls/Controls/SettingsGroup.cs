using System;
using System.Web.UI.WebControls;

using CMS.DataEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Control representing settings group
    /// </summary>
    public class SettingsGroup : CMSUserControl
    {
        #region "Variables"

        /// <summary>
        /// Category, which groups are to be displayed.
        /// </summary>
        protected SettingsCategoryInfo mCategory;


        /// <summary>
        /// Indicates if control allow edit his values.
        /// </summary>
        private bool mAllowEdit = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Code name of displayed category. This category must be group (CategoryIsGroup = true).
        /// </summary>
        public SettingsCategoryInfo Category
        {
            get
            {
                return mCategory;
            }
            set
            {
                mCategory = value;
            }
        }


        /// <summary>
        /// Indicates if control should be enabled.
        /// </summary>
        public bool AllowEdit
        {
            get
            {
                return mAllowEdit;
            }
            set
            {
                mAllowEdit = value;
            }
        }

        
        /// <summary>
        /// Indicates the module identifier of current settings group.
        /// </summary>
        public int ModuleID
        {
            get;
            set;
        }

        #endregion


        #region "Events"

        /// <summary>
        /// Event raised, when edit/delete/moveUp/moveDown category button is clicked.
        /// </summary>
        public event CommandEventHandler ActionPerformed;


        /// <summary>
        /// Event raised, when asked to add key.
        /// </summary>
        public event CommandEventHandler OnNewKey;


        /// <summary>
        /// Event raised, when unigrid's button is clicked.
        /// </summary>
        public event OnActionEventHandler OnKeyAction;

        #endregion


        #region "Methods"

        /// <summary>
        /// Handles the whole category actions.
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event arguments</param>
        protected void CategoryActionPerformed(object sender, CommandEventArgs e)
        {
            if (ActionPerformed != null)
            {
                ActionPerformed.Invoke(sender, e);
            }
        }


        /// <summary>
        /// Handles request for creating of new settings key.
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event arguments</param>
        protected void CreateNewKey(object sender, EventArgs e)
        {
            if (OnNewKey != null)
            {
                CommandEventArgs args = new CommandEventArgs("newkey", "parentgroup=" + Category.CategoryName + "&moduleid=" + ModuleID);
                OnNewKey(sender, args);
            }
        }


        /// <summary>
        /// Handles settings keys actions (delete, edit, move up, move down).
        /// </summary>
        /// <param name="actionName">Name of the action</param>
        /// <param name="argument">Argument of the action</param>
        protected void KeyAction(string actionName, object argument)
        {
            if (OnKeyAction != null)
            {
                OnKeyAction(actionName, argument);
            }
        }

        #endregion
    }
}