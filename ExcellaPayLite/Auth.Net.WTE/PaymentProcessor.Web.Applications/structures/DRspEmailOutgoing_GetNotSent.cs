using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspEmailOutgoing_GetNotSent : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string EmailOutgoingID = "EmailOutgoingID";
			public const string EmailFrom = "EmailFrom";
			public const string EmailTo = "EmailTo";
			public const string Subject = "Subject";
			public const string Body = "Body";
			public const string IsSent = "IsSent";
		}

		public DRspEmailOutgoing_GetNotSent(DataSet ds)
		{
			base.setData(ds);
		}

		public int EmailOutgoingID(int index) 
		{
			return base.getValueInteger(index, Columns.EmailOutgoingID);
		}

		public string EmailFrom(int index) 
		{
			return base.getValue(index, Columns.EmailFrom);
		}

		public string EmailTo(int index) 
		{
			return base.getValue(index, Columns.EmailTo);
		}

		public string Subject(int index) 
		{
			return base.getValue(index, Columns.Subject);
		}

		public string Body(int index) 
		{
			return base.getValue(index, Columns.Body);
		}

		public bool IsSent(int index) 
		{
			return base.getValueBool(index, Columns.IsSent);
		}

	}
}
