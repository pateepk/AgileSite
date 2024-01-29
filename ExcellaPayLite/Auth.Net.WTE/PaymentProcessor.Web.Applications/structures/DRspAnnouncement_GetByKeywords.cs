namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

    public class DRspAnnouncement_GetByKeywords : TableReaderBase
    {
        public class Columns
        {
            public const string AnnouncementID = "AnnouncementID";
            public const string IsEnable = "IsEnable";
            public const string IsSentEmail = "IsSentEmail";
            public const string PostDate = "PostDate";
            public const string UserID = "UserID";
            public const string Subject = "Subject";
            public const string Messages = "Messages";
            public const string FullName = "FullName";
            public const string RoleIDs = "RoleIDs";
            public const string FileNames = "FileNames";
        }

        public DRspAnnouncement_GetByKeywords(DataSet ds)
        {
            base.setData(ds);
        }

        public int AnnouncementID(int index)
        {
            return base.getValueInteger(index, Columns.AnnouncementID);
        }

        public bool IsEnable(int index)
        {
            return base.getValueBool(index, Columns.IsEnable);
        }

        public bool IsSentEmail(int index)
        {
            return base.getValueBool(index, Columns.IsSentEmail);
        }

        public DateTime PostDate(int index)
        {
            return base.getValueDate(index, Columns.PostDate);
        }

        public int UserID(int index)
        {
            return base.getValueInteger(index, Columns.UserID);
        }

        public string Subject(int index)
        {
            return base.getValue(index, Columns.Subject);
        }

        public string Messages(int index)
        {
            return base.getValue(index, Columns.Messages);
        }

        public string FullName(int index)
        {
            return base.getValue(index, Columns.FullName);
        }

        public string RoleIDs(int index)
        {
            return base.getValue(index, Columns.RoleIDs);
        }

        public string FileNames(int index)
        {
            return base.getValue(index, Columns.FileNames);
        }

    }
}
