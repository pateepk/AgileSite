using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspTA_CheckTransactions_Insert : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string CheckTransactionID = "CheckTransactionID";
		}

		public DRspTA_CheckTransactions_Insert(DataSet ds)
		{
			base.setData(ds);
		}

		public int CheckTransactionID(int index) 
		{
			return base.getValueInteger(index, Columns.CheckTransactionID);
		}

	}
}
