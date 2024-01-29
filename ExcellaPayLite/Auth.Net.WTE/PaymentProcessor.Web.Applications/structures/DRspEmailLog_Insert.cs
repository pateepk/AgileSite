namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

    public class DRspEmailLog_Insert : TableReaderBase
    {
        public class Columns
        {
            public const string EmailLogID = "EmailLogID";
        }

        public DRspEmailLog_Insert(DataSet ds)
        {
            base.setData(ds);
        }

        public Int64 EmailLogID(int index)
        {
            return base.getValueInteger64(index, Columns.EmailLogID);
        }

    }
}
