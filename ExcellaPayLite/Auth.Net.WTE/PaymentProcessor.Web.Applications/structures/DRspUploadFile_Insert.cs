namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

    public class DRspUploadFile_Insert : TableReaderBase
    {
        public class Columns
        {
            public const string UploadFileID = "UploadFileID";
            public const string FileName = "FileName";
        }

        public DRspUploadFile_Insert(DataSet ds)
        {
            base.setData(ds);
        }

        public int UploadFileID(int index)
        {
            return base.getValueInteger(index, Columns.UploadFileID);
        }

        public string FileName(int index)
        {
            return base.getValue(index, Columns.FileName);
        }

    }
}
