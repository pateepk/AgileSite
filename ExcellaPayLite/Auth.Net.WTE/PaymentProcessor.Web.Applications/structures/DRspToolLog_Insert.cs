namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

	public class DRspToolLog_Insert : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string ToolLogID = "ToolLogID";
		}

		public DRspToolLog_Insert(DataSet ds)
		{
			base.setData(ds);
		}

		public int ToolLogID(int index) 
		{
			return base.getValueInteger(index, Columns.ToolLogID);
		}

	}
}
