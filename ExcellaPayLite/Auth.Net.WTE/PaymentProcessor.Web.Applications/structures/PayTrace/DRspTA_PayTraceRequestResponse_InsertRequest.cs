using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspTA_PayTraceRequestResponse_InsertRequest : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string PayTraceRequestID = "PayTraceRequestID";
		}

		public DRspTA_PayTraceRequestResponse_InsertRequest(DataSet ds)
		{
			base.setData(ds);
		}

		public int PayTraceRequestID(int index) 
		{
			return base.getValueInteger(index, Columns.PayTraceRequestID);
		}

	}
}
