using System.Collections.Generic;

using CMS;
using CMS.ContactManagement.Web.UI;
using CMS.ContactManagement.Web.UI.Internal;

[assembly: RegisterImplementation(typeof(IContactDetailsControllerService), typeof(ContactDetailsControllerService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement.Web.UI.Internal
{
    /// <summary>
    /// Provides service methods used in <see cref="ContactDetailsController"/>.
    /// </summary>
    public interface IContactDetailsControllerService
    {
        /// <summary>
        /// Registers given <paramref name="fieldResolver"/> for given <paramref name="fieldName"/>. Once the field with <paramref name="fieldName"/> will be 
        /// proceeded, result of <paramref name="fieldResolver"/> will be used as the field value.
        /// </summary>
        /// <param name="fieldName">Name of the field the <paramref name="fieldResolver"/> is registered for</param>
        /// <param name="fieldResolver">Implementation fo <see cref="IContactDetailsFieldResolver"/> resolving the value for given <paramref name="fieldName"/></param>
        void RegisterContactDetailsFieldResolver(string fieldName, IContactDetailsFieldResolver fieldResolver);


        /// <summary>
        /// Gets collection of <see cref="ContactDetailsViewModel"/> for the given <paramref name="contactID"/>. 
        /// </summary>
        /// <param name="contactID">ID of contact the collection of <see cref="ContactDetailsViewModel"/> is obtained for</param>
        /// <returns>Collection of <see cref="ContactDetailsViewModel"/> for the given <paramref name="contactID"/></returns>
        IEnumerable<ContactDetailsViewModel> GetContactDetailsViewModel(int contactID);
    }
}