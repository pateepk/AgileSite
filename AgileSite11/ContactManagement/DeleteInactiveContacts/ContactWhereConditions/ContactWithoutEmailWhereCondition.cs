using CMS.DataEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Represents object with additional where condition for <see cref="ContactInfo"/> query.
    /// </summary>
    internal class ContactWithoutEmailWhereCondition : IContactsWhereCondition
    {
        /// <summary>
        /// Returns contacts without email.
        /// </summary>
        public WhereCondition GetWhere()
        {
            return new WhereCondition().WhereEmpty("ContactEmail");
        }
    }
}