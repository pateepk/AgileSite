using System;
using System.Collections.Generic;

using CMS;
using CMS.ContactManagement;
using CMS.DataProtection;

[assembly: RegisterImplementation(typeof(IConsentAgreementService), typeof(ConsentAgreementService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.DataProtection
{
    /// <summary>
    /// Service to manage consent agreements.
    /// </summary>
    public interface IConsentAgreementService
    {
        /// <summary>
        /// Inserts an <see cref="ConsentAgreementInfo">agreement</see> of the given <see cref="ConsentInfo">consent</see> and the given <see cref="ContactInfo">contact</see>.
        /// </summary>
        /// <remarks>
        /// If an active agreement already exists then this active agreement is kept and its hash is updated with a one from the given consent.
        /// </remarks>
        /// <param name="contact">Contact</param>
        /// <param name="consent">Consent</param>
        /// <returns>Returns the agreement.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="contact"/> is <c>null</c> -or <paramref name="consent"/> is <c>null</c></exception>
        ConsentAgreementInfo Agree(ContactInfo contact, ConsentInfo consent);


        /// <summary>
        /// Indicates whether the given <see cref="ContactInfo">contact</see> has agreed with the specified <see cref="ConsentInfo">consent</see>.
        /// </summary>
        /// <param name="contact">Contact.</param>
        /// <param name="consent">Consent.</param>
        /// <returns>Returns <c>true</c> when the given contact agreed with the specified consent and is still valid.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="contact"/> is <c>null</c> -or <paramref name="consent"/> is <c>null</c></exception>
        bool IsAgreed(ContactInfo contact, ConsentInfo consent);


        /// <summary>
        /// Revokes an agreement of the given <see cref="ConsentInfo">consent</see> for the given <see cref="ContactInfo">contact</see>.
        /// </summary>
        /// <param name="contact">Contact</param>
        /// <param name="consent">Consent</param>
        /// <returns>Returns the agreement object that has been revoked.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="contact"/> is <c>null</c> -or <paramref name="consent"/> is <c>null</c></exception>
        ConsentAgreementInfo Revoke(ContactInfo contact, ConsentInfo consent);


        /// <summary>
        /// Returns agreed consents for the given contact.
        /// </summary>
        /// <param name="contact">Contact.</param>
        /// <remarks>Only returns agreed consents, revoked consents are not included.</remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="contact"/> is <c>null</c>.</exception>
        IEnumerable<Consent> GetAgreedConsents(ContactInfo contact);
    }
}
