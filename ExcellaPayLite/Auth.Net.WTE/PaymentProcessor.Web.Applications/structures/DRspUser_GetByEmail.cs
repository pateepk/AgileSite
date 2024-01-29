namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

    public class DRspUser_GetByEmail : TableReaderBase
    {
        public class Columns
        {
            public const string UserID = "UserID";
            public const string FullName = "FullName";
            public const string LoginID = "LoginID";
            public const string Email = "Email";
            public const string IsActive = "IsActive";
            public const string RoleIDs = "RoleIDs";
            public const string IsExternalPassword = "IsExternalPassword";
        }

        public DRspUser_GetByEmail(DataSet ds)
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

        public string RoleIDs(int index)
        {
            return base.getValue(index, Columns.RoleIDs);
        }

        public bool IsExternalPassword(int index)
        {
            return base.getValueBool(index, Columns.IsExternalPassword);
        }

    }
}
