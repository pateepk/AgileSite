namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

	public class DRspReportAccess_GetByUserID : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string ReportID = "ReportID";
		}

		public DRspReportAccess_GetByUserID(DataSet ds)
		{
			base.setData(ds);
		}

		public int ReportID(int index) 
		{
			return base.getValueInteger(index, Columns.ReportID);
		}

	}
}
