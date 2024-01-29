using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspTA_custom_egiving_Events_UpdateNumber_of_Tickets : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string RowUpdated = "RowUpdated";
			public const string Number_of_Tickets = "Number_of_Tickets";
		}

		public DRspTA_custom_egiving_Events_UpdateNumber_of_Tickets(DataSet ds)
		{
			base.setData(ds);
		}

		public int RowUpdated(int index) 
		{
			return base.getValueInteger(index, Columns.RowUpdated);
		}

		public int Number_of_Tickets(int index) 
		{
			return base.getValueInteger(index, Columns.Number_of_Tickets);
		}

	}
}
