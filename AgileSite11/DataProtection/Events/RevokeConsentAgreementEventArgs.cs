using CMS.Base;
using CMS.ContactManagement;

namespace CMS.DataProtection
{
    /// <summary>
    /// Arguments of the event represented by <see cref="RevokeConsentAgreementHandler"/>.
    /// </summary>
    public sealed class RevokeConsentAgreementEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Gets the consent agreement that has been revoked.
        /// </summary>
        public ConsentAgreementInfo ConsentAgreement
        {
            get;
            internal set;
        }


        /// <summary>
        /// Gets the consent that has been revoked.
        /// </summary>
        public ConsentInfo Consent
        {
            get;
            internal set;
        }


        /// <summary>
        /// Gets the contact who has revoked the consent.
        /// </summary>
        public ContactInfo Contact
        {
            get;
            internal set;
        }
    }
}
