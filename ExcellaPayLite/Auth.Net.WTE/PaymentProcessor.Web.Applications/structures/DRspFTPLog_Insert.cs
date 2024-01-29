namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

    public class DRspFTPLog_Insert : TableReaderBase
    {
        public class Columns
        {
            public const string FTPLogID = "FTPLogID";
        }

        public DRspFTPLog_Insert(DataSet ds)
        {
            base.setData(ds);
        }

        public int FTPLogID(int index)
        {
            return base.getValueInteger(index, Columns.FTPLogID);
        }

    }
}
