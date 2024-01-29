namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

	public class DRspUser_Insert : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string UserID = "UserID";
		}

		public DRspUser_Insert(DataSet ds)
		{
			base.setData(ds);
		}

		public int UserID(int index) 
		{
			return base.getValueInteger(index, Columns.UserID);
		}

	}
}
