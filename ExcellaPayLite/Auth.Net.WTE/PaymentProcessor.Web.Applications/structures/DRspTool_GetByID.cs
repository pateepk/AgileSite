namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

    public class DRspTool_GetByID : TableReaderBase
    {
        public class Columns
        {
            public const string ToolID = "ToolID";
            public const string Enable = "Enable";
            public const string Title = "Title";
            public const string SortOrder = "SortOrder";
            public const string StoredProcedure = "StoredProcedure";
            public const string DocumentID = "DocumentID";
            public const string SPCreated = "SPCreated";
            public const string SPLastAltered = "SPLastAltered";
        }

        public DRspTool_GetByID(DataSet ds)
        {
            base.setData(ds);
        }

        public int ToolID
        {
            get
            {
                return base.getValueInteger(0, Columns.ToolID);
            }
        }

        public bool Enable
        {
            get
            {
                return base.getValueBool(0, Columns.Enable);
            }
        }

        public string Title
        {
            get
            {
                return base.getValue(0, Columns.Title);
            }
        }

        public int SortOrder
        {
            get
            {
                return base.getValueInteger(0, Columns.SortOrder);
            }
        }

        public string StoredProcedure
        {
            get
            {
                return base.getValue(0, Columns.StoredProcedure);
            }
        }

        public Int64 DocumentID
        {
            get
            {
                return base.getValueInteger64(0, Columns.DocumentID);
            }
        }

        public DateTime SPCreated
        {
            get
            {
                return base.getValueDate(0, Columns.SPCreated);
            }
        }

        public DateTime SPLastAltered
        {
            get
            {
                return base.getValueDate(0, Columns.SPLastAltered);
            }
        }

    }
}
