using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Edit menu event arguments
    /// </summary>
    public class EditMenuEventArgs : CMSEventArgs
    {
        #region "Properties"

        /// <summary>
        /// Context of the action.
        /// </summary>
        public string ActionName
        {
            get;
            private set;
        }


        /// <summary>
        /// Validation java script to be performed
        /// </summary>
        public string ValidationScript
        {
            get;
            set;
        }


        /// <summary>
        /// JavaScript code to be performed on object menu action.
        /// </summary>
        public string ClientActionScript
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="action">Action name</param>
        public EditMenuEventArgs(string action)
        {
            ActionName = action;
        }

        #endregion
    }
}
