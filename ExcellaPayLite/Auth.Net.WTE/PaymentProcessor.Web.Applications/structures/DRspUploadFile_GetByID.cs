namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

    public class DRspUploadFile_GetByID : TableReaderBase
    {
        public class Columns
        {
            public const string UploadFileID = "UploadFileID";
            public const string UploadDate = "UploadDate";
            public const string UserID = "UserID";
            public const string FileDescription = "FileDescription";
            public const string FileName = "FileName";
            public const string FileStatus = "FileStatus";
            public const string FullName = "FullName";
        }

        public DRspUploadFile_GetByID(DataSet ds)
        {
            base.setData(ds);
        }

        public int UploadFileID(int index)
        {
            return base.getValueInteger(index, Columns.UploadFileID);
        }

        public DateTime UploadDate(int index)
        {
            return base.getValueDate(index, Columns.UploadDate);
        }

        public int UserID(int index)
        {
            return base.getValueInteger(index, Columns.UserID);
        }

        public string FileDescription(int index)
        {
            return base.getValue(index, Columns.FileDescription);
        }

        public string FileName(int index)
        {
            return base.getValue(index, Columns.FileName);
        }

        public bool FileStatus(int index)
        {
            return base.getValueBool(index, Columns.FileStatus);
        }

        public string FullName(int index)
        {
            return base.getValue(index, Columns.FullName);
        }

    }
}
