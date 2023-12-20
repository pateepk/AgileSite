namespace CMS.ContactManagement
{
    /// <summary>
    /// Deletes contacts on one site based on settings (days, batch size).
    /// </summary>
    public interface IDeleteContacts
    {
        /// <summary>
        /// Deletes batch of contacts.
        /// </summary>
        /// <param name="days">Days that are set in the settings. Is used to delete contacts older than this number of days.</param>
        /// <param name="batchSize">Size of the batch that should be deleted at most.</param>
        /// <returns>Number of contacts remaining to delete.</returns>
        int Delete(int days, int batchSize);
    }
}