namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

    public class DRspWebErrorLogs_Insert : TableReaderBase
    {
        private class Columns
        {
            public const string WebErrorID = "WebErrorID";
        }

        public DRspWebErrorLogs_Insert(DataSet ds)
        {
            base.setData(ds);
        }

        public Int64 WebErrorID(int index)
        {
            return base.getValueInteger64(index, Columns.WebErrorID);
        }

    }
}