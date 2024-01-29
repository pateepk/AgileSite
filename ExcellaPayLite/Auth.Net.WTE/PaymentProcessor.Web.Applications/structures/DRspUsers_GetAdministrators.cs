using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspUsers_GetAdministrators : TableReaderBase 
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
			public const string Phone = "Phone";
			public const string LastLoginDate = "LastLoginDate";
			public const string BackdoorGUID = "BackdoorGUID";
		}

		public DRspUsers_GetAdministrators(DataSet ds)
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

		public string CustNum(int index) 
		{
			return base.getValue(index, Columns.CustNum);
		}

		public string Phone(int index) 
		{
			return base.getValue(index, Columns.Phone);
		}

		public DateTime LastLoginDate(int index) 
		{
			return base.getValueDate(index, Columns.LastLoginDate);
		}

		public string BackdoorGUID(int index) 
		{
			return base.getValue(index, Columns.BackdoorGUID);
		}

	}
}
