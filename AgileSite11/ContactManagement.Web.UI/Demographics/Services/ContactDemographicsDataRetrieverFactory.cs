using System;
using System.Collections.Generic;

using CMS.ContactManagement.Web.UI.Internal;
using CMS.Core;

namespace CMS.ContactManagement.Web.UI
{
    internal class ContactDemographicsDataRetrieverFactory : IContactDemographicsDataRetrieverFactory
    {
        private readonly Dictionary<string, Type> mRegisteredRetrievers = new Dictionary<string, Type>();


        public void Register(string identifier, Type contactDemographicsDataRetriever)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("Identifier has to be specified.", nameof(identifier));
            }

            if (contactDemographicsDataRetriever == null)
            {
                throw new ArgumentNullException(nameof(contactDemographicsDataRetriever));
            }

            if (!typeof(IContactDemographicsDataRetriever).IsAssignableFrom(contactDemographicsDataRetriever))
            {
                throw new ArgumentException($"Given type has to be assignable from {nameof(IContactDemographicsDataRetriever)}.", nameof(contactDemographicsDataRetriever));    
            }

            Service.Use(contactDemographicsDataRetriever, contactDemographicsDataRetriever, $"{contactDemographicsDataRetriever.FullName}-{Guid.NewGuid()}");
            mRegisteredRetrievers.Add(identifier, contactDemographicsDataRetriever);
        }


        public IContactDemographicsDataRetriever Get(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("Identifier has to be specified", nameof(identifier));
            }

            if (!mRegisteredRetrievers.ContainsKey(identifier))
            {
                throw new InvalidOperationException("No retriever was registered with given identifier.");
            }

            return (IContactDemographicsDataRetriever) Service.Resolve(mRegisteredRetrievers[identifier]);
        }
    }
}