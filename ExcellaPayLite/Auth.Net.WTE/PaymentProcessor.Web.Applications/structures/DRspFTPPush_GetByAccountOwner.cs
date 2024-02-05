namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

    public class DRspFTPPush_GetByAccountOwner : TableReaderBase
    {
        public class Columns
        {
            public const string FTPPushID = "FTPPushID";
            public const string AccountOwner = "AccountOwner";
            public const string FTPSourceTypeID = "FTPSourceTypeID";
            public const string SourceDataOrURL = "SourceDataOrURL";
            public const string DestinationURL = "DestinationURL";
            public const string CheckLink = "CheckLink";
            public const string IsEnable = "IsEnable";
        }

        public DRspFTPPush_GetByAccountOwner(DataSet ds)
        {
            base.setData(ds);
        }

        public int FTPPushID(int index)
        {
            return base.getValueInteger(index, Columns.FTPPushID);
        }

        public string AccountOwner(int index)
        {
            return base.getValue(index, Columns.AccountOwner);
        }

        public int FTPSourceTypeID(int index)
        {
            return base.getValueInteger(index, Columns.FTPSourceTypeID);
        }

        public string SourceDataOrURL(int index)
        {
            return base.getValue(index, Columns.SourceDataOrURL);
        }

        public string DestinationURL(int index)
        {
            return base.getValue(index, Columns.DestinationURL);
        }

        public string CheckLink(int index)
        {
            return base.getValue(index, Columns.CheckLink);
        }

        public bool IsEnable(int index)
        {
            return base.getValueBool(index, Columns.IsEnable);
        }

    }
}
