using System;
using System.Collections.Generic;
using System.Collections.Specialized;

using CMS.ContactManagement.Internal;
using CMS.ContactManagement.Web.UI.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.ContactManagement.Web.UI
{
    internal class ContactDemographicsControllerService : IContactDemographicsControllerService
    {
        private readonly IContactDemographicsGroupService mContactDemographicsGroupService;


        public ContactDemographicsControllerService(IContactDemographicsGroupService contactDemographicsGroupService)
        {
            mContactDemographicsGroupService = contactDemographicsGroupService;
        }


        public IEnumerable<ContactsGroupedByLocationViewModel> GetGroupedByCountry(string retrieverIdentifier)
        {
            return mContactDemographicsGroupService.GroupContactsByCountry(GetContacts(retrieverIdentifier));
        }

        
        public IEnumerable<ContactsGroupedByLocationViewModel> GetGroupedByUSAStates(string retrieverIdentifier)
        {
            return mContactDemographicsGroupService.GroupContactsByState(GetContacts(retrieverIdentifier));
        }


        public IEnumerable<ContactsGroupedByGenderViewModel> GetGroupedByGender(string retrieverIdentifier)
        {
            return mContactDemographicsGroupService.GroupContactsByGender(GetContacts(retrieverIdentifier));
        }


        public IEnumerable<ContactsGroupedByAgeViewModel> GetGroupedByAge(string retrieverIdentifier)
        {
            return mContactDemographicsGroupService.GroupContactsByAge(GetContacts(retrieverIdentifier));
        }


        private ObjectQuery<ContactInfo> GetContacts(string retrieverIdentifier)
        {
            return GetRetriever(retrieverIdentifier).GetContactObjectQuery(GetQueryParameters());
        }


        private IContactDemographicsDataRetriever GetRetriever(string retrieverIdentifier)
        {
            return Service.Resolve<IContactDemographicsDataRetrieverFactory>().Get(retrieverIdentifier);
        }


        private NameValueCollection GetQueryParameters()
        {
            return CMSHttpContext.Current.Request.QueryString;
        }
    }
}