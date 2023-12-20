using CMS.ContactManagement;
using CMS.DataEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Provides helper methods which are able to return different subset of subscribers per issue or newsletter
    /// </summary>
    public interface IRecipientsProvider
    {
        /// <summary>
        /// Return all contacts which are subscribed to an issue (or newsletter), includes opted out contacts and bounced.
        /// </summary>
        ObjectQuery<ContactInfo> GetAllRecipients();


        /// <summary>
        /// Return all contacts which are subscribed to the issue (or newsletter), excludes opted out contacts and bounced.
        /// </summary>
        ObjectQuery<ContactInfo> GetMarketableRecipients();
    }
}