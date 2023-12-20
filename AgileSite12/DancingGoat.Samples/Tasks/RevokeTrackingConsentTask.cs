using CMS.ContactManagement;
using CMS.Helpers;
using CMS.Scheduler;

namespace CMS.DancingGoat.Samples
{
    internal class RevokeTrackingConsentTask : ITask
    {
        public string Execute(TaskInfo task)
        {
            var contactId = ValidationHelper.GetInteger(task.TaskData, 0);
            if (contactId <= 0)
            {
                return "Invalid contact ID.";
            }

            var contact = ContactInfoProvider.GetContactInfo(contactId);
            DancingGoatSamplesModule.DeleteContactActivities(contact);

            return null;
        }
    }
}
