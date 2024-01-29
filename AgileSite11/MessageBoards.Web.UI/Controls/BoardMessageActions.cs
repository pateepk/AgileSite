using System;

using CMS.UIControls;

namespace CMS.MessageBoards.Web.UI
{
    ///<summary>
    /// Summary description for BoardMessageDetail
    /// </summary>
    public class BoardMessageActions : CMSUserControl
    {
        /// <summary>
        /// References to the method called when some message action is fired.
        /// </summary>
        /// <param name="actionName">Name of the action fired</param>
        /// <param name="argument">Argument related to the action</param>
        public delegate void OnBoardMessageAction(string actionName, object argument);

        ///<summary>
        /// Event occurring on message action link click
        /// </summary>
        public event OnBoardMessageAction OnMessageAction;


        #region "Public properties"

        /// <summary>
        ///Message board ID.
        /// </summary>
        public int MessageBoardID
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the EDIT button should be displayed under current conditions.
        /// </summary>
        public bool ShowEdit
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the DELETE button should be displayed under current conditions.
        /// </summary>
        public bool ShowDelete
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the APPROVE button should be displayed under current conditions.
        /// </summary>
        public bool ShowApprove
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the REJECT button should be displayed under current conditions.
        /// </summary>
        public bool ShowReject
        {
            get;
            set;
        }


        /// <summary>
        /// Board message ID.
        /// </summary>
        public int MessageID
        {
            get;
            set;
        }

        #endregion


        ///<summary>
        /// Fires action on control action link button click
        /// </summary>
        /// <param name="actionName">String representation of action</param>
        /// <param name="argument">Parameter related to the action</param>
        protected void FireOnBoardMessageAction(string actionName, object argument)
        {
            OnMessageAction(actionName, argument);
        }
    }
}