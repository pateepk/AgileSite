using System;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Contains data of group button click event
    /// </summary>
    public class CMSButtonActionClickedEventArgs : EventArgs
    {
        /// <summary>
        /// Name of the clicked action
        /// </summary>
        public string ActionName
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor. 
        /// </summary>
        /// <param name="actionName">Name of clicked action</param>
        public CMSButtonActionClickedEventArgs(string actionName = null)
        {
            ActionName = actionName;
        }
    }
}