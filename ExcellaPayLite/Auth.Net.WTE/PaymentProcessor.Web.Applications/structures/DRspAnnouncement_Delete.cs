namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

    public class DRspAnnouncement_Delete : TableReaderBase
    {
        public class Columns
        {
            public const string RowCounted = "RowCounted";
        }

        public DRspAnnouncement_Delete(DataSet ds)
        {
            base.setData(ds);
        }

        public int RowCounted(int index)
        {
            return base.getValueInteger(index, Columns.RowCounted);
        }

    }
}
