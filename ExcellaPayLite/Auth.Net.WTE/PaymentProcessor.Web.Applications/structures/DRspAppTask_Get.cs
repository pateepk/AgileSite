namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

	public class DRspAppTask_Get : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string AppTaskID = "AppTaskID";
			public const string StartDate = "StartDate";
			public const string EndDate = "EndDate";
			public const string LastExecuted = "LastExecuted";
			public const string Period = "Period";
			public const string URL = "URL";
            public const string SPName = "SPName";
		}

		public DRspAppTask_Get(DataSet ds)
		{
			base.setData(ds);
		}

		public int AppTaskID(int index) 
		{
			return base.getValueInteger(index, Columns.AppTaskID);
		}

		public DateTime StartDate(int index) 
		{
			return base.getValueDate(index, Columns.StartDate);
		}

		public DateTime EndDate(int index) 
		{
			return base.getValueDate(index, Columns.EndDate);
		}

		public DateTime LastExecuted(int index) 
		{
			return base.getValueDate(index, Columns.LastExecuted);
		}

		public Int64 Period(int index) 
		{
			return base.getValueInteger64(index, Columns.Period);
		}

		public string URL(int index) 
		{
			return base.getValue(index, Columns.URL);
		}

        public string SPName(int index)
        {
            return base.getValue(index, Columns.SPName);
        }

	}
}
