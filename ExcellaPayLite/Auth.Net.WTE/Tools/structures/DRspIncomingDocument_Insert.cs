using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;


// copy from web app
namespace ssch.tools
{
    public class DRspIncomingDocument_Insert : TableReaderBase
    {
        public class Columns
        {
            public const string IncomingDocumentID = "IncomingDocumentID";
            public const string RecipientFirstName = "RecipientFirstName";
            public const string RecipientLastName = "RecipientLastName";
            public const string RecipientMID = "RecipientMID";
        }

        public DRspIncomingDocument_Insert(DataSet ds)
        {
            base.setData(ds);
        }

        public Int64 IncomingDocumentID(int index)
        {
            return base.getValueInteger64(index, Columns.IncomingDocumentID);
        }

        public string RecipientFirstName(int index)
        {
            return base.getValue(index, Columns.RecipientFirstName);
        }

        public string RecipientLastName(int index)
        {
            return base.getValue(index, Columns.RecipientLastName);
        }

        public string RecipientMID(int index)
        {
            return base.getValue(index, Columns.RecipientMID);
        }

    }
}
