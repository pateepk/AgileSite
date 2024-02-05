using System;

using CMS.UIControls;

namespace CMS.Notifications.Web.UI
{
    /// <summary>
    /// Base class for the control which is used for editing notification template text.
    /// </summary>
    public class TemplateTextEdit : CMSUserControl
    {
        #region "Public properties"

        /// <summary>
        /// Gateway ID.
        /// </summary>
        public int GatewayID
        {
            get;
            set;
        }


        /// <summary>
        /// Template ID.
        /// </summary>
        public int TemplateID
        {
            get;
            set;
        }


        /// <summary>
        /// Template subject.
        /// </summary>
        public virtual string TemplateSubject
        {
            get
            {
                return String.Empty;
            }
            set
            {
                ;
            }
        }


        /// <summary>
        /// Template plain text.
        /// </summary>
        public virtual string TemplatePlainText
        {
            get
            {
                return String.Empty;
            }
            set
            {
                ;
            }
        }


        /// <summary>
        /// Template HTML text.
        /// </summary>
        public virtual string TemplateHTMLText
        {
            get
            {
                return String.Empty;
            }
            set
            {
                ;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Builts object and saves it to the database.
        /// </summary>
        public void SaveData()
        {
            // Get object or create new
            NotificationTemplateTextInfo ntti = NotificationTemplateTextInfoProvider.GetNotificationTemplateTextInfo(GatewayID, TemplateID);
            if (ntti == null)
            {
                ntti = new NotificationTemplateTextInfo();
            }

            // Setup properties
            ntti.TemplateHTMLText = TemplateHTMLText;
            ntti.TemplatePlainText = TemplatePlainText;
            ntti.TemplateSubject = TemplateSubject;
            ntti.TemplateID = TemplateID;
            ntti.GatewayID = GatewayID;

            // Set object
            NotificationTemplateTextInfoProvider.SetNotificationTemplateTextInfo(ntti);
        }

        #endregion
    }
}