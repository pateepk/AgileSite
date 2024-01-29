using CMS.DataEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Represents object with additional where condition for <see cref="ContactInfo"/> query.
    /// </summary>
    internal interface IContactsWhereCondition
    {
        /// <summary>
        /// Where condition for <see cref="ContactInfo"/> query.
        /// </summary>
        WhereCondition GetWhere();
    }
}