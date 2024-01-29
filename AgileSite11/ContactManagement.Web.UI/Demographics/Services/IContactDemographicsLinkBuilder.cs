using System.Collections.Specialized;

using CMS;
using CMS.ContactManagement.Web.UI;
using CMS.ContactManagement.Web.UI.Internal;

[assembly: RegisterImplementation(typeof(IContactDemographicsLinkBuilder), typeof(ContactDemographicsLinkBuilder), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement.Web.UI.Internal
{
    /// <summary>
    /// Provides method for obtaining the link leading to the contact demographics control.
    /// </summary>
    public interface IContactDemographicsLinkBuilder
    {
        /// <summary>
        /// Gets the link leading to the contact demographics control.
        /// </summary>
        string GetDemographicsLink(string retrieverIdentifier, NameValueCollection additionalParameters = null);
    }
}