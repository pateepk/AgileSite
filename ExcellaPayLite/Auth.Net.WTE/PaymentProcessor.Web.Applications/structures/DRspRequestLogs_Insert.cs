namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

    public class DRspRequestLogs_Insert : TableReaderBase
    {
        private class Columns
        {
            public const string RequestID = "RequestID";
        }

        public DRspRequestLogs_Insert(DataSet ds)
        {
            base.setData(ds);
        }


        public Int64 RequestID(int index)
        {
            return base.getValueInteger64(index, Columns.RequestID);
        }

    }
}
