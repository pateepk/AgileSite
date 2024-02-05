namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

    public class DRspUsers_GetByRole : TableReaderBase
    {
        public class Columns
        {
            public const string UserID = "UserID";
            public const string FullName = "FullName";
            public const string LoginID = "LoginID";
            public const string Email = "Email";
            public const string IsActive = "IsActive";
        }

        public DRspUsers_GetByRole(DataSet ds)
        {
            base.setData(ds);
        }

        public int UserID(int index)
        {
            return base.getValueInteger(index, Columns.UserID);
        }

        public string FullName(int index)
        {
            return base.getValue(index, Columns.FullName);
        }

        public string LoginID(int index)
        {
            return base.getValue(index, Columns.LoginID);
        }

        public string Email(int index)
        {
            return base.getValue(index, Columns.Email);
        }

        public bool IsActive(int index)
        {
            return base.getValueBool(index, Columns.IsActive);
        }

    }
}
