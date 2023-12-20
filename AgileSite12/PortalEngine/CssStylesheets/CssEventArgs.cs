using System;
using System.Text;

namespace CMS.PortalEngine
{
    /// <summary>
    /// The object of this class is used to pass CSS preprocessing arguments to a callback method.
    /// </summary>
    public class CssEventArgs : EventArgs
    {
        #region "Properties"

        /// <summary>
        /// Stylesheet code.
        /// </summary>
        public string Code
        {
            get;
            set;
        }

        
        /// <summary>
        /// Error message.
        /// </summary>
        public string ErrorMessage
        {
            get;
            set;
        }


        /// <summary>
        /// Warning message.
        /// </summary>
        public string WarningMessage
        {
            get;
            set;
        }

        #endregion
    }
}
