using System;

using CMS.Core;
using CMS.Core.Internal;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Scheduler;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Deletes inactive contacts. Takes a batch of 1000 which is deleted and replans itself if there are more to delete. 
    /// </summary>
    public class DeleteInactiveContacts : ITask
    {
        internal const string RESCHEDULED_TO_DELETE_MORE_CONTACTS_IN_A_MINUTE = "Rescheduled to delete more contacts in a minute";
        internal const string NO_MORE_CONTACTS_TO_DELETE_RESCHEDULED_TO_TRY_AGAIN_IN_THE_NEXT_OFF_PEAK_PERIOD = "No more contacts to delete. Rescheduled to try again in the next off-peak period";
        internal const string RESCHEDULED_TO_DELETE_MORE_CONTACTS_IN_NEXT_OFF_PEAK_PERIOD = "Rescheduled to delete more contacts in next off-peak period";
        internal const string INSUFFICIENT_LICENSE_MESSAGE = "Delete inactive contacts";
        internal const string INSUFFICIENT_LICENSE_MESSAGE_RESOURCE = "licenselimitation.featurenotavailable";

        private readonly IDateTimeNowService mDateTimeNowService;
        private readonly IOffPeakService mOffPeakService;
        private readonly IDeleteContactsService mDeleteContactsService;
        

        /// <summary>
        /// Constructor
        /// </summary>
        public DeleteInactiveContacts() : this(Service.Resolve<IDateTimeNowService>(), Service.Resolve<IOffPeakService>(), Service.Resolve<IDeleteContactsService>())
        {
            
        }


        internal DeleteInactiveContacts(IDateTimeNowService dateTimeNowService, IOffPeakService offPeakService, IDeleteContactsService deleteContactsService)
        {
            mDateTimeNowService = dateTimeNowService;
            mOffPeakService = offPeakService;
            mDeleteContactsService = deleteContactsService;
        }

        /// <summary>
        /// Deletes inactive contacts.
        /// Takes a batch of 1000 which is deleted and reschedules itself if there are more to delete. 
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            if (InsufficientLicense())
            {
                return String.Format(ResHelper.GetString(INSUFFICIENT_LICENSE_MESSAGE_RESOURCE), INSUFFICIENT_LICENSE_MESSAGE);
            }
            
            DateTime now = mDateTimeNowService.GetDateTimeNow();
            DateTime nextRunTime;
            string result;
            int contactsToDeleteInOneBatch = ValidationHelper.GetInteger(task.TaskData, 1000);

            if (mOffPeakService.IsOffPeak(now))
            {
                var moreContactsToDelete = mDeleteContactsService.Delete(contactsToDeleteInOneBatch);
                if (moreContactsToDelete)
                {
                    nextRunTime = mDateTimeNowService.GetDateTimeNow().AddMinutes(1);
                    result = RESCHEDULED_TO_DELETE_MORE_CONTACTS_IN_A_MINUTE;
                }
                else
                {
                    nextRunTime = mOffPeakService.GetNextOffPeakPeriodStart(now);
                    result = NO_MORE_CONTACTS_TO_DELETE_RESCHEDULED_TO_TRY_AGAIN_IN_THE_NEXT_OFF_PEAK_PERIOD;
                }
            }
            else
            {
                nextRunTime = mOffPeakService.GetNextOffPeakPeriodStart(now);
                result = RESCHEDULED_TO_DELETE_MORE_CONTACTS_IN_NEXT_OFF_PEAK_PERIOD;
            }

            task.TaskInterval = SchedulingHelper.EncodeInterval(new TaskInterval
            {
                Period = SchedulingHelper.PERIOD_ONCE,
                UseSpecificTime = true,
                StartTime = nextRunTime
            });

            return result;
        }


        private bool InsufficientLicense()
        {
            return !LicenseHelper.IsFeatureAvailableInBestLicense(FeatureEnum.FullContactManagement, ModuleName.CONTACTMANAGEMENT);
        }
    }
}