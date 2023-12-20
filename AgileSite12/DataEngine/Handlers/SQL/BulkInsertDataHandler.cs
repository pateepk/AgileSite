using System.Data;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Bulk insert handler enables insert result processing.
    /// </summary>
    public class BulkInsertDataHandler : AdvancedHandler<BulkInsertDataHandler, BulkInsertDataEventArgs>
    {
        /// <summary>
        /// Initiates the event handling.
        /// </summary>
        /// <param name="sourceData">Source data.</param>
        /// <param name="targetTable">Target table.</param>
        /// <param name="insertSettings">Bulk insert configuration.</param>
        /// <param name="connection">Data connection.</param>
        public BulkInsertDataHandler StartEvent(DataTable sourceData, string targetTable, BulkInsertSettings insertSettings, IDataConnection connection)
        {
            var e = new BulkInsertDataEventArgs(sourceData, targetTable, insertSettings, connection);

            var h = StartEvent(e);

            return h;
        }
    }
}
