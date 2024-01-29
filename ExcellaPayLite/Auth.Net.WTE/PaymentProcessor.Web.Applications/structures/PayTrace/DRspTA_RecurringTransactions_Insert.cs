using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspTA_RecurringTransactions_Insert : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string RecurringTransactionID = "RecurringTransactionID";
		}

		public DRspTA_RecurringTransactions_Insert(DataSet ds)
		{
			base.setData(ds);
		}

		public int RecurringTransactionID(int index) 
		{
			return base.getValueInteger(index, Columns.RecurringTransactionID);
		}

	}
}
