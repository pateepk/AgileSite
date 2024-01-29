using System;

using CMS.Base;
using CMS.ContactManagement;

namespace CMS.Newsletters
{
    internal class Recipient : AbstractDataContainer<Recipient>
    {
        private const string UNKNOWN_CONTACT = "UNKNOWN_CONTACT";
        private readonly SubscriberInfo subscriber;
        private ContactInfo mContact;


        private ContactInfo Contact
        {
            get
            {
                if (mContact == null)
                {
                    mContact = NewsletterHelper.GetContactInfo(subscriber);
                }

                return mContact;
            }
        }


        #region Properties

        [RegisterColumn]
        public string FirstName => subscriber.SubscriberFirstName;


        [RegisterColumn]
        public string LastName => subscriber.SubscriberLastName;


        [RegisterColumn]
        public string Email => subscriber.SubscriberEmail;


        [RegisterColumn]
        public string CompanyName => Contact?.ContactCompanyName;


        [RegisterColumn]
        public DateTime? Birthday => Contact?.ContactBirthday;


        [RegisterColumn]
        public int PersonaID => Contact?.ContactPersonaID ?? 0;


        [RegisterColumn]
        public string StrandsUserID => Contact?.ContactGUID.ToString() ?? UNKNOWN_CONTACT;


        [RegisterColumn(Hidden = true)]
        public bool IsFake => subscriber.SubscriberID <= 0;

        #endregion


        public Recipient(SubscriberInfo subscriber)
        {
            this.subscriber = subscriber;
        }
    }
}
