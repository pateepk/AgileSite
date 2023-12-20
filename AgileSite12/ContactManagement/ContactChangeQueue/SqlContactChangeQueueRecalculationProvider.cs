using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides method for reading the contact changes from the SQL database.
    /// </summary>
    internal class SqlContactChangeQueueRecalculationProvider : IContactChangeQueueRecalculationProvider
    {
        /// <summary>
        /// Returns top 10000 items from the queue and at the same time it removes them from the queue.
        /// </summary>
        public IEnumerable<ContactChangeData> Dequeue()
        {
            var itemsDataSet = ConnectionHelper.ExecuteQuery("OM.ContactChangeRecalculationQueue.FetchContactChanges", new QueryDataParameters());
            return itemsDataSet.Tables[0].Rows.Cast<DataRow>().Select(dataRow => new ContactChangeData(new DataRowContainer(dataRow)));
        }
    }
}
