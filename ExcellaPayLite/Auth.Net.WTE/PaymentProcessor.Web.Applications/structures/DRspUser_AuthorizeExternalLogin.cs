namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

    public class DRspUser_AuthorizeExternalLogin : TableReaderBase
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
            public const string CustNum = "CustNum";
            public const string BackdoorGUID = "BackdoorGUID";
        }

        public DRspUser_AuthorizeExternalLogin(DataSet ds)
        {
            base.setData(ds);
        }

        public int UserID
        {
            get
            {
                return base.getValueInteger(0, Columns.UserID);
            }
        }

        public string FullName
        {
            get
            {
                return base.getValue(0, Columns.FullName);
            }
        }

        public string LoginID
        {
            get
            {
                return base.getValue(0, Columns.LoginID);
            }
        }

        public string Email
        {
            get
            {
                return base.getValue(0, Columns.Email);
            }
        }

        public bool IsActive
        {
            get
            {
                return base.getValueBool(0, Columns.IsActive);
            }
        }

        public string RoleIDs
        {
            get
            {
                return base.getValue(0, Columns.RoleIDs);
            }
        }

        public bool IsExternalPassword
        {
            get
            {
                return base.getValueBool(0, Columns.IsExternalPassword);
            }
        }

        public string CustNum
        {
            get
            {
                return base.getValue(0, Columns.CustNum);
            }
        }

        public string BackdoorGUID
        {
            get
            {
                return base.getValue(0, Columns.BackdoorGUID);
            }
        }

    }
}
