using CMS.Core;

namespace CMS.Newsletters
{
    /// <summary>
    /// Email address blocked which is retrieving data from database for every checked email.
    /// </summary>
    internal class OnDemandUnsubscribedEmailsAddressBlocker : IEmailAddressBlocker
    {
        private readonly int mNewsletterID;
        private readonly IUnsubscriptionProvider mUnsubscriptionProvider = Service.Resolve<IUnsubscriptionProvider>();

        public OnDemandUnsubscribedEmailsAddressBlocker(int newsletterID)
        {
            mNewsletterID = newsletterID;
        }


        public bool IsBlocked(string emailAddress)
        {
            return mUnsubscriptionProvider.IsUnsubscribedFromSingleNewsletter(emailAddress, mNewsletterID);
        }
    }
}