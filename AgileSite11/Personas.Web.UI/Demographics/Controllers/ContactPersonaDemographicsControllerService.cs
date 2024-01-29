using System.Collections.Generic;
using System.Collections.Specialized;

using CMS.ContactManagement;
using CMS.ContactManagement.Web.UI.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Personas.Web.UI.Internal
{
    internal class ContactPersonaDemographicsControllerService : IContactPersonaDemographicsControllerService
    {
        private readonly IContactPersonaDemographicsGroupService mContactPersonaDemographicsGroupService;


        public ContactPersonaDemographicsControllerService(IContactPersonaDemographicsGroupService contactPersonaDemographicsGroupService)
        {
            mContactPersonaDemographicsGroupService = contactPersonaDemographicsGroupService;
        }


        public IEnumerable<ContactsGroupedByPersonaViewModel> GetGroupedByPersona(string retrieverIdentifier)
        {
            return mContactPersonaDemographicsGroupService.GroupContactsByPersona(GetContacts(retrieverIdentifier));
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