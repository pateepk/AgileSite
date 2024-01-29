namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

    public class DRspForgotPasswordRequest_Insert : TableReaderBase
    {
        public class Columns
        {
            public const string ForgotPasswordRequestID = "ForgotPasswordRequestID";
        }

        public DRspForgotPasswordRequest_Insert(DataSet ds)
        {
            base.setData(ds);
        }

        public Int64 ForgotPasswordRequestID(int index)
        {
            return base.getValueInteger64(index, Columns.ForgotPasswordRequestID);
        }

    }
}
