using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Error control for displaying error message on live site.
    /// </summary>
    public abstract class ErrorMessageControl : CMSUserControl
    {
        #region "Properties"

        /// <summary>
        /// Error title.
        /// </summary>
        public virtual string ErrorTitle
        {
            get
            {
                return null;
            }
            set
            {
            }
        }


        /// <summary>
        /// Error message.
        /// </summary>
        public virtual string ErrorMessage
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        #endregion
    }
}