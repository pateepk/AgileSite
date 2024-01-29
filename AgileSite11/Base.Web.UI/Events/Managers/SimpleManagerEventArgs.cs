namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Manager event arguments
    /// </summary>
    public class SimpleManagerEventArgs : CMSEventArgs
    {
        #region "Properties"

        /// <summary>
        /// Action name. For general events (DATA_SAVED, LOAD_DATA...)
        /// </summary>
        public string ActionName
        {
            get;
            protected set;
        }


        /// <summary>
        /// Indicates if the action is valid
        /// </summary>
        public bool IsValid
        {
            get;
            set;
        }


        /// <summary>
        /// Error message in case action is not valid
        /// </summary>
        public string ErrorMessage
        {
            get;
            set;
        }
        

        /// <summary>
        /// Indicates if default check should be performed
        /// </summary>
        public bool CheckDefault
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="actionName">Action name</param>
        public SimpleManagerEventArgs(string actionName)
        {
            IsValid = true;
            ActionName = actionName;
            CheckDefault = true;
        }

        #endregion
    }
}
