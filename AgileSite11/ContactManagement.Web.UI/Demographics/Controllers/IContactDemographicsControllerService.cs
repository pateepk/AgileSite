using System.Collections.Generic;

using CMS;
using CMS.ContactManagement.Web.UI;
using CMS.ContactManagement.Web.UI.Internal;

[assembly: RegisterImplementation(typeof(IContactDemographicsControllerService), typeof(ContactDemographicsControllerService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement.Web.UI
{
    internal interface IContactDemographicsControllerService
    {
        IEnumerable<ContactsGroupedByLocationViewModel> GetGroupedByCountry(string retrieverIdentifier);


        IEnumerable<ContactsGroupedByLocationViewModel> GetGroupedByUSAStates(string retrieverIdentifier);


        IEnumerable<ContactsGroupedByGenderViewModel> GetGroupedByGender(string retrieverIdentifier);


        IEnumerable<ContactsGroupedByAgeViewModel> GetGroupedByAge(string retrieverIdentifier);
    }
}