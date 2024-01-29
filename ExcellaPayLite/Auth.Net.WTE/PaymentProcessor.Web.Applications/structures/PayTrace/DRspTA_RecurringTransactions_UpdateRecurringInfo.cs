using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspTA_RecurringTransactions_UpdateRecurringInfo : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string RowUpdated = "RowUpdated";
		}

		public DRspTA_RecurringTransactions_UpdateRecurringInfo(DataSet ds)
		{
			base.setData(ds);
		}

		public int RowUpdated(int index) 
		{
			return base.getValueInteger(index, Columns.RowUpdated);
		}

	}
}
