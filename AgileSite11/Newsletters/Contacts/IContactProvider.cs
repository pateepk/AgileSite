using CMS.ContactManagement;
using CMS.Membership;

namespace CMS.Newsletters
{
    /// <summary>
    /// Provides methods for retrieving and preparing <see cref="ContactInfo" /> objects so it can be subscribed to the newsletter.
    /// </summary>
    public interface IContactProvider
    {
        /// <summary>
        /// Gets contact associated with <paramref name="email"/>.
        /// When contact doesn't exist, contact is created and inserted to database.
        /// </summary>
        /// <param name="email">Email used to get or update contact.</param>
        /// <param name="firstName">First name used to update contact.</param>
        /// <param name="lastName">Last name used to update contact.</param>
        /// <returns>Retrieved contact.</returns>
        ContactInfo GetContactForSubscribing(string email, string firstName = null, string lastName = null);

        /// <summary>
        /// Gets contact associated with <paramref name="user"/>.
        /// When contact doesn't exist, contact is created and inserted to database
        /// </summary>
        /// <param name="user">User used to get and update contact.</param>
        /// <returns>Retrieved contact.</returns>
        ContactInfo GetContactForSubscribing(UserInfo user);
    }
}
