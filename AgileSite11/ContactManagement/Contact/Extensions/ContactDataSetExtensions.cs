using System.Collections.Generic;
using System.Data;

using CMS.Helpers;

namespace CMS.ContactManagement
{
    /// <summary>
    /// <see cref="ContactInfo"/> <see cref="DataSet"/> extension methods.
    /// </summary>
    public static class ContactDataSetExtensions
    {
        /// <summary>
        /// Converts <see cref="ContactInfo"/> <see cref="DataSet"/> into list.
        /// </summary>
        /// <param name="contactDataSet">Data set</param>
        /// <returns>ContactInfo list.</returns>
        public static List<ContactInfo> ToContactList(this DataSet contactDataSet)
        {
            var result = new List<ContactInfo>();
            if (!DataHelper.DataSourceIsEmpty(contactDataSet))
            {
                // Get items from arraylist
                foreach (DataRow item in contactDataSet.Tables[0].Rows)
                {
                    result.Add(new ContactInfo(item));
                }
            }
            return result;
        }
    }
}