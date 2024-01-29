using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspTA_DirectTransactions_Insert : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string DirectTransactionID = "DirectTransactionID";
		}

		public DRspTA_DirectTransactions_Insert(DataSet ds)
		{
			base.setData(ds);
		}

		public int DirectTransactionID(int index) 
		{
			return base.getValueInteger(index, Columns.DirectTransactionID);
		}

	}
}
