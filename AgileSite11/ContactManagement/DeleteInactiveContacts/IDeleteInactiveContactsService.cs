namespace CMS.ContactManagement
{
    /// <summary>
    /// Service responsible for deleting contacts. Is used for deleting inactive contacts.
    /// </summary>
    public interface IDeleteContactsService
    {
        /// <summary>
        /// Deletes batch of contacts.
        /// </summary>
        /// <param name="batchSize">Number of contacts to delete in one call.</param>
        /// <returns>Returns true when there are more contacts to delete</returns>
        bool Delete(int batchSize);
    }
}