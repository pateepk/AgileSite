using System;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class for contact deletion action.
    /// </summary>
    public class DeleteContactAction : ContactAutomationAction
    {
        /// <summary>
        /// Executes action.
        /// </summary>
        public override void Execute()
        {
            if (Contact != null)
            {
                // Delete contact
                ContactInfoProvider.DeleteContactInfo(Contact);

                // Refresh contact to ensure that if current contact was deleted, instance is reloaded
                RefreshObject();
            }
        }
    }
}
