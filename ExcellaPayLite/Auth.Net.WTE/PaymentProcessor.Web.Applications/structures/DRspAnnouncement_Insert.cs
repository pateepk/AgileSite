namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

    public class DRspAnnouncement_Insert : TableReaderBase
    {
        public class Columns
        {
            public const string AnnouncementID = "AnnouncementID";
        }

        public DRspAnnouncement_Insert(DataSet ds)
        {
            base.setData(ds);
        }

        public int AnnouncementID(int index)
        {
            return base.getValueInteger(index, Columns.AnnouncementID);
        }

    }
}
