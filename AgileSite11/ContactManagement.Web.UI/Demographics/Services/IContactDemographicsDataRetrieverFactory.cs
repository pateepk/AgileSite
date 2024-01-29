using System;

using CMS;
using CMS.ContactManagement.Web.UI;
using CMS.ContactManagement.Web.UI.Internal;

[assembly: RegisterImplementation(typeof(IContactDemographicsDataRetrieverFactory), typeof(ContactDemographicsDataRetrieverFactory), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement.Web.UI.Internal
{
    /// <summary>
    /// Provides methods for registering and retrieving implementations of <see cref="IContactDemographicsDataRetriever"/>.
    /// </summary>
    public interface IContactDemographicsDataRetrieverFactory
    {
        /// <summary>
        /// Registers given <paramref name="contactDemographicsDataRetriever"/> under the given <paramref name="identifier"/>.
        /// </summary>
        /// <param name="identifier">Identifier the given <paramref name="contactDemographicsDataRetriever"/> is registered under</param>
        /// <param name="contactDemographicsDataRetriever">Implementation of <see cref="IContactDemographicsDataRetriever"/></param>
        void Register(string identifier, Type contactDemographicsDataRetriever);


        /// <summary>
        /// Gets implementation of <see cref="IContactDemographicsDataRetriever"/> previously registered under given <paramref name="identifier"/>.
        /// </summary>
        /// <param name="identifier">Identifier the desired <see cref="IContactDemographicsDataRetriever"/> is registered under</param>
        IContactDemographicsDataRetriever Get(string identifier);
    }
}