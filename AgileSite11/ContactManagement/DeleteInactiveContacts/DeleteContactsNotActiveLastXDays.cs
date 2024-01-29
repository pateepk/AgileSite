using System.Linq;

using CMS.ContactManagement;

[assembly: RegisterDeleteContactsImplementation(
    DeleteContactsNotActiveLastXDays.NAME, 
    "settingskey.om.deleteinactivecontacts.method.notactivelastxdays", 
    typeof(DeleteContactsNotActiveLastXDays))]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Deletes contacts not active last x days.
    /// </summary>
    internal sealed class DeleteContactsNotActiveLastXDays : IDeleteContacts
    {
        /// <summary>
        /// Implementation name.
        /// </summary>
        public const string NAME = "DeleteContactsNotActiveLastXDays";

        /// <summary>
        /// Deletes batch of contacts not active last x days.
        /// </summary>
        /// <returns>Number of contacts remaining to delete</returns>
        public int Delete(int days, int batchSize)
        {
            var whereCondition = new ContactLastActivityOlderThanWhereCondition(days).GetWhere().ToString(true);
            ContactInfoProvider.DeleteContactInfos(whereCondition, batchSize);
            return ContactInfoProvider.GetContacts().Where(whereCondition).Count();
        }
    }
}
