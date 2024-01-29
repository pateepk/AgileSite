using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspEmailOutgoing_UpdateToSent : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string RowUpdated = "RowUpdated";
		}

		public DRspEmailOutgoing_UpdateToSent(DataSet ds)
		{
			base.setData(ds);
		}

		public int RowUpdated(int index) 
		{
			return base.getValueInteger(index, Columns.RowUpdated);
		}

	}
}
