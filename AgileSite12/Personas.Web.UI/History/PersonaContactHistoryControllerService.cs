using System.Collections.Generic;
using System.Linq;

using CMS.Core.Internal;
using CMS.Personas.Web.UI.Internal;

namespace CMS.Personas.Web.UI
{
    internal class PersonaContactHistoryControllerService : IPersonaContactHistoryControllerService
    {
        private readonly IDateTimeNowService mDateTimeNowService;


        public PersonaContactHistoryControllerService(IDateTimeNowService dateTimeNowService)
        {
            mDateTimeNowService = dateTimeNowService;
        }


        public IEnumerable<PersonaContactHistoryViewModel> GetPersonaContactHistoryData()
        {
            return PersonaContactHistoryInfoProvider.GetPersonaContactHistory()
                                                    .WhereLessOrEquals("PersonaContactHistoryDate", mDateTimeNowService.GetDateTimeNow().Date)
                                                    .OrderBy("PersonaContactHistoryDate")
                                                    .Select(c =>
                                                            new PersonaContactHistoryViewModel
                                                            {
                                                                PersonaID = c.PersonaContactHistoryPersonaID,
                                                                Date = c.PersonaContactHistoryDate,
                                                                Contacts = c.PersonaContactHistoryContacts
                                                            }
                                                    );
        }
    }
}