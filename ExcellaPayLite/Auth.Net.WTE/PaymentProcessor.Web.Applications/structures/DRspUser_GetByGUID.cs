using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspUser_GetByGUID : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string UserID = "UserID";
			public const string FullName = "FullName";
			public const string LoginID = "LoginID";
			public const string Email = "Email";
			public const string IsActive = "IsActive";
			public const string ExternalPassword = "ExternalPassword";
			public const string DocumentID = "DocumentID";
			public const string Phone = "Phone";
			public const string LastLoginDate = "LastLoginDate";
			public const string BackdoorGUID = "BackdoorGUID";
		}

		public DRspUser_GetByGUID(DataSet ds)
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

		public string ExternalPassword(int index) 
		{
			return base.getValue(index, Columns.ExternalPassword);
		}

		public Int64 DocumentID(int index) 
		{
			return base.getValueInteger64(index, Columns.DocumentID);
		}

		public string Phone(int index) 
		{
			return base.getValue(index, Columns.Phone);
		}

		public DateTime LastLoginDate(int index) 
		{
			return base.getValueDate(index, Columns.LastLoginDate);
		}

        public Guid BackdoorGUID(int index)
        {
            return base.getValueGuid(index, Columns.BackdoorGUID);
        }

	}
}
