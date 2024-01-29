using CMS.Activities;
using CMS.Activities.Loggers;

using CMS.ContactManagement;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;

namespace APIExamples
{
    /// <summary>
    /// Holds activity API examples.
    /// </summary>
    /// <pageTitle>Activities</pageTitle>
    internal class Activities
    {
        /// <heading>Logging an activity for a contact</heading>
        private void CreateActivity()
        {
            // Prepares an instance of the activity logging service
            IActivityLogService service = CMS.Core.Service.Resolve<IActivityLogService>();

            // Obtains parameters based on the current context in which the activity is being logged
            UserInfo user = MembershipContext.AuthenticatedUser;
            TreeNode currentPage = DocumentContext.CurrentDocument;
            int contactId = ContactManagementContext.CurrentContactID;

            // Prepares an initializer for logging activities of the "User login" type
            var activityInitializer = new UserLoginActivityInitializer(user, currentPage, contactId);

            // Logs the activity based on the context of the current request
            service.Log(activityInitializer, CMSHttpContext.Current.Request);
        }


        /// <heading>Updating logged activities</heading>
        private void GetAndUpdateActivity()
        {
            // Gets the first contact whose last name is 'Smith'
            ContactInfo contact = ContactInfoProvider.GetContacts()
                                                .WhereEquals("ContactLastName", "Smith")
                                                .FirstObject;

            if (contact != null)
            {
                // Gets all activities logged for the contact
                var updateActivities = ActivityInfoProvider.GetActivities().WhereEquals("ActivityContactID", contact.ContactID);

                // Loops through individual activities
                foreach (ActivityInfo activity in updateActivities)
                {
                    // Updates the activity title
                    activity.ActivityTitle = activity.ActivityTitle.ToUpper();

                    // Saves the modified activity to the database
                    ActivityInfoProvider.SetActivityInfo(activity);
                }
            }
        }


        /// <heading>Deleting logged activities</heading>
        private void DeleteActivity()
        {
            // Gets the first contact whose last name is 'Smith'
            ContactInfo contact = ContactInfoProvider.GetContacts()
                                                .WhereEquals("ContactLastName", "Smith")
                                                .FirstObject;

            if (contact != null)
            {
                // Gets all activities logged for the contact
                var activities = ActivityInfoProvider.GetActivities().WhereEquals("ActivityContactID", contact.ContactID);

                // Loops through individual activities
                foreach (ActivityInfo activity in activities)
                {
                    // Deletes the activity
                    ActivityInfoProvider.DeleteActivityInfo(activity);
                }
            }
        }
    }
}
