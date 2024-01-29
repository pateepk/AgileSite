namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

	public class DRspActivityHistory_Select : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string ActivityHistoryID = "ActivityHistoryID";
			public const string ActivityID = "ActivityID";
			public const string Activity = "Activity";
			public const string UserID = "UserID";
			public const string FullName = "FullName";
			public const string ActivityDate = "ActivityDate";
            public const string IsAnyData = "IsAnyData";
		}

		public DRspActivityHistory_Select(DataSet ds)
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

		public string Activity(int index) 
		{
			return base.getValue(index, Columns.Activity);
		}

		public int UserID(int index) 
		{
			return base.getValueInteger(index, Columns.UserID);
		}

		public string FullName(int index) 
		{
			return base.getValue(index, Columns.FullName);
		}

		public DateTime ActivityDate(int index) 
		{
			return base.getValueDate(index, Columns.ActivityDate);
		}

        public bool IsAnyData(int index)
        {
            return base.getValueBool(index, Columns.IsAnyData);
        }

	}
}
