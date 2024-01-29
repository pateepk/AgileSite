namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Manager event arguments
    /// </summary>
    public class ManagerEventArgs : SimpleManagerEventArgs
    {
        #region "Properties"

        /// <summary>
        /// Mode of the manager (Insert, Update, New language version)
        /// </summary>
        public FormModeEnum Mode
        {
            get;
            protected set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mode">Manager mode</param>
        /// <param name="actionName">Action name</param>
        public ManagerEventArgs(FormModeEnum mode, string actionName)
            : base(actionName)
        {
            Mode = mode;
        }

        #endregion
    }
}
