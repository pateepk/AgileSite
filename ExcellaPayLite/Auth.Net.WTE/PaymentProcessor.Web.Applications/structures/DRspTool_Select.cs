namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

	public class DRspTool_Select : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string ToolID = "ToolID";
			public const string Enable = "Enable";
			public const string Title = "Title";
			public const string SortOrder = "SortOrder";
			public const string StoredProcedure = "StoredProcedure";
			public const string DocumentID = "DocumentID";
            public const string SPCreated = "SPCreated";
            public const string SPLastAltered = "SPLastAltered";
		}

		public DRspTool_Select(DataSet ds)
		{
			base.setData(ds);
		}

		public int ToolID(int index) 
		{
			return base.getValueInteger(index, Columns.ToolID);
		}

		public bool Enable(int index) 
		{
			return base.getValueBool(index, Columns.Enable);
		}

		public string Title(int index) 
		{
			return base.getValue(index, Columns.Title);
		}

		public int SortOrder(int index) 
		{
			return base.getValueInteger(index, Columns.SortOrder);
		}

		public string StoredProcedure(int index) 
		{
			return base.getValue(index, Columns.StoredProcedure);
		}

		public Int64 DocumentID(int index) 
		{
			return base.getValueInteger64(index, Columns.DocumentID);
		}

        public DateTime SPCreated(int index)
        {
            return base.getValueDate(index, Columns.SPCreated);
        }

        public DateTime SPLastAltered(int index)
        {
            return base.getValueDate(index, Columns.SPLastAltered);
        }

	}
}
