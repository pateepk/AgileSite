using System;

using CMS.ContactManagement;
using CMS.DataEngine;

namespace CMS.DataProtection
{
    /// <summary>
    /// Service to agree consent in forms.
    /// </summary>
    internal sealed class FormConsentAgreementService : IFormConsentAgreementService
    {
        private readonly IContactDataInjector contactDataInjector;
        private readonly IConsentAgreementService consentAgreementService;
        private readonly IContactCreator contactCreator;
        private readonly IContactPersistentStorage contactPersistentStorage;


        /// <summary>
        /// Creates an instance of <see cref="FormConsentAgreementService"/> class.
        /// </summary>
        /// <param name="contactDataInjector">Contact data injector.</param>
        /// <param name="consentAgreementService">Contact agreement service.</param>
        /// <param name="contactCreator">Contact creator.</param>
        /// <param name="contactPersistentStorage">Contact persistent storage.</param>
        public FormConsentAgreementService(IContactDataInjector contactDataInjector, IConsentAgreementService consentAgreementService, IContactCreator contactCreator, IContactPersistentStorage contactPersistentStorage)
        {
            this.contactDataInjector = contactDataInjector;
            this.consentAgreementService = consentAgreementService;
            this.contactCreator = contactCreator;
            this.contactPersistentStorage = contactPersistentStorage;
        }


        /// <summary>
        /// Agrees the given <see cref="ConsentInfo">consent</see> with the given <see cref="ContactInfo">contact</see>.
        /// </summary>
        /// <param name="contact">Contact.</param>
        /// <param name="consent">Consent.</param>
        /// <param name="data">Info object being edited by the form.</param>
        /// <returns>The <see cref="ConsentAgreementInfo">agreement</see>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="consent"/> or <paramref name="data"/> is null.</exception>
        /// <remarks>When contact is null, ensures contact and maps data to a contact based on mapping configuration of an object.</remarks>
        public ConsentAgreementInfo Agree(ContactInfo contact, ConsentInfo consent, BaseInfo data)
        {
            if (consent == null)
            {
                throw new ArgumentNullException(nameof(consent));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (contact == null)
            {
                contact = MapAndSetAnonymousContact(data);
            }

            return consentAgreementService.Agree(contact, consent);
        }


        /// <summary>
        /// Revokes the given <see cref="ConsentInfo">consent</see> for the given <see cref="ContactInfo">contact</see>.
        /// </summary>
        /// <param name="contact">Contact.</param>
        /// <param name="consent">Consent.</param>
        /// <param name="data">Info object being edited by the form.</param>
        /// <returns>The revoked <see cref="ConsentAgreementInfo">agreement</see>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="consent"/> or <paramref name="data"/> is null.</exception>
        /// <remarks>When contact is null, ensures contact and maps data to a contact based on mapping configuration of an object.</remarks>
        public ConsentAgreementInfo Revoke(ContactInfo contact, ConsentInfo consent, BaseInfo data)
        {
            if (consent == null)
            {
                throw new ArgumentNullException(nameof(consent));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (contact == null)
            {
                contact = MapAndSetAnonymousContact(data);
            }

            return consentAgreementService.Revoke(contact, consent);
        }


        private ContactInfo MapAndSetAnonymousContact(BaseInfo data)
        {
            var contact = contactCreator.CreateAnonymousContact();

            var dci = DataClassInfoProvider.GetDataClassInfo(data.TypeInfo.ObjectClassName);
            if (dci != null)
            {
                var mapper = new ContactDataMapper(dci.ClassName, dci.ClassContactOverwriteEnabled);
                contactDataInjector.Inject(data, contact.ContactID, mapper);
            }

            contactPersistentStorage.SetPersistentContact(contact);

            return contact;
        }
    }

}
