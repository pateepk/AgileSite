using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;


namespace ssch.tools
{
    public class DRspScannedDocument_GetRecipientID : TableReaderBase
    {
        public class Columns
        {
            public const string RecipientID = "RecipientID";
        }

        public DRspScannedDocument_GetRecipientID(DataSet ds)
        {
            base.setData(ds);
        }

        public int RecipientID(int index)
        {
            return base.getValueInteger(index, Columns.RecipientID);
        }

    }
}
