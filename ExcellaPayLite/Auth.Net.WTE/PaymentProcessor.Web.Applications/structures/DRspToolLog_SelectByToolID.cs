namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

	public class DRspToolLog_SelectByToolID : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string ToolLogID = "ToolLogID";
			public const string ToolID = "ToolID";
			public const string ExecuteDate = "ExecuteDate";
			public const string UserID = "UserID";
            public const string FullName = "FullName";
			public const string XMLDataSet = "XMLDataSet";
		}

		public DRspToolLog_SelectByToolID(DataSet ds)
		{
			base.setData(ds);
		}

		public int ToolLogID(int index) 
		{
			return base.getValueInteger(index, Columns.ToolLogID);
		}

		public int ToolID(int index) 
		{
			return base.getValueInteger(index, Columns.ToolID);
		}

		public DateTime ExecuteDate(int index) 
		{
			return base.getValueDate(index, Columns.ExecuteDate);
		}

		public int UserID(int index) 
		{
			return base.getValueInteger(index, Columns.UserID);
		}

        public string FullName(int index)
        {
            return base.getValue(index, Columns.FullName);
        }

		public string XMLDataSet(int index) 
		{
			return base.getValue(index, Columns.XMLDataSet);
		}

	}
}
