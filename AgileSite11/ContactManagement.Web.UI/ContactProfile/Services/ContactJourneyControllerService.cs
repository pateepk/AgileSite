namespace CMS.ContactManagement.Web.UI.Internal
{
    internal class ContactJourneyControllerService : IContactJourneyControllerService
    {
        private readonly IContactJourneyService mContactJourneyService;


        public ContactJourneyControllerService(IContactJourneyService contactJourneyService)
        {
            mContactJourneyService = contactJourneyService;
        }

        public ContactJourneyViewModel GetContactJourneyForContact(int contactID)
        {
            return mContactJourneyService.GetContactJourneyForContact(contactID);
        }
    }
}