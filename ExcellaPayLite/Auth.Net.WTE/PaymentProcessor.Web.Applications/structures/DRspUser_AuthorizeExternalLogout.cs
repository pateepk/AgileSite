using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspUser_AuthorizeExternalLogout : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string UserID = "UserID";
		}

		public DRspUser_AuthorizeExternalLogout(DataSet ds)
		{
			base.setData(ds);
		}

		public int UserID(int index) 
		{
			return base.getValueInteger(index, Columns.UserID);
		}

	}
}
