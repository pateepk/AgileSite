using System;

using CMS;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.DataProtection;

[assembly: RegisterImplementation(typeof(IFormConsentAgreementService), typeof(FormConsentAgreementService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.DataProtection
{

    /// <summary>
    /// Service to agree consent in forms.
    /// </summary>
    public interface IFormConsentAgreementService
    {
        /// <summary>
        /// Agrees the given <see cref="ConsentInfo">consent</see> with the given <see cref="ContactInfo">contact</see>.
        /// </summary>
        /// <param name="contact">Contact.</param>
        /// <param name="consent">Consent.</param>
        /// <param name="data">Info object being edited by the form.</param>
        /// <returns>The <see cref="ConsentAgreementInfo">agreement</see>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="consent"/> or <paramref name="data"/> is null.</exception>
        /// <remarks>When contact is null, ensures contact and maps data to a contact based on mapping configuration of an object.</remarks>
        ConsentAgreementInfo Agree(ContactInfo contact, ConsentInfo consent, BaseInfo data);


        /// <summary>
        /// Revokes the given <see cref="ConsentInfo">consent</see> for the given <see cref="ContactInfo">contact</see>.
        /// </summary>
        /// <param name="contact">Contact.</param>
        /// <param name="consent">Consent.</param>
        /// <param name="data">Info object being edited by the form.</param>
        /// <returns>The revoked <see cref="ConsentAgreementInfo">agreement</see>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="consent"/> or <paramref name="data"/> is null.</exception>
        /// <remarks>When contact is null, ensures contact and maps data to a contact based on mapping configuration of an object.</remarks>
        ConsentAgreementInfo Revoke(ContactInfo contact, ConsentInfo consent, BaseInfo data);
    }
}
