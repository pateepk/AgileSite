using System;
using System.Collections.Generic;

using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Personas.Web.UI.Internal;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(ContactPersonaDemographicsController))]

namespace CMS.Personas.Web.UI.Internal
{
    /// <summary>
    /// Provides endpoint for retrieving persona-based demographic statistics of all <see cref="ContactInfo"/>s.
    /// </summary>
    /// <exclude />
    [IsFeatureAvailable(FeatureEnum.FullContactManagement)]
    [AllowOnlyEditor]
    [IsAuthorizedPerResource(ModuleName.CONTACTMANAGEMENT, "Read")]
    [IsAuthorizedPerResource(ModuleName.PERSONAS, "Read")]
    [HandleExceptions]
    public sealed class ContactPersonaDemographicsController : CMSApiController
    {
        private readonly IContactPersonaDemographicsControllerService mContactPersonaDemographicsControllerService;

        /// <summary>
		/// Instantiates new instance of <see cref="ContactPersonaDemographicsController"/>.
		/// </summary>
		public ContactPersonaDemographicsController() : this(Service.Resolve<IContactPersonaDemographicsControllerService>()) { }


        internal ContactPersonaDemographicsController(IContactPersonaDemographicsControllerService contactPersonaDemographicsControllerService)
        {
            mContactPersonaDemographicsControllerService = contactPersonaDemographicsControllerService;
        }


        /// <summary>
        /// Returns collection of personas associated with a number of contacts belonging to the persona. 
        /// </summary>
        public IEnumerable<ContactsGroupedByPersonaViewModel> GetGroupedByPersona(string retrieverIdentifier)
        {
            if (string.IsNullOrEmpty(retrieverIdentifier))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(retrieverIdentifier));
            }

            return mContactPersonaDemographicsControllerService.GetGroupedByPersona(retrieverIdentifier);
        }
    }
}