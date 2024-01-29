using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspTA_DirectTransactions_Events_Insert : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string DirectTransactions_EventsID = "DirectTransactions_EventsID";
		}

		public DRspTA_DirectTransactions_Events_Insert(DataSet ds)
		{
			base.setData(ds);
		}

		public int DirectTransactions_EventsID(int index) 
		{
			return base.getValueInteger(index, Columns.DirectTransactions_EventsID);
		}

	}
}
