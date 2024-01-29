namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

    public class DRspReports_GetReports : TableReaderBase
    {
        public class Columns
        {
            public const string ReportID = "ReportID";
            public const string ReportTypeID = "ReportTypeID";
            public const string Title = "Title";
            public const string StoredProcedure = "StoredProcedure";
            public const string Path = "Path";
            public const string DataSourceName = "DataSourceName";
            public const string TypeName = "TypeName";
            public const string SelectMethod = "SelectMethod";
            public const string IsParameters = "IsParameters";
            public const string IsDefaultDenied = "IsDefaultDenied";
        }

        public DRspReports_GetReports(DataSet ds)
        {
            base.setData(ds);
        }

        public int ReportID(int index)
        {
            return base.getValueInteger(index, Columns.ReportID);
        }

        public int ReportTypeID(int index)
        {
            return base.getValueInteger(index, Columns.ReportTypeID);
        }

        public string Title(int index)
        {
            return base.getValue(index, Columns.Title);
        }

        public string StoredProcedure(int index)
        {
            return base.getValue(index, Columns.StoredProcedure);
        }

        public string Path(int index)
        {
            return base.getValue(index, Columns.Path);
        }

        public string DataSourceName(int index)
        {
            return base.getValue(index, Columns.DataSourceName);
        }

        public string TypeName(int index)
        {
            return base.getValue(index, Columns.TypeName);
        }

        public string SelectMethod(int index)
        {
            return base.getValue(index, Columns.SelectMethod);
        }

        public bool IsParameters(int index)
        {
            return base.getValueBool(index, Columns.IsParameters);
        }

        public bool IsDefaultDenied(int index)
        {
            return base.getValueBool(index, Columns.IsDefaultDenied);
        }

    }
}
