namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

	public class DRspReportParameter_GetByReportID : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string ReportParameterID = "ReportParameterID";
			public const string ReportID = "ReportID";
			public const string ParamLabel = "ParamLabel";
			public const string ParamName = "ParamName";
			public const string ParamType = "ParamType";
			public const string ParamDefaultValue = "ParamDefaultValue";
		}

		public DRspReportParameter_GetByReportID(DataSet ds)
		{
			base.setData(ds);
		}

		public int ReportParameterID(int index) 
		{
			return base.getValueInteger(index, Columns.ReportParameterID);
		}

		public int ReportID(int index) 
		{
			return base.getValueInteger(index, Columns.ReportID);
		}

		public string ParamLabel(int index) 
		{
			return base.getValue(index, Columns.ParamLabel);
		}

		public string ParamName(int index) 
		{
			return base.getValue(index, Columns.ParamName);
		}

		public string ParamType(int index) 
		{
			return base.getValue(index, Columns.ParamType);
		}

		public string ParamDefaultValue(int index) 
		{
			return base.getValue(index, Columns.ParamDefaultValue);
		}

	}
}
