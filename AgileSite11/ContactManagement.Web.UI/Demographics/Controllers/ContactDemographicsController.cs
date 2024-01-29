using System;
using System.Collections.Generic;

using CMS.ContactManagement.Web.UI.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(ContactDemographicsController))]

namespace CMS.ContactManagement.Web.UI.Internal
{
    /// <summary>
    /// Provides endpoint for retrieving demographic statistics of all <see cref="ContactInfo"/>s.
    /// </summary>
    /// <exclude />
    [IsFeatureAvailable(FeatureEnum.FullContactManagement)]
    [AllowOnlyEditor]
    [IsAuthorizedPerResource(ModuleName.CONTACTMANAGEMENT, "Read")]
    [HandleExceptions]
    public sealed class ContactDemographicsController : CMSApiController
    {
        private readonly IContactDemographicsControllerService mContactDemographicsControllerService;
        
        /// <summary>
		/// Instantiates new instance of <see cref="ContactDemographicsController"/>.
		/// </summary>
		public ContactDemographicsController() : this(Service.Resolve<IContactDemographicsControllerService>()) { }


        internal ContactDemographicsController(IContactDemographicsControllerService contactDemographicsControllerService)
        {
            mContactDemographicsControllerService = contactDemographicsControllerService;
        }


        /// <summary>
        /// Returns collection of countries associated with a number of contacts from the country. 
        /// </summary>
        /// <param name="retrieverIdentifier">Specifies which implementation of <see cref="IContactDemographicsDataRetriever"/> should be used</param>
        /// <exception cref="ArgumentException"><paramref name="retrieverIdentifier"/> is empty</exception>
        public IEnumerable<ContactsGroupedByLocationViewModel> GetGroupedByCountry(string retrieverIdentifier)
        {
            EnsureRetrieverIdentifier(retrieverIdentifier);
            return mContactDemographicsControllerService.GetGroupedByCountry(retrieverIdentifier);
        }


        /// <summary>
        /// Returns collection of USA's states associated with a number of contacts from the state. 
        /// </summary>
        /// <param name="retrieverIdentifier">Specifies which implementation of <see cref="IContactDemographicsDataRetriever"/> should be used</param>
        /// <exception cref="ArgumentException"><paramref name="retrieverIdentifier"/> is empty</exception>
        public IEnumerable<ContactsGroupedByLocationViewModel> GetGroupedByUSAStates(string retrieverIdentifier)
        {
            EnsureRetrieverIdentifier(retrieverIdentifier);
            return mContactDemographicsControllerService.GetGroupedByUSAStates(retrieverIdentifier);
        }


        /// <summary>
        /// Returns collection of genders associated with a number of contacts in the gender. 
        /// </summary>
        /// <param name="retrieverIdentifier">Specifies which implementation of <see cref="IContactDemographicsDataRetriever"/> should be used</param>
        /// <exception cref="ArgumentException"><paramref name="retrieverIdentifier"/> is empty</exception>
        public IEnumerable<ContactsGroupedByGenderViewModel> GetGroupedByGender(string retrieverIdentifier)
        {
            EnsureRetrieverIdentifier(retrieverIdentifier);
            return mContactDemographicsControllerService.GetGroupedByGender(retrieverIdentifier);
        }


        /// <summary>
        /// Returns collection of age categories associated with a number of contacts in each category.
        /// </summary>
        /// <param name="retrieverIdentifier">Specifies which implementation of <see cref="IContactDemographicsDataRetriever"/> should be used</param>
        /// <exception cref="ArgumentException"><paramref name="retrieverIdentifier"/> is empty</exception>
        public IEnumerable<ContactsGroupedByAgeViewModel> GetGroupedByAge(string retrieverIdentifier)
        {
            EnsureRetrieverIdentifier(retrieverIdentifier);
            return mContactDemographicsControllerService.GetGroupedByAge(retrieverIdentifier);
        }
        

        private void EnsureRetrieverIdentifier(string retrieverIdentifier)
        {
            if (string.IsNullOrEmpty(retrieverIdentifier))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(retrieverIdentifier));
            }
        }
    }
}