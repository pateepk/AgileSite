using System.Linq;

using CMS.Base;
using CMS.ContactManagement;

namespace CMS.Ecommerce.Internal
{
    /// <summary>
    /// Maps address data to contact.
    /// </summary>
    public class AddressContactDataMapper : IContactDataMapper
    {
        /// <summary>
        /// Maps address <paramref name="data"/> to provided <paramref name="contact"/>.
        /// </summary>
        /// <param name="data">Source data.</param>
        /// <param name="contact">Contact.</param>
        public bool Map(ISimpleDataContainer data, ContactInfo contact)
        {
            var address = data as IAddress;

            if (address == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(contact.ContactAddress1))
            {
                return false;
            }

            contact.ContactAddress1 = GetContactAddress(address);
            contact.ContactCity = address.AddressCity;
            contact.ContactZIP = address.AddressZip;
            contact.ContactMobilePhone = address.AddressPhone;
            contact.ContactCountryID = address.AddressCountryID;
            contact.ContactStateID = address.AddressStateID;
                
            return true;
        }


        private static string GetContactAddress(IAddress addressInfo)
        {
            var address = new[]
            {
                addressInfo.AddressLine1,
                addressInfo.AddressLine2
            };

            return GetTruncatedAddress(string.Join(", ", address.Where(x => !string.IsNullOrEmpty(x))));
        }


        private static string GetTruncatedAddress(string address)
        {
            return address.Length > 100 ? address.Substring(0, 100) : address;
        }
    }
}
