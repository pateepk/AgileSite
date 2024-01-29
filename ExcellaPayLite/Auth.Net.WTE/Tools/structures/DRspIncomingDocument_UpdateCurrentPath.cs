using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;


namespace ssch.tools
{
    public class DRspIncomingDocument_UpdateCurrentPath : TableReaderBase
    {
        public class Columns
        {
            public const string RowUpdated = "RowUpdated";
        }

        public DRspIncomingDocument_UpdateCurrentPath(DataSet ds)
        {
            base.setData(ds);
        }

        public int RowUpdated(int index)
        {
            return base.getValueInteger(index, Columns.RowUpdated);
        }

    }
}
