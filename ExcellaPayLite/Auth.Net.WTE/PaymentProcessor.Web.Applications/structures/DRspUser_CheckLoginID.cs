using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspUser_CheckLoginID : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string LoginID = "LoginID";
		}

		public DRspUser_CheckLoginID(DataSet ds)
		{
			base.setData(ds);
		}

		public string LoginID(int index) 
		{
			return base.getValue(index, Columns.LoginID);
		}

	}
}
