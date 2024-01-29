namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

	public class DRspForgotPasswordRequest_GetByKeys : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string ForgotPasswordRequestID = "ForgotPasswordRequestID";
			public const string RequestDate = "RequestDate";
			public const string Email = "Email";
			public const string KeyID = "KeyID";
			public const string UserID = "UserID";
			public const string Attempt = "Attempt";
			public const string IsExpired = "IsExpired";
		}

		public DRspForgotPasswordRequest_GetByKeys(DataSet ds)
		{
			base.setData(ds);
		}

		public Int64 ForgotPasswordRequestID(int index) 
		{
			return base.getValueInteger64(index, Columns.ForgotPasswordRequestID);
		}

		public DateTime RequestDate(int index) 
		{
			return base.getValueDate(index, Columns.RequestDate);
		}

		public string Email(int index) 
		{
			return base.getValue(index, Columns.Email);
		}

		public Guid KeyID(int index) 
		{
			return base.getValueGuid(index, Columns.KeyID);
		}

		public int UserID(int index) 
		{
			return base.getValueInteger(index, Columns.UserID);
		}

		public int Attempt(int index) 
		{
			return base.getValueInteger(index, Columns.Attempt);
		}

		public bool IsExpired(int index) 
		{
			return base.getValueBool(index, Columns.IsExpired);
		}

	}
}
