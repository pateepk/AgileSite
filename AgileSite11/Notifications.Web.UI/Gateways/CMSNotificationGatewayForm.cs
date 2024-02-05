using CMS.UIControls;

namespace CMS.Notifications.Web.UI
{
    /// <summary>
    /// Base class for notification gateway forms.
    /// </summary>
    public class CMSNotificationGatewayForm : CMSUserControl
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets the form value.
        /// </summary>
        public virtual object Value
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


        #region "Public methods"

        /// <summary>
        /// Gets the notification gateway form which is loaded according to the specified path.
        /// </summary>
        /// <param name="path">Virtual path to the control to load</param>        
        public virtual CMSNotificationGatewayForm GetNotificationGatewayForm(string path)
        {
            return null;
        }


        /// <summary>
        /// Validates user entered/selected target(s)
        /// </summary>        
        public virtual string Validate()
        {
            return string.Empty;
        }


        /// <summary>
        /// Clears the form (this method is called when successful subscription is made).
        /// </summary>        
        public virtual void ClearForm()
        {
        }

        #endregion
    }
}