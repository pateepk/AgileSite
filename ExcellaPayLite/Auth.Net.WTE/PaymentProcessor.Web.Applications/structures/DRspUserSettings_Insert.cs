using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspUserSettings_Insert : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string UserID = "UserID";
			public const string Name = "Name";
			public const string Value = "Value";
		}

		public DRspUserSettings_Insert(DataSet ds)
		{
			base.setData(ds);
		}

		public int UserID(int index) 
		{
			return base.getValueInteger(index, Columns.UserID);
		}

		public string Name(int index) 
		{
			return base.getValue(index, Columns.Name);
		}

		public string Value(int index) 
		{
			return base.getValue(index, Columns.Value);
		}

	}
}
