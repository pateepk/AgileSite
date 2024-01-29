namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

    public class DRspCheckIfSPNameCanBeExecuted : TableReaderBase
    {
        public class Columns
        {
            public const string StoredProcedureName = "StoredProcedureName";
            public const string IsCanBeExecuted = "IsCanBeExecuted";
        }

        public DRspCheckIfSPNameCanBeExecuted(DataSet ds)
        {
            base.setData(ds);
        }

        public string StoredProcedureName
        {
            get
            {
                return base.getValue(0, Columns.StoredProcedureName);
            }
        }

        public bool IsCanBeExecuted
        {
            get
            {
                return base.getValueBool(0, Columns.IsCanBeExecuted);
            }
        }

    }
}
