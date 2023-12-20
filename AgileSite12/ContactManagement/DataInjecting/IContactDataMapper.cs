using CMS.Base;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Maps data to contact.
    /// </summary>
    public interface IContactDataMapper
    {
        /// <summary>
        /// Maps <paramref name="data"/> to provided <paramref name="contact"/>.
        /// </summary>
        /// <param name="data">Data to modify contact with.</param>
        /// <param name="contact">Contact.</param>
        /// <returns>Returns <c>true</c> if there were any properties to map.</returns>
        bool Map(ISimpleDataContainer data, ContactInfo contact);
    }
}
