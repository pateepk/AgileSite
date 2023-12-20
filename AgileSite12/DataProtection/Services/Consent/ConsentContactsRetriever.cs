using CMS.ContactManagement;
using CMS.DataEngine;

namespace CMS.DataProtection.Internal
{
    /// <summary>
    /// Retrieves contacts who have agreed to the given consent.
    /// </summary>
    public sealed class ConsentContactsRetriever
    {
        private readonly ConsentInfo consent;


        /// <summary>
        /// Initializes a new instance of the <see cref="ConsentContactsRetriever"/> class.
        /// </summary>
        /// <param name="consent">The consent which contacts will be returned for.</param>
        public ConsentContactsRetriever(ConsentInfo consent)
        {
            this.consent = consent;
        }


        /// <summary>
        /// Returns contacts who agreed to the consent.
        /// </summary>
        public ObjectQuery<ContactInfo> GetContacts()
        {
            var contactIdsWhoAgreed = ConsentAgreementService.GetContactIDsWhoAgreed(consent);

            return ContactInfoProvider.GetContacts()
                                      .WhereIn("ContactID", contactIdsWhoAgreed);
        }
    }
}
