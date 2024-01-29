using System;

using CMS.SiteProvider;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class for Set contact status action.
    /// </summary>
    public class ContactStatusAction : ContactAutomationAction
    {
        #region "Parameters"

        /// <summary>
        /// Gets selected status
        /// </summary>
        protected virtual string ContactStatus
        {
            get
            {
                return GetResolvedParameter<string>("ContactStatus", String.Empty);
            }
        }

        #endregion


        /// <summary>
        /// Executes action
        /// </summary>
        public override void Execute()
        {
            if (Contact != null)
            {
                ContactStatusInfo status = ContactStatusInfoProvider.GetContactStatusInfo(ContactStatus);

                if ((status != null) || ((Contact.ContactStatusID > 0) && String.IsNullOrEmpty(ContactStatus)))
                {
                    // Update contact object 
                    Contact.ContactStatusID = (status != null) ? status.ContactStatusID : 0;
                    Contact.Update();
                }
            }
        }
    }
}
