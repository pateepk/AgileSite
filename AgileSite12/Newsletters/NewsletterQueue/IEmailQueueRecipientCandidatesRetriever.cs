using System.Collections.Generic;
using System.Data;

namespace CMS.Newsletters
{
    /// <summary>
    /// Provides methods to get contacts for whom the newsletter should be send.
    /// </summary>
    internal interface IEmailQueueRecipientCandidatesRetriever
    {
        /// <summary>
        /// Fills given table with contact data from subscribed contacts and contact groups.
        /// </summary>
        /// <param name="table">Data table.</param>
        /// <param name="siteId">Site ID.</param>
        /// <param name="generatedEmails">E-mail addresses that are to be ignored. The method also fills this set with generated e-mail addresses.</param>
        void FillContactTable(DataTable table, int siteId, HashSet<string> generatedEmails);


        /// <summary>
        /// Fills given table with contact data from specified contact group.
        /// </summary>
        /// <param name="table">Data table.</param>
        /// <param name="contactGroupId">Contact group ID.</param>
        /// <param name="subscriberId">Subscriber ID.</param>
        /// <param name="currentEmails">Hash table with emails that have been already added.</param>
        /// <param name="setSending">Indicate if new records should be set with 'sending' status.</param>
        void FillContactGroupTable(DataTable table, int contactGroupId, int subscriberId, HashSet<string> currentEmails, bool setSending);
    }
}