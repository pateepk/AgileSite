using System;
using System.Data;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Object data handler enables manipulation with object data.
    /// </summary>
    public class ObjectDataEventHandler : AdvancedHandler<ObjectDataEventHandler, ObjectDataEventArgs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ObjectDataEventHandler()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentHandler">Parent handler</param>
        public ObjectDataEventHandler(ObjectDataEventHandler parentHandler)
        {
            Parent = parentHandler;
        }


        /// <summary>
        /// Initiates the event handling.
        /// </summary>
        /// <param name="result">Data set with object data.</param>
        /// <param name="query">Default object query used to select data in case data set is not returned by event.</param>
        /// <param name="totalRecords">Number of records which should by used for paging. If not set, number of rows in dataset is calculated.</param>
        public ObjectDataEventHandler StartEvent(DataSet result, IObjectQuery query, int totalRecords)
        {
            var e = new ObjectDataEventArgs
            {
                Data = result,
                Query = query,
                TotalRecords = totalRecords
            };

            return StartEvent(e);
        }
    }
}
