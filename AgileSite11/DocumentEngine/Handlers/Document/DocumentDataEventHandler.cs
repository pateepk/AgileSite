using System;
using System.Data;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document data handler enables manipulation with document data.
    /// </summary>
    public class DocumentDataEventHandler : AdvancedHandler<DocumentDataEventHandler, DocumentDataEventArgs>
    {
        /// <summary>
        /// Initiates the event handling.
        /// </summary>
        /// <param name="result">Data set with document data.</param>
        /// <param name="query">Default document query used to select data in case data set is not returned by event.</param>
        /// <param name="totalRecords">Number of records which should by used for paging. If not set, number of rows in dataset is calculated.</param>
        public DocumentDataEventHandler StartEvent(DataSet result, IDocumentQuery query, int totalRecords)
        {
            var e = new DocumentDataEventArgs
            {
                Data = result,
                Query = query,
                TotalRecords = totalRecords
            };

            return StartEvent(e);
        }
    }
}
