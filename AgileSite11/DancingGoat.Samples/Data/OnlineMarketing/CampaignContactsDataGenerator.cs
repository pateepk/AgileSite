using System.Linq;

using CMS.ContactManagement;

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Contains methods for generating sample contacts for the Campaign module.
    /// </summary>
    internal class CampaignContactsDataGenerator
    {
        private const int NUMBER_OF_GENERATED_CONTACTS = 531;
        private readonly string mContactFirstNamePrefix;
        private readonly string mContactLastNamePrefix;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="contactFirstNamePrefix">First name prefix of contacts generated for sample campaigns.</param>
        /// <param name="contactLastNamePrefix">Last name prefix of contacts generated for sample campaigns.</param>
        public CampaignContactsDataGenerator(string contactFirstNamePrefix, string contactLastNamePrefix)
        {
            mContactFirstNamePrefix = contactFirstNamePrefix;
            mContactLastNamePrefix = contactLastNamePrefix;
        }

        /// <summary>
        /// Performs campaign contacts sample data generating.
        /// </summary>
        public void Generate()
        {
            DeleteOldContacts();
            GenerateContacts();
        }


        private void DeleteOldContacts()
        {
            ContactInfoProvider.GetContacts()
                               .WhereStartsWith("ContactFirstName", mContactFirstNamePrefix)
                               .ToList()
                               .ForEach(ContactInfoProvider.DeleteContactInfo);
        }


        private void GenerateContacts()
        {
            for (var i = 0; i < NUMBER_OF_GENERATED_CONTACTS; i++)
            {
                var contact = new ContactInfo()
                {
                    ContactFirstName = mContactFirstNamePrefix + i,
                    ContactLastName = mContactLastNamePrefix + i,
                    ContactEmail = string.Format("{0}{1}@localhost.local", mContactFirstNamePrefix, i)
                };
                ContactInfoProvider.SetContactInfo(contact);
            }
        }
    }
}
