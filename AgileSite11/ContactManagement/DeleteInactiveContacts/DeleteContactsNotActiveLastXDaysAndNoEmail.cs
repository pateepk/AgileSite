using System;
using System.Linq;
using System.Text;

using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.SiteProvider;

[assembly: RegisterDeleteContactsImplementation(
    DeleteContactsNotActiveLastXDaysAndNoEmail.NAME,
    "settingskey.om.deleteinactivecontacts.method.notactivelastxdaysandnoemail",
    typeof(DeleteContactsNotActiveLastXDaysAndNoEmail))]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Deletes contacts not active last x days and with no email.
    /// </summary>
    internal sealed class DeleteContactsNotActiveLastXDaysAndNoEmail : IDeleteContacts
    {
        /// <summary>
        /// Implementation name.
        /// </summary>
        public const string NAME = "DeleteContactsNotActiveLastXDaysAndNoEmail";


        /// <summary>
        /// Deletes batch of contacts not active last x days and with no email.
        /// </summary>
        /// <returns>Number of contacts remaining to delete</returns>
        public int Delete(int days, int batchSize)
        {
            var whereCondition = GetWhereCondition(days);
            ContactInfoProvider.DeleteContactInfos(whereCondition, batchSize);
            return ContactInfoProvider.GetContacts().Where(whereCondition).Count();
        }


        private string GetWhereCondition(int days)
        {
            var condition = new WhereCondition();

            condition.And(new ContactWithoutEmailWhereCondition().GetWhere());
            condition.And(new ContactLastActivityOlderThanWhereCondition(days).GetWhere());

            return condition.ToString(true);
        }
    }
}