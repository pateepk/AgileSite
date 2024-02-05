using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Core;
using CMS.Helpers;

namespace CMS.Newsletters
{
    /// <summary>
    /// Email blocker which caches all the unsubscriptions in the memory when the first email is checked. Subsequent checks do not query database.
    /// </summary>
    internal class PreloadedUnsubscribedEmailsAddressBlocker : IEmailAddressBlocker
    {
        private readonly Lazy<ISet<string>> mLazyUnsubscribedEmails;

        public PreloadedUnsubscribedEmailsAddressBlocker(int newsletterID)
        {
            mLazyUnsubscribedEmails = new Lazy<ISet<string>>(() => LoadUnsubscribedEmails(newsletterID));
        }


        public bool IsBlocked(string emailAddress)
        {
            return mLazyUnsubscribedEmails.Value.Contains(emailAddress);
        }


        private ISet<string> LoadUnsubscribedEmails(int newsletterID)
        {
            var service = Service.Resolve<IUnsubscriptionProvider>();

            var unsubscriptions = service.GetUnsubscriptionsFromSingleNewsletter(newsletterID).Column("UnsubscriptionEmail").Select(row => DataHelper.GetStringValue(row, "UnsubscriptionEmail"));

            return new HashSet<string>(unsubscriptions, StringComparer.OrdinalIgnoreCase);
        }
    }
}