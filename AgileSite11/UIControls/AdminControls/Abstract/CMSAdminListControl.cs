using System;
using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Summary description for CMSAdminListControl.
    /// </summary>
    public abstract class CMSAdminListControl : CMSAdminControl
    {
        #region "Events"

        /// <summary>
        /// On edit event.
        /// </summary>
        public event EventHandler OnEdit;


        /// <summary>
        /// On delete event.
        /// </summary>
        public event EventHandler OnDelete;


        /// <summary>
        /// On create event.
        /// </summary>
        public event EventHandler OnCreate;

        /// <summary>
        /// General On action event (when unigrid has more actions than edit and delete).
        /// </summary>
        public event CommandEventHandler OnAction;

        #endregion


        #region "Properties"

        /// <summary>
        /// Selected item ID.
        /// </summary>
        public int SelectedItemID
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["SelectedItemID"], 0);
            }
            set
            {
                ViewState["SelectedItemID"] = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Raises the OnEdit event.
        /// </summary>
        public void RaiseOnEdit()
        {
            if (OnEdit != null)
            {
                OnEdit(this, null);
            }
        }


        /// <summary>
        /// Raises the OnDelete event.
        /// </summary>
        public void RaiseOnDelete()
        {
            if (OnDelete != null)
            {
                OnDelete(this, null);
            }
        }


        /// <summary>
        /// Raises the OnCreate event.
        /// </summary>
        public void RaiseOnCreate()
        {
            if (OnCreate != null)
            {
                OnCreate(this, null);
            }
        }


        /// <summary>
        /// Raises the OnCreate event.
        /// </summary>
        /// <param name="action">Action name</param>
        /// <param name="argument">Argument of the action</param>
        public void RaiseOnAction(string action, object argument)
        {
            if (OnAction != null)
            {
                OnAction(this, new CommandEventArgs(action, argument));
            }
        }

        #endregion
    }
}