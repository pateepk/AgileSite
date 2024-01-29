using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspTA_PayTraceCustomerExport_Insert : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string PayTraceCustomerExportID = "PayTraceCustomerExportID";
		}

		public DRspTA_PayTraceCustomerExport_Insert(DataSet ds)
		{
			base.setData(ds);
		}

		public Int64 PayTraceCustomerExportID(int index) 
		{
			return base.getValueInteger64(index, Columns.PayTraceCustomerExportID);
		}

	}
}
