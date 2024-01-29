namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

	public class DRspReportAccess_GetByReportID : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string ReportAccessID = "ReportAccessID";
			public const string ReportID = "ReportID";
			public const string IsRole = "IsRole";
			public const string ExceptionRoleOrUserID = "ExceptionRoleOrUserID";
		}

		public DRspReportAccess_GetByReportID(DataSet ds)
		{
			base.setData(ds);
		}

		public int ReportAccessID(int index) 
		{
			return base.getValueInteger(index, Columns.ReportAccessID);
		}

		public int ReportID(int index) 
		{
			return base.getValueInteger(index, Columns.ReportID);
		}

		public bool IsRole(int index) 
		{
			return base.getValueBool(index, Columns.IsRole);
		}

		public int ExceptionRoleOrUserID(int index) 
		{
			return base.getValueInteger(index, Columns.ExceptionRoleOrUserID);
		}

	}
}
