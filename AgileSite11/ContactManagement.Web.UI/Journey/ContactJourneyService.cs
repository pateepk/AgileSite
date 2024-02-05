using System;

using CMS.Activities;
using CMS.ContactManagement.Web.UI.Internal;
using CMS.Core;
using CMS.Core.Internal;
using CMS.Helpers;

namespace CMS.ContactManagement.Web.UI
{
    internal class ContactJourneyService : IContactJourneyService
    {
        private readonly ILocalizationService mLocalizationService;
        private readonly IDateTimeNowService mDateTimeNowService;


        public ContactJourneyService(ILocalizationService localizationService, IDateTimeNowService dateTimeNowService)
        {
            mLocalizationService = localizationService;
            mDateTimeNowService = dateTimeNowService;
        }


        public ContactJourneyViewModel GetContactJourneyForContact(int contactID)
        {
            var firstActivity = ActivityInfoProvider.GetContactsFirstActivity(contactID, "");
            if (firstActivity == null)
            {
                return null;
            }

            var lastActivity = ActivityInfoProvider.GetContactsLastActivity(contactID, "");

            return new ContactJourneyViewModel
            {
                JourneyLengthDaysText = GetJourneyLengthText(firstActivity),
                JourneyLengthStartedDate = GetJourneyLengthStartDate(firstActivity),
                LastActivityDaysAgoText = GetLastActivityText(lastActivity),
                LastActivityDate = GetFormattedActivityCreationDate(lastActivity)
            };
        }


        private string GetJourneyLengthStartDate(ActivityInfo firstActivity)
        {
            return string.Format(mLocalizationService.GetString("om.contact.journey.started"), GetFormattedActivityCreationDate(firstActivity));
        }


        private string GetFormattedActivityCreationDate(ActivityInfo activity)
        {
            return activity.ActivityCreated.ToString("d", CultureHelper.PreferredUICultureInfo);
        }


        private string GetLastActivityText(ActivityInfo lastActivity)
        {
            int numberOfDays = (mDateTimeNowService.GetDateTimeNow() - lastActivity.ActivityCreated.Date).Days;
            switch (numberOfDays)
            {
                case 0:
                    return mLocalizationService.GetString("om.contact.journey.lastactivityvalue.today");
                case 1:
                    return string.Format(mLocalizationService.GetString("om.contact.journey.lastactivityvalue.singular"), GetFormattedNumber(numberOfDays));
                default:
                    return string.Format(mLocalizationService.GetString("om.contact.journey.lastactivityvalue.plural"), GetFormattedNumber(numberOfDays));
            }
        }


        private string GetJourneyLengthText(ActivityInfo firstActivity)
        {
            // Journey length is always at least 1 because all days are counted (journey has started yesterday so today it's 2 days)
            int numberOfDays = (mDateTimeNowService.GetDateTimeNow() - firstActivity.ActivityCreated).Days + 1;
            if (numberOfDays == 1)
            {
                return string.Format(mLocalizationService.GetString("om.contact.journey.lengthvalue.singular"), GetFormattedNumber(numberOfDays));
            }

            return string.Format(mLocalizationService.GetString("om.contact.journey.lengthvalue.plural"), GetFormattedNumber(numberOfDays));
        }


        private string GetFormattedNumber(int number)
        {
            return number.ToString("N0", CultureHelper.PreferredUICultureInfo);
        }
    }
}