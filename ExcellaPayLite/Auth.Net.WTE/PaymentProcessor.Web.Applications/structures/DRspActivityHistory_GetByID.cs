namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

	public class DRspActivityHistory_GetByID : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string ActivityHistoryID = "ActivityHistoryID";
			public const string ActivityID = "ActivityID";
			public const string UserID = "UserID";
			public const string ActivityDate = "ActivityDate";
			public const string DataXml = "DataXml";
		}

		public DRspActivityHistory_GetByID(DataSet ds)
		{
			base.setData(ds);
		}

		public Int64 ActivityHistoryID(int index) 
		{
			return base.getValueInteger64(index, Columns.ActivityHistoryID);
		}

		public int ActivityID(int index) 
		{
			return base.getValueInteger(index, Columns.ActivityID);
		}

		public int UserID(int index) 
		{
			return base.getValueInteger(index, Columns.UserID);
		}

		public DateTime ActivityDate(int index) 
		{
			return base.getValueDate(index, Columns.ActivityDate);
		}

		public string DataXml(int index) 
		{
			return base.getValue(index, Columns.DataXml);
		}

	}
}
