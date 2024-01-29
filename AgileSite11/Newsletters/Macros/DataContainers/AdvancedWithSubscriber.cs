using CMS.ContactManagement;

namespace CMS.Newsletters
{
    internal class AdvancedWithSubscriber : Advanced
    {
        private readonly SubscriberInfo subscriber;
        private ContactInfo mContact;


        #region Properties

        [RegisterColumn]
        public ContactInfo ContactInfo
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

        #endregion


        public AdvancedWithSubscriber(NewsletterInfo newsletter, IssueInfo issue, SubscriberInfo subscriber)
            :base (newsletter, issue)
        {
            this.subscriber = subscriber;
        }
    }
}
