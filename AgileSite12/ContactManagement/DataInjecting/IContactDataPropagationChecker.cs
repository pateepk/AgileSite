namespace CMS.ContactManagement
{
    /// <summary>
    /// Checks whether the propagation of data to Contact object is allowed.
    /// </summary>
    public interface IContactDataPropagationChecker
    {
        /// <summary>
        /// Checks whether the propagation of data to Contact object is allowed.
        /// </summary>
        bool IsAllowed();
    }
}