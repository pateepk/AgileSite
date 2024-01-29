using System.Collections.Generic;

using CMS;
using CMS.ContactManagement.Web.UI;
using CMS.ContactManagement.Web.UI.Internal;
using CMS.DataEngine;

[assembly: RegisterImplementation(typeof(IContactDemographicsGroupService), typeof(ContactDemographicsGroupService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement.Web.UI
{
    internal interface IContactDemographicsGroupService
    { 
        IEnumerable<ContactsGroupedByLocationViewModel> GroupContactsByCountry(ObjectQuery<ContactInfo> contacts);


        IEnumerable<ContactsGroupedByLocationViewModel> GroupContactsByState(ObjectQuery<ContactInfo> contacts);


        IEnumerable<ContactsGroupedByGenderViewModel> GroupContactsByGender(ObjectQuery<ContactInfo> contacts);


        IEnumerable<ContactsGroupedByAgeViewModel> GroupContactsByAge(ObjectQuery<ContactInfo> contacts);
    }
}
