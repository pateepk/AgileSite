namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

	public class DRspReportAccess_Update : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string RowCounted = "RowCounted";
		}

		public DRspReportAccess_Update(DataSet ds)
		{
			base.setData(ds);
		}

		public int RowCounted(int index) 
		{
			return base.getValueInteger(index, Columns.RowCounted);
		}

	}
}
