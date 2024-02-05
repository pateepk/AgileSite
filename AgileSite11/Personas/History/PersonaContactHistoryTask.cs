using System;

using CMS.Core;
using CMS.Core.Internal;
using CMS.DataEngine;
using CMS.Scheduler;

namespace CMS.Personas.Internal
{
    /// <summary>
    /// Scheduled task for persona history recalculation.
    /// </summary>    
    public class PersonaContactHistoryTask : ITask
    {
        internal const string INSUFFICIENT_LICENSE_MESSAGE_RESOURCE = "licenselimitation.featurenotavailable";
        internal const string PERSONA_HISTORY_WAS_ALREADY_STORED_TODAY = "personas.personacontacthistory.alreadystoredtoday";

        private readonly IPersonaSnapshooter mPersonaSnapshooter;
        internal ILicenseService mLicenseService;
        private readonly ILocalizationService mLocalizationService;
        private readonly IDateTimeNowService mDateTimeNowService;


        /// <summary>
        /// Instantiates new instance of <see cref="PersonaContactHistoryTask"/>.
        /// </summary>
        public PersonaContactHistoryTask() : this(Service.Resolve<IPersonaSnapshooter>(), Service.Resolve<ILocalizationService>(), Service.Resolve<IDateTimeNowService>())
        {}


        /// <summary>
        /// Instantiates new instance of <see cref="PersonaContactHistoryTask"/>.
        /// </summary>
        public PersonaContactHistoryTask(IPersonaSnapshooter personaSnapshooter, ILocalizationService localizationService, IDateTimeNowService dateTimeNowService)
        {
            mPersonaSnapshooter = personaSnapshooter;
            mLicenseService = ObjectFactory<ILicenseService>.StaticSingleton();
            mLocalizationService = localizationService;
            mDateTimeNowService = dateTimeNowService;
        }


        /// <summary>
        /// Stores the current contact/persona distribution.
        /// </summary>
        public string Execute(TaskInfo task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (InsufficientLicense())
            {
                return string.Format(mLocalizationService.GetString(INSUFFICIENT_LICENSE_MESSAGE_RESOURCE), $"{FeatureEnum.FullContactManagement} or {FeatureEnum.Personas}");
            }

            if (HistoryWasAlreadyStoredToday())
            {
                return mLocalizationService.GetString(PERSONA_HISTORY_WAS_ALREADY_STORED_TODAY);
            }

            PersonaContactHistoryInfoProvider.BulkInsert(mPersonaSnapshooter.GetSnapshotOfCurrentState());

            DateTime tomorrowAtTwoAM = mDateTimeNowService.GetDateTimeNow().AddDays(1).Date.AddHours(2);
            task.TaskInterval = SchedulingHelper.EncodeInterval(new TaskInterval
            {
                Period = SchedulingHelper.PERIOD_ONCE,
                UseSpecificTime = true,
                StartTime = tomorrowAtTwoAM
            });

            return null;
        }


        private bool HistoryWasAlreadyStoredToday()
        {
            return PersonaContactHistoryInfoProvider.GetPersonaContactHistory().WhereEquals("PersonaContactHistoryDate", mDateTimeNowService.GetDateTimeNow().Date).Count > 0;
        }


        private bool InsufficientLicense()
        {
            return !mLicenseService.IsFeatureAvailable(FeatureEnum.FullContactManagement) || !mLicenseService.IsFeatureAvailable(FeatureEnum.Personas);
        }
    }
}